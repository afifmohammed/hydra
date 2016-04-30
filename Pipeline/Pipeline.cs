using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EventSourcing;
using Requests;

namespace RequestPipeline
{
    public delegate Response<Unit> Dispatch<in TCommand>(TCommand command) where TCommand : IRequest<Unit>, ICorrelated;
    public delegate Response<TReturn> Dispatch<TRequest, TReturn>(TRequest request) where TRequest : IRequest<TReturn>, ICorrelated where TReturn : class;

    public static class RequestPipeline<TRequest, TResult>
        where TRequest : IRequest<TResult>, ICorrelated
        where TResult : class
    {
        public static Dispatch<TRequest, TResult> Dispatch = input =>
            RequestThroughPipeline(
                input,
                request => Request<bool>.By(new Authenticate<TRequest, TResult> { Request = request }).All(x => x),
                request => Request<bool>.By(new Authorise<TRequest, TResult> { Request = request }).All(x => x),
                request => Request<IEnumerable<KeyValuePair<string, string>>>.By(new Validate<TRequest, TResult> { Request = request }).SelectMany(x => x),
                request => Request<TResult>.By(request));

        public static Response<TResult> RequestThroughPipeline(
            TRequest request,
            Func<TRequest, bool> authenticate,
            Func<TRequest, bool> authorise,
            Func<TRequest, IEnumerable<KeyValuePair<string, string>>> validator,
            Func<TRequest, IEnumerable<TResult>> dispatch) 
        {
            if (!authenticate(request))
                return new Response<TResult>
                {
                    Correlations = request.Correlations,
                    Status = HttpStatusCode.Unauthorized                    
                };

            if (!authorise(request))
                return new Response<TResult>
                {
                    Correlations = request.Correlations,
                    Status = HttpStatusCode.Forbidden
                };

            var response = new Response<TResult>
            {
                Correlations = request.Correlations,
                ValidationMessages = validator(request)
            }.With(x =>
                x.Status = x.ValidationMessages.Any()
                    ? HttpStatusCode.BadRequest
                    : HttpStatusCode.Accepted);

            if (response.Status == HttpStatusCode.Accepted)
                return response.With(x => x.Result = dispatch(request));

            return response;
        }
    }

    public static class RequestPipeline<TCommand, TEventStoreEndpoint, TTransportEndpoint> 
        where TCommand : IRequest<Unit>, ICorrelated
        where TEventStoreEndpoint : class
        where  TTransportEndpoint : class
    {
        public static Dispatch<TCommand> Dispatch = input => 
            DispatchToPipeline(
                input,
                cmd => Request<bool>.By(new Authenticate<TCommand, Unit> { Request = cmd }).All(x => x),
                cmd => Request<bool>.By(new Authorise<TCommand, Unit> { Request = cmd }).All(x => x),
                cmd => Request<IEnumerable<KeyValuePair<string, string>>>.By(new Validate<TCommand, Unit> { Request = cmd }).SelectMany(x => x),
                cmd => Mailbox<TEventStoreEndpoint, TTransportEndpoint>.Notify(new Placed<TCommand> {Command = cmd}));

        public static Response<Unit> DispatchToPipeline(
            TCommand command, 
            Func<TCommand, bool> authenticate,
            Func<TCommand, bool> authorise,
            Func<TCommand, IEnumerable<KeyValuePair<string, string>>> validator,
            Action<TCommand> dispatch)
        {
            if (!authenticate(command))
                return new Response<Unit>
                {
                    Correlations = command.Correlations,
                    Status = HttpStatusCode.Unauthorized
                };

            if(!authorise(command))
                return new Response<Unit>
                {
                    Correlations = command.Correlations,
                    Status = HttpStatusCode.Forbidden
                };

            var response = new Response<Unit>
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