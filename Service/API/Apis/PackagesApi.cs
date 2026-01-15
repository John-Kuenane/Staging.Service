using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Staging.API.Application.Commands.PackageAggregate;
using Staging.API.Application.Common.Filters;
using Staging.API.Application.Dtos;
using Staging.API.Application.Models.Pagination;
using Staging.API.Application.Queries.PackageAggregate;

public static class PackagesApi
{
    public static RouteGroupBuilder MapPackagesApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/packages")
            .HasApiVersion(1.0);

        api.MapGet("/", GetPackagesForDeviceAsync);
        api.MapGet("/packages-for-management", GetPackagesForManagementAsync);
        api.MapPut("/upload", SavePayloadAsync);

        api.MapGet("{packageId:int}", GetPackageForManagementAsync);
        api.MapGet("{packageId:int}/events-for-management", GetPackageEventsForManagementAsync);
        api.MapGet("{packageId:int}/data-flags", GetPackageDataFlagsAsync);

        api.MapGet("{packageId:int}/events/{packageEventId:int}/management-households", GetPackageEventHouseholdsForManagementAsync);
        api.MapGet("{packageId:int}/events/{packageEventId:int}/collection-households", GetCollectionPackageHouseholdsAsync);
        api.MapGet("{packageId:int}/events/{packageEventId:int}/community-validation-households", GetCommunityValidationPackageHouseholdsAsync);
        api.MapPut("{packageId:int}/events/{packageEventId:int}/change-status-data-collection", ChangeStatusToDataCollectionAsync);
        api.MapPut("{packageId:int}/events/{packageEventId:int}/change-status-accepted", ChangeStatusToAcceptedAsync);
        api.MapPut("{packageId:int}/events/{packageEventId:int}/change-status-gateway", ChangeStatusToGatewayAsync);

        api.MapGet("{packageId:int}/events/{packageEventId:int}/households/{packageEventHouseholdId:int}/data-flags", GetHouseholdDataFlagsAsync);
        api.MapGet("{packageId:int}/events/{packageEventId:int}/households/{packageEventHouseholdId:int}/latest-synch", GetLatestSynchForHouseholdAsync);
        api.MapPost("{packageId:int}/events/{packageEventId:int}/households/{packageEventHouseholdId:int}/data-flag-add", AddDataFlagAsync);
        api.MapPut("{packageId:int}/events/{packageEventId:int}/households/{packageEventHouseholdId:int}/change-status-accepted", ChangeHouseholdStatusToAcceptedAsync);
        api.MapPut("{packageId:int}/events/{packageEventId:int}/households/{packageEventHouseholdId:int}/change-status-rejected", ChangeHouseholdStatusToRejectedAsync);

        api.MapGet("{packageId:int}/events/{packageEventId:int}/data-flags/{packageEventDataFlagId:int}", GetPackageEventDataFlagAsync);
        api.MapGet("{packageId:int}/events/{packageEventId:int}/data-flags/{packageEventDataFlagId:int}/submissions", GetPackageEventDataFlagSubmissionsAsync);

        //api.MapPost("{packageId:int}/events/{packageEventId:int}/process-synch", ProcessPayloadsAsync);
        //api.MapPost("{packageId:int}/process-synch", ProcessAllPayloadsAsync);

        return api;
    }

    public static async Task<Ok<PackageForManagementDto>> GetPackageForManagementAsync(int packageId, [AsParameters] PackageServices services)
    {
        var packageForManagement = await services.Queries.GetPackageForManagementAsync(packageId);
        return TypedResults.Ok(packageForManagement);
    }

    public static async Task<Ok<DataFlagForDetailDto>> GetPackageEventDataFlagAsync(int packageEventDataFlagId, [AsParameters] PackageServices services)
    {
        var dataFlag = await services.Queries.GetPackageEventDataFlagAsync(packageEventDataFlagId);
        return TypedResults.Ok(dataFlag);
    }

    public static async Task<Ok<PackageEventHouseholdSynchForManagementDto>> GetLatestSynchForHouseholdAsync(int packageId, int packageEventId, int packageEventHouseholdId, [AsParameters] PackageServices services)
    {
        var synch = await services.Queries.GetLatestSynchForHouseholdAsync(packageEventId, packageEventHouseholdId);
        return TypedResults.Ok(synch);
    }

    public static async Task<Ok<IEnumerable<PackageForManagementDto>>> GetPackagesForManagementAsync([AsParameters] PackageServices services)
    {
        var packages = await services.Queries.GetPackagesForManagementAsync();
        return TypedResults.Ok(packages);
    }

    public static async Task<Ok<IEnumerable<PackageDto>>> GetPackagesForDeviceAsync([AsParameters] PackageServices services)
    {
        var packages = await services.Queries.GetCollectionPackagesForDeviceAsync("DVC-001");
        return TypedResults.Ok(packages);
    }

    public static async Task<Ok<IEnumerable<PackageEventForManagementDto>>> GetPackageEventsForManagementAsync(int packageId, [FromQuery] int packageStatusId, [AsParameters] PackageServices services)
    {
        var households = await services.Queries.GetPackageEventsForManagementAsync(packageId, packageStatusId);
        return TypedResults.Ok(households);
    }

    public static async Task<Ok<PagedApiResponse<PackageEventHouseholdForManagementDto>>> GetPackageEventHouseholdsForManagementAsync(
        int packageEventId,
        [AsParameters] PackageServices services,
        [FromQuery] PackageEventHouseholdFilter filter = PackageEventHouseholdFilter.Default,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize)
    {
        var pagination = new PaginationRequest(page, pageSize);

        var pagedResult = await services.Queries
            .GetPackageEventHouseholdsForManagementAsync(
                packageEventId,
                filter,
                pagination);

        var response = new PagedApiResponse<PackageEventHouseholdForManagementDto>(
            Value: pagedResult.Items,
            RecordCount: pagedResult.TotalCount,
            Pagination: new PaginationMeta(
                TotalItems: pagedResult.TotalCount,
                PageIndex: pagination.Page - 1,      // Angular = 0-based
                PageSize: pagination.PageSize,
                TotalPages: pagedResult.TotalPages
            )
        );

        return TypedResults.Ok(response);
    }

    public static async Task<Ok<PagedApiResponse<DataFlagForListDto>>> GetPackageDataFlagsAsync(
        int packageId,
        [AsParameters] PackageServices services,
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize)
    {
        var pagination = new PaginationRequest(page, pageSize);

        var pagedResult = await services.Queries
            .GetDataFlagsAsync(
                packageId: packageId,
                packageEventId: 0,
                packageEventHouseholdId: 0,
                searchTerm,
                pagination);

        var response = new PagedApiResponse<DataFlagForListDto>(
            Value: pagedResult.Items,
            RecordCount: pagedResult.TotalCount,
            Pagination: new PaginationMeta(
                TotalItems: pagedResult.TotalCount,
                PageIndex: pagination.Page - 1,      // Angular = 0-based
                PageSize: pagination.PageSize,
                TotalPages: pagedResult.TotalPages
            )
        );

        return TypedResults.Ok(response);
    }

    public static async Task<Ok<PagedApiResponse<DataFlagForListDto>>> GetHouseholdDataFlagsAsync(
        int packageId,
        int packageEventId,
        int packageEventHouseholdId,
        [AsParameters] PackageServices services,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize)
    {
        var pagination = new PaginationRequest(page, pageSize);

        var pagedResult = await services.Queries
            .GetDataFlagsAsync(
                packageId,
                packageEventId,
                packageEventHouseholdId,
                string.Empty,
                pagination);

        var response = new PagedApiResponse<DataFlagForListDto>(
            Value: pagedResult.Items,
            RecordCount: pagedResult.TotalCount,
            Pagination: new PaginationMeta(
                TotalItems: pagedResult.TotalCount,
                PageIndex: pagination.Page - 1,      // Angular = 0-based
                PageSize: pagination.PageSize,
                TotalPages: pagedResult.TotalPages
            )
        );

        return TypedResults.Ok(response);
    }

    public static async Task<Ok<PagedApiResponse<ExternalSubmissionDto>>> GetPackageEventDataFlagSubmissionsAsync(
        int packageEventId,
        [AsParameters] PackageServices services,
        [FromQuery] int packageEventDataFlagId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize)
    {
        var pagination = new PaginationRequest(page, pageSize);

        var pagedResult = await services.SubmissionQueries
            .GetExternalSubmissionsForDataFlagAsync(
                packageEventDataFlagId,
                pagination);

        var response = new PagedApiResponse<ExternalSubmissionDto>(
            Value: pagedResult.Items,
            RecordCount: pagedResult.TotalCount,
            Pagination: new PaginationMeta(
                TotalItems: pagedResult.TotalCount,
                PageIndex: pagination.Page - 1,      // Angular = 0-based
                PageSize: pagination.PageSize,
                TotalPages: pagedResult.TotalPages
            )
        );

        return TypedResults.Ok(response);
    }

    public static async Task<Ok<IEnumerable<PackageEventHouseholdDto>>> GetCollectionPackageHouseholdsAsync(int packageEventId, [AsParameters] PackageServices services)
    {
        var households = await services.Queries.GetCollectionPackageHouseholdsAsync(packageEventId);
        return TypedResults.Ok(households);
    }

    public static async Task<Ok<IEnumerable<PackageEventHouseholdDto>>> GetCommunityValidationPackageHouseholdsAsync(int packageEventId, [AsParameters] PackageServices services)
    {
        var households = await services.Queries.GetCommunityValidationPackageHouseholdsAsync(packageEventId);
        return TypedResults.Ok(households);
    }

    public static async Task<Results<Ok<int>, BadRequest<string>>> SavePayloadAsync(
        SavePayloadCommand request,
        [AsParameters] PackageServices services)
    {
            services.Logger.LogInformation(
            "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            request.GetGenericTypeName(),
            nameof(request.HouseholdId),
            request.HouseholdId,
            request);

        var commandResult = await services.Mediator.Send(request);

        if (commandResult.Status == false)
        {
            //return TypedResults.Problem(detail: "Cancel package failed to process.", statusCode: 500);
            // SK TODO Handle error code
            return TypedResults.BadRequest($"Command not created: {commandResult.StatusMessage}");
        }

        return TypedResults.Ok(commandResult.RecordId);
    }

    public static async Task<Results<Ok<int>, BadRequest<string>>> AddDataFlagAsync(
        AddDataFlagCommand request,
        [AsParameters] PackageServices services)
    {
        services.Logger.LogInformation(
        "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
        request.GetGenericTypeName(),
        nameof(request.PackageEventHouseholdId),
        request.PackageEventHouseholdId,
        request);

        var commandResult = await services.Mediator.Send(request);

        if (commandResult.Status == false)
        {
            //return TypedResults.Problem(detail: "Cancel package failed to process.", statusCode: 500);
            // SK TODO Handle error code
            return TypedResults.BadRequest($"Command not created: {commandResult.StatusMessage}");
        }

        return TypedResults.Ok(commandResult.RecordId);
    }

    public static async Task<Results<Ok<bool>, BadRequest<string>>> ProcessPayloadsAsync(int packageEventId, 
        [AsParameters] PackageServices services)
    {
        ProcessPayloadsCommand request = new(packageEventId);

        services.Logger.LogInformation(
        "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
        request.GetGenericTypeName(),
        nameof(request.PackageEventId),
        request.PackageEventId,
        request);

        var commandResult = await services.Mediator.Send(request);

        if (!commandResult)
        {
            //return TypedResults.Problem(detail: "Cancel package failed to process.", statusCode: 500);
            // SK TODO Handle error code
            return TypedResults.BadRequest("Command not created");
        }

        return TypedResults.Ok(commandResult);
    }

    public static async Task<Results<Ok<bool>, BadRequest<string>>> ProcessAllPayloadsAsync(int packageId,
        [AsParameters] PackageServices services)
    {
        ProcessAllPayloadsCommand request = new(packageId);

        services.Logger.LogInformation(
        "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
        request.GetGenericTypeName(),
        nameof(request.PackageId),
        request.PackageId,
        request);

        var commandResult = await services.Mediator.Send(request);

        if (!commandResult)
        {
            //return TypedResults.Problem(detail: "Cancel package failed to process.", statusCode: 500);
            // SK TODO Handle error code
            return TypedResults.BadRequest("Command not created");
        }

        return TypedResults.Ok(commandResult);
    }

    public static async Task<Results<Ok<bool>, BadRequest<string>>> ChangeStatusToDataCollectionAsync(int packageEventId,
        [AsParameters] PackageServices services)
    {
        ChangeEventStatusToDataCollectionCommand request = new(packageEventId);

        services.Logger.LogInformation(
        "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
        request.GetGenericTypeName(),
        nameof(request.PackageEventId),
        request.PackageEventId,
        request);

        var commandResult = await services.Mediator.Send(request);

        if (!commandResult)
        {
            //return TypedResults.Problem(detail: "Cancel package failed to process.", statusCode: 500);
            // SK TODO Handle error code
            return TypedResults.BadRequest("Command not created");
        }

        return TypedResults.Ok(commandResult);
    }

    public static async Task<Results<Ok<bool>, BadRequest<string>>> ChangeStatusToAcceptedAsync(int packageEventId,
        [AsParameters] PackageServices services)
    {
        ChangeEventStatusToAcceptedCommand request = new(packageEventId);

        services.Logger.LogInformation(
        "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
        request.GetGenericTypeName(),
        nameof(request.PackageEventId),
        request.PackageEventId,
        request);

        var commandResult = await services.Mediator.Send(request);

        if (!commandResult)
        {
            //return TypedResults.Problem(detail: "Cancel package failed to process.", statusCode: 500);
            // SK TODO Handle error code
            return TypedResults.BadRequest("Command not created");
        }

        return TypedResults.Ok(commandResult);
    }

    public static async Task<Results<Ok<bool>, BadRequest<string>>> ChangeStatusToGatewayAsync(int packageEventId,
        [AsParameters] PackageServices services)
    {
        ChangeEventStatusToGatewayCommand request = new(packageEventId);

        services.Logger.LogInformation(
        "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
        request.GetGenericTypeName(),
        nameof(request.PackageEventId),
        request.PackageEventId,
        request);

        var commandResult = await services.Mediator.Send(request);

        if (!commandResult)
        {
            //return TypedResults.Problem(detail: "Cancel package failed to process.", statusCode: 500);
            // SK TODO Handle error code
            return TypedResults.BadRequest("Command not created");
        }

        return TypedResults.Ok(commandResult);
    }

    public static async Task<Results<Ok<bool>, BadRequest<string>>> ChangeHouseholdStatusToAcceptedAsync(int packageEventId, int packageEventHouseholdId,
        [AsParameters] PackageServices services)
    {
        ChangeHouseholdStatusToAcceptedCommand request = new(packageEventId, packageEventHouseholdId);

        services.Logger.LogInformation(
        "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
        request.GetGenericTypeName(),
        nameof(request.PackageEventId),
        request.PackageEventId,
        request);

        var commandResult = await services.Mediator.Send(request);

        if (!commandResult)
        {
            //return TypedResults.Problem(detail: "Cancel package failed to process.", statusCode: 500);
            // SK TODO Handle error code
            return TypedResults.BadRequest("Command not created");
        }

        return TypedResults.Ok(commandResult);
    }

    public static async Task<Results<Ok<bool>, BadRequest<string>>> ChangeHouseholdStatusToRejectedAsync(int packageEventId, int packageEventHouseholdId,
        [AsParameters] PackageServices services)
    {
        ChangeHouseholdStatusToRejectedCommand request = new(packageEventId, packageEventHouseholdId);

        services.Logger.LogInformation(
        "Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
        request.GetGenericTypeName(),
        nameof(request.PackageEventId),
        request.PackageEventId,
        request);

        var commandResult = await services.Mediator.Send(request);

        if (!commandResult)
        {
            //return TypedResults.Problem(detail: "Cancel package failed to process.", statusCode: 500);
            // SK TODO Handle error code
            return TypedResults.BadRequest("Command not created");
        }

        return TypedResults.Ok(commandResult);
    }

}


