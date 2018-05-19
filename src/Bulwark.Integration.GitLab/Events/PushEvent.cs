using Bulwark.Integration.GitLab.Api.Hooks;
using Newtonsoft.Json;

namespace Bulwark.Integration.GitLab.Events
{
    public class PushEvent
    {
        public PushHook Push { get; set; }
    }
}