namespace Bulwark.Integration.GitLab.Events
{
    public class UpdateMergeRequestEvent
    {
        public int ProjectId { get; set; }
        
        public int MergeRequestIid { get; set; }
    }
}