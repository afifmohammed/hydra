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

            Put["/inventory/{id}/deactivate"] = store => Commands.Channel<DeactivateInventoryItem, AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>
                    .Dispatch(new DeactivateInventoryItem { Id = store["id"] });
        }
    }
}