using Bulwark.Integration.GitLab.Api.Hooks;

namespace Bulwark.Integration.GitLab.Events
{
    public class MergeRequestEvent
    {
        public MergeRequestHook MergeRequest { get; set; }
    }
}