using System.Collections.Generic;

namespace FluentPassFinder.Contracts.Public
{
    public class Settings
    {
        public SearchOptions SearchOptions { get; set; } = new();
        public string PluginTotpPlaceholder { get; set; }
        public string PluginTotpFieldConfig { get; set; }

        public string MainAction { get; set; }
        public string ShiftAction { get; set; }
        public string ControlAction { get; set; }
        public string AltAction { get; set; }

        public Dictionary<string, int> ActionSorting { get; set; } = new();
        public bool ShowActionsForCustomFields { get; set; }
        public List<string> ExcludeActionsForFields { get; set; } = new();


        public string GlobalHotkeyCurrentScreen { get; set; }
        public string GlobalHotkeyPrimaryScreen { get; set; }
        public string Theme { get; set; }
    }
}
