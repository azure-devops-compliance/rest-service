namespace SecurePipelineScan.VstsService.Response
{
    public class BuildDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TeamProjectReference Project { get; set; }
    }
}