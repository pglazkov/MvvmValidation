using JetBrains.Annotations;

namespace MvvmValidation.Internal
{
    // ReSharper disable InconsistentNaming
    [UsedImplicitly]
    internal static class GuardReSharperTemplates
    {
        [SourceTemplate]
        [UsedImplicitly]
        public static void guardNotNull(this object x)
        {
            Guard.NotNull(x, nameof(x));
        }

        [SourceTemplate]
        [UsedImplicitly]
        public static void guardNotNullOrEmpty(this string x)
        {
            Guard.NotNullOrEmpty(x, nameof(x));
        }

        [SourceTemplate]
        [UsedImplicitly]
        public static void guardAssertNotNull(this object x)
        {
            Guard.Assert(x != null, "$x$ != null");
        }
    }

    // ReSharper restore InconsistentNaming
}