using System.Text.Json;
using System.Text.Json.Serialization;

namespace NewTaxSdk.Business;

internal static class JsonSerializerConfig
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter()  }
    };
}