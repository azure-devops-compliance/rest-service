﻿using Domain;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VstsService
{
    public class VstsScanner : IVstsScanner
    {
        private const string authorisation = "authorization";

        private readonly string authorisationHeader;
        private readonly string accountName;
        private string project;

        /// <summary>
        /// A VSTS project scanner
        /// </summary>
        /// <param name="authorisationHeader"></param>
        /// <param name="accountName"></param>
        public VstsScanner(string authorisationHeader, string accountName)
        {
            this.authorisationHeader = authorisationHeader;
            this.accountName = accountName;
        }

        /// <summary>
        /// Scans the name of the project
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public ProjectScanResult ScanProject(string projectName)
        {
            project = projectName;

            ProjectScanResult result = new ProjectScanResult(projectName)
            {
                ReleaseDefinitions = ScanReleaseDefinitions()
            };

            var builds = result.ReleaseDefinitions.SelectMany(x => x.BuildDefinitions);
            result.BuildDefinitions = ScanBuildDefinitions(builds);

            return result;
        }

        private IEnumerable<Domain.BuildDefinition> ScanBuildDefinitions(IEnumerable<BuilDefinition> builddefinitions)
        {
            foreach (var build in builddefinitions)
            {
                yield return new Domain.BuildDefinition
                {
                    Name = build.Name,
                };
            }
        }

        private IEnumerable<Domain.ReleaseDefinition> ScanReleaseDefinitions()
        {
            var releaseDefs = new List<ReleaseDefinition.RootObject>();

            var listOfReleaseDefs = new List<Domain.ReleaseDefinition>();

            foreach (var id in GetReleaseDefinitions().
                value.Select(x => x.id))
            {
                yield return AddEnvironments(id);
            }
        }

        private Domain.ReleaseDefinition AddEnvironments(int id)
        {
            var rleaseDef = GetReleaseDefinitionById(id.ToString());

            var envs = new List<Domain.Environment>();
            foreach (var env in rleaseDef.environments)
            {
                bool artifactFilter = GetArtifactFilter(env);
                var approvers = GetPreDeployApprovers(env);

                envs.Add(new Domain.Environment
                {
                    Name = env.name,
                    ArtifactFilter = artifactFilter,
                    PreDeployApprovers = approvers,
                });
            }

            return (new Domain.ReleaseDefinition
            {
                Name = rleaseDef.name,
                Environments = envs,
                Id = rleaseDef.id,
                BuildDefinitions = rleaseDef.artifacts.
                Select(x => new BuilDefinition
                {
                    ProjectId = x.definitionReference.project.id,
                    BuildId = x.definitionReference.definition.id,
                    Name = x.definitionReference.definition.name,
                })
            });
        }

        private IEnumerable<string> GetPreDeployApprovers(ReleaseDefinition.Environment env)
        {
            foreach (var approval in env.preDeployApprovals.approvals)
            {
                if (approval.isAutomated)
                {
                    break;
                }

                yield return approval.approver.displayName;
            }
        }

        private static bool GetArtifactFilter(ReleaseDefinition.Environment env)
        {
            bool artifactFilter = false;

            var artifactConditions = env.conditions.
                FirstOrDefault(x => x.conditionType == "artifact");

            if (artifactConditions != null)
            {
                var releaseDefinitions = JsonConvert.DeserializeObject<ArtifactCondition.RootObject>(artifactConditions.value);
                artifactFilter = releaseDefinitions.sourceBranch.Equals("master");
            }

            return artifactFilter;
        }

        public void GetReleasesByTeam()
        {
            IRestResponse response = ExecuteGetRequest(
                $"https://{accountName}.vsrm.visualstudio.com/{project}/_apis/release/releases?api-version=4.1-preview.6");

            var rel = JsonConvert.DeserializeObject<Releases.RootObject>(response.Content);

            var allReleaseDefinitions = rel.value.Select(x => x.releaseDefinition.url).Distinct();

            foreach (var releasedef in allReleaseDefinitions)
            {
                GetReleaseDefinitionById(releasedef);
            }
            Console.WriteLine(rel);
        }

        private ReleaseDefinitions.RootObject GetReleaseDefinitions()
        {
            IRestResponse response = ExecuteGetRequest($"https://{accountName}.vsrm.visualstudio.com/{project}/_apis/Release/definitions/");

            return JsonConvert.DeserializeObject<ReleaseDefinitions.RootObject>(response.Content);
        }

        private ReleaseDefinition.RootObject GetReleaseDefinitionById(string releasedef)
        {
            // Get Releases
            IRestResponse response = ExecuteGetRequest($"https://{accountName}.vsrm.visualstudio.com/{project}/_apis/Release/definitions/{releasedef}");

            var releaseDef = JsonConvert.DeserializeObject<ReleaseDefinition.RootObject>(response.Content);

            // Get Builds for Releases
            foreach (var artifact in releaseDef.artifacts)
            {
                string defintionId = artifact.definitionReference.definition.id;

                GetBuildDefinitionById(defintionId);
            }
            return releaseDef;
        }

        private void GetBuildDefinitionById(string definitionId)
        {
            IRestResponse response = ExecuteGetRequest($"https://{accountName}.visualstudio.com/{project}/_apis/build/definitions/{definitionId}?api-version=4.1");

            var buildDef = JsonConvert.DeserializeObject<BuildDefinition.RootObject>(response.Content);

            Console.WriteLine(buildDef);

            GetGitRepositoryById(buildDef.repository.id);

            GetPolicies();
        }

        private void GetPolicies()
        {
            string url = $"https://{accountName}.visualstudio.com/{project}/_apis/policy/configurations?api-version=5.0-preview.1";
            IRestResponse response = ExecuteGetRequest(url);

            var policies = JsonConvert.DeserializeObject<Policies.RootObject>(response.Content);
        }

        private void GetGitRepositoryById(string id)
        {
            string url = $"https://{accountName}.visualstudio.com/{project}/_apis/policy/configurations?api-version=5.0-preview.1";
            IRestResponse response = ExecuteGetRequest(url);

            var gitDef = JsonConvert.DeserializeObject<GitRepositories.RootObject>(response.Content);

            foreach (var gitRepo in gitDef.value)
            {
            }
        }

        /// <summary>
        /// Executes a GET request with auth header
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private IRestResponse ExecuteGetRequest(string url)
        {
            var client = new RestClient(url);

            var request = new RestRequest(Method.GET);
            request.AddHeader(authorisation, authorisationHeader);

            return client.Execute(request);
        }
    }
}