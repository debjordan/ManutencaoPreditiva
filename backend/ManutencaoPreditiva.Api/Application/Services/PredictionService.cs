    // Application/Services/PredictionService.cs
    using ManutencaoPreditiva.Api.Domain.Entities;
    using ManutencaoPreditiva.Api.Infrastructure.Data;
    using Microsoft.EntityFrameworkCore;
    using System.Diagnostics;
    namespace ManutencaoPreditiva.Api.Application.Services;
    public class PredictionService
    {
        private readonly AppDbContext _dbContext;
        public PredictionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<double> PredictFailureAsync()
        {
            var latestSensor = await _dbContext.Sensors
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefaultAsync();
            if (latestSensor == null)
                return 0.0;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"/app/predict.py {latestSensor.Vibration} {latestSensor.Temperature}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            var probability = double.Parse(output.Trim());
            _dbContext.Predictions.Add(new Prediction
            {
                MachineId = latestSensor.MachineId,
                FailureProbability = probability,
                Timestamp = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
            return probability;
        }
    }
