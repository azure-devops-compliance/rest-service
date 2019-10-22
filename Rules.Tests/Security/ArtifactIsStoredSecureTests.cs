using System.Threading.Tasks;
using SecurePipelineScan.Rules.Security;
using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Requests;
using Response = SecurePipelineScan.VstsService.Response;
using Shouldly;
using Xunit;
using AutoFixture;
using AutoFixture.AutoNSubstitute;

namespace SecurePipelineScan.Rules.Tests.Security
{
    public class ArtifactIsStoredSecureTests : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;
        private readonly Fixture _fixture = new Fixture { RepeatCount = 1 };

        public ArtifactIsStoredSecureTests(TestConfig config)
        {
            _config = config;
            _fixture.Customize(new AutoNSubstituteCustomization());
        }

        [Fact]
        public async Task EvaluateBuildIntegrationTest()
        {
            var client = new VstsRestClient(_config.Organization, _config.Token);
            var projectId = (await client.GetAsync(Project.Properties(_config.Project))).Id;
            var buildPipeline = await client.GetAsync(Builds.BuildDefinition(projectId, "2"))
                .ConfigureAwait(false);

            var rule = new ArtifactIsStoredSecure();
            var result = await rule.EvaluateAsync(projectId, buildPipeline);

            result.GetValueOrDefault().ShouldBeTrue();
        }

        [Fact]
        public async Task GivenPipeline_WhenYaml_ThenEvaluatesToNull()
        {
            _fixture.Customize<Response.BuildProcess>(ctx => ctx
                .With(p => p.Type, 2));
            var buildPipeline = _fixture.Create<Response.BuildDefinition>();
            var projectId = _fixture.Create<string>();

            var rule = new ArtifactIsStoredSecure();
            var result = await rule.EvaluateAsync(projectId, buildPipeline);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task GivenPipeline_WhenGuiAndNoPhases_ThenEvaluatesToException()
        {
            _fixture.Customize<Response.BuildProcess>(ctx => ctx
                .With(p => p.Type, 1)
                .Without(p => p.Phases));
            var buildPipeline = _fixture.Create<Response.BuildDefinition>();
            var projectId = _fixture.Create<string>();

            var rule = new ArtifactIsStoredSecure();
            var exception = await Record.ExceptionAsync(async () =>
                await rule.EvaluateAsync(projectId, buildPipeline));

            exception.ShouldNotBeNull();
        }

        [Fact]
        public async Task GivenPipeline_WhenGuiAndMavenAndTaskIsNotFound_ThenEvaluatesToNull()
        {
            _fixture.Customize<Response.BuildProcess>(ctx => ctx
                .With(p => p.Type, 1));
            _fixture.Customize<Response.BuildStep>(ctx => ctx
                .With(s => s.Enabled, true));
            _fixture.Customize<Response.BuildTask>(ctx => ctx
                .With(t => t.Id, "ac4ee482-65da-4485-a532-7b085873e532"));
            var buildPipeline = _fixture.Create<Response.BuildDefinition>();
            var projectId = _fixture.Create<string>();

            var rule = new ArtifactIsStoredSecure();
            var result = await rule.EvaluateAsync(projectId, buildPipeline);

            result.ShouldBeNull();
        }
    }
}