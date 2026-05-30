// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Views;

namespace FluentPassFinder.Services
{
    internal class SearchWindowInteractionService : ISearchWindowInteractionService
    {
        // Time for the OS to hide the window and restore foreground before an autotype is sent.
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

            // Off the UI thread, so the deferred Hide() can run before the action fires.
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
