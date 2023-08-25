using System.Collections.Generic;

namespace FluentPassFinderContracts
{
    public class Settings
    {
        public SearchOptions SearchOptions { get; set; }
        public string PluginTotpPlaceholder { get; set; }
        
        public string GlobalHotkeyCurrentScreen { get; set; }
        public string GlobalHotkeyPrimaryScreen { get; set; }

        public ActionType MainAction { get; set; }
        public ActionType ShiftAction { get; set; }
        public ActionType ControlAction { get; set; }
        public ActionType AltAction { get; set; }

        public Dictionary<string, int> ActionSorting { get; set; }
    }
}
