using System;
using System.Collections.Generic;
using Commands;
using EventSourcing;

namespace ValuationService.Domain
{
    public struct ValuationData
    {
        public Guid LoanId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public ValuationTypes ValuationType { get; set; }
        public ValuationReasons ValuationReason { get; set; }
        public string PropertyType { get; set; }
        public EstimatedMarketTypes EstimatedMarketType { get; set; }
        public string Notes { get; set; }
    }
    
    public class ValuationCommandHandler
    {
        public static PublisherSubscriptions Subscriptions()
        {
            return new PublisherBuilder<ValuationData>()
                //.Given<CustomerNameChangedEvent>(Map)
                //.Given<CustomerAddressChangedEvent>(Map)
                .Given<LoanApplicationCreatedEvent>(Map)
                .Correlate(x => x.LoanId, y => y.LoanId)
                .When<Received<CreateValuationCommand>>()
                .Correlate(x => x.Command.LoanId, x => x.LoanId)
                .Then(Handle);
        }

        public static IEnumerable<IDomainEvent> Handle(ValuationData d, Received<CreateValuationCommand> e)
        {
            return new[] { new ValuationRequestPublishedEvent { LoanId = e.Command.LoanId } };
        }

        public static ValuationData Map(CustomerNameChangedEvent e, ValuationData d)
        {
            return new ValuationData
            {
                LoanId = d.LoanId,
                CustomerName = e.Name,
                CustomerAddress = d.CustomerAddress,
                ValuationType = d.ValuationType,
                ValuationReason = d.ValuationReason,
                PropertyType = d.PropertyType,
                EstimatedMarketType = d.EstimatedMarketType,
                Notes = d.Notes
            };
        }

        public static ValuationData Map(CustomerAddressChangedEvent e, ValuationData d)
        {
            return new ValuationData
            {
                LoanId = d.LoanId,
                CustomerName = d.CustomerName,
                CustomerAddress = e.Address,
                ValuationType = d.ValuationType,
                ValuationReason = d.ValuationReason,
                PropertyType = d.PropertyType,
                EstimatedMarketType = d.EstimatedMarketType,
                Notes = d.Notes
            };
        }

        public static ValuationData Map(LoanApplicationCreatedEvent e, ValuationData d)
        {
            return new ValuationData
            {
                LoanId = e.LoanId,
                CustomerName = d.CustomerName,
                CustomerAddress = d.CustomerAddress,
                ValuationType = d.ValuationType,
                ValuationReason = d.ValuationReason,
                PropertyType = d.PropertyType,
                EstimatedMarketType = d.EstimatedMarketType,
                Notes = d.Notes
            };
        }
    }
}