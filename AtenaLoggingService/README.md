# AtenaLoggingService (.NET 8)

Self-provisioning, dynamic logging package for ASP.NET Core (.NET 8).

## Install & Use

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAtenaLogging(options =>
{
    options.Database = new AtenaLoggingService.Abstractions.DbInfo
    {
        Server = "localhost,1433",
        Database = "AtenaLogs",
        UserId = "sa",
        Password = "Strong@Pass1"
    };
    options.Projections.Add(new AtenaLoggingService.Abstractions.JsonProjection("Path", "$.http.path"));
    options.Projections.Add(new AtenaLoggingService.Abstractions.JsonProjection("UserId", "$.user.id"));
});

var app = builder.Build();
app.UseAtenaLogging();
app.MapControllers();
app.Run();
```

No manual migrations. On first run, it creates DB (optional), schema `Log`, table `Event`, base indexes, and projection columns.