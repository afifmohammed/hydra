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
        }

        public class StateEventB : IDomainEvent
        {
            public string StateEventBIdM { get; set; }
            public int StateEventBIdN { get; set; }
            public int StateEventBSomeCounter { get; set; }
        }

        public class TriggerEventA : IDomainEvent
        {
            public string TriggerEventAIdM { get; set; }
            public int TriggerEventAIdN { get; set; }
            public int TriggerEventASomeMinLimit { get; set; }
        }

        public class TriggerEventB : IDomainEvent
        {
            public string TriggerEventBIdM { get; set; }
            public int TriggerEventBIdN { get; set; }
            public int TriggerEventBSomeMaxLimit { get; set; }
        }

        public class DecisionEventX : IDomainEvent
        {
            public string DecisionEventXIdM { get; set; }
            public int DecisionEventXIdN { get; set; }
            public string DecisionEventXIdO { get; set; }
        }

        public class DecisionEventY : IDomainEvent
        {
            public string DecisionEventYIdM { get; set; }
            public int DecisionEventYIdN { get; set; }
            public string DecisionEventYIdO { get; set; }
        }

        public struct TakeDecisionX
        {
            public string IdM { get; set; }
            public int IdN { get; set; }
        }

        public static class TakeDecisionXHandler
        {
            public static DecisionEventX OnADoX(TakeDecisionX x, TriggerEventA a)
            {
                return new DecisionEventX();
            }

            public static DecisionEventY OnADoY(TakeDecisionX x, TriggerEventA a)
            {
                return new DecisionEventY();
            }

            public static DecisionEventX OnBDoX(TakeDecisionX x, TriggerEventB b)
            {
                return new DecisionEventX();
            }
        }

        [Fact]
        public void WorksOutofTheBox()
        {
            new UseCase<TakeDecisionX>()
                .Given<StateEventA>(e => d => { })
                    .Correlate(x => x.StateEventAIdM, x => x.IdM)
                .Given<StateEventB>(e => d => { })
                    .Correlate(x => x.StateEventBIdM, x => x.IdM)
                    .Correlate(x => x.StateEventBIdN, x => x.IdN)
                .When<TriggerEventA>(e => d => { })
                    .Correlate(x => x.TriggerEventAIdM, x => x.IdM)
                    .Correlate(x => x.TriggerEventAIdN, x => x.IdN)
                    .Then(TakeDecisionXHandler.OnADoX)
                        .Correlation(x => x.DecisionEventXIdM)
                        .Correlation(x => x.DecisionEventXIdN)
                        .Correlation(x => x.DecisionEventXIdO)
                    .Then(TakeDecisionXHandler.OnADoY)
                        .Correlation(x => x.DecisionEventYIdM)
                        .Correlation(x => x.DecisionEventYIdN)
                        .Correlation(x => x.DecisionEventYIdO)
                .When<TriggerEventB>(e => d => { })
                    .Correlate(x => x.TriggerEventBIdM, x => x.IdM)
                    .Correlate(x => x.TriggerEventBIdN, x => x.IdN)
                    .Then(TakeDecisionXHandler.OnBDoX); 
                    // notice we didn't specify the corelations for DecisionEventX 
                    // as they have already been specified above for OnA
        }
    }
}
