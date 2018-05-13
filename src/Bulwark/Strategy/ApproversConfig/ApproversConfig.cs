using System.Collections.Generic;

namespace Bulwark.Strategy.ApproversConfig
{
    public class ApproversConfig
    {
        public ApproversConfig(IReadOnlyCollection<string> approvers, IReadOnlyCollection<string> appendedApprovers)
        {
            Approvers = approvers;
            AppendedApprovers = appendedApprovers;
        }

        public IReadOnlyCollection<string> Approvers { get; }

        public IReadOnlyCollection<string> AppendedApprovers { get; }
    }
}