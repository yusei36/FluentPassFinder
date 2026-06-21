// SPDX-FileCopyrightText: Copyright (C) 2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.ViewModels
{
    /// <summary>
    /// Backs the "Create entry" overlay. Produces a new entry in the chosen target group of the
    /// active KeePass database via <see cref="IPluginProxy.CreateEntry"/>. Supports a blank
    /// entry or one seeded from a template (KeePass core or KPEntryTemplates).
    /// </summary>
    internal partial class CreateEntryViewModel : ObservableObject
    {
        private static readonly (string FieldName, string Title, TemplateFieldType Type, bool Protected)[] StandardFields =
        {
            (Consts.UserNameField, "User name", TemplateFieldType.Text, false),
            (Consts.PasswordField, "Password", TemplateFieldType.ProtectedText, true),
            (Consts.UrlField,      "URL",       TemplateFieldType.Text, false),
            (Consts.NotesField,    "Notes",     TemplateFieldType.MultiLine, false),
        };

        private static readonly TimeSpan SavedDisplayDuration = TimeSpan.FromMilliseconds(800);

        private readonly IPluginProxy pluginProxy;
        private readonly Lazy<SearchWindowViewModel> lazySearchWindowViewModel;
        private DispatcherTimer closeTimer;

        [ObservableProperty] private string title = string.Empty;
        [ObservableProperty] private bool saved;

        public bool IsEditable => !Saved;

        partial void OnSavedChanged(bool value) => OnPropertyChanged(nameof(IsEditable));

        [ObservableProperty] private ObservableCollection<TemplateChoiceViewModel> templates = new();
        [ObservableProperty] private TemplateChoiceViewModel selectedTemplate;
        [ObservableProperty] private ObservableCollection<DynamicFieldViewModel> fields = new();
        [ObservableProperty] private ObservableCollection<GroupDto> groups = new();
        [ObservableProperty] private GroupDto selectedGroup;
        [ObservableProperty] private string errorMessage;

        public CreateEntryViewModel(IPluginProxy pluginProxy, Lazy<SearchWindowViewModel> lazySearchWindowViewModel)
        {
            this.pluginProxy = pluginProxy;
            this.lazySearchWindowViewModel = lazySearchWindowViewModel;
        }

        /// <summary>Resets the form, loads templates, and optionally preselects one.</summary>
        public void Open(string preselectTemplateUuid = null, string initialTitle = null)
        {
            ErrorMessage = null;
            Saved = false;
            closeTimer?.Stop();
            Title = initialTitle ?? string.Empty;

            LoadTemplates();
            LoadGroups();

            // Preselect the requested template; otherwise default to the first available
            // template (Uuid != null), falling back to the blank entry when none exist.
            var choice = preselectTemplateUuid != null
                ? Templates.FirstOrDefault(t => t.Uuid == preselectTemplateUuid)
                : Templates.FirstOrDefault(t => t.Uuid != null);

            SelectedTemplate = choice ?? Templates.FirstOrDefault();
        }

        private void LoadTemplates()
        {
            Templates.Clear();
            Templates.Add(TemplateChoiceViewModel.Blank);
            foreach (var template in pluginProxy.GetTemplates())
                Templates.Add(new TemplateChoiceViewModel(template));
        }

        // Loads the database's groups, defaulting the selection to the configured target group.
        private void LoadGroups()
        {
            var configured = pluginProxy.Settings?.EntryCreation?.NewEntryGroupUuid ?? Consts.DefaultNewEntryGroupUuid;
            Groups = GroupChoices.Build(pluginProxy.GetGroups(), configured);
            SelectedGroup = GroupChoices.Select(Groups, configured);
        }

        partial void OnSelectedTemplateChanged(TemplateChoiceViewModel value) => BuildForm(value?.Template);

        private void BuildForm(TemplateDto template)
        {
            var list = new List<DynamicFieldViewModel>();

            if (template != null)
            {
                // Show exactly the fields the template defines, honoring their order (_etm_position_).
                foreach (var field in template.Fields ?? Array.Empty<TemplateFieldDto>())
                    list.Add(new DynamicFieldViewModel(field, pluginProxy));
            }
            else
            {
                // Blank entry: offer the standard fields.
                foreach (var std in StandardFields)
                    list.Add(new DynamicFieldViewModel(new TemplateFieldDto
                    {
                        FieldName = std.FieldName,
                        Title = std.Title,
                        Type = std.Type,
                        IsProtected = std.Protected,
                    }, pluginProxy));
            }

            Fields = new ObservableCollection<DynamicFieldViewModel>(list);
        }

        [RelayCommand]
        private void Create()
        {
            if (Saved) return;

            if (!pluginProxy.IsAnyDatabaseOpen)
            {
                ErrorMessage = "No KeePass database is open.";
                return;
            }

            var values = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(Title))
                values[Consts.TitleField] = Title;

            foreach (var field in Fields)
            {
                if (field.Type == TemplateFieldType.Divider) continue;

                var value = field.GetResultValue();
                if (string.IsNullOrEmpty(value)) continue;

                values[field.FieldName] = value;
            }

            var createdUuid = pluginProxy.CreateEntry(SelectedTemplate?.Uuid, values, SelectedGroup?.Uuid);
            if (createdUuid == null)
            {
                ErrorMessage = "Failed to create the entry.";
                return;
            }

            Saved = true;
            closeTimer?.Stop();
            closeTimer = new DispatcherTimer { Interval = SavedDisplayDuration };
            closeTimer.Tick += (_, _) =>
            {
                closeTimer.Stop();
                lazySearchWindowViewModel.Value.CloseWindowAfterCreate();
            };
            closeTimer.Start();
        }

        [RelayCommand]
        private void Cancel() => lazySearchWindowViewModel.Value.CloseCreateEntry();
    }
}
