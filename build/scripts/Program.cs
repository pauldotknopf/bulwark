using System;
using Build.Buildary;
using static Bullseye.Targets;
using static Build.Buildary.Directory;
using static Build.Buildary.Path;
using static Build.Buildary.Shell;
using static Build.Buildary.Runner;
using static Build.Buildary.GitVersion;
using static Build.Buildary.File;

namespace Build
{
    static class Program
    {
        static System.Threading.Tasks.Task Main(string[] args)
        {
            var options = ParseOptions<Options>(args);
            var gitversion = GetGitVersion("./");
            var nugetSource = "https://api.nuget.org/v3/index.json";
            var nugetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");
            var dockerUsername = Environment.GetEnvironmentVariable("DOCKER_USERNAME");
            var dockerPassword = Environment.GetEnvironmentVariable("DOCKER_PASSWORD");

            var commandBuildArgs = $"--configuration {options.Configuration}";
            if (!string.IsNullOrEmpty(gitversion.PreReleaseTag))
            {
                commandBuildArgs += $" --version-suffix \"{gitversion.PreReleaseTag}\"";
            }
            
            Target("clean", () =>
            {
                CleanDirectory(ExpandPath("./output"));
            });
            
            Target("build", () =>
            {
                RunShell($"dotnet build Bulwark.sln {commandBuildArgs}");
            });
            
            Target("test", () =>
            {
                RunShell("dotnet test test/Bulwark.Tests/");
            });
            
            Target("deploy", DependsOn("clean"), () =>
            {
                // Deploy our nuget packages.
                RunShell($"dotnet pack --output {ExpandPath("./output")} {commandBuildArgs}");
                RunShell($"dotnet publish src/integration/Bulwark.Integration.WebHook --output {ExpandPath("./output/webhook/linux-x64")} --runtime linux-x64 {commandBuildArgs}");
                CopyFile("./build/docker/Dockerfile", "./output/Dockerfile");
                RunShell("docker build output --tag pauldotknopf/bulwark:build");
            });

            Target("update-version", () =>
            {
                if (FileExists("./build/version.props"))
                {
                    DeleteFile("./build/version.props");
                }
                
                WriteFile("./build/version.props",
$@"<Project>
    <PropertyGroup>
        <VersionPrefix>{gitversion.Version}</VersionPrefix>
    </PropertyGroup>
</Project>");
            });
            
            Target("publish", () =>
            {
                if(Travis.IsTravis)
                {
                    // If we are on travis, we only want to deploy if this is a release tag.
                    if(Travis.EventType != Travis.EventTypeEnum.Push)
                    {
                        // Only pushes (no cron jobs/api/pull rqeuests) can deploy.
                        Log.Warning("Not a push build, skipping publish...");
                        return;
                    }

                    if(Travis.Branch != "master")
                    {
                        // We aren't on master.
                        Log.Warning("Not on master, skipping publish...");
                        return;
                    }
                }

                if(string.IsNullOrEmpty(nugetApiKey))
                {
                    throw new Exception("No NUGET_API_KEY provided.");
                }

                if (string.IsNullOrEmpty(dockerUsername))
                {
                    throw new Exception("No DOCKER_USERNAME provided.");
                }

                if (string.IsNullOrEmpty(dockerPassword))
                {
                    throw new Exception("No DOCKER_PASSWORD provided.");
                }

                foreach(var file in GetFiles("./output", "*.nupkg"))
                {
                    Log.Info($"Deploying {file}");
                    RunShell($"dotnet nuget push {file} --source \"{nugetSource}\" --api-key \"{nugetApiKey}\"");
                }
                
                RunShell($"docker login -u {dockerUsername} -p {dockerPassword}");
                RunShell($"docker tag pauldotknopf/bulwark:build pauldotknopf/bulwark:v{gitversion.FullVersion}");
                RunShell("docker tag pauldotknopf/bulwark:build pauldotknopf/bulwark:latest");
                RunShell($"docker push pauldotknopf/bulwark:v{gitversion.FullVersion}");
                RunShell("docker push pauldotknopf/bulwark:latest");
            });
            
            Target("ci", DependsOn("update-version", "test", "deploy", "publish"));
            
            Target("default", DependsOn("build"));

            return Run(options);
        }

        // ReSharper disable ClassNeverInstantiated.Local
        class Options : RunnerOptions
        // ReSharper restore ClassNeverInstantiated.Local
        {
            [PowerArgs.ArgShortcut("config"), PowerArgs.ArgDefaultValue("Release")]
            public string Configuration { get; set; }
        }
    }
}
