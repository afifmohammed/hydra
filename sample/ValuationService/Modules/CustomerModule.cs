using AdoNet;
using Nancy;
using ValuationService.Domain;

namespace ValuationService.Modules
{
    public class CustomerModule : NancyModule
    {
        public CustomerModule()
        {
            Put["/customer/{id}/{name}/update"] = dictionary =>
                Commands.Channel<UpdateCustomerCommand, AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Dispatch(new UpdateCustomerCommand
                {
                    CustomerId = dictionary["id"],
                    CustomerName = dictionary["name"]
                });
        }
    }
}