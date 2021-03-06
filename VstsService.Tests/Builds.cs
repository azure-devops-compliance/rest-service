using System;
using System.Linq;
using System.Threading.Tasks;
using SecurePipelineScan.VstsService.Requests;
using Shouldly;
using Xunit;

namespace SecurePipelineScan.VstsService.Tests
{
    public class Builds : IClassFixture<TestConfig>
    {
        private readonly TestConfig _config;
        private readonly IVstsRestClient _client;

        public Builds(TestConfig config)
        {
            _config = config;
            _client = new VstsRestClient(_config.Organization, _config.Token);
        }

        [Fact]
        [Trait("category", "integration")]
        public void QueryArtifacts()
        {
            var artifacts = _client.Get(Requests.Builds.Artifacts(_config.Project, _config.BuildId)).ToList();
            artifacts.ShouldNotBeEmpty();

            var artifact = artifacts.First();
            artifact.Id.ShouldNotBe(0);

            artifact.Resource.ShouldNotBeNull();
            artifact.Resource.Type.ShouldBe("Container");
        }

        [Fact]
        [Trait("category", "integration")]
        public async Task QueryBuild()
        {
            var build = await _client.GetAsync(Requests.Builds.Build(_config.Project, _config.BuildId));
            build.ShouldNotBeNull();
            build.Id.ShouldNotBe(0);
            build.Definition.ShouldNotBeNull();
            build.Project.ShouldNotBeNull();
            build.Result.ShouldNotBeNull();
        }

        [Fact]
        [Trait("category", "integration")]
        public void QueryLongRunningBuilds()
        {
            var queryOrder = "startTimeAscending";
            var minTime = DateTime.UtcNow.AddHours(-6).ToString("O");
            var build = _client.Get(Requests.Builds.LongRunningBuilds(_config.Project, queryOrder, minTime)).ToList();
            build.ShouldNotBeNull();
        }

        [Fact]
        [Trait("category", "integration")]
        public async Task QueryBuildDefinition()
        {
            var buildDefinition =
                await _client.GetAsync(Requests.Builds.BuildDefinition(_config.Project, _config.BuildDefinitionId));

            buildDefinition.ShouldNotBeNull();
            buildDefinition.Id.ShouldNotBeNull();
            buildDefinition.Name.ShouldNotBeNull();
            buildDefinition.Project.ShouldNotBeNull();
            buildDefinition.Process.Type.ShouldNotBeNull();
            buildDefinition.Process.Phases.First().Steps.First().Task.Id.ShouldNotBeNull();
            buildDefinition.Repository.ShouldNotBeNull();
            buildDefinition.Repository.Url.ShouldNotBeNull();
        }

        [Fact]
        public async Task QueryBuildDefinitionsReturnsBuildDefinitionsWithTeamProjectReference()
        {
            var projectId = (await _client.GetAsync(Project.Properties(_config.Project))).Id;

            var buildDefinitions = _client.Get(Requests.Builds.BuildDefinitions(projectId)).ToList();

            buildDefinitions.ShouldNotBeNull();
            buildDefinitions.First().Id.ShouldNotBeNull();
            buildDefinitions.First().Project.Id.ShouldNotBeNull();
        }

        [Fact]
        public async Task QueryBuildDefinitionsReturnsBuildDefinitionsWithExtendedProperties()
        {
            var projectId = (await _client.GetAsync(Project.Properties(_config.Project))).Id;

            var buildDefinitions = await _client.GetAsync(Requests.Builds.BuildDefinitions(projectId, true).Request.AsJson());

            buildDefinitions.ShouldNotBeNull();
            buildDefinitions.SelectTokens("value[*].process").Count().ShouldBeGreaterThan(0);
        }

        [Fact]
        [Trait("category", "integration")]
        public async Task CanQueryBuildDefinitionsByProcessType()
        {
            var projectId = (await _client.GetAsync(Project.Properties(_config.Project))).Id;

            var buildDefinitions = _client.Get(Requests.Builds.BuildDefinitions(projectId, 2)).ToList();

            buildDefinitions.ShouldNotBeNull();
            buildDefinitions.First().Id.ShouldNotBeNull();
            buildDefinitions.First().Project.Id.ShouldNotBeNull();
        }

        [Fact]
        [Trait("category", "integration")]
        public async Task GetProjectRetentionSetting()
        {
            var retentionSettings = await _client.GetAsync(Requests.Builds.Retention(_config.Project))
                .ConfigureAwait(false);

            retentionSettings.ShouldNotBeNull();
            retentionSettings.PurgeRuns.ShouldNotBeNull();
            retentionSettings.PurgeRuns.Value.ShouldNotBeNull();
        }
    }
}