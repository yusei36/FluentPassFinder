// SPDX-FileCopyrightText: Copyright (C) 2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System;

namespace FluentPassFinder.Contracts.Public
{
    /// <summary>
    /// A KeePass entry that can seed a new entry. This covers both KeePass core templates
    /// (any entry in the database's template group) and the third-party KPEntryTemplates
    /// plugin format (templates carrying <c>_etm_*</c> field-definition strings).
    /// </summary>
    public class TemplateDto
    {
        /// <summary>Base64-encoded UUID of the template entry.</summary>
        public string Uuid { get; set; }

        public string Name { get; set; }

        /// <summary>Pre-rendered 24x24 icon as PNG bytes.</summary>
        public byte[] Icon { get; set; }

        /// <summary>
        /// All editable fields (everything except Title) in display order. For KPEntryTemplates
        /// templates this honors <c>_etm_position_</c>, with standard fields (UserName/Password/URL/Notes)
        /// interleaved at their defined positions.
        /// </summary>
        public TemplateFieldDto[] Fields { get; set; } = Array.Empty<TemplateFieldDto>();
    }

    public class TemplateFieldDto
    {
        /// <summary>The actual entry string key the value is stored under.</summary>
        public string FieldName { get; set; }

        /// <summary>Human-readable label shown in the form (KPEntryTemplates title; falls back to FieldName).</summary>
        public string Title { get; set; }

        public TemplateFieldType Type { get; set; }

        public bool IsProtected { get; set; }

        public string DefaultValue { get; set; }

        /// <summary>Selectable options for <see cref="TemplateFieldType.ListBox"/>.</summary>
        public string[] Options { get; set; } = Array.Empty<string>();

        /// <summary>Number of visible lines for <see cref="TemplateFieldType.MultiLine"/> (defaults to 1).</summary>
        public int Lines { get; set; } = 1;
    }

    public enum TemplateFieldType
    {
        Text,
        ProtectedText,
        MultiLine,
        Checkbox,
        Date,
        Time,
        DateTime,
        ListBox,
        Divider,
    }
}
