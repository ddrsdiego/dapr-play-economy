namespace Play.Common.Application;

public readonly struct WriteRequest
{
    public WriteRequest(string key, byte[] content)
        : this(key, string.Empty, content)
    {
    }

    public WriteRequest(string key, string subKey, byte[] content)
    {
        Key = key;
        SubKey = subKey;
        Content = content;
    }

    public readonly string Key;
    public readonly string SubKey;
    public readonly byte[] Content;
}