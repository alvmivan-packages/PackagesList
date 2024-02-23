using UnityEditor;

namespace PackagesList.Utilities
{
    public class EditorPrefsIntField : IField<int>
    {
        readonly string key;
        readonly int defaultValue;

        public EditorPrefsIntField(string key, int defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public int Value
        {
            get => EditorPrefs.GetInt(key, defaultValue);
            set => EditorPrefs.SetInt(key, value);
        }
    }
}