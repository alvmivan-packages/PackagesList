using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PackagesList.TokenSecure
{
    public static class TokenManagement
    {
        internal static string TokenCache;
        const int MinCharsForPassword = 8;
        const int MaxCharsForPassword = 32;
        const string SavingKey = "GH_Token_PackageList";
        const string SavingKeyPasswordValidator = "GH_Token_PackageList_PasswordValidator";

        static readonly HashSet<char> PasswordCharHashSet =
            new("qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890._");

        const string PasswordValidationText = "123456abcde";

        const string GoToGithubTokens = "https://github.com/settings/tokens/new";

        public static void OpenGitHubForToken() => Application.OpenURL(GoToGithubTokens);

        static (string token, string passwordValidator) LoadStoredToken()
        {
            var encryptedToken = EditorPrefs.GetString(SavingKey, string.Empty);
            var encryptedPasswordValidator = EditorPrefs.GetString(SavingKeyPasswordValidator, string.Empty);
            return (encryptedToken, encryptedPasswordValidator);
        }

        internal static bool TryGetToken(string password, out string token)
        {
            var (encryptedToken, encryptedPasswordValidator) = LoadStoredToken();
            var decryptedPasswordValidator = SecureTokenStorage.DecryptToken(encryptedPasswordValidator, password);
            if (decryptedPasswordValidator != PasswordValidationText)
            {
                token = null;
                return false;
            }

            token = SecureTokenStorage.DecryptToken(encryptedToken, password);
            return !string.IsNullOrEmpty(token);
        }


        static bool IsValidPass(string pass)
        {
            return !string.IsNullOrEmpty(pass) &&
                   !string.IsNullOrWhiteSpace(pass) &&
                   pass.Length is >= MinCharsForPassword and <= MaxCharsForPassword &&
                   pass.All(PasswordCharHashSet.Contains);
        }


        internal static bool SaveToken(string token, string password)
        {
            if (!IsValidPass(password)) return false;
            var encryptedToken = SecureTokenStorage.EncryptToken(token, password);
            var encryptedPasswordValidator = SecureTokenStorage.EncryptToken(PasswordValidationText, password);

            EditorPrefs.SetString(SavingKey, encryptedToken);
            EditorPrefs.SetString(SavingKeyPasswordValidator, encryptedPasswordValidator);
            return true;
        }
    }
}