using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PackagesList.TokenSecure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RendererUtils;

namespace PackagesList
{
    public class ListPackages : EditorWindow
    {
        public string githubUser = "";
        bool isOrganization;

        string token;
        readonly List<PackageInfo> packages = new();

        [MenuItem("Packages/List")]
        public static void ShowWindow()
        {
            var window = GetWindow<ListPackages>(false, "Packages List");
            window.minSize = new Vector2(900, 700);
            window.Show();
        }


        void OnGUI()
        {
            HandleUserName();

            GUILayout.Space(4);


            if (packages.Count > 0)
            {
                if (GUILayout.Button("Clear List"))
                {
                    packages.Clear();
                }
            }
            else
            {
                if (GUILayout.Button("Fetch Packages"))
                {
                    packages.Clear();
                    Fetch();
                }
            }

            GUILayout.Space(2);
            DisplayPackages();

            GUILayout.Space(4);
        }

        void HandleUserName()
        {
            const string localUserNameStored = "PackagesList.UserName";
            const string localIsOrgStored = "PackagesList.UserName.IsOrg";

            EditorGUILayout.BeginHorizontal();

            githubUser = EditorGUILayout.TextField("Github User", githubUser, GUILayout.MinWidth(600));
            isOrganization = EditorGUILayout.Toggle("Org", isOrganization, GUILayout.MaxWidth(200));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save User"))
            {
                EditorPrefs.SetString(localUserNameStored, githubUser);
                EditorPrefs.SetBool(localIsOrgStored, isOrganization);
            }

            if (GUILayout.Button("Load User"))
            {
                githubUser = EditorPrefs.GetString(localUserNameStored, githubUser);
                isOrganization = EditorPrefs.GetBool(localIsOrgStored, isOrganization);
            }

            EditorGUILayout.EndHorizontal();
        }

        void DisplayPackages()
        {
            if (packages.Count == 0)
            {
                EditorGUILayout.HelpBox("No packages found", MessageType.Info);
                return;
            }

            var displayPrivates = false;

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


            Render(displayPrivates);
        }

        void Render(bool displayPrivates)
        {
            using var scrollView = new EditorGUILayout.ScrollViewScope(scroll);
            foreach (var package in packages)
            {
                if (package.isPrivate && !displayPrivates) continue;

                var suffix = package.isPrivate ? "(private) " + "" : "";

                EditorGUILayout.BeginHorizontal();


                EditorGUILayout.LabelField(package.name + suffix);

                if (GUILayout.Button("View On Github"))
                {
                    Application.OpenURL(package.url);
                }

                if (GUILayout.Button("Copy UPM Url"))
                {
                    EditorGUIUtility.systemCopyBuffer = package.urlForUPM;
                    //Open the package manager window
                    EditorApplication.ExecuteMenuItem("Window/Package Manager");
                }


                EditorGUILayout.EndHorizontal();
            }

            scroll = scrollView.scrollPosition;
        }

        Vector2 scroll;

        async void Fetch()
        {
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "UnityWebRequest");

            var url = PrepareWebClient(webClient);

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
                        isPrivate = repository.@private
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

        string PrepareWebClient(WebClient webClient)
        {
            var auth = !string.IsNullOrEmpty(token);
            if (auth)
            {
                webClient.Headers.Add("Authorization", "token " + token);
                if (isOrganization)
                {
                    return $"https://api.github.com/orgs/{githubUser}/repos";
                }
            }

            var orgOrUser = isOrganization ? "orgs" : "users";
            return $"https://api.github.com/{orgOrUser}/{githubUser}/repos";
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
}