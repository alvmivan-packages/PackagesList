﻿using System;
using System.Threading.Tasks;

namespace PackagesList.TokenSecure
{
    public static class Tokens
    {
        static void GetToken(Action<string> onGetToken, bool forceDrawGUI = false)
        {
            if (TokenProjectStorage.TryGetFromLocalStorageToken(out var myToken))
            {
                onGetToken.Invoke(myToken);
                return;
            }

            if (string.IsNullOrEmpty(TokenManagement.TokenCache) || forceDrawGUI)
            {
                TokenManagementView.GetToken(onGetToken);
            }
            else
            {
                onGetToken.Invoke(TokenManagement.TokenCache);
            }
        }

        //async version
        public static async Task<string> GetTokenAsync(bool forceDrawGUI = false)
        {
            var tcs = new TaskCompletionSource<string>();

            GetToken(t => tcs.SetResult(t), forceDrawGUI);

            return await tcs.Task;
        }

        public static void OpenTokenPage() => TokenManagement.OpenGitHubForToken();
    }
}