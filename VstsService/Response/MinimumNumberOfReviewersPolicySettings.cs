﻿using System.Collections.Generic;

namespace SecurePipelineScan.VstsService.Response
{
    public class MinimumNumberOfReviewersPolicySettings
    {
        public int? MinimumApproverCount { get; set; }
        public bool? CreatorVoteCounts { get; set; }
        public bool? AllowDownvotes { get; set; }
        public bool? ResetOnSourcePush { get; set; }

        public List<Scope> Scope { get; set; }

    }
}