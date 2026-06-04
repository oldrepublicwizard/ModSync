// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Avalonia;
using Avalonia.Data.Converters;

using ModSync.Core.Utility;

namespace ModSync.Converters
{
    public partial class GuidListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is List<string> stringList
                ? stringList.Select(s => Guid.Parse(Serializer.FixGuidString(s))).ToList()
                : AvaloniaProperty.UnsetValue;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is List<Guid> guidList
                ? guidList.Select(guid => guid.ToString()).ToList()
                : AvaloniaProperty.UnsetValue;
    }
}
