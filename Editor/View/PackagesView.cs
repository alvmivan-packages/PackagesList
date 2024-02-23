using PackagesList.Database;
using UnityEditor;
using UnityEngine;

namespace PackagesList.View
{
    public class PackagesView
    {
        static PackageInfo? selectedPackage;

        public static void Select(PackageInfo package) => selectedPackage = package;
        public static void Deselect() => selectedPackage = null;
        public static PackageInfo? GetSelected() => selectedPackage;


        Vector2 scroll;
        readonly PackageDrawer packageDrawer;
        readonly int rowHeight;

        public PackagesView(int rowHeight)
        {
            packageDrawer = new PackageDrawer(rowHeight);
        }

        public void RenderList()
        {
            using var scrollView = new EditorGUILayout.ScrollViewScope(scroll);
            EditorViewTools.DrawSeparatorHorizontal();
            foreach (var package in PackagesDatabase.List)
            {
                //ask for a rect on the current position, on the available width and the row height
                var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(1));
                rect.height = rowHeight + 1;

                packageDrawer.DrawRow(package);
                EditorViewTools.DrawSeparatorHorizontal();
            }

            scroll = scrollView.scrollPosition;
        }

        public void RenderInspector()
        {
            if (selectedPackage is not { } package) return;
            
            packageDrawer.DrawContent(package);
            
        }
    }
}