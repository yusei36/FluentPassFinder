namespace FluentPassFinderContracts
{
    public class SearchOptions
    {
        public bool IncludeTitleFiled { get; set; }
        public bool IncludeUserNameField { get; set; }
        public bool IncludePasswordField { get; set; }
        public bool IncludeUrlField { get; set; }
        public bool IncludeNotesField { get; set; }

        public bool IncludeCustomFields { get; set; }
        public bool IncludeProtectedCustomFields { get; set; }

        public string PluginTotpPlaceholder { get; set; }
        
        public string GlobalHotkeyCurrentScreen { get; set; }
        public string GlobalHotkeyPrimaryScreen { get; set; }
    }
}
