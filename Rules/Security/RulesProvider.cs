using System;
using System.Collections.Generic;
using SecurePipelineScan.VstsService;

namespace SecurePipelineScan.Rules.Security
{
    public interface IRulesProvider
    {
        IEnumerable<IProjectRule> GlobalPermissions(IVstsRestClient client);
        IEnumerable<IRule> RepositoryRules(IVstsRestClient client);
        IEnumerable<IRule> BuildRules(IVstsRestClient client);
    }

    public class RulesProvider : IRulesProvider
    {
        public IEnumerable<IProjectRule> GlobalPermissions(IVstsRestClient client)
        {
            yield return new NobodyCanDeleteTheTeamProject(client);
        }

        public IEnumerable<IRule> RepositoryRules(IVstsRestClient client)
        {
            yield return new NobodyCanDeleteTheRepository(client);
            yield return new ReleaseBranchesProtectedByPolicies(client);
        }

        public IEnumerable<IRule> BuildRules(IVstsRestClient client)
        {
            yield return NobodyCanDeleteThePipeline.Build(client);
        }
    }
}