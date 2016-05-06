using Nancy;
using RetailDomain.Inventory;

namespace WebApi.Modules
{
    public class InventoryModule : NancyModule
    {
        public InventoryModule()
        {
            Get["/inventory/{id}"] = _ => "not available";

            Put["/inventory/{id}/create"] = store => ApplicationRequestPipeline.Dispatch(new CreateInventoryItem { Id = store["id"] });

            Put["/inventory/{id}/deactivate"] = store => ApplicationRequestPipeline.Dispatch(new DeactivateInventoryItem { Id = store["id"] });
        }
    }
}