﻿USE [EventStore]

CREATE TYPE [dbo].[EventTableType] AS TABLE(
	[EventName] [nvarchar](250) NULL,
	[PropertyName] [nvarchar](250) NULL,
	[PropertyValue] [nvarchar](250) NULL
)

CREATE TABLE [dbo].[EventCorrelations](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NOT NULL,
	[PropertyName] [nvarchar](250) NOT NULL,
	[PropertyValue] [nvarchar](250) NOT NULL,	
	CONSTRAINT [PK_EventCorrelations] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Events](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Content] [varchar](max) NOT NULL,
	[When] [datetimeoffset](7) NOT NULL,
	[EventName] [nvarchar](250) NOT NULL,
	CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[EventCorrelations]  WITH CHECK ADD  CONSTRAINT [FK_EventCorrelations_Events] FOREIGN KEY([EventId])
REFERENCES [dbo].[Events] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[EventCorrelations] CHECK CONSTRAINT [FK_EventCorrelations_Events]