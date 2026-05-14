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

            cachedSettings = Send<GetSettingsRequest, GetSettingsResponse>(new GetSettingsRequest { Id = NewId() })?.Settings;
        }

        // ── IPluginProxy ──────────────────────────────────────────────────────────

        public IEnumerable<EntryDto> SearchEntries(string query)
        {
            var response = Send<SearchEntriesRequest, SearchEntriesResponse>(
                new SearchEntriesRequest { Id = NewId(), Query = query });
            return response?.Entries ?? Array.Empty<EntryDto>();
        }

        public string GetPlaceholderValue(string placeholder, string entryUuid, string databaseUuid, bool resolveAll)
        {
            var response = Send<GetPlaceholderValueRequest, GetPlaceholderValueResponse>(
                new GetPlaceholderValueRequest
                {
                    Id           = NewId(),
                    Placeholder  = placeholder,
                    EntryUuid    = entryUuid,
                    DatabaseUuid = databaseUuid,
                    ResolveAll   = resolveAll,
                });
            return response?.Value;
        }

        public string GetStringFromCustomConfig(string configId, string defaultValue)
        {
            var response = Send<GetStringFromCustomConfigRequest, GetStringFromCustomConfigResponse>(
                new GetStringFromCustomConfigRequest
                {
                    Id           = NewId(),
                    ConfigId     = configId,
                    DefaultValue = defaultValue,
                });
            return response?.Value ?? defaultValue;
        }

        public Settings Settings => cachedSettings ?? (cachedSettings = FetchSettings());

        public bool IsAnyDatabaseOpen
        {
            get
            {
                var response = Send<IsAnyDatabaseOpenRequest, IsAnyDatabaseOpenResponse>(
                    new IsAnyDatabaseOpenRequest { Id = NewId() });
                return response?.IsOpen ?? false;
            }
        }

        public void CopyField(string entryUuid, string databaseUuid, string fieldName)
        {
            Send<CopyFieldRequest, PipeResponse>(new CopyFieldRequest
            {
                Id = NewId(), EntryUuid = entryUuid, DatabaseUuid = databaseUuid, FieldName = fieldName,
            });
        }

        public void AutoTypeField(string entryUuid, string databaseUuid, string fieldName)
        {
            Send<AutoTypeFieldRequest, PipeResponse>(new AutoTypeFieldRequest
            {
                Id = NewId(), EntryUuid = entryUuid, DatabaseUuid = databaseUuid, FieldName = fieldName,
            });
        }

        public void CopyToClipboard(string value, string entryUuid, string databaseUuid)
        {
            Send<CopyToClipboardRequest, PipeResponse>(new CopyToClipboardRequest
            {
                Id = NewId(), Value = value, EntryUuid = entryUuid, DatabaseUuid = databaseUuid,
            });
        }

        public void PerformAutoType(string entryUuid, string databaseUuid, string sequence = null)
        {
            Send<PerformAutoTypeRequest, PipeResponse>(new PerformAutoTypeRequest
            {
                Id = NewId(), EntryUuid = entryUuid, DatabaseUuid = databaseUuid, Sequence = sequence,
            });
        }

        public void OpenEntryUrl(string entryUuid, string databaseUuid)
        {
            Send<OpenEntryUrlRequest, PipeResponse>(new OpenEntryUrlRequest
            {
                Id = NewId(), EntryUuid = entryUuid, DatabaseUuid = databaseUuid,
            });
        }

        public void SelectEntry(string entryUuid, string databaseUuid)
        {
            Send<SelectEntryRequest, PipeResponse>(new SelectEntryRequest
            {
                Id = NewId(), EntryUuid = entryUuid, DatabaseUuid = databaseUuid,
            });
        }

        public void SaveSettings(Settings settings)
        {
            Send<SaveSettingsRequest, PipeResponse>(new SaveSettingsRequest { Id = NewId(), Settings = settings });
            cachedSettings = settings;
        }

        // ── Internals ─────────────────────────────────────────────────────────────

        private Settings FetchSettings()
        {
            return Send<GetSettingsRequest, GetSettingsResponse>(new GetSettingsRequest { Id = NewId() })?.Settings;
        }

        private TRes Send<TReq, TRes>(TReq request)
            where TReq : PipeRequest
            where TRes : PipeResponse
        {
            lock (syncLock)
            {
                PipeProtocol.WriteRequest(clientStream, request);
                return PipeProtocol.ReadResponse<TRes>(clientStream);
            }
        }

        private static string NewId() => Guid.NewGuid().ToString("N");

        public void Dispose()
        {
            clientStream?.Dispose();
        }
    }
}
