// SPDX-FileCopyrightText: Copyright (C) 2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Media.Imaging;
using System.IO;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.ViewModels
{
    /// <summary>An item in the "save new entries in" group pickers (settings and create-entry).</summary>
    internal class GroupViewModel
    {
        public string Uuid { get; }
        public string Path { get; }
        public Bitmap Icon { get; }

        public GroupViewModel(GroupDto group, string path)
        {
            Uuid = group.Uuid;
            Path = path;
            Icon = LoadIcon(group.Icon);
        }

        public override string ToString() => Path;

        private static Bitmap LoadIcon(byte[] iconBytes)
        {
            if (iconBytes == null || iconBytes.Length == 0) return null;
            using var ms = new MemoryStream(iconBytes);
            return new Bitmap(ms);
        }
    }
}
