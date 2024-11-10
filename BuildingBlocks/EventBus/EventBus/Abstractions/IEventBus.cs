namespace Staging.BuildingBlocks.EventBus.EventBus.Abstractions;

public interface IEventBus
{
    Task PublishAsync(IntegrationEvent @event);
}
