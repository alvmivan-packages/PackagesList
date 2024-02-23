using UnityEditor;

namespace PackagesList.Utilities
{
    public class EditorPrefsStringField : IField<string>
    {
        readonly string key;
        readonly string defaultValue;

        public EditorPrefsStringField(string key, string defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public string Value
        {
            get => EditorPrefs.GetString(key, defaultValue);
            set => EditorPrefs.SetString(key, value);
        }
    }
}