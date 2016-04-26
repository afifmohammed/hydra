﻿using System;
using System.Collections.Generic;
using System.Linq;
using Requests;
using Xunit;

namespace Tests
{
    public class DriveByDemo
    {
        class Connection<T> : IDisposable
        {
            public Connection(List<T> list)
            {
                List = list;
            }

            public List<T> List { get; private set; }

            public void Dispose()
            {
                List = null;
            }
        }

        class WordsStartingWith : IRequest<string>
        {
            public string Prefix;
        }

        class WordsOfGivenLength
        {
            public int Length { get; set; }
        }

        class Total
        {
            public Total(int value)
            {
                Value = value;
            }
            public readonly int Value;
        }

        class TotalWordsForGivenLength
        {
            public int Length { get; set; }
        }

        static IEnumerable<string> ByLength(WordsOfGivenLength query, Connection<string> connection)
        {
            return connection.List.Where(x => x.Length == query.Length);
        }

        static IEnumerable<string> ByPrefix(WordsStartingWith query, Connection<string> connection)
        {
            return connection.List.Where(x => x.StartsWith(query.Prefix));
        }

        static Total TotalByLength(TotalWordsForGivenLength query, Connection<string> connection)
        {
            return new Total(ByLength(new WordsOfGivenLength { Length = query.Length }, connection).Count());
        }

        [Fact]
        public void WorksOutOfTheBox()
        {
            var list = new[] { "gone", "gost", "goose", "guava" }.ToList();
            var anotherList = new[] { "great", "gofer" }.ToList();

            new RequestsRegistration<Connection<string>>(() => new Connection<string>(list))
                .Register<WordsStartingWith, string>(ByPrefix)
                .Register<WordsOfGivenLength, string>(ByLength);

            new RequestsRegistration<Connection<string>>(() => new Connection<string>(anotherList))
                .Register<WordsStartingWith, string>(ByPrefix)
                .Register<TotalWordsForGivenLength, Total>(TotalByLength, Return.List);

            Assert.Equal(
                new[] { "gone", "gost", "goose", "gofer" }
                    .OrderBy(x => x)
                    .ToList(),
                Request.For(new WordsStartingWith { Prefix = "go" })
                    .OrderBy(x => x)
                    .ToList());

            Assert.Equal(
                new[] { "goose", "guava" }
                    .OrderBy(x => x)
                    .ToList(),
                Request<string>.By(new WordsOfGivenLength { Length = 5 })
                    .OrderBy(x => x)
                    .ToList());

            Assert.Equal(
                2,
                Request<Total>.By(new TotalWordsForGivenLength { Length = 5 })
                    .Single()
                    .Value);
        }
    }
}