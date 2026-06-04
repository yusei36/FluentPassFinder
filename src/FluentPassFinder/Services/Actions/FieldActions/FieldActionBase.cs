// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.FieldActions
{
    internal abstract class FieldActionBase : ActionBase, IFieldAction
    {
        public string FieldName { get; private set; }

        public void Initialize(IPluginProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService, string fieldName)
        {
            FieldName = fieldName;
            Initialize(hostProxy, searchWindowInteractionService);
        }

        public override string IconGlyph => FieldName switch
        {
            Consts.UserNameField => Icons.Person,
            Consts.PasswordField => Icons.Lock,
            Consts.TitleField    => Icons.Text,
            Consts.NotesField    => Icons.Document,
            Consts.UrlField      => Icons.Globe,
            _                    => Icons.Tag,
        };

        public override bool CanRunAction(EntrySearchResult searchResult)
        {
            if (!searchResult.Entry.Fields.TryGetValue(FieldName, out var field))
                return false;

            return field.HasValue;
        }
    }
}
