﻿using SecurePipelineScan.Rules.Reports;
using SecurePipelineScan.VstsService.Response;

namespace Rules.Reports
{
    public class RepositoryReport : EndpointReport
    {
        public string Project { get; set; }
        public string Repository { get; set; }
        public bool HasRequiredReviewerPolicy { get; set; }
    }
}