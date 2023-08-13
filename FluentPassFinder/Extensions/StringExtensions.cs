namespace FluentPassFinder.Extensions
{
    static class StringExtensions
    {
        public static bool Contains(this string sourceValue, string value, StringComparison stringComparison)
        {
            return sourceValue?.IndexOf(value, stringComparison) >= 0;
        }
    }
}
