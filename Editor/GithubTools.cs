using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PackagesList
{
    public class GithubConnectionValidation
    {
        const string GithubUriSshPrefix = "git@github.com:";
        const string GithubUriHttpsPrefix = "https://github.com/";
        const string GitListCommandArguments = "ls-remote";
        const string GitPathSshSuffix = ".git";

        readonly string findFileArguments;
        readonly string connectRepoArguments;

        public GithubConnectionValidation(string packageURL, string packageBranch, string token,
            string fileYouAreLookingFor)
        {
            var uri = new Uri(packageURL);
            var pathSegments = uri.AbsolutePath.Split('/');
            var githubUser = pathSegments[1];
            var packageName = pathSegments[2];
            var isUsingSsh = string.IsNullOrEmpty(token);

            var repositoryPath =
                isUsingSsh ? $"{githubUser}/{packageName}{GitPathSshSuffix}" : $"{githubUser}/{packageName}";

            var fileExpression = $"{packageBranch}:{fileYouAreLookingFor}";
            var githubUriPrefix = isUsingSsh ? GithubUriSshPrefix : GithubUriHttpsPrefix;
            connectRepoArguments = $"{GitListCommandArguments} {githubUriPrefix}{repositoryPath}";
            findFileArguments = connectRepoArguments + " " + fileExpression;
        }

        static ProcessStartInfo GitCommand(string arguments) => new()
        {
            FileName = "git",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        public Task<bool> CanFindFile() => RunProcess(GitCommand(findFileArguments));

        public Task<bool> CanConnectToRepo() => RunProcess(GitCommand(connectRepoArguments));

        static async Task<bool> RunProcess(ProcessStartInfo processInfo)
        {
            using var process = new Process();
            process.StartInfo = processInfo;
            process.Start();
            process.WaitForExit();
            await Task.Yield();
            return process.ExitCode == 0;
        }
    }


    public static class GithubTools
    {
        static readonly Dictionary<string, bool> CanConnectResults = new();

        public static async Task<bool> HasFile(string packageURL, string packageBranch, string token,
            string fileYouAreLookingFor)
        {
            var validation = new GithubConnectionValidation(
                packageURL,
                packageBranch,
                token,
                fileYouAreLookingFor
            );
            
            if (CanConnectResults.TryGetValue(packageURL, out var canConnectCache))
            {
                return canConnectCache && await validation.CanFindFile();
            }

            if (await validation.CanConnectToRepo())
            {
                CanConnectResults[packageURL] = true;
                return await validation.CanFindFile();
            }

            CanConnectResults[packageURL] = false;
            Debug.LogError("error connecting to repo: " + packageURL);
            return false;
        }
    }
}