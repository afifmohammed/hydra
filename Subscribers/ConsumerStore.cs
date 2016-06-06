using System;
using Hydra.Core;

namespace Hydra.Subscribers
{
    delegate void Handler<TUowProvider>(
        SubscriberMessage messageToConsumer,
        ProjectorsBySubscription<TUowProvider> projectorsBySubscription,
        NotificationsByCorrelations notificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TUowProvider provider)
        where TUowProvider : IUowProvider;

    public static class ProjectorStore<TEventStoreUowProvider, TProjectorUowProvider> 
        where TProjectorUowProvider : IUowProvider
        where TEventStoreUowProvider : IUowProvider
    {
        public static CommitWork<TProjectorUowProvider> CommitProjectionProvider { get; set; }
        public static CommitWork<TEventStoreUowProvider> CommitEventStoreProvider { get; set; }
        public static NotificationsByCorrelationsFunction<TEventStoreUowProvider> NotificationsByCorrelationsFunction { get; set; }

        public static Func<ProjectorsBySubscription<TProjectorUowProvider>, Subscriber> Subscriber = 
            projectorsBySubscription => 
                message => 
                    HandleAndCommit
                    (
                        message,
                        projectorsBySubscription,
                        Handle,
                        NotificationsByCorrelationsFunction,
                        CommitProjectionProvider,
                        CommitEventStoreProvider,
                        () => DateTimeOffset.Now
                    );

        internal static void HandleAndCommit(
            SubscriberMessage message,
            ProjectorsBySubscription<TProjectorUowProvider> projectorsBySubscription,
            Handler<TProjectorUowProvider> handler,
            NotificationsByCorrelationsFunction<TEventStoreUowProvider> notificationsByCorrelationsFunction,
            CommitWork<TProjectorUowProvider> commitProjectionProvider,
            CommitWork<TEventStoreUowProvider> commitEventStoreProvider,
            Func<DateTimeOffset> clock)
        {
            commitProjectionProvider
            (
                projectionProvider => commitEventStoreProvider
                (
                    eventStoreProvider => handler
                    (
                        message,
                        projectorsBySubscription,
                        notificationsByCorrelationsFunction(eventStoreProvider),
                        clock,
                        projectionProvider
                    )
                )
            );
        }

        internal static void Handle(
            SubscriberMessage message,
            ProjectorsBySubscription<TProjectorUowProvider> projectorsBySubscription,
            NotificationsByCorrelations notificationsByCorrelations,
            Func<DateTimeOffset> clock,
            TProjectorUowProvider integrationProvider)
        {
            var consumer = projectorsBySubscription[message.Subscription];

            consumer
            (
                message.Event,
                notificationsByCorrelations,
                clock,
                integrationProvider
            );
        }
    }
}