using FluentPassFinder.Contracts.Public;
using FluentPassFinder.Contracts.Public.Ipc;
using System;
using System.Collections.Generic;
using System.IO.Pipes;

namespace FluentPassFinder.Ipc
{
    public class PipeClient : IPluginProxy, IDisposable
    {
        private readonly string pipeName;
        private NamedPipeClientStream clientStream;
        private readonly object syncLock = new object();
        private Settings cachedSettings;

        public PipeClient(string pipeName)
        {
            this.pipeName = pipeName;
        }

        public void Connect()
        {
            clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            clientStream.Connect(10000);

            // Warm up: fetch and cache settings once on connect
            cachedSettings = SendRequest(new PipeRequest { Id = NewId(), Type = PipeRequestTypes.GetSettings })
                             .Settings;
        }

        // ── IPluginProxy ──────────────────────────────────────────────────────────

        public IEnumerable<EntryDto> SearchEntries(string query)
        {
            var response = SendRequest(new PipeRequest
            {
                Id    = NewId(),
                Type  = PipeRequestTypes.SearchEntries,
                Query = query,
            });
            return response.Entries ?? Array.Empty<EntryDto>();
        }

        public string GetPlaceholderValue(string placeholder, string entryUuid, string databaseUuid, bool resolveAll)
        {
            return SendRequest(new PipeRequest
            {
                Id           = NewId(),
                Type         = PipeRequestTypes.GetPlaceholderValue,
                Placeholder  = placeholder,
                EntryUuid    = entryUuid,
                DatabaseUuid = databaseUuid,
                ResolveAll   = resolveAll,
            }).StringValue;
        }

        public string GetStringFromCustomConfig(string configId, string defaultValue)
        {
            return SendRequest(new PipeRequest
            {
                Id           = NewId(),
                Type         = PipeRequestTypes.GetStringFromCustomConfig,
                ConfigId     = configId,
                DefaultValue = defaultValue,
            }).StringValue;
        }

        public Settings Settings => cachedSettings
                                    ?? (cachedSettings = FetchSettings());

        public bool IsAnyDatabaseOpen
        {
            get
            {
                var response = SendRequest(new PipeRequest { Id = NewId(), Type = PipeRequestTypes.IsAnyDatabaseOpen });
                return response.BoolValue == true;
            }
        }

        public void CopyField(string entryUuid, string databaseUuid, string fieldName)
        {
            SendRequest(new PipeRequest
            {
                Id           = NewId(),
                Type         = PipeRequestTypes.CopyField,
                EntryUuid    = entryUuid,
                DatabaseUuid = databaseUuid,
                FieldName    = fieldName,
            });
        }

        public void AutoTypeField(string entryUuid, string databaseUuid, string fieldName)
        {
            SendRequest(new PipeRequest
            {
                Id           = NewId(),
                Type         = PipeRequestTypes.AutoTypeField,
                EntryUuid    = entryUuid,
                DatabaseUuid = databaseUuid,
                FieldName    = fieldName,
            });
        }

        public void CopyToClipboard(string value, string entryUuid, string databaseUuid)
        {
            SendRequest(new PipeRequest
            {
                Id           = NewId(),
                Type         = PipeRequestTypes.CopyToClipboard,
                Value        = value,
                EntryUuid    = entryUuid,
                DatabaseUuid = databaseUuid,
            });
        }

        public void PerformAutoType(string entryUuid, string databaseUuid, string sequence = null)
        {
            SendRequest(new PipeRequest
            {
                Id           = NewId(),
                Type         = PipeRequestTypes.PerformAutoType,
                EntryUuid    = entryUuid,
                DatabaseUuid = databaseUuid,
                Sequence     = sequence,
            });
        }

        public void OpenEntryUrl(string entryUuid, string databaseUuid)
        {
            SendRequest(new PipeRequest
            {
                Id           = NewId(),
                Type         = PipeRequestTypes.OpenEntryUrl,
                EntryUuid    = entryUuid,
                DatabaseUuid = databaseUuid,
            });
        }

        public void SelectEntry(string entryUuid, string databaseUuid)
        {
            SendRequest(new PipeRequest
            {
                Id           = NewId(),
                Type         = PipeRequestTypes.SelectEntry,
                EntryUuid    = entryUuid,
                DatabaseUuid = databaseUuid,
            });
        }

        // ── Internals ─────────────────────────────────────────────────────────────

        private Settings FetchSettings()
        {
            return SendRequest(new PipeRequest { Id = NewId(), Type = PipeRequestTypes.GetSettings }).Settings;
        }

        private PipeResponse SendRequest(PipeRequest request)
        {
            lock (syncLock)
            {
                PipeProtocol.WriteMessage(clientStream, request);
                return PipeProtocol.ReadMessage<PipeResponse>(clientStream);
            }
        }

        private static string NewId() => Guid.NewGuid().ToString("N");

        public void Dispose()
        {
            clientStream?.Dispose();
        }
    }
}
