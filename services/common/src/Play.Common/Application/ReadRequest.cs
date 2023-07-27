namespace Play.Common.Application;

public readonly struct ReadRequest
{
    public ReadRequest(string key)
        : this(key, string.Empty)
    {
    }

    public ReadRequest(string key, string subKey)
    {
        Key = key;
        SubKey = subKey;
    }

    public readonly string Key;
    public readonly string SubKey;
}