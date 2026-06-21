// SPDX-FileCopyrightText: Copyright (C) 2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using FluentPassFinder.Contracts.Public;
using FluentPassFinder.Services.Actions;

namespace FluentPassFinder.ViewModels
{
    /// <summary>
    /// One editable field rendered with the control type the template asks for (text, masked
    /// text, multi-line, checkbox, date/time, list box). Maps the typed input back to the
    /// string value KeePass stores via <see cref="GetResultValue"/>.
    /// </summary>
    internal partial class DynamicFieldViewModel : ObservableObject
    {
        private readonly IPluginProxy pluginProxy;

        public string FieldName { get; }
        public string Title { get; }
        public TemplateFieldType Type { get; }
        public bool IsProtected { get; }
        public string[] Options { get; }
        public int Lines { get; }

        [ObservableProperty] private string textValue = string.Empty;
        [ObservableProperty] private bool boolValue;
        [ObservableProperty] private string selectedOption;
        [ObservableProperty] private DateTimeOffset? dateValue;
        [ObservableProperty] private TimeSpan? timeValue;
        [ObservableProperty] private bool revealValue;

        public DynamicFieldViewModel(TemplateFieldDto dto, IPluginProxy pluginProxy)
        {
            this.pluginProxy = pluginProxy;
            FieldName = dto.FieldName;
            Title = string.IsNullOrEmpty(dto.Title) ? dto.FieldName : dto.Title;
            Type = dto.Type;
            IsProtected = dto.IsProtected;
            Options = dto.Options ?? Array.Empty<string>();
            Lines = dto.Lines > 0 ? dto.Lines : 1;

            InitializeFromDefault(dto.DefaultValue);
        }

        public bool IsDivider => Type == TemplateFieldType.Divider;
        public bool IsNotDivider => !IsDivider;
        public bool IsCheckbox => Type == TemplateFieldType.Checkbox;
        public bool IsListBox => Type == TemplateFieldType.ListBox;
        public bool IsMultiLine => Type == TemplateFieldType.MultiLine;
        public bool IsProtectedText => Type == TemplateFieldType.ProtectedText || (Type == TemplateFieldType.Text && IsProtected);
        public bool IsPlainText => Type == TemplateFieldType.Text && !IsProtected;
        public bool ShowDate => Type == TemplateFieldType.Date || Type == TemplateFieldType.DateTime;
        public bool ShowTime => Type == TemplateFieldType.Time || Type == TemplateFieldType.DateTime;
        public bool IsDateLike => ShowDate || ShowTime;

        // Height of the multi-line text box, sized to the template's requested line count
        // (mirrors the original plugin's per-textbox line sizing). ~19px per line at FontSize 13
        // plus the box's vertical padding and border.
        public double MultiLineHeight => Lines * 19 + 14;

        public bool IsPasswordField => string.Equals(FieldName, Consts.PasswordField, StringComparison.Ordinal);
        public string RevealGlyph => RevealValue ? Icons.Eye : Icons.EyeOff;

        partial void OnRevealValueChanged(bool value) => OnPropertyChanged(nameof(RevealGlyph));

        [RelayCommand]
        private void GeneratePassword() => TextValue = pluginProxy.GeneratePassword();

        private void InitializeFromDefault(string defaultValue)
        {
            if (string.IsNullOrEmpty(defaultValue)) return;

            switch (Type)
            {
                case TemplateFieldType.Checkbox:
                    BoolValue = bool.TryParse(defaultValue, out var b) && b;
                    break;
                case TemplateFieldType.ListBox:
                    SelectedOption = defaultValue;
                    break;
                case TemplateFieldType.Date:
                case TemplateFieldType.Time:
                case TemplateFieldType.DateTime:
                    if (DateTime.TryParse(defaultValue, out var dt))
                    {
                        DateValue = new DateTimeOffset(dt.Date);
                        TimeValue = dt.TimeOfDay;
                    }
                    break;
                default:
                    TextValue = defaultValue;
                    break;
            }
        }

        /// <summary>The string value to persist, or empty when the user left the field blank.</summary>
        public string GetResultValue()
        {
            switch (Type)
            {
                case TemplateFieldType.Divider:
                    return string.Empty;
                case TemplateFieldType.Checkbox:
                    return BoolValue ? bool.TrueString : bool.FalseString;
                case TemplateFieldType.ListBox:
                    return SelectedOption ?? string.Empty;
                case TemplateFieldType.Date:
                case TemplateFieldType.Time:
                case TemplateFieldType.DateTime:
                    if (DateValue == null && TimeValue == null) return string.Empty;
                    var date = DateValue?.DateTime.Date ?? DateTime.Today;
                    var time = TimeValue ?? TimeSpan.Zero;
                    return (date + time).ToString();
                default:
                    return TextValue ?? string.Empty;
            }
        }
    }
}
