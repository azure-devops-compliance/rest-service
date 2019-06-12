using System.Threading.Tasks;
using SecurePipelineScan.Rules.Security;
using SecurePipelineScan.VstsService;
using Shouldly;
using Xunit;

namespace SecurePipelineScan.Rules.Tests.Security
{
    public class NobodyCanDeleteReleasePipelinesTests : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;

        public NobodyCanDeleteReleasePipelinesTests(TestConfig config)
        {
            _config = config;
        }

        [Fact]
        public async Task EvaluateReleasePipelineIntegrationTest()
        {
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var projectId = (await client.GetAsync(VstsService.Requests.Project.Properties(_config.Project))).Id;

            var rule = new NobodyCanDeleteReleasePipelines(client);
            (await rule.Evaluate(projectId, "1")).ShouldBeTrue();
        }

        [Fact]
        public async Task ReconcileReleasePipelineIntegrationTest()
        {
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var projectId = (await client.GetAsync(VstsService.Requests.Project.Properties(_config.Project))).Id;

            var rule = new NobodyCanDeleteReleasePipelines(client) as IReconcile;
            await rule.Reconcile(projectId, "1");
        }
    }
}