using System;
namespace ManutencaoPreditiva.Application.DTOs;
public record SensorData(string machine_id, double vibration, double temperature, string timestamp);
