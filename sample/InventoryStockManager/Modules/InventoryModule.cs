using Nancy;

namespace InventoryStockManager.Modules
{
    public class InventoryModule : NancyModule
    {
        public InventoryModule()
        {
            Get["/inventory/{id}"] = _ => "not available";
        }
    }
}