using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Hydra.Core;
using Hydra.Requests;

namespace Hydra.RequestPipeline
{
    public delegate Response<TReturn> Dispatch<in TRequest, TReturn>(TRequest request) where TRequest : IRequest<TReturn>, ICorrelated where TReturn : class;

    public static class RequestPipeline<TRequest, TResult>
        where TRequest : IRequest<TResult>, ICorrelated
        where TResult : class
    {
        public static Dispatch<TRequest, TResult> Dispatch = input =>
            DispatchThroughPipeline(
                input,
                request => Request<bool>.By(new Authenticate<TRequest, TResult> { Request = request }).All(x => x),
                request => Request<bool>.By(new Authorise<TRequest, TResult> { Request = request }).All(x => x),
                request => Request<IEnumerable<KeyValuePair<string, string>>>.By(new Validate<TRequest, TResult> { Request = request }).SelectMany(x => x),
                request => Request<TResult>.By(request));

        public static Response<TResult> DispatchThroughPipeline(
            TRequest input,
            Func<TRequest, IEnumerable<TResult>> dispatch)
        {
            return DispatchThroughPipeline(
                input,
                request => Request<bool>.By(new Authenticate<TRequest, TResult> { Request = request }).All(x => x),
                request => Request<bool>.By(new Authorise<TRequest, TResult> { Request = request }).All(x => x),
                request => Request<IEnumerable<KeyValuePair<string, string>>>.By(new Validate<TRequest, TResult> { Request = request }).SelectMany(x => x),
                dispatch);
        }

        public static Response<TResult> DispatchThroughPipeline(
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
}