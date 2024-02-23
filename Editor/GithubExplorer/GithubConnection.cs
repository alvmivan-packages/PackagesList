using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace PackagesList.GithubExplorer
{
    public class GithubConnection
    {
        const string FormatHttps = "https://raw.githubusercontent.com/{0}/{1}/{2}";

        static string GetHttpUri(string packageURL, string packageBranch, string file) =>
            string.Format(FormatHttps, packageURL, packageBranch, file);


        readonly string packageURL;
        readonly string packageBranch;
        readonly string token;

        public GithubConnection(string packageURL, string packageBranch, string token)
        {
            this.packageURL = packageURL;
            this.packageBranch = packageBranch;
            this.token = token;
        }

        string GetUri(string file) => GetHttpUri(packageURL, packageBranch, file);


        public async Task<string> GetFileContent(string file)
        {
            var uri = GetUri(file);
            Debug.Log("uri is " + uri);
            using var client = new WebClient();
            client.Headers.Add("User-Agent", "UnityWebRequest");
            if (!string.IsNullOrEmpty(token))
            {
                client.Headers.Add("Authorization", "token " + token);
            }

            return await client.DownloadStringTaskAsync(uri);
        }
    }
}