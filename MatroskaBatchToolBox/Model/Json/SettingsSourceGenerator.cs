using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Model.Json
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(GlobalSettingsContainer))]
    [JsonSerializable(typeof(LocalSettingsContainer))]
    internal sealed partial class SettingsSourceGenerator
        : JsonSerializerContext
    {
    }
}
