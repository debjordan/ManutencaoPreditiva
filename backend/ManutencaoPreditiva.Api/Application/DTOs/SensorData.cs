using System;
namespace ManutencaoPreditiva.Api.Application.DTOs;
public record SensorData(string machine_id, double vibration, double temperature, string timestamp);
