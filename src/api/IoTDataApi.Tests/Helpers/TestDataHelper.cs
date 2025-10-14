using IoTDataApi.Models;

namespace IoTDataApi.Tests.Helpers;

public static class TestDataHelper
{
    public static List<IoTData> GetTestIoTData()
    {
        return new List<IoTData>
        {
            new IoTData
            {
                Id = 1,
                Topic = "machine/001/temperature",
                Message = "25.5",
                ReceivedAt = "2024-01-01T10:00:00"
            },
            new IoTData
            {
                Id = 2,
                Topic = "machine/001/vibration",
                Message = "0.8",
                ReceivedAt = "2024-01-01T10:01:00"
            },
            new IoTData
            {
                Id = 3,
                Topic = "machine/002/temperature",
                Message = "30.2",
                ReceivedAt = "2024-01-01T10:02:00"
            },
            new IoTData
            {
                Id = 4,
                Topic = "machine/002/pressure",
                Message = "101.3",
                ReceivedAt = "2024-01-01T10:03:00"
            },
            new IoTData
            {
                Id = 5,
                Topic = "machine/001/humidity",
                Message = "60.5",
                ReceivedAt = "2024-01-01T10:04:00"
            }
        };
    }

    public static IoTData CreateTestData(int id, string machineId, string sensorType, string value, string receivedAt)
    {
        return new IoTData
        {
            Id = id,
            Topic = $"machine/{machineId}/{sensorType}",
            Message = value,
            ReceivedAt = receivedAt
        };
    }
}
