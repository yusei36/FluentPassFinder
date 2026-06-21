// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace FluentPassFinder.Contracts.Public.Ipc
{
    internal sealed class PipeRequestConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(PipeRequest);
        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var type = jo[nameof(PipeRequest.Type)]?.Value<string>();

            PipeRequest target;
            switch (type)
            {
                case PipeRequestTypes.SearchEntries:             target = new SearchEntriesRequest();            break;
                case PipeRequestTypes.HasTotp:                   target = new HasTotpRequest();                  break;
                case PipeRequestTypes.GetSettings:               target = new GetSettingsRequest();              break;
                case PipeRequestTypes.IsAnyDatabaseOpen:         target = new IsAnyDatabaseOpenRequest();        break;
                case PipeRequestTypes.CopyField:                 target = new CopyFieldRequest();                break;
                case PipeRequestTypes.CopyToClipboard:           target = new CopyToClipboardRequest();          break;
                case PipeRequestTypes.AutoTypeField:             target = new AutoTypeFieldRequest();             break;
                case PipeRequestTypes.PerformAutoType:           target = new PerformAutoTypeRequest();          break;
                case PipeRequestTypes.OpenEntryUrl:              target = new OpenEntryUrlRequest();             break;
                case PipeRequestTypes.SelectEntry:               target = new SelectEntryRequest();              break;
                case PipeRequestTypes.SaveSettings:              target = new SaveSettingsRequest();             break;
                case PipeRequestTypes.GetTemplates:              target = new GetTemplatesRequest();             break;
                case PipeRequestTypes.CreateEntry:               target = new CreateEntryRequest();              break;
                case PipeRequestTypes.GeneratePassword:          target = new GeneratePasswordRequest();         break;
                default: return null;
            }

            serializer.Populate(jo.CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => throw new NotSupportedException();
    }
}
