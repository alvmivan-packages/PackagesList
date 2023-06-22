using UnityEditor;

namespace PackagesList
{
    public interface IField<T>
    {
        T Value { get; set; }
        void Set(T t) => Value = t;
        T Get() => Value;
    }


    public class InMemoryField<T> : IField<T>
    {
        T value;

        public InMemoryField(T value = default)
        {
            this.value = value;
        }

        public T Value
        {
            get => value;
            set => this.value = value;
        }
    }

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