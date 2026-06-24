// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using KeePass.Forms;
using KeePassLib;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FluentPassFinder.Services
{
    /// <summary>
    /// Renders a KeePass entry's icon (custom or built-in) to 24x24 PNG bytes for transport to
    /// the app. Built-in icons are cached because they are shared across many entries.
    /// </summary>
    internal class EntryIconRenderer
    {
        private readonly MainForm mainWindow;
        private readonly Dictionary<int, byte[]> builtInIconCache = new Dictionary<int, byte[]>();

        public EntryIconRenderer(MainForm mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public byte[] GetEntryIconBytes(PwEntry entry, PwDatabase db)
        {
            if (!entry.CustomIconUuid.Equals(PwUuid.Zero))
            {
                var customImage = db.GetCustomIcon(entry.CustomIconUuid, 24, 24);
                if (customImage != null)
                    return ImageToBytes(customImage);
            }

            return GetBuiltInIconCached((int)entry.IconId);
        }

        public byte[] GetGroupIconBytes(PwGroup group, PwDatabase db)
        {
            if (!group.CustomIconUuid.Equals(PwUuid.Zero))
            {
                var customImage = db.GetCustomIcon(group.CustomIconUuid, 24, 24);
                if (customImage != null)
                    return ImageToBytes(customImage);
            }

            return GetBuiltInIconCached((int)group.IconId);
        }

        private byte[] GetBuiltInIconCached(int iconId)
        {
            if (!builtInIconCache.TryGetValue(iconId, out var bytes))
            {
                var image = mainWindow.ClientIcons.Images[iconId];
                bytes = ImageToBytes(image);
                builtInIconCache[iconId] = bytes;
            }
            return bytes;
        }

        private static byte[] ImageToBytes(Image image)
        {
            if (image == null) return null;
            using (var ms = new MemoryStream())
            using (var bmp = new Bitmap(image))
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
