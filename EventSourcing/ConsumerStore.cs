﻿using System;

namespace EventSourcing
{
    public delegate void Handler<TProvider>(
        SubscriberMessage messageToConsumer,
        ExportersBySubscription<TProvider> exportersBySubscription,
        NotificationsByCorrelations notificationsByCorrelations,
        Func<DateTimeOffset> clock,
        TProvider provider);

    public static class ExporterStore<TExportProvider, TEventStoreProvider> 
        where TExportProvider : IProvider
        where TEventStoreProvider : IProvider
    {
        public static ExportersBySubscription<TExportProvider> ExportersBySubscription { get; set; }
        public static CommitWork<TExportProvider> CommitExportProvider { get; set; }
        public static CommitWork<TEventStoreProvider> CommitEventStoreProvider { get; set; }
        public static NotificationsByCorrelationsFunction<TEventStoreProvider> NotificationsByCorrelationsFunction { get; set; }

        public static Subscriber Subscriber = message => 
            HandleAndCommit
            (
                message,
                ExportersBySubscription,
                Handle,
                NotificationsByCorrelationsFunction,
                CommitExportProvider,
                CommitEventStoreProvider,
                () => DateTimeOffset.Now
            );

        internal static void HandleAndCommit(
            SubscriberMessage message,
            ExportersBySubscription<TExportProvider> exportersBySubscription,
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
                        exportersBySubscription,
                        notificationsByCorrelationsFunction(eventStoreProvider),
                        clock,
                        exportProvider
                    )
                )
            );
        }

        internal static void Handle(
            SubscriberMessage message,
            ExportersBySubscription<TExportProvider> exportersBySubscription,
            NotificationsByCorrelations notificationsByCorrelations,
            Func<DateTimeOffset> clock,
            TExportProvider integrationProvider)
        {
            var consumer = exportersBySubscription[message.Subscription];

            consumer
            (
                message.Notification,
                notificationsByCorrelations,
                clock,
                integrationProvider
            );
        }
    }
}