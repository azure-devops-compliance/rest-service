using System;
using SecurePipelineScan.Rules.Release;
using Shouldly;
using Xunit;
using Rules.Tests.Helpers;

namespace SecurePipelineScan.Rules.Release.Tests
{
    public class LastModifiedByNotTheSameAsApprovedByTests
    {
        private readonly LastModifiedByNotTheSameAsApprovedBy rule = new LastModifiedByNotTheSameAsApprovedBy();
        
        [Fact]
        public void GivenReleaseModified_ApprovedByEqualsLastModifiedBy_ThenResultFalse()
        {
            var id = Guid.NewGuid();
            var release = ReleaseBuilder.Create(id, id);

            rule.GetResult(release).ShouldBeFalse();
        }

        [Fact]
        public void GivenReleaseModified_ApprovedByNotEqualsLastModifiedBy_ThenResultTrue()
        {
            var release = ReleaseBuilder.Create();
            rule.GetResult(release).ShouldBeTrue();
        }

    }
}