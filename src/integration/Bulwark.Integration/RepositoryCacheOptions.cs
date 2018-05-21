namespace Bulwark.Integration
{
    public class RepositoryCacheOptions
    {
        public RepositoryCacheOptions()
        {
            RepositoryCacheLocation = "repository-cache";
        }
        
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string RepositoryCacheLocation { get; set; }
    }
}