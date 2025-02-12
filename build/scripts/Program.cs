using Build.Buildary;
using static Build.Buildary.Runner;

namespace Build
{
    static class Program
    {
        static void Main(string[] args)
        {
            var options = ParseOptions<RunnerOptions>(args);
            ProjectDefinition.Register(options, new ProjectDefinition
            {
                SolutionPath = "./Bulwark.sln"
            });
            Execute(options);
        }
    }
}
