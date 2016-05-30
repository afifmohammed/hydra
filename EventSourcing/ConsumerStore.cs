using System;
using Hydra.Core;

namespace Hydra.Subscribers
{
    delegate void Handler<TProvider>(
        SubscriberMessage messageToConsumer,
        ProjectorsBySubscription<TProvider> projectorsBySubscription,
        NotificationsByCorrelations notificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TProvider provider)
        where TProvider : IProvider;

    public static class ExporterStore<TEventStoreProvider, TExportProvider> 
        where TExportProvider : IProvider
        where TEventStoreProvider : IProvider
    {
        public static CommitWork<TExportProvider> CommitExportProvider { get; set; }
        public static CommitWork<TEventStoreProvider> CommitEventStoreProvider { get; set; }
        public static NotificationsByCorrelationsFunction<TEventStoreProvider> NotificationsByCorrelationsFunction { get; set; }

        public static Func<ProjectorsBySubscription<TExportProvider>, Subscriber> Subscriber = 
            exportersBySubscription => 
                message => 
                    HandleAndCommit
                    (
                        message,
                        exportersBySubscription,
                        Handle,
                        NotificationsByCorrelationsFunction,
                        CommitExportProvider,
                        CommitEventStoreProvider,
                        () => DateTimeOffset.Now
                    );

        internal static void HandleAndCommit(
            SubscriberMessage message,
            ProjectorsBySubscription<TExportProvider> projectorsBySubscription,
            Handler<TExportProvider> handler,
            NotificationsByCorrelationsFunction<TEventStoreProvider> notificationsByCorrelationsFunction,
            CommitWork<TExportProvider> commitExportProvider,
            CommitWork<TEventStoreProvider> commitEventStoreProvider,
            Func<DateTimeOffset> clock)
        {
            commitExportProvider
            (
                exportProvider => commitEventStoreProvider
                (
                    eventStoreProvider => handler
                    (
                        message,
                        projectorsBySubscription,
                        notificationsByCorrelationsFunction(eventStoreProvider),
                        clock,
                        exportProvider
                    )
                )
            );
        }

        internal static void Handle(
            SubscriberMessage message,
            ProjectorsBySubscription<TExportProvider> projectorsBySubscription,
            NotificationsByCorrelations notificationsByCorrelations,
            Func<DateTimeOffset> clock,
            TExportProvider integrationProvider)
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