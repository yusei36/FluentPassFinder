namespace FluentPassFinder.Contracts.Public
{
    public class EntryFieldDto
    {
        public bool IsProtected { get; set; }

        /// <summary>
        /// True if the field has a non-empty value (or is protected, in which case the value
        /// is assumed to be present to avoid reading secrets unnecessarily).
        /// </summary>
        public bool HasValue { get; set; }
    }
}
