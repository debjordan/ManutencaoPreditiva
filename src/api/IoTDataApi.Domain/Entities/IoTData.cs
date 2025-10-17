namespace IoTDataApi.Domain.Entities;

public class IoTData
{
    public int Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ReceivedAt { get; set; } = string.Empty;
}