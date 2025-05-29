using System;
namespace ManutencaoPreditiva.Api.Application.DTOs;
public record ProductionData(
    string machine_id,
    double cycle_time,
    double man_time,
    double machine_time,
    double availability,
    double performance,
    double quality,
    string timestamp);
