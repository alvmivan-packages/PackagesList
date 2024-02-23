using UnityEngine;

namespace PackagesList.View
{
    public static class EditorViewTools
    {
        public static void DrawSeparatorHorizontal()
        {
            GUILayout.Space(1);
            GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Space(1);
        }
        
        public static void DrawSeparatorVertical()
        {
            GUILayout.Space(1);
            GUILayout.Box(string.Empty, GUILayout.ExpandHeight(true), GUILayout.Width(1));
            GUILayout.Space(1);
        }
    }
}