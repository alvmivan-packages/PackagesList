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
            try
            {
                var json = await webClient.DownloadStringTaskAsync(url);


                return JsonHelper.FromJson<RepositoryDto>(json).Select(ToPackageInfo).ToList();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        string GetUrlForUpm(RepositoryDto repoDto) => repoDto.@private
            ? $"https://x-access-token:{token}@github.com/{githubUser}/{repoDto.name}.git#{repoDto.default_branch}"
            : $"{repoDto.html_url}.git#{repoDto.default_branch}";

        PackageInfo ToPackageInfo(RepositoryDto repository) => new PackageInfo(repository.name, repository.html_url,
            GetUrlForUpm(repository), repository.@private);

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

            var orgOrUser = isOrganization ? "orgs" : "users";
            return $"https://api.github.com/{orgOrUser}/{githubUser}/repos";
        }
    }
}