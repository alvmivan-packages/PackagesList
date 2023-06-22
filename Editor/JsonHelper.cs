using System;
using UnityEngine;

namespace PackagesList
{
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string fixedJson = "{\"repositories\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(fixedJson);
            return wrapper.repositories;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] repositories;
        }
    }
}