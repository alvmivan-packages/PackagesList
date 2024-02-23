using System;
using UnityEngine;

namespace PackagesList.GithubExplorer
{
    public static class RepositoriesJsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            var fixedJson = "{\"repositories\":" + json + "}";
            var wrapper = JsonUtility.FromJson<Wrapper<T>>(fixedJson);
            return wrapper.repositories;
        }

        [Serializable]
        class Wrapper<T>
        {
            public T[] repositories;
        }
    }
}