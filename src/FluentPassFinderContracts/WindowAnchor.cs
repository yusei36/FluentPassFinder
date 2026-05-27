// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public
{
    public enum WindowAnchor
    {
        CenterCenter = 0,
        CenterTop,
        CenterBottom,
        LeftCenter,
        LeftTop,
        LeftBottom,
        RightCenter,
        RightTop,
        RightBottom,
    }

    public static class WindowAnchorExtensions
    {
        public static bool IsBottom(this WindowAnchor anchor) =>
            anchor == WindowAnchor.LeftBottom ||
            anchor == WindowAnchor.CenterBottom ||
            anchor == WindowAnchor.RightBottom;
    }
}
