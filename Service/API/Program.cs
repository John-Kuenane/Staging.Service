using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

public partial class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // Read configuration from appsettings.json + environment-specific appsettings
            var env = builder.Environment.EnvironmentName;

            // Configure Serilog from configuration
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            Log.Information("Starting up in {Environment} environment", env);

            builder.AddServiceDefaults();
            builder.AddApplicationServices();
            builder.Services.AddProblemDetails();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    policy => policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(_ => true)
                        .AllowCredentials());
            });

            var withApiVersioning = builder.Services.AddApiVersioning(opt =>
            {
                opt.ApiVersionReader = new HeaderApiVersionReader("Api-Version");
                opt.DefaultApiVersion = new ApiVersion(1, 0); ;
                opt.AssumeDefaultVersionWhenUnspecified = true;
            });

            builder.AddDefaultOpenApi(withApiVersioning);

            // ------------------------------------------------------------
            // Build app
            // ------------------------------------------------------------
            var app = builder.Build();

            app.UseCors("AllowSpecificOrigin");

            app.MapDefaultEndpoints();

            var packages = app.NewVersionedApi("Packages");

            //packages.MapPackagesApiV1()
            //      .RequireAuthorization();
            packages.MapPackagesApiV1();
            packages.MapHealthApiV1();

            app.UseDefaultOpenApi();
            app.Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}