namespace ObjectToObjectMapper
{
    public abstract class ObjectCopyBase<S, T>
    {
        public abstract void MapTypes();
        public abstract void Copy(S source, T target);
    }
}