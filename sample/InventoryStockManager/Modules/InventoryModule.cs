using System.Transactions;
using AdoNet;
using InventoryStockManager.Domain;
using Nancy;

namespace InventoryStockManager.Modules
{
    public class InventoryModule : NancyModule
    {
        public InventoryModule()
        {
            Get["/inventory/{id}"] = _ => "not available";

            Put["/inventory/{id}/create"] = store => RequestPipeline.RequestPipeline<CreateInventoryItem, AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>
                    .Dispatch(new CreateInventoryItem { Id = store["id"] });

            Put["/inventory/{id}/deactivate"] = store => RequestPipeline.RequestPipeline<DeactivateInventoryItem, AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>
                    .Dispatch(new DeactivateInventoryItem { Id = store["id"] });
        }
    }
}