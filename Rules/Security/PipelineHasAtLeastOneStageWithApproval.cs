using System;
using System.Linq;
using System.Threading.Tasks;
using SecurePipelineScan.VstsService.Response;

namespace SecurePipelineScan.Rules.Security
{
    public class PipelineHasAtLeastOneStageWithApproval : IReleasePipelineRule
    {
        string IRule.Description => "Release pipeline contains 4-eyes approval";
        string IRule.Why =>
            "To make sure production releases are approved by at least one other person";
        bool IRule.IsSox => true;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<bool> EvaluateAsync(string projectId, ReleaseDefinition releasePipeline)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (releasePipeline == null)
                throw new ArgumentNullException(nameof(releasePipeline));

            return releasePipeline
                .Environments
                .Select(p => p.PreDeployApprovals)
                .Any(p =>
                    p.ApprovalOptions != null
                    && !p.ApprovalOptions.ReleaseCreatorCanBeApprover
                    && p.Approvals.Any(a => a.Approver != null));
        }
    }
}