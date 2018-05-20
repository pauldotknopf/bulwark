using System;
using System.Threading.Tasks;
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
        static Task<int> Main(string[] args)
        {
            var options = ParseOptions<Options>(args);
            var gitversion = GetGitVersion("./");
            var nugetSource = "https://api.nuget.org/v3/index.json";
            var nugetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");

            var commandBuildArgs = $"--configuration {options.Configuration}";
            if (!string.IsNullOrEmpty(gitversion.PreReleaseTag))
            {
                commandBuildArgs += $" --version-suffix \"{gitversion.PreReleaseTag}\"";
            }
            
            Add("clean", () =>
            {
                CleanDirectory(ExpandPath("./output"));
            });
            
            Add("build", () =>
            {
                RunShell($"dotnet build Bulwark.sln {commandBuildArgs}");
            });
            
            Add("test", () =>
            {
                RunShell("dotnet test test/Bulwark.Tests/");
            });
            
            Add("deploy", DependsOn("clean"), () =>
            {
                // Deploy our nuget packages.
                RunShell($"dotnet pack --output {ExpandPath("./output")} {commandBuildArgs}");
            });

            Add("update-version", () =>
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
            
            Add("publish", () =>
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
                    }
                }

                if(string.IsNullOrEmpty(nugetApiKey))
                {
                    throw new Exception("No NUGET_API_KEY provided.");
                }

                foreach(var file in GetFiles("./output", "*.nupkg"))
                {
                    Log.Info($"Deploying {file}");
                    RunShell($"dotnet nuget push {file} --source \"{nugetSource}\" --api-key \"{nugetApiKey}\"");
                }
            });
            
            Add("ci", DependsOn("update-version", "test", "deploy", "publish"));
            
            Add("default", DependsOn("build"));

            return Run(options);
        }

        // ReSharper disable ClassNeverInstantiated.Local
        class Options : RunnerOptions
        // ReSharper restore ClassNeverInstantiated.Local
        {
            [PowerArgs.ArgShortcut("config"), PowerArgs.ArgDefaultValue("Release")]
            public string Configuration => null;
        }
    }
}
