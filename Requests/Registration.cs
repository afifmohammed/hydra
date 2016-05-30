using System;
using System.Collections.Generic;

namespace Hydra.Requests
{
    public sealed class Unit { }

    public interface IRequest<TResult>
    { }

    public class RequestsRegistration<TUowProvider> where TUowProvider : IDisposable
    {
        readonly Func<TUowProvider> _providerFactory;

        public RequestsRegistration(Func<TUowProvider> providerFactory)
        {
            _providerFactory = providerFactory;
        }

        public RequestsRegistration<TUowProvider> RegisterRequest<TInput, TOutput>(
            Func<TInput, TUowProvider, TOutput> function,
            Func<Func<TInput, TUowProvider, TOutput>, Func<TInput, TUowProvider, IEnumerable<TOutput>>> list)
            where TInput : IRequest<TOutput>
        {
            Register(function, list);
            return this;
        }

        public RequestsRegistration<TUowProvider> Register<TInput, TOutput>(
            Func<TInput, TUowProvider, TOutput> function,
            Func<Func<TInput, TUowProvider, TOutput>, Func<TInput, TUowProvider, IEnumerable<TOutput>>> list)
        {
            RequestHandlers.Routes.Add(Function.ToKvp(list(function), ProvideProvider));
            return this;
        }

        public RequestsRegistration<TUowProvider> RegisterRequest<TInput, TOutput>(
            Func<TInput, TUowProvider, IEnumerable<TOutput>> function)
            where TInput : IRequest<TOutput>
        {
            Register(function);
            return this;
        }

        public RequestsRegistration<TUowProvider> Register<TInput, TOutput>(
            Func<TInput, TUowProvider, IEnumerable<TOutput>> function)
        {
            RequestHandlers.Routes.Add(Function.ToKvp(function, ProvideProvider));
            return this;
        }

        Func<TInput, IEnumerable<TResult>> ProvideProvider<TInput, TResult>(
            Func<TInput, TUowProvider, IEnumerable<TResult>> query)
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
