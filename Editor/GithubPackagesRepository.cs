using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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


        GithubPackagesRepository(string token, string githubUser, bool isOrganization)
        {
            this.token = token;
            this.githubUser = githubUser;
            this.isOrganization = isOrganization;
        }


        async Task<IReadOnlyList<PackageInfo>> DownloadPackages()
        {
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "UnityWebRequest");

            var url = PrepareWebClient(webClient);

            var json = await webClient.DownloadStringTaskAsync(url);

            var fromJson = JsonHelper.FromJson<RepositoryDto>(json);

            if (fromJson.Length == 0) return new List<PackageInfo>();

            Debug.Log(fromJson.Length + " repositories found " + json);

            var list = fromJson.Select(ToPackageInfo).ToList();


            var validations = list.Select(IsThisAPackage).ToArray();

            var results = await Task.WhenAll(validations);

            list = list.Where((_, i) => results[i]).ToList();

            return list;
        }

        string GetUrlForUpm(RepositoryDto repoDto) => repoDto.@private
            ? $"https://x-access-token:{token}@github.com/{githubUser}/{repoDto.name}.git#{repoDto.default_branch}"
            : $"{repoDto.html_url}.git#{repoDto.default_branch}";

        PackageInfo ToPackageInfo(RepositoryDto repository) => new(repository.name, repository.html_url,
            GetUrlForUpm(repository), repository.@private, repository.default_branch);

        string PrepareWebClient(WebClient webClient)
        {
            var auth = !string.IsNullOrEmpty(token);
            if (auth)
            {
                webClient.Headers.Add("Authorization", "token " + token);
                if (isOrganization)
                {
                    return $"https://api.github.com/orgs/{githubUser}/repos";
                }
            }
            //
            // var orgOrUser = isOrganization ? "orgs" : "users";
            // return $"https://api.github.com/{orgOrUser}/{githubUser}/repos";

            //use git ssh
            return $"https://api.github.com/users/{githubUser}/repos";
        }

        async Task<bool> IsThisAPackage(PackageInfo package)
        {
            var hasPackageJson = await GithubTools.HasFile(package.url, package.branch, token, "package.json");


            Debug.Log($"Result {package.name} hasPackageJson: {hasPackageJson}");
            Debug.Log("-------------------------------------------------");

            return hasPackageJson;
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