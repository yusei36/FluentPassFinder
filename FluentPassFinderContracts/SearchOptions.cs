﻿namespace FluentPassFinder.Contracts.Public
{
    public class SearchOptions
    {
        public bool IncludeTitleField { get; set; }
        public bool IncludeUserNameField { get; set; }
        public bool IncludePasswordField { get; set; }
        public bool IncludeUrlField { get; set; }
        public bool IncludeNotesField { get; set; }
        public bool IncludeTags { get; set; }
        public bool IncludeCustomFields { get; set; }
        public bool IncludeProtectedCustomFields { get; set; }


        public bool ExcludeExpiredEntries { get; set; }
        public bool ExcludeGroupsBySearchSetting { get; set; }

        public bool ResolveFieldReferences { get; set; }
    }
}
