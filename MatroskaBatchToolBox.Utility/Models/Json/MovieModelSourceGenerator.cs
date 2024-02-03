using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Utility.Models.Json
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(MovieChapterContainer))]
    [JsonSerializable(typeof(MovieChapterTagContainer))]
    [JsonSerializable(typeof(MovieFormatContainer))]
    [JsonSerializable(typeof(MovieInformationContainer))]
    [JsonSerializable(typeof(MovieStreamDispositionContainer))]
    [JsonSerializable(typeof(MovieStreamInfoContainer))]
    public partial class MovieModelSourceGenerator
        : JsonSerializerContext
    {
    }
}
