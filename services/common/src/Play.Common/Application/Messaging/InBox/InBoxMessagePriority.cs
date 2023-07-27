namespace Play.Common.Application.Messaging.InBox;

using System.Linq;
using CSharpFunctionalExtensions;

public readonly struct InBoxMessagePriority
{
    public static readonly InBoxMessagePriority[] PriorityFromHighToNoneCache;

    public static int MaxPriority => PriorityFromHighToNoneCache.Max(x => x.Value);

    public static int MinPriority => PriorityFromHighToNoneCache.Min(x => x.Value);

    static InBoxMessagePriority()
    {
        PriorityFromHighToNoneCache = GetAll()
            .OrderByDescending(p => p.Value)
            .ToArray();
    }

    internal InBoxMessagePriority(int value, string description)
    {
        Value = value;
        Description = description;
    }

    public readonly int Value;
    public readonly string Description;

    public static readonly InBoxMessagePriority None = new(0, nameof(None));
    public static readonly InBoxMessagePriority Low = new(1, nameof(Low));
    public static readonly InBoxMessagePriority Medium = new(2, nameof(Medium));
    public static readonly InBoxMessagePriority High = new(3, nameof(High));

    private static InBoxMessagePriority[] GetAll() => new[] { None, Low, Medium, High };

    public static InBoxMessagePriority[] GetAllFromHighToNone()
    {
        return GetAll().OrderByDescending(p => p.Value).ToArray();
    }

    public static Result<InBoxMessagePriority> GetByValue(int priority)
    {
        var values = GetAll();
        for (var index = 0; index < values.Length; index++)
        {
            if (values[index].Value == priority)
                return Result.Success(values[index]);
        }

        return Result.Failure<InBoxMessagePriority>("Priority not found");
    }

    public override string ToString() => $"{Value}-{Description}";
}