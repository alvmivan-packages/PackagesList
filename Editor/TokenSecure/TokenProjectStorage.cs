using PackagesList.CustomStorage;
using UnityEngine;

namespace PackagesList.TokenSecure
{
    public static class TokenProjectStorage
    {
        const string StorageKey = "PackagesList_TokenProjectStorage";

        public static bool TryGetFromLocalStorageToken(out string token) =>
            !string.IsNullOrEmpty(CustomLocalStorage.Get(StorageKey).SetTo(out token));


        public static void SetToken(string token)
        {
            CustomLocalStorage.Set(StorageKey, token);
            Debug.Log($"Token stored correctly at : {CustomLocalStorage.StorageLocation}");
        }

        static T SetTo<T>(this T v, out T a) => a = v;
    }
}