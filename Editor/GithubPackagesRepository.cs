using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PackagesList
{
    public class GithubPackagesRepository
    {
        public static Task<IReadOnlyList<PackageInfo>> DownloadPackages(string token, string githubUser,
            bool isOrganization) =>
            new GithubPackagesRepository(token, githubUser, isOrganization)
                .DownloadPackages();

        readonly string token;
        readonly string githubUser;
        readonly bool isOrganization;
        readonly WebClient webClient;
        readonly string url;


        GithubPackagesRepository(string token, string githubUser, bool isOrganization)
        {
            this.token = token;
            this.githubUser = githubUser;
            this.isOrganization = isOrganization;
            webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "UnityWebRequest");
            url = PrepareWebClient();
        }


        async Task<IReadOnlyList<PackageInfo>> DownloadPackages()
        {
            var json = await webClient.DownloadStringTaskAsync(url);


            var fromJson = RepositoriesJsonHelper.FromJson<RepositoryDto>(json);

            if (fromJson.Length == 0) return new List<PackageInfo>();

            Debug.Log(fromJson.Length + " repositories found " + json);

            var list = fromJson.Select(ToPackageInfo).ToList();


            var packageJsonsTask = list.Select(GetPackageJson).ToArray();


            var packageJsons = await Task.WhenAll(packageJsonsTask);


            list = list
                .Select((package, i) => package.SetPackageInfo(packageJsons[i]))
                .Where(p => !string.IsNullOrEmpty(p.version))
                .ToList();

            return list;
        }

        string GetUrlForUpm(RepositoryDto repoDto) => repoDto.@private
            ? $"https://x-access-token:{token}@github.com/{githubUser}/{repoDto.name}.git#{repoDto.default_branch}"
            : $"{repoDto.html_url}.git#{repoDto.default_branch}";

        PackageInfo ToPackageInfo(RepositoryDto repository) => new(repository.name, repository.html_url,
            GetUrlForUpm(repository), repository.@private, repository.default_branch);

        string PrepareWebClient()
        {
            var auth = !string.IsNullOrEmpty(token);

            var noAuth = $"https://api.github.com/users/{githubUser}/repos";

            if (!auth) return noAuth;

            webClient.Headers.Add("Authorization", "token " + token);

            if (isOrganization)
            {
                return $"https://api.github.com/orgs/{githubUser}/repos";
            }

            return noAuth;
        }


        async Task<string> GetPackageJson(PackageInfo package)
        {
            try
            {
                return await GithubTools.GetFileContent(package.innerUrl, package.branch, token, "package.json");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return string.Empty;
        }


        [Serializable]
        struct RepositoryDto
        {
            public string name;
            public string html_url;
            public string default_branch;
            public bool @private;
        }
    }
}