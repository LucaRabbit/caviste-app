using System.Text.Json;
using System.Text.Json.Serialization;

namespace CavisteApp.WPF.Services.ApiClient;

// Options de sérialisation JSON utilisées par défaut pour les appels API, notamment pour gérer les enums en tant que chaînes
internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}