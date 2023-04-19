namespace Play.Common.Application.Infra.Repositories.Dapr
{
    using System;

    public sealed class StateEntryNameAttribute : Attribute
    {
        internal const int StateEntryNamePosition = 0;

        internal static readonly string FullNameStateEntryNameAttribute = typeof(StateEntryNameAttribute).FullName;

        public StateEntryNameAttribute(string name) => Name = name.ToLowerInvariant();

        public readonly string Name;
    }
}