using System;
using System.Collections.Generic;
using PackagesList.Database;
using PackagesList.GithubExplorer;
using PackagesList.Tokens;
using PackagesList.Utilities;
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

        const int ListWidth = 300;
        const int InspectorWidth = 600;
        const int ListRowHeight = 20;
        const int Height = 900;

        static readonly IField<string> HiddenToken = new InMemoryField<string>();
        public static readonly IField<string> Token = new InMemoryField<string>();
        static readonly IField<string> GithubUser = new EditorPrefsStringField("PackagesList.UserName");
        static readonly IField<bool> IsOrganization = new EditorPrefsBoolField("PackagesList.UserName.IsOrg");


        readonly PackagesView packagesView = new(ListRowHeight);

        [MenuItem(CompanyName + "/Packages/Clear Cache")]
        public static void ClearCache()
        {
            PackagesDatabase.Clear();
            HiddenToken.Value = string.Empty;
            Token.Value = string.Empty;
        }

        [MenuItem(CompanyName + "/Packages/List")]
        public static void ShowWindow()
        {
            var window = GetWindow<ListPackages>(false, "Packages List");
            PackagesView.Deselect();
            HiddenToken.Value = string.Empty;
            Token.Value = string.Empty;
            window.minSize = new Vector2(ListWidth, Height);
            window.Show();
        }

        void OnGUI()
        {
            if (PackagesView.GetSelected() is null)
            {
                minSize = new Vector2(ListWidth, Height);
            }
            else
            {
                minSize = new Vector2(ListWidth + InspectorWidth, Height);
            }

            if (string.IsNullOrEmpty(Token.Value))
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

            if (string.IsNullOrEmpty(HiddenToken.Value))
            {
                TokenInfo.DrawHelpBox();
                if (GUILayout.Button("Find Token", GUILayout.MaxWidth(200)))
                {
                    TokenHandler.OpenGithubPageToGenerateAToken();
                }

                HiddenToken.Value = await TokenHandler.GetToken();
                return;
            }

            if (GUILayout.Button("Get Token", GUILayout.MaxWidth(200)))
            {
                Token.Value = HiddenToken.Value;
            }
        }

        void DrawRegularWindow()
        {
            EditorGUILayout.BeginHorizontal();
            GithubUser.Value = EditorGUILayout.TextField("Github User", GithubUser.Value, GUILayout.MinWidth(600));
            IsOrganization.Value = EditorGUILayout.Toggle("Org", IsOrganization.Value, GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();

            EditorViewTools.DrawSeparatorHorizontal();

            if (PackagesDatabase.List.Count > 0)
            {
                if (GUILayout.Button("Clear List"))
                {
                    PackagesDatabase.Clear();
                }
            }
            else
            {
                if (GUILayout.Button("Fetch Packages"))
                {
                    FetchPackages();
                }
            }

            EditorViewTools.DrawSeparatorHorizontal();

            DisplayPackages();
        }


        async void FetchPackages()
        {
            PackagesDatabase.Clear();
            EditorUtility.DisplayProgressBar("Downloading Packages", "Fetching packages from Github", 0.25f);
            try
            {
                var newPackages = await GithubPackagesRepository.DownloadPackages(Token.Value, GithubUser.Value,
                    IsOrganization.Value);
                PackagesDatabase.Set(newPackages);
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
            if (PackagesDatabase.List.Count == 0)
            {
                "NoPackagesFound".DrawHelpBox();
                return;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(ListWidth));
            packagesView.RenderList();
            EditorGUILayout.EndVertical();

            EditorViewTools.DrawSeparatorVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(InspectorWidth));
            packagesView.RenderInspector();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
    }

    public static class StringEditorExtensions
    {
        public static void DrawHelpBox(this string text) => EditorGUILayout.HelpBox(text, MessageType.Info);
    }
}