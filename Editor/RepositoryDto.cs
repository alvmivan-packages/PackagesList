using System;

namespace PackagesList
{
    [Serializable]
    struct RepositoryDto
    {
        public string name;
        public string html_url;
        public string default_branch;
        public bool @private;
        
        
    }
}