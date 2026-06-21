// SPDX-FileCopyrightText: Copyright (C) 2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Media.Imaging;
using System.IO;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.ViewModels
{
    /// <summary>An item in the create-entry template selector. The first item is a blank entry.</summary>
    internal class TemplateChoiceViewModel
    {
        public string Uuid { get; }
        public string Name { get; }
        public TemplateDto Template { get; }
        public Bitmap Icon { get; }
        public bool HasIcon => Icon != null;

        private TemplateChoiceViewModel()
        {
            Name = "(Blank entry)";
        }

        public TemplateChoiceViewModel(TemplateDto template)
        {
            Template = template;
            Uuid = template.Uuid;
            Name = string.IsNullOrEmpty(template.Name) ? "(Unnamed template)" : template.Name;
            Icon = LoadIcon(template.Icon);
        }

        public static TemplateChoiceViewModel Blank { get; } = new TemplateChoiceViewModel();

        public override string ToString() => Name;

        private static Bitmap LoadIcon(byte[] iconBytes)
        {
            if (iconBytes == null || iconBytes.Length == 0) return null;
            using var ms = new MemoryStream(iconBytes);
            return new Bitmap(ms);
        }
    }
}
