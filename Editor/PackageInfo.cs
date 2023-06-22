using System;

namespace PackagesList
{
    [Serializable]
    public struct PackageInfo
    {
        public string name;
        public string url;
        public string urlForUPM;
        public bool isPrivate;
        public string branch;

        public PackageInfo(string name, string url, string urlForUpm, bool isPrivate, string branch)
        {
            this.name = name;
            this.url = url;
            urlForUPM = urlForUpm;
            this.isPrivate = isPrivate;
            this.branch = branch;
        }
    }
}