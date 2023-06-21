using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PackagesList.TokenSecure;
using UnityEditor;
using UnityEngine;

public class ListPackages : EditorWindow
{
    public const string githubUser = "alvmivan-packages";

    [MenuItem("Packages/List")]
    public static void ShowWindow()
    {
        var window = GetWindow<ListPackages>(false, "Packages List");
        window.Show();
    }

    string token;
    List<PackageInfo> packages = new List<PackageInfo>();

    void OnGUI()
    {
        if (GUILayout.Button("Fetch Packages"))
        {
            Fetch();
        }

        if (GUILayout.Button("Clear List"))
        {
            packages.Clear();
        }

        DisplayPackages();
    }

    void DisplayPackages()
    {
        if (packages.Count == 0)
        {
            EditorGUILayout.HelpBox("No packages found", MessageType.Info);
            return;
        }

        bool displayPrivates = false;

        if (packages.Any(p => p.isPrivate))
        {
            if (string.IsNullOrEmpty(token))
            {
                EditorGUILayout.LabelField("<<There are private packages, please enter your token>>");
                if (GUILayout.Button("Get Token"))
                {
                    Tokens.GetToken(t => token = t);
                    return;
                }
            }
            else
            {
                displayPrivates = true;
            }
        }


        const string EmojiDeCandadoAbierto = "🔓";
        const string EmojiDeCandadoCerrado = "🔒";


        foreach (var package in packages)
        {
            if (package.isPrivate && !displayPrivates) continue;

            var suffix = package.isPrivate ? "(private) " + EmojiDeCandadoCerrado : EmojiDeCandadoAbierto;

            EditorGUILayout.BeginHorizontal();


            EditorGUILayout.LabelField(package.name + suffix);

            if (GUILayout.Button("Copy Url"))
            {
                EditorGUIUtility.systemCopyBuffer = package.url;
            }

            if (GUILayout.Button("Copy UPM Url"))
            {
                EditorGUIUtility.systemCopyBuffer = package.urlForUPM;
            }


            EditorGUILayout.EndHorizontal();
        }
    }

    async void Fetch()
    {
        var webClient = new WebClient();
        webClient.Headers.Add("User-Agent", "UnityWebRequest");
        string url;
 

        if (string.IsNullOrEmpty(token))
        {
            url = $"https://api.github.com/orgs/{githubUser}/repos";  //get public repos from the org
            Debug.Log("Asking without token");
        }
        else
        {
            url = $"https://api.github.com/orgs/{githubUser}/repos";  //get all repos from the org
            webClient.Headers.Add("Authorization", "token " + token);
            Debug.Log("Adding auth : " + token);
        }


        try
        {
            var json = await webClient.DownloadStringTaskAsync(url);
            LogJson(json);
            var repositories = JsonHelper.FromJson<RepositoryDto>(json);
            packages.Clear();

            foreach (var repository in repositories)
            {
                var package = new PackageInfo
                {
                    name = repository.name,
                    url = repository.html_url,
                    urlForUPM = GetUrlForUpm(repository),
                };

                packages.Add(package);
            }
        }
        catch (Exception e)
        {
            var error = "Failed to fetch packages: " + e.Message;
            if (EditorUtility.DisplayDialog("Error", error, "OK"))
            {
                Debug.LogError(error);
                Debug.LogException(e);
            }
        }
    }


    static void LogJson(string json)
    {
        //copy json to clipboard to debug in another place
        EditorGUIUtility.systemCopyBuffer = $"START\n{json}\n END!";
    }

    string GetUrlForUpm(RepositoryDto repoDto) => repoDto.@private
        ? $"https://x-access-token:{token}@github.com/{githubUser}/{repoDto.name}.git#{repoDto.default_branch}"
        : $"{repoDto.html_url}.git#{repoDto.default_branch}";
}

[Serializable]
struct RepositoryDto
{
    public string name;
    public string html_url;
    public string default_branch;
    public bool @private;
}

[Serializable]
public struct PackageInfo
{
    public string name;
    public string url;
    public string urlForUPM;
    public bool isPrivate;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string fixedJson = "{\"repositories\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(fixedJson);
        return wrapper.repositories;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] repositories;
    }
}