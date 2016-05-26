using System;
using System.Collections.Generic;

namespace Hydra.Requests
{
    public sealed class Unit { }

    public interface IRequest<TResult>
    { }

    public class RequestsRegistration<TProvider> where TProvider : IDisposable
    {
        readonly Func<TProvider> _providerFactory;

        public RequestsRegistration(Func<TProvider> providerFactory)
        {
            _providerFactory = providerFactory;
        }

        public RequestsRegistration<TProvider> Register<TInput, TOutput>(
            Func<TInput, TProvider, TOutput> function,
            Func<Func<TInput, TProvider, TOutput>, Func<TInput, TProvider, IEnumerable<TOutput>>> list)
        {
            RequestHandlers.Routes.Add(Function.ToKvp(list(function), ProvideProvider));
            return this;
        }

        public RequestsRegistration<TProvider> Register<TInput, TOutput>(
            Func<TInput, TProvider, IEnumerable<TOutput>> function)
        {
            RequestHandlers.Routes.Add(Function.ToKvp(function, ProvideProvider));
            return this;
        }

        Func<TInput, IEnumerable<TResult>> ProvideProvider<TInput, TResult>(
            Func<TInput, TProvider, IEnumerable<TResult>> query)
        {
            return input =>
            {
                using (var c = _providerFactory())
                    return query(input, c);
            };
        }
    }

    static class RequestHandlers
    {
        public static readonly List<KeyValuePair<FunctionContract, object>> Routes =
            new List<KeyValuePair<FunctionContract, object>>();
    }
}
