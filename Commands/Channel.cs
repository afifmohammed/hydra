using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EventSourcing;
using Requests;

namespace Commands
{
    public delegate Response Dispatch<in TCommand>(TCommand command) where TCommand : ICommand;

    public static class Channel<TCommand, TEventStoreEndpoint, TTransportEndpoint> 
        where TCommand : ICommand
        where TEventStoreEndpoint : class
        where  TTransportEndpoint : class
    {
        public static Dispatch<TCommand> Dispatch = input => 
            DispatchToPipeline(
                input,
                cmd => Request<Unit<bool>>.By(new Authenticate { Command = cmd }).All(x => x.Value),
                cmd => Request<Unit<bool>>.By(new Authorise { Command = cmd }).All(x => x.Value),
                cmd => Request<IEnumerable<KeyValuePair<string, string>>>.By(new Validate { Command = cmd }).SelectMany(x => x),
                cmd => Mailbox<TEventStoreEndpoint, TTransportEndpoint>.Notify(new Received<TCommand> {Command = cmd}));

        public static Response DispatchToPipeline(
            TCommand command, 
            Func<ICommand, bool> authenticate,
            Func<ICommand, bool> authorise,
            Func<ICommand, IEnumerable<KeyValuePair<string, string>>> validator,
            Action<TCommand> dispatch)
        {
            if (!authenticate(command))
                return new Response
                {
                    Correlations = command.Correlations,
                    Status = HttpStatusCode.Unauthorized
                };

            if(!authorise(command))
                return new Response
                {
                    Correlations = command.Correlations,
                    Status = HttpStatusCode.Forbidden
                };

            var response = new Response
            {
                Correlations = command.Correlations,
                ValidationMessages = validator(command)
            }.With(x =>
                x.Status = x.ValidationMessages.Any()
                    ? HttpStatusCode.BadRequest
                    : HttpStatusCode.Accepted);

            if (response.Status == HttpStatusCode.Accepted)
                dispatch(command);

            return response;
        }
    }
}