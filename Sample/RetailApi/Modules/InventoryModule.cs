using Hydra.Subscriptions;
using Nancy;
using RetailDomain.Inventory;

namespace WebApi.Modules
{
    public class InventoryModule : NancyModule
    {
        public InventoryModule()
        {
            Get["/inventory/{id}"] = _ => "not available";

            Put["/inventory/{id}/create"] = store => SubscriberPipeline.Dispatch(new CreateInventoryItem { Id = store["id"] });

            Put["/inventory/{id}/deactivate"] = store => SubscriberPipeline.Dispatch(new DeactivateInventoryItem { Id = store["id"] });
        }
    }
}