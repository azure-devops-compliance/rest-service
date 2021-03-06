using System;
using System.Threading.Tasks;
using SecurePipelineScan.VstsService.Requests;
using SecurePipelineScan.VstsService.Response;

namespace SecurePipelineScan.VstsService.Permissions
{
    public class ForMasterBranch : IFor
    {
        private const string MasterBranchId = "6d0061007300740065007200";
        private const string MasterBranchName = "master";

        private readonly IVstsRestClient _client;
        private readonly string _projectId;
        private readonly string _itemId;

        public ForMasterBranch(IVstsRestClient client, string projectId, string itemId)
        {
            _client = client;
            _projectId = projectId;
            _itemId = itemId;
        }

        public Task<ApplicationGroups> IdentitiesAsync() =>
            _client.GetAsync(Requests.ApplicationGroup.ExplicitIdentitiesMasterBranch(_projectId, SecurityNamespaceIds.GitRepositories, _itemId));

        public Task<PermissionsSetId> PermissionSetAsync(Response.ApplicationGroup identity)
        {
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));

            return _client.GetAsync(Requests.Permissions.PermissionsGroupMasterBranch(_projectId, SecurityNamespaceIds.GitRepositories, identity.TeamFoundationId, _itemId));
        }

        public System.Threading.Tasks.Task UpdateAsync(Response.ApplicationGroup identity,
                Response.PermissionsSetId permissionSet, VstsService.Response.Permission permission)
        {
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));

            return _client.PostAsync(Requests.Permissions.ManagePermissions(_projectId),
                    new ManagePermissionsData(identity.TeamFoundationId, permissionSet.DescriptorIdentifier,
                    permissionSet.DescriptorIdentityType, ExtractToken(permission.PermissionToken), permission).Wrap());
        }

        private static string ExtractToken(string token) => 
            token.Replace(MasterBranchId, MasterBranchName, StringComparison.CurrentCulture);
    }
}