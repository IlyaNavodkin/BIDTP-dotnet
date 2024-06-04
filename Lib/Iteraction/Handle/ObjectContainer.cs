namespace Lib.Iteraction.Handle;

/// <summary>
///  The object container of the context
/// </summary>
public class ObjectContainer
{
    /// <summary>
    /// Additional data of the request for mutation 
    /// </summary>
    public Dictionary<Type, object> Objects { get; } = new Dictionary<Type, object>();

    /// <summary>
    /// Add an object to the container 
    /// </summary>
    /// <param name="obj"> The object. </param>
    /// <typeparam name="T"> The type of the object.</typeparam>
    public void AddObject<T>(T obj)
    {
        var key = typeof(T);

        if (Objects.ContainsKey(key))
        {
            throw new ArgumentException($"An object of type '{typeof(T)}' already exists in the container.");
        }

        Objects.Add(key, obj);
    }

    /// <summary>
    /// Get an object from the container 
    /// </summary>
    /// <typeparam name="T"> The type of the object.</typeparam>
    /// <returns> The object of type T. </returns>
    public T GetObject<T>()
    {
        if (Objects.TryGetValue(typeof(T), out object obj))
        {
            return (T)obj;
        }

        return default;
    }
}
