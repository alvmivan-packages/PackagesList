namespace PackagesList.Utilities
{
    public class InMemoryField<T> : IField<T>
    {
        public InMemoryField(T value = default) => Value = value;
        public T Value { get; set; }
    }
}