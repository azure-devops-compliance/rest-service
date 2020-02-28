using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using SecurePipelineScan.VstsService.Response;

namespace SecurePipelineScan.Rules.Security
{
    public class PipelineHasAtLeastOneStageWithApproval : IReleasePipelineRule
    {
        [ExcludeFromCodeCoverage] string IRule.Description => "Release pipeline contains 4-eyes approval (SOx)";
        [ExcludeFromCodeCoverage] string IRule.Link => "https://confluence.dev.rabobank.nl/x/DGjlCw";

        public Task<bool?> EvaluateAsync(string projectId,
            ReleaseDefinition releasePipeline)
        {
            if (releasePipeline == null)
                throw new ArgumentNullException(nameof(releasePipeline));

            bool? result = releasePipeline
                .Environments
                .Select(p => p.PreDeployApprovals)
                .Any(p =>
                    p.ApprovalOptions != null
                    && !p.ApprovalOptions.ReleaseCreatorCanBeApprover
                    && p.Approvals.Any(a => a.Approver != null));

            return Task.FromResult(result);
        }
    }
}