using System;

namespace PackagesList.GithubExplorer
{
    [Serializable]
    public struct RepositoryDto
    {
        public string name;
        public string html_url;
        public string default_branch;
        public bool @private;
    }
}