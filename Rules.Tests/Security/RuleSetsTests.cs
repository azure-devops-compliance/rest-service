using System.Linq;
using NSubstitute;
using SecurePipelineScan.Rules.Security;
using SecurePipelineScan.VstsService;
using Shouldly;
using Xunit;

namespace SecurePipelineScan.Rules.Tests
{
    public class RuleSetsTests
    {
        [Fact]
        public void GlobalPermissions()
        {
            var rules = RuleSets.GlobalPermissions(Substitute.For<IVstsRestClient>());
            rules.OfType<NobodyCanDeleteTheTeamProject>().ShouldNotBeEmpty();
        }
    }
}