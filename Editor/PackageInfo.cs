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

        public PackageInfo(string name, string url, string urlForUpm, bool isPrivate)
        {
            this.name = name;
            this.url = url;
            urlForUPM = urlForUpm;
            this.isPrivate = isPrivate;
        }
    }
}