// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public.Ipc
{
    public class PipeResponse
    {
        public virtual string Type => null;
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
