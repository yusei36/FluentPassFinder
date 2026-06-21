// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts.Public.Ipc;
using FluentPassFinder.Services;
using KeePass.Plugins;
using System;
using System.Linq;

namespace FluentPassFinder.Ipc
{
    /// <summary>
    /// Routes an incoming pipe request to the service that handles it. Owns the per-domain
    /// services (search, entry operations, entry creation, settings) and the shared KeePass
    /// context; contains no KeePass logic itself.
    /// </summary>
    internal class PluginRequestHandler
    {
        private readonly KeePassContext context;
        private readonly SettingsStore settingsStore;
        private readonly EntrySearchService searchService;
        private readonly EntryOperationService operationService;
        private readonly EntryCreationService creationService;

        public PluginRequestHandler(IPluginHost pluginHost)
        {
            context = new KeePassContext(pluginHost);
            settingsStore = new SettingsStore(pluginHost, context);
            var iconRenderer = new EntryIconRenderer(context.MainWindow);
            searchService = new EntrySearchService(context, settingsStore, iconRenderer);
            operationService = new EntryOperationService(context);
            creationService = new EntryCreationService(context, iconRenderer, settingsStore);
        }

        public PipeResponse Handle(PipeRequest request)
        {
            try
            {
                switch (request)
                {
                    case SearchEntriesRequest req:             return searchService.Search(req);
                    case HasTotpRequest req:                   return operationService.HasTotp(req);
                    case GetSettingsRequest _:                 return new GetSettingsResponse { Success = true, Settings = settingsStore.Current };
                    case IsAnyDatabaseOpenRequest _:           return new IsAnyDatabaseOpenResponse { Success = true, IsOpen = context.Invoke(() => context.OpenDatabases.Any()) };
                    case CopyFieldRequest req:                 return operationService.CopyField(req);
                    case CopyToClipboardRequest req:           return operationService.CopyToClipboard(req);
                    case AutoTypeFieldRequest req:             return operationService.AutoTypeField(req);
                    case PerformAutoTypeRequest req:           return operationService.PerformAutoType(req);
                    case OpenEntryUrlRequest req:              return operationService.OpenEntryUrl(req);
                    case SelectEntryRequest req:               return operationService.SelectEntry(req);
                    case SaveSettingsRequest req:              settingsStore.Save(req.Settings); return Ack();
                    case GetTemplatesRequest _:                return creationService.GetTemplates();
                    case GetGroupsRequest _:                   return creationService.GetGroups();
                    case CreateEntryRequest req:               return creationService.CreateEntry(req);
                    case GeneratePasswordRequest _:            return creationService.GeneratePassword();
                    default:                                   return Ack(success: false, error: $"Unknown request type: {request.Type}");
                }
            }
            catch (Exception ex)
            {
                return Ack(success: false, error: ex.Message);
            }
        }

        private static PipeResponse Ack(bool success = true, string error = null) =>
            new PipeResponse { Success = success, Error = error };
    }
}
