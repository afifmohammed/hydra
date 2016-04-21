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

            Put["/inventory/{id}/deactivate"] = id => Commands.Channel<DeactivateInventoryItem, AdoNetTransaction<ApplicationStore>, TransactionScope>
                    .Dispatch(new DeactivateInventoryItem {Id = id});
        }
    }
}