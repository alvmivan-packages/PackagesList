using PackagesList.UnityPackages;
using UnityEditor;
using UnityEngine;

namespace PackagesList.View
{
    public static class PackageDrawer
    {
        const string LockResource = "package-lock";
        const int RowHeight = 22;
        const int ItemsPaddingSides = 2;

        static readonly GUILayoutOption[] InstalledLabelViewConf = { GUILayout.Width(100) };
        static readonly GUILayoutOption[] VisibilityLabelViewConf = { GUILayout.Width(14), GUILayout.Height(14) };
        static readonly GUILayoutOption[] PackageNameLabelViewConf = { GUILayout.Width(250) };
        static readonly GUILayoutOption[] RowLayoutOptions = { GUILayout.Height(RowHeight) };

        public static void DrawRow(PackageInfo package)
        {
            EditorGUILayout.BeginHorizontal(RowLayoutOptions);
            PaddingSides();
            DrawPackageRow(package);
            PaddingSides();
            EditorGUILayout.EndHorizontal();
        }

        static void PaddingSides() => GUILayout.Space(ItemsPaddingSides);

        static GUIContent LockedIcon()
        {
            return new GUIContent(Resources.Load<Texture2D>(LockResource));
        }


        static void DrawPackageRow(PackageInfo package)
        {
            var visibilityLabel = package.isPrivate ? "priv" : "   ";
            var visibilityIcon = package.isPrivate ? LockedIcon() : GUIContent.none;

            EditorGUILayout.LabelField(package.name);
            GUILayout.Space(4);
            EditorGUILayout.LabelField(package.packageName, EditorStyles.miniLabel, PackageNameLabelViewConf);
            // EditorGUILayout.LabelField(visibilityLabel, EditorStyles.miniLabel, VisibilityLabelViewConf);
            EditorGUILayout.LabelField(visibilityIcon, VisibilityLabelViewConf);
            EditorGUILayout.LabelField(package.version, EditorStyles.miniLabel, InstalledLabelViewConf);

            var currentVersion = PackageInstaller.CurrentVersion(package);


            if (package.IsInstalled)
            {
                GUILayout.Label("Installed", InstalledLabelViewConf);
            }
            else
            {
                GUILayout.Label(string.Empty, InstalledLabelViewConf);
            }
        }
    }
}