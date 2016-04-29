using System.Collections.Generic;
using Commands;
using EventSourcing;

namespace ValuationService.Domain
{
    public class UpdateCustomerHandler
    {
        public static PublisherSubscriptions Subscriptions()
        {
            return new PublisherBuilder<Customer>()
                .When<Placed<UpdateCustomerCommand>>()
                .Correlate(x => x.Command.CustomerId, x => x.Id)
                .Then(Handle);
        }

        public static IEnumerable<IDomainEvent> Handle(Customer d, Placed<UpdateCustomerCommand> e)
        {
            return new[]
            {
                new CustomerChangedEvent
                {
                    CustomerId = e.Command.CustomerId,
                    Name = e.Command.CustomerName
                }
            };
        }
    }
}