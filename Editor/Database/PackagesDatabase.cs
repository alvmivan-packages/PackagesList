using System;
using System.Collections.Generic;
using PackagesList.Utilities;
using UnityEngine;

namespace PackagesList.Database
{
    public static class PackagesDatabase
    {
        static readonly IField<string> DatabaseJson = new EditorPrefsStringField("PackagesList.Database");
        static readonly List<PackageInfo> InstanceList = new();

        public static IReadOnlyList<PackageInfo> List
        {
            get
            {
                if (InstanceList.Count == 0)
                {
                    TryGetSavedInfo();
                }

                return InstanceList;
            }
        }

        static void TryGetSavedInfo()
        {
            if (string.IsNullOrEmpty(DatabaseJson.Value)) return;

            try
            {
                var database = JsonUtility.FromJson<DatabaseDto>(DatabaseJson.Value);
                InstanceList.Clear();
                InstanceList.AddRange(database.packages);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static void Save()
        {
            DatabaseJson.Value = JsonUtility.ToJson(new DatabaseDto { packages = InstanceList });
        }

        public static void Clear()
        {
            InstanceList.Clear();
            DatabaseJson.Value = string.Empty;
        }

        public static void Set(IReadOnlyList<PackageInfo> newPackages)
        {
            InstanceList.Clear();
            InstanceList.AddRange(newPackages);
            Save();
        }
    }

    [Serializable]
    public class DatabaseDto
    {
        public List<PackageInfo> packages;
    }
}