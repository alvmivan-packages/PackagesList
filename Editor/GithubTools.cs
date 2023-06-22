using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Plastic.Newtonsoft.Json;

namespace PackagesList
{
    public static class GithubTools
    {
        static readonly HttpClient HttpClient = new()
        {
            DefaultRequestHeaders =
            {
                UserAgent = { ProductInfoHeaderValue.Parse("PackagesList/0.2") }
            }
        };

        const string JsonMediaType = "application/json";
        const string GithubGraphQLURL = "https://api.github.com/graphql";


        public static async Task<bool> HasFile(string packageURL, string packageBranch, string token,
            string fileYouAreLookingFor)
        {
            var uri = new Uri(packageURL);
            var pathSegments = uri.AbsolutePath.Split('/');
            var githubUser = pathSegments[1];
            var packageName = pathSegments[2];

            var query = CreateQuery(githubUser, packageName, packageBranch, fileYouAreLookingFor);
            var jsonQuery = JsonConvert.SerializeObject(new { query });

            var content = new StringContent(jsonQuery, Encoding.UTF8, JsonMediaType);

            PrepareToken(token);

            try
            {
                var response = await HttpClient.PostAsync(GithubGraphQLURL, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var graphQlResponse = JsonConvert.DeserializeObject<GraphQlResponse>(responseContent);

                // Verifica si Repository es null antes de intentar acceder a Object
                return graphQlResponse.Data?.Repository?.Object != null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        static void PrepareToken(string token)
        {
            HttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        static string CreateQuery(string githubUser, string packageName, string packageBranch,
            string fileYouAreLookingFor)
        {
            return @"query { 
                    repository(owner: """ + githubUser + @""", name: """ + packageName + @""") {
                        object(expression: """ + packageBranch + $@":{fileYouAreLookingFor}"") {{
                            ... on Blob {{
                                id
                            }}
                        }}
                    }}
                }}";
        }
    }


    public class GraphQlResponse
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public Repository Repository { get; set; }
    }

    public class Repository
    {
        public Blob Object { get; set; }
    }

    public class Blob
    {
        public string Id { get; set; }
    }
}