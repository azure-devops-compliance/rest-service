﻿namespace SecurePipelineScan.VstsService.Response
{
    public class Repository
    {
        public string Name { get; set; }
        public string Id { get; internal set; }

        public string DefaultBranch { get; set; }

        public Project Project { get; set; }
    }
}