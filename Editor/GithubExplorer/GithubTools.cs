using System;
using System.Threading.Tasks;

namespace PackagesList.GithubExplorer
{
    public static class GithubTools
    {
        public static async Task<string> GetFileContent(string packageURL, string packageBranch, string token,
            string fileYouAreLookingFor)
        {
            var validation = new GithubConnection(
                packageURL,
                packageBranch,
                token
            );

            try
            {
                return await validation.GetFileContent(fileYouAreLookingFor);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}