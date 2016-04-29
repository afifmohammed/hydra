using AdoNet;
using Nancy;
using ValuationService.Domain;

namespace ValuationService.Modules
{
    public class ValuationModule : NancyModule
    {
        public ValuationModule()
        {
            Put["/valuation/{loanId}/{customerId}/create"] = dictionary => 
            Commands.Channel<RequestValuationCommand, AdoNetTransaction<ApplicationStore>, AdoNetTransactionScope>.Dispatch(new RequestValuationCommand
            {
                LoanId = dictionary["loanId"],
                CustomerId = dictionary["customerId"]
            });
        }
    }
}
