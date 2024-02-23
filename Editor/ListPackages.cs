using System;
using System.Collections.Generic;
using PackagesList.Tokens;
using PackagesList.View;
using UnityEditor;
using UnityEngine;

namespace PackagesList
{
    public class ListPackages : EditorWindow
    {
        const string CompanyName = "Orbitar";

        const string TokenInfo =
            "You can generate a token on github, then you need to setup the token on your command line\n" +
            "then you must setup it on git, you can do it by running the following command:\n" +
            "git config --global github.token YOUR_TOKEN\n";

        Vector2 scroll;

        readonly IField<string> hiddenToken = new InMemoryField<string>();
        readonly IField<string> token = new InMemoryField<string>();
        readonly IField<string> githubUser = new EditorPrefsStringField("PackagesList.UserName");
        readonly IField<bool> isOrganization = new EditorPrefsBoolField("PackagesList.UserName.IsOrg");
        readonly List<PackageInfo> packages = new();

        [MenuItem(CompanyName + "/Packages/List")]
        public static void ShowWindow()
        {
            var window = GetWindow<ListPackages>(false, "Packages List");
            window.minSize = new Vector2(900, 700);
            window.Show();
        }

        void OnGUI()
        {
            if (string.IsNullOrEmpty(token.Value))
            {
                DrawTokenField();
            }
            else
            {
                DrawRegularWindow();
            }
        }

        async void DrawTokenField()
        {
            EditorGUILayout.LabelField("Token is required to fetch packages from Github");

            //you can find your token here : [button find token] 


            if (string.IsNullOrEmpty(hiddenToken.Value))
            {
                TokenInfo.DrawHelpBox();
                if (GUILayout.Button("Find Token", GUILayout.MaxWidth(200)))
                {
                    TokenHandler.OpenGithubPageToGenerateAToken();
                }

                hiddenToken.Value = await TokenHandler.GetToken();
                return;
            }

            if (GUILayout.Button("Get Token", GUILayout.MaxWidth(200)))
            {
                token.Value = hiddenToken.Value;
            }
        }

        void DrawRegularWindow()
        {
            EditorGUILayout.BeginHorizontal();
            githubUser.Value = EditorGUILayout.TextField("Github User", githubUser.Value, GUILayout.MinWidth(600));
            isOrganization.Value = EditorGUILayout.Toggle("Org", isOrganization.Value, GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();

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

            RenderList();
        }

        static void DrawSeparator()
        {
            GUILayout.Space(1);
            GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Space(1);
        }

        void RenderList()
        {
            using var scrollView = new EditorGUILayout.ScrollViewScope(scroll);

            DrawSeparator();
            foreach (var package in packages)
            {
                PackageDrawer.DrawRow(package);
                DrawSeparator();
            }

            scroll = scrollView.scrollPosition;
        }
    }

    public static class StringEditorExtensions
    {
        public static void DrawHelpBox(this string text) => EditorGUILayout.HelpBox(text, MessageType.Info);
    }
}