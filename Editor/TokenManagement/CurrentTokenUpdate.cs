using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace PackagesList.TokenManagement
{
    public class CurrentTokenUpdate
    {
        const string OrbitarPackagesPrefix = "com.orbitar";

        static bool IsAnOrbitarPackage(string packageJsonEntryKey) =>
            packageJsonEntryKey.StartsWith(OrbitarPackagesPrefix);

        static bool EntryValueIsAnTokenizedGithubUrl(string entry)
        {
            const string githubTokenPrefix = "ghp_";
            const string prefix = "https://x-access-token:";
            const string suffix = "@github.com";
            var prefixIndex = entry.IndexOf(prefix, StringComparison.Ordinal);
            var suffixIndex = entry.IndexOf(suffix, StringComparison.Ordinal);
            if (prefixIndex < 0 || suffixIndex < 0) return false; // not a tokenized github url
            var oldToken = entry.Substring(prefixIndex + prefix.Length, suffixIndex - prefixIndex - prefix.Length);
            return oldToken.StartsWith(githubTokenPrefix); // not a github token
        }

        static string ReplaceOldTokenForNewOne(string oldEntry, string newToken)
        {
            // packages entries will have any of this forms:
            // 1) "com.orbitar.testprivatepackage": "https://x-access-token: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@github.com/alvmivan-packages/TestPrivatePackage.git#master",
            // 2) "com.unity.collab-proxy": "2.0.1",
            // 3) "com.somecompany.somepackage" : "https:github.com/somecompany/somepackage.git#master"


            //we will consider just case 1
            //then we will find "https://x-access-token:" string and "@github.com" string, we know everything inside is the old token
            //so replace with the new one. (Also add a validation, if the old token doen't start with ghp_ then don't replace it, cause it's not a github token)


            const string githubTokenPrefix = "ghp_";
            const string prefix = "https://x-access-token:";
            const string suffix = "@github.com";

            if (!newToken.StartsWith(githubTokenPrefix)) return oldEntry; // not a github token

            var prefixIndex = oldEntry.IndexOf(prefix, StringComparison.Ordinal);
            var suffixIndex = oldEntry.IndexOf(suffix, StringComparison.Ordinal);

            if (prefixIndex < 0 || suffixIndex < 0) return oldEntry; // not the case 1

            var oldToken =
                oldEntry.Substring(prefixIndex + prefix.Length, suffixIndex - prefixIndex - prefix.Length);

            if (!oldToken.StartsWith(githubTokenPrefix)) return oldEntry; // not a github token

            return oldEntry.Replace(oldToken, newToken);
        }


        public static void UpdateCurrentToken(string newToken)
        {
            // retrieve current unity project manifest
            var manifestJsonFolder =
                Application.dataPath.Replace("Assets", "Packages/manifest.json").Replace("//", "/");

            // parse json

            var fileContent = File.ReadAllText(manifestJsonFolder);

            var manifestJson = JObject.Parse(fileContent);

            // find dependencies

            if (manifestJson["dependencies"] is not JObject entries) return;


            //find the correct entries

            var entriesToReplace = entries.Properties()
                .Where(entry => IsAnOrbitarPackage(entry.Name))
                .Where(e => EntryValueIsAnTokenizedGithubUrl(e.Value.ToString()))
                .Select(entry => entry.Name)
                .ToList();

            //replace the old token for the new one

            foreach (var entryToReplace in entriesToReplace)
            {
                var jToken = entries[entryToReplace];
                if (jToken == null) continue;
                var oldEntry = jToken?.ToString();
                var newEntry = ReplaceOldTokenForNewOne(oldEntry, newToken);
                entries[entryToReplace] = newEntry;
            }

            //set dependencies 

            manifestJson["dependencies"] = entries;

            // get string json PrettyPrinted

            var newContent = manifestJson.ToString();

            // write back to manifest.json

            File.WriteAllText(manifestJsonFolder, newContent);


            //print updated manifest

            Debug.Log(newContent);

            //call refresh to update the packages list

            UnityEditor.PackageManager.Client.Resolve();
            
            
        }
    }
}