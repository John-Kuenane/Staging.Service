using eStaging.API.Application.Validations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Staging.API.Application.Mapper;
using Staging.API.Application.Mapper.FreshDesk;
using Staging.API.Application.Queries.ExternalSubmissionAggregate;
using Staging.API.Application.Queries.PackageAggregate;
using Staging.API.Application.Services.ExternalSubmissions;
using Staging.API.Infrastructure.BackgroundProcessing;
using Staging.API.Infrastructure.BackgroundProcessing.ExternalSubmission;
using Staging.API.Infrastructure.BackgroundProcessing.Sync;
using Staging.API.Infrastructure.HealthChecks;
using Staging.API.Infrastructure.Processors;
using Staging.API.Infrastructure.Services.ExternalSubmissions.FreshDesk;
using Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification;
using Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification.Golsabs;
using Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification.NICR;
using Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;
using Staging.Domain.AggregatesModel.PackageAggregate;
using Staging.Domain.Factories;
using Staging.Domain.ProcessorActivities;
using Staging.Domain.Services;
using Staging.Infrastructure.Factories;
using Staging.Infrastructure.Repositories;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add the authentication services to DI
        //builder.AddDefaultAuthentication();

        builder.AddApplicationMappers();
        builder.AddDatabase();
        builder.AddApplicationOptions();
        builder.AddIntegrations();
        builder.AddPersistence();
        builder.AddApplicationPipeline();
        builder.AddHostedWorkers();
        builder.AddWorkerHealthChecks();

        //services.AddMigration<ISSNContext, ISSNContextSeed>();

        // Add the integration services that consume the DbContext
        services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService<DatabaseContext>>();

        //services.AddTransient<IOrderingIntegrationEventService, OrderingIntegrationEventService>();

        //builder.AddRabbitMqEventBus("eventbus")
        //       .AddEventBusSubscriptions();

        services.AddHttpContextAccessor();
        //services.AddTransient<IIdentityService, IdentityService>();

        services.AddTransient<PayloadProcessor<DataListingEventPayloadActivity>, DataListingEventProcessor>();
        services.AddTransient<PayloadProcessor<DataCollectionEventPayloadActivity>, DataCollectionEventProcessor>();
        services.AddTransient<IPayloadProcessorFactory>(
            fac =>
            {
                var factory = new PayloadProcessorFactory();

                factory.AddProcessor(fac.GetService<PayloadProcessor<DataListingEventPayloadActivity>>());
                factory.AddProcessor(fac.GetService<PayloadProcessor<DataCollectionEventPayloadActivity>>());

                return factory;
            }     
        );

        services.AddScoped<IRequestManager, RequestManager>();
    }

    private static void AddEventBusSubscriptions(this IEventBusBuilder eventBus)
    {
        //eventBus.AddSubscription<GracePeriodConfirmedIntegrationEvent, GracePeriodConfirmedIntegrationEventHandler>();
        //eventBus.AddSubscription<OrderStockConfirmedIntegrationEvent, OrderStockConfirmedIntegrationEventHandler>();
        //eventBus.AddSubscription<OrderStockRejectedIntegrationEvent, OrderStockRejectedIntegrationEventHandler>();
        //eventBus.AddSubscription<OrderPaymentFailedIntegrationEvent, OrderPaymentFailedIntegrationEventHandler>();
        //eventBus.AddSubscription<OrderPaymentSucceededIntegrationEvent, OrderPaymentSucceededIntegrationEventHandler>();
    }

    private static void AddApplicationMappers(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddScoped<IVendorMapper<PackageEventDataFlag>, FreshdeskDataFlagMapper>();
        services.AddScoped<IVendorPayloadMapper, VendorPayloadMapper>();
    }

    private static void AddDatabase(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        
        // Pooling is disabled because of the following error:
        // Unhandled exception. System.InvalidOperationException:
        // The DbContext of type 'ISSNContext' cannot be pooled because it does not have a public constructor accepting a single parameter of type DbContextOptions or has more than one constructor.
        services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseSqlServer(builder.Configuration["ConnectionStrings:StagingDbContext"],
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    sqlOptions.UseCompatibilityLevel(120);
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Staging", "staging");
                });
        },
            ServiceLifetime.Scoped  //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request)
        );
    }

    private static void AddApplicationOptions(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Workers
        services.Configure<SyncWorkerOptions>(
            builder.Configuration.GetSection("Workers:Sync"));

        services.Configure<ExternalSubmissionWorkerOptions>(
            builder.Configuration.GetSection("Workers:ExternalSubmissions"));

        // Integrations
        services.Configure<FreshDeskSettings>(
            builder.Configuration.GetSection("Integrations:Freshdesk"));

        services.Configure<IdentityVerificationSettings>(
            builder.Configuration.GetSection("Integrations:IdentityVerification"));
    }

    private static void AddIntegrations(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // External submission handlers
        services.AddScoped<IExternalSubmissionHandler, FreshdeskDataFlagSubmissionHandler>();
        services.AddScoped<IExternalSubmissionHandler, IdentityVerificationSubmissionHandler>();

        services.AddScoped<IIdentityVerificationProvider, NicrIdentityVerificationProvider>();
        services.AddScoped<IIdentityVerificationProvider, GolsabsIdentityVerificationProvider>();
        services.AddScoped<IIdentityVerificationProviderResolver, IdentityVerificationProviderResolver>();
    }

    private static void AddPersistence(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IPackageEventRepository, PackageEventRepository>();
        services.AddScoped<IPackageEventHouseholdRepository, PackageEventHouseholdRepository>();
        services.AddScoped<IExternalSubmissionRepository, ExternalSubmissionRepository>();
        services.AddScoped<IHouseholdIdAllocator, HouseholdIdAllocator>();

        services.AddScoped<IPackageQueries>(sp =>
            new PackageQueries(builder.Configuration["ConnectionStrings:StagingDbContext"]));

        services.AddScoped<IExternalSubmissionQueries>(sp =>
            new ExternalSubmissionQueries(builder.Configuration["ConnectionStrings:StagingDbContext"]));
    }

    private static void AddApplicationPipeline(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Program>();
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
            //cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssemblyContaining<AddDataFlagCommandValidator>();
    }

    private static void AddHostedWorkers(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Sync
        services.AddSingleton<IWorkQueue<SyncWorkItem>>(_ => new BoundedChannelQueue<SyncWorkItem>(10_000));
        services.AddSingleton<SyncWorkerMonitor>();
        services.AddScoped<IWorkerProcessor<SyncWorkItem>, SyncWorkerProcessor>();
        services.AddHostedService<WorkerProcessorService<SyncWorkItem, SyncWorkerOptions, SyncWorkerMonitor>>();

        // External submissions
        services.AddSingleton<IWorkQueue<ExternalSubmissionWorkItem>>(_ => new BoundedChannelQueue<ExternalSubmissionWorkItem>(10_000));
        services.AddSingleton<ExternalSubmissionWorkerMonitor>();
        services.AddScoped<IWorkerProcessor<ExternalSubmissionWorkItem>, ExternalSubmissionWorkerProcessor>();
        services.AddHostedService<WorkerProcessorService<ExternalSubmissionWorkItem, ExternalSubmissionWorkerOptions, ExternalSubmissionWorkerMonitor>>();
    }

    private static void AddWorkerHealthChecks(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHealthChecks()
            .AddCheck<SyncQueueHealthCheck>("sync_queue")
            .AddCheck<SyncWorkerHealthCheck>("sync_workers")
            .AddCheck<SyncProcessingHealthCheck>("sync_processing")
            .AddCheck<SyncFailureHealthCheck>("sync_failures")
            .AddCheck<ExternalSubmissionQueueHealthCheck>("external_submission_queue")
            .AddCheck<ExternalSubmissionWorkerHealthCheck>("external_submission_workers")
            .AddCheck<ExternalSubmissionProcessingHealthCheck>("external_submission_processing");
    }
}
