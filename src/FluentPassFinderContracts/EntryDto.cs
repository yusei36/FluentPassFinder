// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.Generic;

namespace FluentPassFinder.Contracts.Public
{
    public class EntryDto
    {
        public string Uuid { get; set; }
        public string DatabaseUuid { get; set; }

        public string Title { get; set; }
        public string UserName { get; set; }
        public string Url { get; set; }

        public bool Expires { get; set; }

        /// <summary>Pre-rendered 24×24 icon as PNG bytes.</summary>
        public byte[] Icon { get; set; }

        /// <summary>All entry fields keyed by field name.</summary>
        public Dictionary<string, EntryFieldDto> Fields { get; set; } = new Dictionary<string, EntryFieldDto>();

        public List<string> Tags { get; set; } = new List<string>();
    }
}
