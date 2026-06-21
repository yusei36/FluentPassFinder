// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.ViewModels
{
    /// <summary>
    /// Builds the group list shown in the "save new entries in" pickers (settings and the
    /// create-entry view). Ensures the target group is always selectable, inserting a synthetic
    /// "New entries" placeholder when the configured group does not exist yet (it is created
    /// lazily on the first entry creation).
    /// </summary>
    internal static class GroupChoices
    {
        public static ObservableCollection<GroupDto> Build(IEnumerable<GroupDto> groups, string defaultUuid)
        {
            var list = (groups ?? Array.Empty<GroupDto>()).ToList();

            // The configured default group is created lazily on first use, so it may not exist in
            // the database yet; insert a placeholder so it is still selectable.
            if (!string.IsNullOrEmpty(defaultUuid) &&
                list.All(g => !string.Equals(g.Uuid, defaultUuid, StringComparison.OrdinalIgnoreCase)))
            {
                list.Insert(0, new GroupDto
                {
                    Uuid = defaultUuid,
                    Name = Consts.DefaultNewEntryGroupName,
                    Path = Consts.DefaultNewEntryGroupName,
                });
            }

            // Mark the default target group in its displayed path.
            var defaultGroup = list.FirstOrDefault(g => string.Equals(g.Uuid, defaultUuid, StringComparison.OrdinalIgnoreCase));
            if (defaultGroup != null)
                defaultGroup.Path += " (Default)";

            return new ObservableCollection<GroupDto>(list);
        }

        public static GroupDto Select(IEnumerable<GroupDto> list, string uuid) =>
            list.FirstOrDefault(g => string.Equals(g.Uuid, uuid, StringComparison.OrdinalIgnoreCase))
            ?? list.FirstOrDefault();
    }
}
