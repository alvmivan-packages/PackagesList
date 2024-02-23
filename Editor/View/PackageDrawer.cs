using PackagesList.UnityPackages;
using UnityEditor;
using UnityEngine;

namespace PackagesList.View
{
    public static class PackageDrawer
    {
        const string PrivateLabelContent = "Private";
        const string PublicLabelContent = "       ";

        public static void DrawRow(PackageInfo package)
        {
            const int itemsPaddingSides = 2;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(itemsPaddingSides);
            DrawPackageRow(package);
            GUILayout.Space(itemsPaddingSides);
            EditorGUILayout.EndHorizontal();
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

            if (GUILayout.Button("Info"))
            {
                Debug.Log(JsonUtility.ToJson(package, true));
            }

            if (package.IsInstalled)
            {
                if (PackageInstaller.CurrentVersion(package) == package.urlForUPM)
                {
                    GUILayout.Label("Installed");
                }
                else
                {
                    if (GUILayout.Button("Update to " + package.urlForUPM))
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
    }
}