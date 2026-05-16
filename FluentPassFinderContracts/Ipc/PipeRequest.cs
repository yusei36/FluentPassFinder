// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Newtonsoft.Json;

namespace FluentPassFinder.Contracts.Public.Ipc
{
    [JsonConverter(typeof(PipeRequestConverter))]
    public abstract class PipeRequest
    {
        public abstract string Type { get; }
    }
}
