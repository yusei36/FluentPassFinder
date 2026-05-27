// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Data.Converters;
using FluentPassFinder.Contracts.Public;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FluentPassFinder.Converters
{
    internal class WindowAnchorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WindowAnchor anchor)
                return Regex.Replace(anchor.ToString(), "(?<=[a-z])(?=[A-Z])", "-");
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
