using System;
using System.Collections.Generic;
using PackagesList.TokenManagement;
using PackagesList.TokenSecure;
using UnityEditor;
using UnityEngine;

namespace PackagesList
{
    public class ListPackages : EditorWindow
    {
        const string TokenSecurityAdvice = @"Your GitHub token is vital for the security of your data. 
When you provide your token to our application, we internally encrypt it with a password before storing it. 
This extra step of security means that even if someone manages to access the application's database, they won't be able to use your token as it will be encrypted.
However, it's important to remember that you should still follow good security practices with your token. 
Only enable 'repo' permissions, rotate it regularly, never share it, and revoke it if you suspect it's compromised. 
Keeping these practices, along with the security measures we implement, will ensure the protection of your GitHub data and resources.";


        const string PrivateLabelContent = "Private";
        const string PublicLabelContent = "       ";


        Vector2 scroll;

        readonly IField<string> token = new InMemoryField<string>();
        readonly IField<string> githubUser = new EditorPrefsStringField("PackagesList.UserName");
        readonly IField<bool> isOrganization = new EditorPrefsBoolField("PackagesList.UserName.IsOrg");
        readonly List<PackageInfo> packages = new();

        [MenuItem("Orbitar/Packages/List")]
        public static void ShowWindow()
        {
            var window = GetWindow<ListPackages>(false, "Packages List");
            window.minSize = new Vector2(900, 700);
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            githubUser.Value = EditorGUILayout.TextField("Github User", githubUser.Value, GUILayout.MinWidth(600));
            isOrganization.Value = EditorGUILayout.Toggle("Org", isOrganization.Value, GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();

            DrawSeparator();

            EditorGUILayout.BeginHorizontal();
            token.Value = EditorGUILayout.TextField("Token", token.Value, GUILayout.MinWidth(600));
            GetTokenButton();
            EditorGUILayout.EndHorizontal();

            DrawSeparator();

            TokenSecurityAdvice.DrawHelpBox();

            GUILayout.Space(4);
            RefreshTokenButton();
            GUILayout.Space(4);
            DrawSeparator();

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
                    FetchPackages();
                }
            }

            DrawSeparator();

            DisplayPackages();
        }

        async void GetTokenButton()
        {
            if (GUILayout.Button("Get Token"))
            {
                token.Value = await Tokens.GetTokenAsync();
            }

            if (GUILayout.Button("New Token"))
            {
                Tokens.OpenTokenPage();
            }
        }

        void RefreshTokenButton()
        {
            if (!string.IsNullOrEmpty(token.Value))
            {
                if (GUILayout.Button("Refresh token into packages"))
                {
                    CurrentTokenUpdate.UpdateCurrentToken(token.Value);
                }
            }
        }

        async void FetchPackages()
        {
            packages.Clear();
            EditorUtility.DisplayProgressBar("Downloading Packages", "Fetching packages from Github", 0.5f);

            try
            {
                var newPackages =
                    await GithubPackagesRepository.DownloadPackages(token.Value, githubUser.Value,
                        isOrganization.Value);
                packages.AddRange(newPackages);
            }
            catch (Exception e)
            {
                //show error message
                EditorUtility.DisplayDialog("Error", e.Message, "Ok");
                Debug.LogException(e);
            }

            EditorUtility.ClearProgressBar();
        }

        void DisplayPackages()
        {
            if (packages.Count == 0)
            {
                "NoPackagesFound".DrawHelpBox();
                return;
            }

            Render();
        }

        static void DrawSeparator()
        {
            GUILayout.Space(1);
            GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Space(1);
        }

        void Render()
        {
            using var scrollView = new EditorGUILayout.ScrollViewScope(scroll);

            const int itemsPaddingSides = 2;
            DrawSeparator();
            foreach (var package in packages)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(itemsPaddingSides);
                DrawPackageRow(package);
                GUILayout.Space(itemsPaddingSides);
                EditorGUILayout.EndHorizontal();
                DrawSeparator();
            }

            scroll = scrollView.scrollPosition;
        }

        static void DrawPackageRow(PackageInfo package)
        {
            var visibilityLabel = package.isPrivate ? PrivateLabelContent : PublicLabelContent;

            EditorGUILayout.LabelField(package.name);
            EditorGUILayout.LabelField(visibilityLabel, EditorStyles.miniLabel);

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
        }
    }

    public static class StringEditorExtensions
    {
        public static void DrawHelpBox(this string text) => EditorGUILayout.HelpBox(text, MessageType.Info);
    }
}