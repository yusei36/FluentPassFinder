// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Views;

namespace FluentPassFinder.Services
{
    internal class SearchWindowInteractionService : ISearchWindowInteractionService
    {
        private const int WindowHideDelayMilliseconds = 150;

        private readonly Lazy<SearchWindow> lazySearchWindow;

        public SearchWindowInteractionService(Lazy<SearchWindow> lazySearchWindow)
        {
            this.lazySearchWindow = lazySearchWindow;
        }
        public void Close()
        {
            lazySearchWindow.Value.HideSearchWindow();
        }

        public void CloseThen(System.Action action)
        {
            Close();

            Task.Run(async () =>
            {
                await Task.Delay(WindowHideDelayMilliseconds);
                action();
            });
        }

        public void FocusSearchBox()
        {
            lazySearchWindow.Value.FocusSearchBox();
        }
    }
}
