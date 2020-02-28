using SecurePipelineScan.VstsService;
using SecurePipelineScan.VstsService.Response;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SecurePipelineScan.Rules.Security
{
    public class ArtifactIsStoredSecure : IPipelineHasTaskRule, IBuildPipelineRule
    {
        private readonly PipelineEvaluatorFactory _pipelineEvaluatorFactory;

        public ArtifactIsStoredSecure(IVstsRestClient client)
        {
            this._pipelineEvaluatorFactory = new PipelineEvaluatorFactory(client);
        }

        public string TaskId => "2ff763a7-ce83-4e1f-bc89-0ae63477cebe";
        public string TaskName => "PublishBuildArtifacts";
        public string StepName => "publish";
        public Dictionary<string, object> Inputs => new Dictionary<string, object>();

        [ExcludeFromCodeCoverage] string IRule.Description => "Artifact is stored in secure artifactory (SOx)";
        [ExcludeFromCodeCoverage] string IRule.Link => "https://confluence.dev.rabobank.nl/x/TI8AD";

        public async Task<bool?> EvaluateAsync(Project project, BuildDefinition buildPipeline)
        {
            return await this._pipelineEvaluatorFactory.Create(buildPipeline).EvaluateAsync(project, buildPipeline, this).ConfigureAwait(false);
        }
    }
}