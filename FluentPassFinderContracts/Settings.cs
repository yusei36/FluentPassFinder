using System.Collections.Generic;

namespace FluentPassFinderContracts
{
    public class Settings
    {
        public SearchOptions SearchOptions { get; set; } = new SearchOptions();
        public string PluginTotpPlaceholder { get; set; }
        public string PluginTotpFieldConfig { get; set; }

        public ActionType MainAction { get; set; }
        public ActionType ShiftAction { get; set; }
        public ActionType ControlAction { get; set; }
        public ActionType AltAction { get; set; }

        public Dictionary<string, int> ActionSorting { get; set; } = new Dictionary<string, int>();
        public bool ShowActionsForCustomFields { get; set; }
        public List<string> ExcludeActionsForFields { get; set; } = new List<string>();


        public string GlobalHotkeyCurrentScreen { get; set; }
        public string GlobalHotkeyPrimaryScreen { get; set; }
        public string Theme { get; set; }
    }
}
