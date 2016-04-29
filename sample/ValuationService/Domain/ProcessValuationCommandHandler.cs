using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using EventSourcing;
using ValuationService.ValexService;

namespace ValuationService.Domain
{
    public class ProcessValuationHandler
    {
        public static PublisherSubscriptions Subscriptions()
        {
            return new PublisherBuilder<Valuation>()
                .Given<CustomerChangedEvent>(Map)
                .Correlate(x => x.CustomerId, y => y.CustomerId)
                .When<ValuationRequestedEvent>()
                .Correlate(x => x.LoanId, x => x.LoanId)
                .Then(Handle);
        }

        public static IEnumerable<IDomainEvent> Handle(Valuation d, ValuationRequestedEvent e)
        {
            if (!d.IsPopulated())
                throw new InvalidDataException() ; //Force a retry as all the given's have not been satisfied yet

            var addressHeader = AddressHeader.CreateAddressHeader("AuthHeader", "https://ws.valex.com.au/soap/lixi/1.3/",
                new AuthHeader
                {
                    UserName = "soap_liberty",
                    Password = "f*SGp4LV+9"
                });

            var endpoint = new EndpointAddress(new Uri("https://vxtest.valex.com.au/soap/lixi/1.3.1/service.php?n=client"), addressHeader);
            Binding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            {
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferSize = int.MaxValue
            };

            var client = new VXWS_ClientPortTypeClient(binding, endpoint);

            var doc = new XmlDocument();
            doc.Load("ValexPayload.xml");

            var response = client.valuationRequest(doc.InnerXml, true);

            if (response == 0)
                return new[] {new ValuationProcessRequestSentEvent {LoanId = e.LoanId}};

            return new[] {new ValuationProcessRequestRejectedEvent {LoanId = e.LoanId}};
        }

        public static Valuation Map(CustomerChangedEvent e, Valuation d)
        {
            d.CustomerName = e.Name;

            return d;
        }
    }
}