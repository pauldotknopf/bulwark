using System.IO;

namespace Bulwark.Strategy.CodeOwners.Impl
{
    public class ContextDirectory
    {
        public string Name { get; set; }
        
        public CodeOwnerConfig Config { get; set; }
    }
}