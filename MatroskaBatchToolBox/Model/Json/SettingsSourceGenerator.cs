using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Model.Json
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(GlobalSettingsContainer))]
    [JsonSerializable(typeof(LocalSettingsContainer))]
    internal partial class SettingsSourceGenerator
        : JsonSerializerContext
    {
    }
}
