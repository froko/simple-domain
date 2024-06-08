namespace SimpleDomain;

public static class VersionExtensions
{
    public static T WithVersion<T>(this T message, int version) where T : INeedVersion
    {
        var versionProperty = message.GetType().GetProperty("Version");
        versionProperty?.SetValue(message, version);
        return message;
    }
}