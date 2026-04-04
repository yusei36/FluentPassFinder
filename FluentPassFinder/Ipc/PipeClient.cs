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
            cachedSettings = Send<GetSettingsRequest, GetSettingsResponse>(
                new GetSettingsRequest { Id = NewId(), Type = PipeRequestTypes.GetSettings }).Settings;
        }

        // ── IPluginProxy ──────────────────────────────────────────────────────────

        public IEnumerable<EntryDto> SearchEntries(string query)
        {
            var response = Send<SearchEntriesRequest, SearchEntriesResponse>(
                new SearchEntriesRequest { Id = NewId(), Type = PipeRequestTypes.SearchEntries, Query = query });
            return response.Entries ?? Array.Empty<EntryDto>();
        }

        public string GetPlaceholderValue(string placeholder, string entryUuid, string databaseUuid, bool resolveAll)
        {
            return Send<GetPlaceholderValueRequest, GetPlaceholderValueResponse>(
                new GetPlaceholderValueRequest
                {
                    Id           = NewId(),
                    Type         = PipeRequestTypes.GetPlaceholderValue,
                    Placeholder  = placeholder,
                    EntryUuid    = entryUuid,
                    DatabaseUuid = databaseUuid,
                    ResolveAll   = resolveAll,
                }).Value;
        }

        public string GetStringFromCustomConfig(string configId, string defaultValue)
        {
            return Send<GetStringFromCustomConfigRequest, GetStringFromCustomConfigResponse>(
                new GetStringFromCustomConfigRequest
                {
                    Id           = NewId(),
                    Type         = PipeRequestTypes.GetStringFromCustomConfig,
                    ConfigId     = configId,
                    DefaultValue = defaultValue,
                }).Value;
        }

        public Settings Settings => cachedSettings ?? (cachedSettings = FetchSettings());

        public bool IsAnyDatabaseOpen
        {
            get
            {
                return Send<IsAnyDatabaseOpenRequest, IsAnyDatabaseOpenResponse>(
                    new IsAnyDatabaseOpenRequest { Id = NewId(), Type = PipeRequestTypes.IsAnyDatabaseOpen }).IsOpen;
            }
        }

        public void CopyField(string entryUuid, string databaseUuid, string fieldName)
        {
            SendVoid(new CopyFieldRequest
            {
                Id = NewId(), Type = PipeRequestTypes.CopyField,
                EntryUuid = entryUuid, DatabaseUuid = databaseUuid, FieldName = fieldName,
            });
        }

        public void AutoTypeField(string entryUuid, string databaseUuid, string fieldName)
        {
            SendVoid(new AutoTypeFieldRequest
            {
                Id = NewId(), Type = PipeRequestTypes.AutoTypeField,
                EntryUuid = entryUuid, DatabaseUuid = databaseUuid, FieldName = fieldName,
            });
        }

        public void CopyToClipboard(string value, string entryUuid, string databaseUuid)
        {
            SendVoid(new CopyToClipboardRequest
            {
                Id = NewId(), Type = PipeRequestTypes.CopyToClipboard,
                Value = value, EntryUuid = entryUuid, DatabaseUuid = databaseUuid,
            });
        }

        public void PerformAutoType(string entryUuid, string databaseUuid, string sequence = null)
        {
            SendVoid(new PerformAutoTypeRequest
            {
                Id = NewId(), Type = PipeRequestTypes.PerformAutoType,
                EntryUuid = entryUuid, DatabaseUuid = databaseUuid, Sequence = sequence,
            });
        }

        public void OpenEntryUrl(string entryUuid, string databaseUuid)
        {
            SendVoid(new OpenEntryUrlRequest
            {
                Id = NewId(), Type = PipeRequestTypes.OpenEntryUrl,
                EntryUuid = entryUuid, DatabaseUuid = databaseUuid,
            });
        }

        public void SelectEntry(string entryUuid, string databaseUuid)
        {
            SendVoid(new SelectEntryRequest
            {
                Id = NewId(), Type = PipeRequestTypes.SelectEntry,
                EntryUuid = entryUuid, DatabaseUuid = databaseUuid,
            });
        }

        // ── Internals ─────────────────────────────────────────────────────────────

        private Settings FetchSettings()
        {
            return Send<GetSettingsRequest, GetSettingsResponse>(
                new GetSettingsRequest { Id = NewId(), Type = PipeRequestTypes.GetSettings }).Settings;
        }

        /// <summary>Send a request and deserialize the response to the concrete type.</summary>
        private TRes Send<TReq, TRes>(TReq request)
            where TReq : PipeEnvelope
            where TRes : PipeEnvelope
        {
            var responseJson = Exchange(request);
            var response = PipeProtocol.Deserialize<TRes>(responseJson);
            if (!response.Success)
                throw new InvalidOperationException(response.Error ?? "Pipe request failed.");
            return response;
        }

        /// <summary>Send a void request (no typed return value).</summary>
        private void SendVoid<TReq>(TReq request) where TReq : PipeEnvelope
        {
            var responseJson = Exchange(request);
            var response = PipeProtocol.Deserialize<PipeEnvelope>(responseJson);
            if (!response.Success)
                throw new InvalidOperationException(response.Error ?? "Pipe request failed.");
        }

        private string Exchange(PipeEnvelope envelope)
        {
            lock (syncLock)
            {
                PipeProtocol.WriteMessage(clientStream, envelope);
                return PipeProtocol.ReadJson(clientStream);
            }
        }

        private static string NewId() => Guid.NewGuid().ToString("N");

        public void Dispose()
        {
            clientStream?.Dispose();
        }
    }
}
