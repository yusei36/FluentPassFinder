using System.Collections.Generic;

namespace FluentPassFinder.Contracts.Public
{
    public interface IPluginProxy
    {
        // Search — plugin side performs the search and returns display-ready DTOs
        IEnumerable<EntryDto> SearchEntries(string query);

        // Value resolution (used for TOTP placeholders and similar)
        string GetPlaceholderValue(string placeholder, string entryUuid, string databaseUuid, bool resolveAll);

        // Config
        string GetStringFromCustomConfig(string configId, string defaultValue);
        Settings Settings { get; }
        bool IsAnyDatabaseOpen { get; }

        // Field actions — plugin reads, resolves and executes; no secret value crosses the boundary
        void CopyField(string entryUuid, string databaseUuid, string fieldName);
        void AutoTypeField(string entryUuid, string databaseUuid, string fieldName);

        // Value actions — caller supplies an already-resolved value (e.g. TOTP)
        void CopyToClipboard(string value, string entryUuid, string databaseUuid);
        void PerformAutoType(string entryUuid, string databaseUuid, string sequence = null);

        // Navigation actions
        void OpenEntryUrl(string entryUuid, string databaseUuid);
        void SelectEntry(string entryUuid, string databaseUuid);
    }
}
