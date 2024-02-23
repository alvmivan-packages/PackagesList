using System.Collections.Generic;
using System.Linq;
using PackagesList.UnityPackages;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace PackagesList.View
{
    public class PackageDrawer
    {
        const string LockResource = "package-lock";

        static readonly GUILayoutOption[] InstalledLabelViewConf = { GUILayout.Width(65) };
        static readonly GUILayoutOption[] PackageNameLabelViewConf = { GUILayout.Width(150) };

        readonly GUILayoutOption[] rowLayoutOptions;
        readonly GUILayoutOption[] visibilityLabelViewConf;
        readonly int rowHeight;

        HashSet<string> packageNameToShowJson = new();

        public PackageDrawer(int height)
        {
            visibilityLabelViewConf = new[] { GUILayout.Width(height - 4), GUILayout.Height(height - 4) };
            rowLayoutOptions = new[] { GUILayout.Height(height) };
            rowHeight = height;
        }

        public void DrawRow(PackageInfo package)
        {
            EditorGUILayout.BeginHorizontal(rowLayoutOptions);

            DrawPackageRow(package);

            EditorGUILayout.EndHorizontal();
        }

        public void DrawContent(PackageInfo package)
        {
            using var verticalScope = new EditorGUILayout.VerticalScope();
            EditorGUILayout.LabelField(package.name, EditorStyles.boldLabel, GUILayout.Height(50));
            EditorViewTools.DrawSeparatorHorizontal();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(package.packageName, EditorStyles.miniLabel);
                EditorGUILayout.LabelField(package.version, EditorStyles.miniLabel);
                DrawInstallButton(package);
            }

            EditorViewTools.DrawSeparatorHorizontal();


            if (package.isPrivate)
            {
                EditorGUILayout.LabelField("Private");
            }

            if (package.author is { Length: > 0 })
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Author", GUILayout.Width(60));
                    EditorGUILayout.LabelField(package.author);
                }
            }

            if (package.description is { Length: > 0 })
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Description", GUILayout.Width(80));
                    EditorGUILayout.LabelField(package.description);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(package.innerUrl);
                if (GUILayout.Button("View On Github", "box", GUILayout.Width(110)))
                {
                    Application.OpenURL(package.url);
                }
            }

            DrawDependencies(package);

            TryDrawJson(package);
        }

        void TryDrawJson(PackageInfo package)
        {
            GUILayout.Space(5);
            var showJson = GUILayout.Toggle(packageNameToShowJson.Contains(package.name), "Show Full Json");
            if (showJson)
            {
                packageNameToShowJson.Add(package.name);
            }
            else
            {
                packageNameToShowJson.Remove(package.name);
            }

            if (!showJson) return;
            GUILayout.Space(5);
            EditorViewTools.DrawSeparatorHorizontal();
            EditorGUILayout.TextArea(JsonUtility.ToJson(package, true), GUILayout.ExpandHeight(true));
            EditorViewTools.DrawSeparatorHorizontal();
        }

        static void DrawInstallButton(PackageInfo package)
        {
            if (package.IsInstalled)
            {
                if (PackageInstaller.CurrentVersion(package) == package.version)
                {
                    GUILayout.Label("Installed");
                }
                else
                {
                    if (GUILayout.Button("Update to [" + package.version + "]"))
                    {
                        PackageInstaller.InstallPackage(package);
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Install"))
                {
                    PackageInstaller.InstallPackage(package);
                }
            }
        }

        void DrawDependencies(PackageInfo package)
        {
            if (string.IsNullOrEmpty(package.dependenciesJson)) return;
            var dependencies = JObject.Parse(package.dependenciesJson);
            if (dependencies.Count == 0) return;

            GUILayout.Space(5);
            EditorViewTools.DrawSeparatorHorizontal();
            GUILayout.Space(1);
            EditorViewTools.DrawSeparatorHorizontal();
            GUILayout.Space(5);

            EditorGUILayout.LabelField("Dependencies", EditorStyles.boldLabel);


            EditorViewTools.DrawSeparatorHorizontal();
            foreach (var dependency in dependencies)
            {
                using var horizontalScope = new EditorGUILayout.HorizontalScope();
                EditorGUILayout.LabelField(dependency.Key, GUILayout.Width(300));
                var dependencyValue = dependency.Value.ToString();
                if (dependencyValue.Contains("github.com"))
                {
                    if (GUILayout.Button(dependencyValue, EditorStyles.label, GUILayout.Width(300)))
                    {
                        Application.OpenURL(dependencyValue);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(dependencyValue, GUILayout.Width(300));
                }

                EditorViewTools.DrawSeparatorHorizontal();
            }

            GUILayout.Space(10);
        }

        static GUIContent LockedIcon()
        {
            return new GUIContent(Resources.Load<Texture2D>(LockResource));
        }


        void DrawPackageRow(PackageInfo package)
        {
            var visibilityIcon = package.isPrivate ? LockedIcon() : GUIContent.none;

            if (GUILayout.Button(package.name, EditorStyles.label, PackageNameLabelViewConf))
            {
                PackagesView.Select(package);
            }

            GUILayout.Space(4);

            EditorGUILayout.LabelField(visibilityIcon, visibilityLabelViewConf);
            var installed = package.IsInstalled;
            if (installed)
            {
                var rect = EditorGUILayout.GetControlRect(false, rowHeight, EditorStyles.miniButtonMid,
                    InstalledLabelViewConf);
                //first draw a green rect
                EditorGUI.DrawRect(rect, Color.green * .2f);
                //then draw the label
                EditorGUI.LabelField(rect, "Installed", EditorStyles.miniLabel);
            }
        }
    }
}