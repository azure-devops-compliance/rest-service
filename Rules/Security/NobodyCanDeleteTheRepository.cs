using System.Collections.Generic;
using System.Linq;
using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Requests;
using SecurePipelineScan.VstsService.Response;
using ApplicationGroup = SecurePipelineScan.VstsService.Response.ApplicationGroup;

namespace SecurePipelineScan.Rules.Security
{
    public class NobodyCanDeleteTheRepository : NobodyCanDeleteThisBase, IRule, IReconcile
    {
        protected override string PermissionsDisplayName => "Delete repository";
        protected override int[] AllowedPermissions => new[] { PermissionId.NotSet, PermissionId.Deny, PermissionId.DenyInherited };

        private readonly IVstsRestClient _client;
        private readonly string _namespaceId;
        private static readonly string[] IgnoredGroups = { "Project Collection Administrators", "Project Collection Service Accounts" };

        string IRule.Description => "Nobody can delete the repository";
        string IRule.Why => "To enforce auditability, no data should be deleted. Therefore, nobody should be able to delete the repository.";

        public NobodyCanDeleteTheRepository(IVstsRestClient client)
        {
            _client = client;
            _namespaceId = _client
                .Get(VstsService.Requests.SecurityNamespace.SecurityNamespaces())
                .First(s => s.DisplayName == "Git Repositories").NamespaceId;
        }

        protected override IEnumerable<ApplicationGroup> WhichGroups(string projectId, string id)
        {
            return _client.Get(VstsService.Requests.ApplicationGroup.ExplicitIdentitiesRepos(projectId, _namespaceId))
                    .Identities
                    .Where(g => !IgnoredGroups.Contains(g.FriendlyDisplayName));
        }

        protected override IEnumerable<Permission> WhichPermissions(string projectId, string id, IEnumerable<ApplicationGroup> groups)
        {
            return groups.SelectMany(g => _client.Get(Permissions.PermissionsGroupRepository(
                projectId, _namespaceId, g.TeamFoundationId, id)).Permissions);
        }

        protected override PermissionsSetId WhichPermissions(string projectId, string id, ApplicationGroup group)
        {
            return _client.Get(Permissions.PermissionsGroupRepository(projectId, _namespaceId, group.TeamFoundationId, id));
        }

        string[] IReconcile.Impact => new[]
        {
            "For all application groups the 'Delete Repository' permission is set to Deny",
            "For all single users the 'Delete Repository' permission is set to Deny"
        };

        protected override void UpdatePermissionToDeny(string projectId, ApplicationGroup group, PermissionsSetId permissionSetId, Permission permission)
        {
            permission.PermissionId = PermissionId.Deny;
            _client.Post(Permissions.ManagePermissions(projectId, new Permissions.ManagePermissionsData(
                group.TeamFoundationId, permissionSetId.DescriptorIdentifier, permissionSetId.DescriptorIdentityType, permission)));
        }
    }
}