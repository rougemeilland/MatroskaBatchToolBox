﻿using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Palmtree;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public abstract class TagCollection
    {
        internal protected readonly IDictionary<string, string> InternalTags;

        internal protected TagCollection(Dictionary<string, JsonElement>? tags)
        {
            InternalTags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (tags is not null)
            {
                foreach (var tag in tags)
                {
                    var tagValue = tag.Value.ValueKind switch
                    {
                        JsonValueKind.Undefined => "undefined",
                        JsonValueKind.Object => "<object>",
                        JsonValueKind.Array => "<array>",
                        JsonValueKind.String => tag.Value.GetString() ?? throw Validation.GetFailErrorException(),
                        JsonValueKind.Number => tag.Value.GetDecimal().ToString(CultureInfo.InvariantCulture),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Null => "null",
                        _ => throw new NotSupportedException($"Object of unknown type.: {tag.Value.ValueKind}"),
                    };
                    if (tag.Value.ValueKind != JsonValueKind.String)
                        throw new ApplicationException($"Metadata value is not a string.: key=\"{tag.Key}\", value={tagValue}");
                    if (!InternalTags.TryAdd(tag.Key, tagValue))
                        throw new ApplicationException($"Duplicate named metadata exists in the stream.: \"{tag.Key}\"");
                }
            }
        }

        public string? this[string tagName] => GetTagValue(InternalTags, tagName);

        public IEnumerable<string> EnumerateTagNames() => InternalTags.Keys.Select(key => key.ToLowerInvariant());

        internal protected static string? GetTagValue(IDictionary<string, string> tags, string tagName)
            => tags.TryGetValue(tagName, out var tagValue) ? tagValue : null;
    }
}
