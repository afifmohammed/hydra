using System;
using System.Collections.Generic;
using System.Linq;
using Hydra.Requests;
using Xunit;

namespace Tests
{
    public class RequestsDriveByDemo
    {
        class InMemoryListProvider<T> : IDisposable
        {
            public InMemoryListProvider(List<T> list)
            {
                List = list;
            }

            public List<T> List { get; private set; }

            public void Dispose()
            {
                List = null;
            }
        }

        class WordsStartingWith
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

        static IEnumerable<string> ByLength(WordsOfGivenLength query, InMemoryListProvider<string> inMemoryListProvider)
        {
            return inMemoryListProvider.List.Where(x => x.Length == query.Length);
        }

        static IEnumerable<string> ByPrefix(WordsStartingWith query, InMemoryListProvider<string> inMemoryListProvider)
        {
            return inMemoryListProvider.List.Where(x => x.StartsWith(query.Prefix));
        }

        static Total TotalByLength(TotalWordsForGivenLength query, InMemoryListProvider<string> inMemoryListProvider)
        {
            return new Total(ByLength(new WordsOfGivenLength { Length = query.Length }, inMemoryListProvider).Count());
        }

        [Fact]
        public void WorksOutOfTheBox()
        {
            var list = new[] { "gone", "gost", "goose", "guava" }.ToList();
            var anotherList = new[] { "great", "gofer" }.ToList();

            new RequestsRegistration<InMemoryListProvider<string>>(() => new InMemoryListProvider<string>(list))
                .Register<WordsStartingWith, string>(ByPrefix)
                .Register<WordsOfGivenLength, string>(ByLength);

            new RequestsRegistration<InMemoryListProvider<string>>(() => new InMemoryListProvider<string>(anotherList))
                .Register<WordsStartingWith, string>(ByPrefix)
                .Register<TotalWordsForGivenLength, Total>(TotalByLength, Return.List);

            Assert.Equal(
                new[] { "gone", "gost", "goose", "gofer" }
                    .OrderBy(x => x)
                    .ToList(),
                Request<string>.By(new WordsStartingWith { Prefix = "go" })
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
