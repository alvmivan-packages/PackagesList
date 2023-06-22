using System;
using UnityEditor;
using UnityEngine;

namespace PackagesList.TokenSecure
{
    public class TokenManagementView : EditorWindow
    {
        static string Token
        {
            get => TokenManagement.TokenCache;
            set => TokenManagement.TokenCache = value;
        }

        string password;
        Action<string> onGetToken;

        internal static void GetToken(Action<string> onGetToken)
        {
            var window = GetWindow<TokenManagementView>();
            window.onGetToken = onGetToken;
            window.ShowModalUtility();
        }

        void OnGUI()
        {
            Token = EditorGUILayout.TextField("Token", Token);

            //If there is some text on the clipboard display a button to paste as the token
            if (GUIUtility.systemCopyBuffer.Length > 0)
            {
                if (GUILayout.Button("Paste Token"))
                {
                    Token = GUIUtility.systemCopyBuffer;
                }
            }

            if (string.IsNullOrEmpty(Token))
            {
                if (GUILayout.Button("Go To Github"))
                {
                    TokenManagement.OpenGitHubForToken();
                }

                if (GUILayout.Button("Load Token"))
                {
                    PasswordWindow.GetPassword(OnPasswordEnteredForLoad);
                }
            }
            else
            {
                if (GUILayout.Button("Save Token"))
                {
                    PasswordWindow.GetPassword(OnPasswordEnteredForSave);
                }
            }

            if (GUILayout.Button("Clear Token"))
            {
                Token = string.Empty;
            }

            if (GUILayout.Button("Clear Password"))
            {
                password = string.Empty;
            }

            if (GUILayout.Button("Get Token"))
            {
                Close();
            }
        }

        void OnDisable() => onGetToken?.Invoke(Token);

        static void OnPasswordEnteredForSave(string pass)
        {
            if (TokenManagement.SaveToken(Token, pass))
            {
                EditorUtility.DisplayDialog("Success", "Token Saved", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Invalid Password", "OK");
            }
        }

        void OnPasswordEnteredForLoad(string pass)
        {
            password = pass;
            if (TokenManagement.TryGetToken(password, out var token))
            {
                Token = token;
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Wrong Password", "OK");
                password = string.Empty;
            }
        }

        
    }
}