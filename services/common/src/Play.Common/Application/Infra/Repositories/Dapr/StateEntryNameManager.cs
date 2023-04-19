namespace Play.Common.Application.Infra.Repositories.Dapr
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class StateEntryNameManager
    {
        private static readonly object LockObject;
        private static ImmutableDictionary<string, string> _cache;

        static StateEntryNameManager()
        {
            LockObject = new object();
            _cache = ImmutableDictionary<string, string>.Empty;
        }

        public static bool TryExtractName<TEntry>(out string topicName) => ExecuteTryExtractName<TEntry>(out topicName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ExecuteTryExtractName<TEntry>(out string stateEntryName)
        {
            stateEntryName = string.Empty;

            var attr = typeof(TEntry).CustomAttributes
                .SingleOrDefault(x =>
                    x.AttributeType.FullName != null
                    && x.AttributeType.FullName.Equals(StateEntryNameAttribute.FullNameStateEntryNameAttribute,
                        StringComparison.InvariantCultureIgnoreCase));

            if (attr == null)
            {
                stateEntryName = string.Empty;
                return false;
            }

            stateEntryName = attr.ConstructorArguments[StateEntryNameAttribute.StateEntryNamePosition].Value.ToString();
            var modeFullName = typeof(TEntry).FullName;

            if (_cache.TryGetValue(modeFullName, out stateEntryName))
                return true;

            lock (LockObject)
            {
                if (_cache.TryGetValue(modeFullName, out stateEntryName))
                    return true;

                stateEntryName = attr.ConstructorArguments[StateEntryNameAttribute.StateEntryNamePosition].Value
                    .ToString();
                _cache = _cache.Add(modeFullName, stateEntryName);
            }

            return true;
        }

        private static bool TryFindStateEntryName(CustomAttributeData x) =>
            x.AttributeType.FullName != null
            && x.AttributeType.FullName.Equals(StateEntryNameAttribute.FullNameStateEntryNameAttribute,
                StringComparison.InvariantCultureIgnoreCase);
    }
}