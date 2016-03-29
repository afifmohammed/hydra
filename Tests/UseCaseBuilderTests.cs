using System;
using System.Collections.Generic;
using EventSourcing;
using Xunit;

namespace Tests
{
    public class UseCaseBuilderTests
    {
        public class StateEventA : IDomainEvent
        {
            public string StateEventAIdM { get; set; }
            public int StateEventASomeCounter { get; set; }

            public IEnumerable<KeyValuePair<string, object>> Correlations
            {
                get
                {
                    yield return this.PropertyNameValue(x => x.StateEventAIdM);
                }
            }
        }

        public class StateEventB : IDomainEvent
        {
            public string StateEventBIdM { get; set; }
            public int StateEventBIdN { get; set; }
            public int StateEventBSomeCounter { get; set; }

            public IEnumerable<KeyValuePair<string, object>> Correlations
            {
                get
                {
                    yield return this.PropertyNameValue(x => x.StateEventBIdM);
                    yield return this.PropertyNameValue(x => x.StateEventBIdN);
                }
            }
        }

        public class TriggerEventA : IDomainEvent
        {
            public string TriggerEventAIdM { get; set; }
            public int TriggerEventAIdN { get; set; }
            public int TriggerEventASomeMinLimit { get; set; }

            public IEnumerable<KeyValuePair<string, object>> Correlations
            {
                get
                {
                    yield return this.PropertyNameValue(x => x.TriggerEventAIdM);
                    yield return this.PropertyNameValue(x => x.TriggerEventAIdN);
                }
            }
        }

        public class TriggerEventB : IDomainEvent
        {
            public string TriggerEventBIdM { get; set; }
            public int TriggerEventBIdN { get; set; }
            public int TriggerEventBSomeMaxLimit { get; set; }

            public IEnumerable<KeyValuePair<string, object>> Correlations
            {
                get
                {
                    yield return this.PropertyNameValue(x => x.TriggerEventBIdM);
                    yield return this.PropertyNameValue(x => x.TriggerEventBIdN);
                }
            }
        }

        public class DecisionEventX : IDomainEvent
        {
            public IEnumerable<KeyValuePair<string, object>> Correlations
            {
                get
                {
                    yield return this.PropertyNameValue(x => x.DecisionEventXIdM);
                    yield return this.PropertyNameValue(x => x.DecisionEventXIdN);
                }
            }

            public string DecisionEventXIdM { get; set; }
            public int DecisionEventXIdN { get; set; }
            public string DecisionEventXIdO { get; set; }
        }

        public struct TakeDecisionX
        {
            public string IdM { get; set; }
            public int IdN { get; set; }
        }

        public static class TakeDecisionXHandler
        {
            public static IEnumerable<IDomainEvent> OnA(TakeDecisionX x, TriggerEventA a)
            {
                yield return new DecisionEventX();
            }            

            public static IEnumerable<IDomainEvent> OnB(TakeDecisionX x, TriggerEventB b)
            {
                yield return new DecisionEventX();
            }
        }

        [Fact]
        public void WorksOutofTheBox()
        {
            var publisher = new UseCase<TakeDecisionX>()
                .Given<StateEventA>(e => d => { /* todo: map e to d */ })
                    .Correlate(x => x.StateEventAIdM, x => x.IdM)
                .Given<StateEventB>(e => d => { /* todo: map e to d */ })
                    .Correlate(x => x.StateEventBIdM, x => x.IdM)
                    .Correlate(x => x.StateEventBIdN, x => x.IdN)
                .When<TriggerEventA>(e => d => { /* todo: map e to d */ })
                    .Correlate(x => x.TriggerEventAIdM, x => x.IdM)
                    .Correlate(x => x.TriggerEventAIdN, x => x.IdN)
                    .Then(TakeDecisionXHandler.OnA)                                                                    
                .When<TriggerEventB>(e => d => { /* todo: map e to d */ })
                    .Correlate(x => x.TriggerEventBIdM, x => x.IdM)
                    .Correlate(x => x.TriggerEventBIdN, x => x.IdN)
                    .Then(TakeDecisionXHandler.OnB)
                .Build();                     
        }
    }
}
