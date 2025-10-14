using IoTDataApi.Data;
using IoTDataApi.Models;
using Microsoft.EntityFrameworkCore;

namespace IoTDataApi.Tests.Helpers;

public static class DatabaseHelper
{
    public static IoTDataContext CreateInMemoryContext(string databaseName = "TestDatabase")
    {
        var options = new DbContextOptionsBuilder<IoTDataContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        var context = new IoTDataContext(options);

        // Seed com dados de teste
        if (!context.IoTData.Any())
        {
            context.IoTData.AddRange(TestDataHelper.GetTestIoTData());
            context.SaveChanges();
        }

        return context;
    }

    public static IoTDataContext CreateEmptyInMemoryContext(string databaseName = "EmptyTestDatabase")
    {
        var options = new DbContextOptionsBuilder<IoTDataContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new IoTDataContext(options);
    }
}
