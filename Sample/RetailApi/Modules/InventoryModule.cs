using Nancy;
using RetailDomain.Inventory;

namespace WebApi.Modules
{
    public class InventoryModule : NancyModule
    {
        public InventoryModule()
        {
            Get["/inventory/{id}"] = _ => "not available";

            Put["/inventory/{id}/create"] = store => ApplicationSubscriptionDispatcher.Dispatch(new CreateInventoryItem { Id = store["id"] });

            Put["/inventory/{id}/deactivate"] = store => ApplicationSubscriptionDispatcher.Dispatch(new DeactivateInventoryItem { Id = store["id"] });
        }
    }
}