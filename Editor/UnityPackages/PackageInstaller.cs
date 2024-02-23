using System.IO;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace PackagesList.UnityPackages
{
    public static class PackageInstaller
    {
        static FileInfo ManifestLocation()
        {
            var dataPath = Application.dataPath;
            var manifestPath = dataPath + "/../Packages/manifest.json";

            var fileInfo = new FileInfo(manifestPath);
            if (!fileInfo.Exists)
            {
                fileInfo = new FileInfo(dataPath + "/../manifest.json");
            }

            if (!fileInfo.Exists)
            {
                Debug.LogError("Manifest file does not exist. " + fileInfo.FullName);
            }

            return fileInfo;
        }
        public static bool IsPackageInstalled(PackageInfo package) => !string.IsNullOrEmpty(CurrentVersion(package));

        public static void InstallPackage(PackageInfo package)
        {
            var name = package.packageName;
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("Cant install package. Package Name is empty. " + package);
                return;
            }

            var version = package.urlForUPM;

            var manifestFile = ManifestLocation();


            if (!manifestFile.Exists)
            {
                // dependencies[name] = version;
                // manifestJson["dependencies"] = dependencies;
                Debug.Log("Manifest file does not exist. Creating a new one. " + manifestFile.FullName);
                return;
            }

            var manifestContent = File.ReadAllText(manifestFile.FullName);
            var manifestJson = JObject.Parse(manifestContent);
            var dependencies = manifestJson["dependencies"];

            if (dependencies is null)
            {
                dependencies = new JObject();
                manifestJson["dependencies"] = dependencies;
            }

            dependencies[name] = version;

            File.WriteAllText(manifestFile.FullName, manifestJson.ToString());

            AssetDatabase.Refresh();

            Debug.Log("Package " + name + " installed successfully.");
        }

        public static string CurrentVersion(PackageInfo info)
        {
            var manifestFile = ManifestLocation();

            if (!manifestFile.Exists)
            {
                Debug.Log("Manifest file does not exist. " + manifestFile.FullName);
                return string.Empty;
            }

            var manifestContent = File.ReadAllText(manifestFile.FullName);

            var manifestJson = JObject.Parse(manifestContent);

            var dependencies = manifestJson["dependencies"];

            if (dependencies is null)
            {
                Debug.Log("No dependencies found in manifest file. " + manifestFile.FullName);
                return string.Empty;
            }

            var version = dependencies[info.packageName];

            return version?.ToString();
        }
    }
}