namespace SimpleDomain;

/// <summary>
/// The type name serialization strategy.
/// </summary>
/// <param name="getTypeName">A function which returns the type name of a given instance.</param>
public class TypeNameStrategy(Func<object, string> getTypeName)
{
    /// <summary>
    /// Creates a new instance of the <see cref="TypeNameStrategy" /> class using fully qualified type name.
    /// </summary>
    public static TypeNameStrategy Strict => new(instance => instance.GetType().AssemblyQualifiedName!);

    /// <summary>
    /// Creates a new instance of the <see cref="TypeNameStrategy" /> class using only the type name and assembly name.
    /// </summary>
    public static TypeNameStrategy Loose =>
        new(instance =>
        {
            var assemblyQualifiedName = instance.GetType().AssemblyQualifiedName!;
            var typeNameAndAssembly = assemblyQualifiedName.Split(',', StringSplitOptions.TrimEntries).Take(2);
            return string.Join(", ", typeNameAndAssembly);
        });

    /// <summary>
    /// Gets the type name of the given instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>The type name.</returns>
    public string GetTypeName(object instance) => getTypeName(instance);
}