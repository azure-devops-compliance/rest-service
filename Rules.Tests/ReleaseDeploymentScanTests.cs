using System;
using System.Collections.Generic;
using System.IO;
using AutoFixture;
using ExpectedObjects;
using Newtonsoft.Json.Linq;
using NSubstitute;
using SecurePipelineScan.Rules.Events;
using SecurePipelineScan.Rules.Reports;
using SecurePipelineScan.VstsService;
using Response = SecurePipelineScan.VstsService.Response;
using Shouldly;
using Xunit;

namespace SecurePipelineScan.Rules.Tests
{
    public class ReleaseDeploymentScanTests
    {
        public class Completed
        {
            private IFixture _fixture = new Fixture();
            
            [Fact]
            public void ApprovalSettingsCorrect()
            {
                var input = ReadInput("Completed", "Approved.json");
                _fixture.Customize<Response.ApprovalOptions>(x => x
                    .With(a => a.ReleaseCreatorCanBeApprover, false));
                    
                var client = Substitute.For<IVstsRestClient>();
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Environment>>())
                    .Returns(_fixture.Create<Response.Environment>());
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Release>>())
                    .Returns(_fixture.Create<Response.Release>());

                var scan = new ReleaseDeploymentScan(Substitute.For<IServiceEndpointValidator>(), client);
                
                var report = scan.Completed(input);
                report.HasApprovalOptions.ShouldBeTrue();
            }

            [Fact]
            public void OnlyAutomatedApprovals()
            {
                var input = ReadInput("Completed", "Approved.json");
                _fixture.Customize<Response.ApprovalOptions>(x => x
                    .With(a => a.ReleaseCreatorCanBeApprover, false));

                _fixture.Customize<Response.Approval>(x => x
                    .With(a => a.IsAutomated, true));
                    
                var client = Substitute.For<IVstsRestClient>();
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Release>>())
                    .Returns(_fixture.Create<Response.Release>());
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Environment>>())
                    .Returns(_fixture.Create<Response.Environment>());

                var scan = new ReleaseDeploymentScan(Substitute.For<IServiceEndpointValidator>(), client);
                
                var report = scan.Completed(input);
                report.HasApprovalOptions.ShouldBeFalse();
            }

            [Fact]
            public void RequestedForCanBeApprover()
            {
                var input = ReadInput("Completed", "NotApproved.json");
                _fixture
                    .Customize<Response.ApprovalOptions>(x => x.With(a => a.ReleaseCreatorCanBeApprover, true));

                var client = Substitute.For<IVstsRestClient>();
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Release>>())
                    .Returns(_fixture.Create<Response.Release>());
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Environment>>())
                    .Returns(_fixture.Create<Response.Environment>());

                var scan = new ReleaseDeploymentScan(Substitute.For<IServiceEndpointValidator>(), client);

                var report = scan.Completed(input);
                report
                    .HasApprovalOptions
                    .ShouldBeFalse();
            }
            
            [Fact]
            public void NoBranchFilterForAllArtifacts()
            {
                var input = ReadInput("Completed", "Approved.json");
                    
                var client = Substitute.For<IVstsRestClient>();
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Release>>())
                    .Returns(_fixture.Create<Response.Release>());
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Environment>>())
                    .Returns(_fixture.Create<Response.Environment>());

                var scan = new ReleaseDeploymentScan(Substitute.For<IServiceEndpointValidator>(), client);
                
                var report = scan.Completed(input);
                report.HasBranchFilterForAllArtifacts.ShouldBeFalse();
            }
            
            [Fact]
            public void BranchFilterForAllArtifacts()
            {
                var input = ReadInput("Completed", "Approved.json");
                _fixture.Customize<Response.Release>(x => x
                    .With(a => a.Artifacts, new []{new Response.ArtifactReference { Alias = "some-build-artifact"}}));

                _fixture.Customize<Response.Environment>(x =>
                    x.With(v => v.Conditions, new[]
                    {
                        new Response.Condition
                        {
                            ConditionType = "artifact",
                            Name = "some-build-artifact"
                        }
                    }));
                    
                var client = Substitute.For<IVstsRestClient>();
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Release>>())
                    .Returns(_fixture.Create<Response.Release>());
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Environment>>())
                    .Returns(_fixture.Create<Response.Environment>());

                var scan = new ReleaseDeploymentScan(Substitute.For<IServiceEndpointValidator>(), client);
                
                var report = scan.Completed(input);
                report.HasBranchFilterForAllArtifacts.ShouldBeTrue();
            }


            [Fact]
            public void ReportInformation()
            {
                var expected = new ReleaseDeploymentCompletedReport
                {
                    Project = "proeftuin",
                    Pipeline = "CheckApproval",
                    Release = "Release-1",
                    Environment = "Stage 1",
                    ReleaseId = "1",
                    CreatedDate = DateTime.Parse("2019-01-11T13:34:58.0366887")
                }.ToExpectedObject(ctx =>
                {
                    ctx.Ignore(x => x.HasApprovalOptions);
                    ctx.Ignore(x => x.UsesProductionEndpoints);
                });

                var client = Substitute.For<IVstsRestClient>();
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Release>>())
                    .Returns(_fixture.Create<Response.Release>());
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Environment>>())
                    .Returns(_fixture.Create<Response.Environment>());

                var input = ReadInput("Completed", "Approved.json");
                var scan = new ReleaseDeploymentScan(Substitute.For<IServiceEndpointValidator>(), client);

                var report = scan.Completed(input);
                expected.ShouldEqual(report);
            }

            [Fact]
            public void UsesServiceEndpointValidatorToScan()
            {
                var input = ReadInput("Completed", "NotApproved.json");
                _fixture.Customize<Response.WorkflowTask>(x => x.With(a => a.Inputs, new Dictionary<string, string>
                {
                    ["some-input"] = Guid.NewGuid().ToString()
                }));
                
                var client = Substitute.For<IVstsRestClient>();
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Release>>())
                    .Returns(_fixture.Create<Response.Release>());
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Environment>>())
                    .Returns(_fixture.Create<Response.Environment>());

                var endpoints = Substitute.For<IServiceEndpointValidator>();
                endpoints
                    .IsProduction(Arg.Any<string>(), Arg.Any<Guid>())
                    .Returns(true);
                
                var scan = new ReleaseDeploymentScan(endpoints, client);
                scan.Completed(input);
                
                endpoints
                    .Received()
                    .IsProduction("TAS", Arg.Any<Guid>());
            }

            [Fact]
            public void IgnoresTasks()
            {
                _fixture
                    .Customize<Response.WorkflowTask>(
                        x => x
                            .With(y => y.Inputs, new Dictionary<string, string> {["connection"] = Guid.NewGuid().ToString()})
                            .With(y => y.TaskId, new Guid("dd84dea2-33b4-4745-a2e2-d88803403c1b")));
                
                var input = JObject.FromObject(new
                {
                    createdDate = DateTime.Now.ToString()
                });

                var client = Substitute.For<IVstsRestClient>();
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Release>>())
                    .Returns(_fixture.Create<Response.Release>());
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Environment>>())
                    .Returns(_fixture.Create<Response.Environment>());
                
                var endpoints = Substitute.For<IServiceEndpointValidator>();
                endpoints
                    .IsProduction(Arg.Any<string>(), Arg.Any<Guid>())
                    .Returns(true);
                
                var scan = new ReleaseDeploymentScan(endpoints, client);
                var report = scan.Completed(input);
                
                report.UsesProductionEndpoints.ShouldBeFalse();
            }

            [Fact]
            public void HowToHandleDefaults()
            {
                var expected = new ReleaseDeploymentCompletedReport
                {
                    // All default null values and false for booleans is fine
                }.ToExpectedObject(ctx => ctx.Member(x => x.CreatedDate).UsesComparison(Expect.NotDefault<DateTime>()));
                
                var input = JObject.FromObject(new
                {
                    createdDate = "2019-01-11T13:34:58.0366887Z"
                });
                
                var client = Substitute.For<IVstsRestClient>();
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Release>>())
                    .Returns(_fixture.Create<Response.Release>());
                
                client
                    .Get(Arg.Any<IVstsRestRequest<Response.Environment>>())
                    .Returns(_fixture.Create<Response.Environment>());

                var scan = new ReleaseDeploymentScan(Substitute.For<IServiceEndpointValidator>(), client);

                var report = scan.Completed(input);
                expected.ShouldEqual(report);
            }
        }

        private static JObject ReadInput(string eventType, string file)
        {
            var dir = Path.Join("Assets", "ReleaseDeployment", eventType);
            return JObject.Parse(File.ReadAllText(Path.Join(dir, file)));
        }
    }
}