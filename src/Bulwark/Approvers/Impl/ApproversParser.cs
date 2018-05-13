using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bulwark.Approvers.Impl
{
    public class ApproversParser : IApproversParser
    {
        public Task<ApproversConfig> ParseConfig(string content)
        {
            List<string> approvers = null;
            List<string> appendedApprovers = null;

            if (string.IsNullOrEmpty(content))
                return Task.FromResult(new ApproversConfig(null, null));

            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    
                    if (line.StartsWith("Approvers:"))
                    {
                        if (approvers == null)
                            approvers = new List<string>();
                        foreach (var approver in line.Substring("Approvers:".Length).Split(',').Select(x => x.Trim().ToLower()))
                            if (!string.IsNullOrEmpty(approver) && !approvers.Contains(approver))
                                approvers.Add(approver);
                    }
                    else if (line.StartsWith("AppendedApprovers:"))
                    {
                        if (appendedApprovers == null)
                            appendedApprovers = new List<string>();
                        foreach (var appendedApprover in line.Substring("AppendedApprovers:".Length).Split(',').Select(x => x.Trim().ToLower()))
                            if (!string.IsNullOrEmpty(appendedApprover) && !appendedApprovers.Contains(appendedApprover))
                                appendedApprovers.Add(appendedApprover);
                    }
                }
            }
            
            return Task.FromResult(new ApproversConfig(approvers?.AsReadOnly(), appendedApprovers?.AsReadOnly()));
        }
    }
}