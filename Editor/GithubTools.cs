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

            var repositoryPath = isUsingSsh
                ? $"{githubUser}/{packageName}.git"
                : $"{githubUser}/{packageName}";

            var expression = $"{packageBranch}:{fileYouAreLookingFor}";

            findFileArguments = isUsingSsh
                ? $"ls-remote git@github.com:{repositoryPath} {expression}"
                : $"ls-remote https://github.com/{repositoryPath} {expression}";

            connectRepoArguments = isUsingSsh
                ? $"ls-remote git@github.com:{repositoryPath}"
                : $"ls-remote https://github.com/{repositoryPath}";
        }

        public async Task<bool> CanFindFile()
        {
            // Use SSH command to check if the file exists

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = findFileArguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };


            Debug.Log("Command to run is " + processStartInfo.FileName + " " + processStartInfo.Arguments);

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();

            await process.StandardOutput.ReadToEndAsync();
            await process.StandardError.ReadToEndAsync();

            return process.ExitCode == 0;
        }

        public async Task<(bool can, string reason)> CanConnectToRepo()
        {
            // Use SSH command to check if the file exists

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = connectRepoArguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Debug.Log("Command to run is " + processStartInfo.FileName + " " + processStartInfo.Arguments);

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();

            await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            return (process.ExitCode == 0, error);
        }
    }


    public static class GithubTools
    {
        //dictionary repo->bool to catch canConnect results
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


            //write can  connect if cache
            if (CanConnectResults.TryGetValue(packageURL, out var canConnectCache))
            {
                return canConnectCache && await validation.CanFindFile();
            }

            var (canConnect, cantConnectReason) = await validation.CanConnectToRepo();

            if (canConnect)
            {
                CanConnectResults[packageURL] = true;
                return await validation.CanFindFile();
            }

            CanConnectResults[packageURL] = false;
            Debug.LogError("error connecting to repo: " + cantConnectReason);
            return false;
        }
    }
}