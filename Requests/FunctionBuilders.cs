﻿using System;
using System.Collections.Generic;

namespace Hydra.Requests
{
    static class Function
    {
        public static KeyValuePair<FunctionContract, object> ToKvp<TInput, TResult, TDependency>(
            Func<TInput, TDependency, IEnumerable<TResult>> function,
            Func<Func<TInput, TDependency, IEnumerable<TResult>>, Func<TInput, IEnumerable<TResult>>> provideDependency)
        {
            return new KeyValuePair<FunctionContract, object>(
                new FunctionContract(typeof(TInput).Contract(), typeof(TResult).Contract()),
                Downcast(provideDependency(function)));
        }

        public static KeyValuePair<FunctionContract, object> ToKvp<TInput, TResult>(
            Func<TInput, IEnumerable<TResult>> function)
        {
            return new KeyValuePair<FunctionContract, object>(
                new FunctionContract(typeof(TInput).Contract(), typeof(TResult).Contract()),
                Downcast(function));                
        }

        static Func<object, IEnumerable<TResult>> Downcast<TInput, TResult>(
            Func<TInput, IEnumerable<TResult>> query) 
        {
            return c => query((TInput)c);                
        }        
    }
}
