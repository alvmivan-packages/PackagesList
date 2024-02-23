using UnityEditor;

namespace PackagesList.Utilities
{
    public class EditorPrefsBoolField : IField<bool>
    {
        readonly string key;
        readonly bool defaultValue;

        public EditorPrefsBoolField(string key, bool defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public bool Value
        {
            get => EditorPrefs.GetBool(key, defaultValue);
            set => EditorPrefs.SetBool(key, value);
        }
    }
}