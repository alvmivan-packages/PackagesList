using System;
using System.Collections.Generic;
using System.Net;
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

    List<PackageInfo> packages = new List<PackageInfo>();

    void OnGUI()
    {
        Fetch();
        DisplayPackages();
    }

    void DisplayPackages()
    {
        if (packages.Count == 0)
        {
            EditorGUILayout.HelpBox("No packages found", MessageType.Info);
            return;
        }

        foreach (var package in packages)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(package.name);

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
        if (GUILayout.Button("Fetch Packages"))
        {
            // Ask GitHub for the repositories of the user
            var url = $"https://api.github.com/users/{githubUser}/repos";
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "UnityWebRequest");

            try
            {
                var json = await webClient.DownloadStringTaskAsync(url);

                var repositories = JsonHelper.FromJson<RepositoryDto>(json);
                packages.Clear();

                foreach (var repository in repositories)
                {
                    var package = new PackageInfo
                    {
                        name = repository.name,
                        url = repository.html_url,
                        urlForUPM = $"{repository.html_url}.git#{repository.default_branch}"
                    };

                    packages.Add(package);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to fetch packages: " + e.Message);
            }
        }
    }
}

[Serializable]
struct RepositoryDto
{
    public string name;
    public string html_url;
    public string default_branch;
}

[Serializable]
public struct PackageInfo
{
    public string name;
    public string url;
    public string urlForUPM;
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