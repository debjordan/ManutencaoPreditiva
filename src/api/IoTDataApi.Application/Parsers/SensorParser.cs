using System.Text.Json;
using IoTDataApi.Application.DTOs;
using IoTDataApi.Domain.Entities;

namespace IoTDataApi.Application.Parsers;

/// <summary>
/// Converte o JSON cru persistido pelo subscriber em um DTO tipado.
/// Isolado aqui para que IoTDataService, RulService e TrendService
/// não dupliquem a lógica de desserialização.
/// </summary>
public static class SensorParser
{
    public static SensorReadingDto? Parse(IoTData record)
    {
        try
        {
            using var document = JsonDocument.Parse(record.Message);
            var root = document.RootElement;

            return new SensorReadingDto
            {
                MachineId   = ReadString(root, "machine_id"),
                MachineName = ReadString(root, "machine_name", ReadString(root, "machine_id")),
                MachineType = ReadString(root, "machine_type"),
                Area        = ReadString(root, "area"),
                State       = ReadString(root, "state", "normal"),
                Vibration   = ReadDouble(root, "vibration"),
                Temperature = ReadDouble(root, "temperature"),
                Pressure    = ReadDouble(root, "pressure"),
                Humidity    = ReadDouble(root, "humidity"),
                Voltage     = ReadDouble(root, "voltage"),
                Current     = ReadDouble(root, "current"),
                Power       = ReadDouble(root, "power"),
                Timestamp   = ReadString(root, "timestamp"),
                ReceivedAt  = record.ReceivedAt,
            };
        }
        catch
        {
            return null;
        }
    }

    private static string ReadString(JsonElement root, string key, string fallback = "")
        => root.TryGetProperty(key, out var prop) ? prop.GetString() ?? fallback : fallback;

    private static double ReadDouble(JsonElement root, string key, double fallback = 0.0)
        => root.TryGetProperty(key, out var prop) && prop.TryGetDouble(out var value)
            ? value
            : fallback;
}
