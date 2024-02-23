using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

namespace PackagesList.Tokens
{
    public static class TokenHandler
    {
        const string GetTokenCommandProgram = "git";
        const string GetTokenCommandArguments = "config --get github.token";
        const string GithubGetTokenPage = "https://github.com/settings/tokens";

        public static void OpenGithubPageToGenerateAToken()
        {
            // open the github page to generate an user token
            Application.OpenURL(GithubGetTokenPage);
        }

        public static async Task<string> GetToken()
        {
            //windows
            //execute command and return the output
            //if any error return null
            var processStartInfo = new ProcessStartInfo
            {
                FileName = GetTokenCommandProgram,
                Arguments = GetTokenCommandArguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(processStartInfo);
            if (process is null)
            {
                return null;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();
            process.Close();

            return output;
        }
    }
}