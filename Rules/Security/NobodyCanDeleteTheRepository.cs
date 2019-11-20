using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecurePipelineScan.Rules.Permissions;
using SecurePipelineScan.VstsService;
using Requests = SecurePipelineScan.VstsService.Requests;
using Response = SecurePipelineScan.VstsService.Response;

namespace SecurePipelineScan.Rules.Security
{
    public sealed class NobodyCanDeleteTheRepository : IRepositoryRule, IReconcile
    {
        private readonly IVstsRestClient _client;

        public NobodyCanDeleteTheRepository(IVstsRestClient client) => _client = client;

        private const int PermissionBitDeleteRepository = 512;
        private const int PermissionBitManagePermissions = 8192;

        string IRule.Description => "Nobody can delete the repository (SOx)";
        string IRule.Link => "https://confluence.dev.rabobank.nl/x/RI8AD";
        bool IRule.IsSox => true;
        bool IReconcile.RequiresStageId => false;

        string[] IReconcile.Impact => new[]
        {
            "For all security groups the 'Delete Repository' permission is set to Deny",
            "For all security groups the 'Manage Permissions' permission is set to Deny"
        };

        public Task<bool> EvaluateAsync(string projectId, string repositoryId, IEnumerable<Response.MinimumNumberOfReviewersPolicy> policies)
        {
            if (projectId == null)
                throw new ArgumentNullException(nameof(projectId));
            if (repositoryId == null)
                throw new ArgumentNullException(nameof(repositoryId));

            return Permissions(projectId, repositoryId)
                .ValidateAsync();
        }

        public Task ReconcileAsync(string projectId, string itemId, string stageId)
        {
            if (projectId == null)
                throw new ArgumentNullException(nameof(projectId));
            if (itemId == null)
                throw new ArgumentNullException(nameof(itemId));

            return Permissions(projectId, itemId)
                .SetToAsync(PermissionId.Deny);
        }

        private ManagePermissions Permissions(string projectId, string itemId) =>
            ManagePermissions
                .ForRepository(_client, projectId, itemId)
                .Permissions(PermissionBitDeleteRepository, PermissionBitManagePermissions)
                .Allow(PermissionId.NotSet, PermissionId.Deny, PermissionId.DenyInherited)
                .Ignore("Project Collection Administrators", "Project Collection Service Accounts");
    }
}