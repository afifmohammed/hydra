﻿using System.Collections.Generic;
using System.Linq;
using EventSourcing;
using Requests;

namespace RequestPipeline
{
    public static class Request
    {
        public static IEnumerable<TResult> For<TResult>(IRequest<TResult> request) where TResult : class
        {
            return Request<TResult>.By(request);
        }
    }

    public static class RequestPipeline<TEndpointConnection> where TEndpointConnection : EndpointConnection
    {
        public static Response<Unit> Dispatch<TRequest>(TRequest input) where TRequest : IRequest<Unit>, ICorrelated => 
            RequestPipeline<TRequest, Unit>.DispatchThroughPipeline(
                input,
                request =>
                {
                    PostBox<TEndpointConnection>.Drop(new Placed<TRequest> {Command = request});
                    return Enumerable.Empty<Unit>();
                });
    }
}