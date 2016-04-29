using System.Collections.Generic;
using Commands;
using EventSourcing;

namespace ValuationService.Domain
{
    public class RequestValuationHandler
    {
        public static PublisherSubscriptions Subscriptions()
        {
            return new PublisherBuilder<Valuation>()
                .When<Placed<RequestValuationCommand>>()
                .Correlate(x => x.Command.LoanId, x => x.LoanId)
                .Correlate(x=>x.Command.CustomerId, x=>x.CustomerId)
                .Then(Handle);
        }

        public static IEnumerable<IDomainEvent> Handle(Valuation d, Placed<RequestValuationCommand> e)
        {
            return new[]
            {
                new ValuationRequestedEvent
                {
                    LoanId = e.Command.LoanId,
                    CustomerId = e.Command.CustomerId
                }
            };
        }
    }
}