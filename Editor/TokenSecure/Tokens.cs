using System;

namespace PackagesList.TokenSecure
{
    public static class Tokens
    {
        public static void GetToken(Action<string> onGetToken, bool forceDrawGUI = false)
        {
            if (string.IsNullOrEmpty(TokenManagement.TokenCache) || forceDrawGUI)
            {
                TokenManagementView.GetToken(onGetToken);
            }
            else
            {
                onGetToken.Invoke(TokenManagement.TokenCache);
            }
        }
    }
}