using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitTools.Testing;
using GitVersion.BuildAgents;
using GitVersion.MsBuild.Tests.Mocks;
using GitVersionCore.Tests;

namespace GitVersion.MsBuild.Tests.Helpers
{
    public class MsBuildTaskFixture
    {
        private readonly RepositoryFixtureBase fixture;
        private KeyValuePair<string, string>[] environmentVariables;

        public MsBuildTaskFixture(RepositoryFixtureBase fixture)
        {
            this.fixture = fixture;
        }

        public void WithEnv(params KeyValuePair<string, string>[] envs)
        {
            environmentVariables = envs;
        }

        public MsBuildTaskFixtureResult<T> Execute<T>(T task) where T : GitVersionTaskBase
        {
            return UsingEnv(() =>
            {
                var buildEngine = new MockEngine();

                task.BuildEngine = buildEngine;

                var versionFile = Path.Combine(task.SolutionDirectory, "gitversion.json");
                fixture.WriteVersionVariables(versionFile);

                task.VersionFile = versionFile;

                var result = task.Execute();

                return new MsBuildTaskFixtureResult<T>(fixture)
                {
                    Success = result,
                    Task = task,
                    Errors = buildEngine.Errors,
                    Warnings = buildEngine.Warnings,
                    Messages = buildEngine.Messages,
                    Log = buildEngine.Log,
                };
            });
        }

        private T UsingEnv<T>(Func<T> func)
        {
            ResetEnvironment();
            SetEnvironmentVariables(environmentVariables);

            try
            {
                return func();
            }
            finally
            {
                ResetEnvironment();
            }
        }

        private static void ResetEnvironment()
        {
            var environmentalVariables = new Dictionary<string, string>
            {
                { TeamCity.EnvironmentVariableName, null },
                { AppVeyor.EnvironmentVariableName, null },
                { TravisCi.EnvironmentVariableName, null },
                { Jenkins.EnvironmentVariableName, null },
                { AzurePipelines.EnvironmentVariableName, null },
                { GitHubActions.EnvironmentVariableName, null },
            };

            SetEnvironmentVariables(environmentalVariables.ToArray());
        }

        private static void SetEnvironmentVariables(KeyValuePair<string, string>[] envs)
        {
            if (envs == null) return;
            foreach (var env in envs)
            {
                System.Environment.SetEnvironmentVariable(env.Key, env.Value);
            }
        }
    }
}
