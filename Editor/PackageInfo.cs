using System;
using PackagesList.UnityPackages;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace PackagesList
{
    [Serializable]
    public struct PackageInfo
    {
        const string GithubHttpsPrefix = "https://github.com/";
        public string name;
        public string url;
        public string urlForUPM;
        public bool isPrivate;
        public string branch;
        public string innerUrl;

        // repo data
        public string version;
        public string packageName;
        public string author;
        public string displayName;
        public string description;
        public string unityVersion;
        public string dependenciesJson;


        public PackageInfo(string name, string url, string urlForUpm, bool isPrivate, string branch)
        {
            this.name = name;
            this.url = url;
            urlForUPM = urlForUpm;
            this.isPrivate = isPrivate;
            this.branch = branch;
            innerUrl = url.Replace(GithubHttpsPrefix, "");
            version = null;
            packageName = null;
            author = null;
            displayName = null;
            description = null;
            unityVersion = null;
            dependenciesJson = null;
        }

        public Version Version => new(version);
        public bool IsInstalled => PackageInstaller.IsPackageInstalled(this);


        public PackageInfo SetPackageInfo(string jsonContent)
        {
            if (string.IsNullOrEmpty(jsonContent)) return default;
            var root = JObject.Parse(jsonContent);
            packageName = root["name"]?.Value<string>();
            //author or author.name
            author = root["author"]?.Type == JTokenType.Object
                ? root["author"]?["name"]?.Value<string>()
                : root["author"]?.Value<string>();
            version = root["version"]?.Value<string>();
            displayName = root["displayName"]?.Value<string>();
            description = root["description"]?.Value<string>();
            unityVersion = root["unity"]?.Value<string>();
            dependenciesJson = root["dependencies"]?.ToString();
            return this;
        }
    }
}