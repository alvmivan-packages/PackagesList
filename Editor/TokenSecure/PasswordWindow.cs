using System;
using UnityEditor;
using UnityEngine;

namespace PackagesList.TokenSecure
{
    public class PasswordWindow : EditorWindow
    {
        string password = "";
        Action<string> callback;

        void OnGUI()
        {
            password = EditorGUILayout.PasswordField("Enter Password", password);

            if (!GUILayout.Button("OK")) return;

            callback.Invoke(password);
            Close();
        }

        public static void GetPassword(Action<string> callback)
        {
            var window = GetWindow<PasswordWindow>();
            window.callback = callback;
            window.ShowModalUtility();
        }
    }
}