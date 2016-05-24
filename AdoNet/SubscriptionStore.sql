USE [SubscriptionStore]

CREATE TABLE [dbo].[Subscriptions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[NotificationContract] [nvarchar](250) NOT NULL,
	[SubscriberContract] [nvarchar](250) NOT NULL,
	CONSTRAINT [PK_Subscriptions] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	) ON [PRIMARY]
) ON [PRIMARY]