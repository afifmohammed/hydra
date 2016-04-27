USE [EventStore]

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

CREATE TABLE [dbo].[Publishers](
	[Name] [varchar](50) NOT NULL,
	[Correlation] [varchar](850) NOT NULL,
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_Publishers] PRIMARY KEY CLUSTERED 
(
	[Name] ASC,
	[Correlation] ASC
)
) ON [PRIMARY]

ALTER TABLE [dbo].[EventCorrelations]  WITH CHECK ADD  CONSTRAINT [FK_EventCorrelations_Events] FOREIGN KEY([EventId])
REFERENCES [dbo].[Events] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE [dbo].[EventCorrelations] CHECK CONSTRAINT [FK_EventCorrelations_Events]

GO
CREATE PROCEDURE AddPublisherEvents(@EventName varchar(50), @Content varchar(max), @When DateTimeOffset, @EventCorrelations dbo.EventTableType READONLY)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @Id bigint

	INSERT INTO [Events] (EventName, Content, [When])
	VALUES (@EventName, @Content, @When)

	SET @id = @@IDENTITY

	INSERT INTO EventCorrelations (EventId, PropertyName, PropertyValue)
	SELECT @Id, PropertyName, PropertyValue
	FROM @eventCorrelations
END

GO

CREATE PROCEDURE proc_GetEventsWithCorrelations (@tvpEvents dbo.EventTableType READONLY)
AS
BEGIN
	SET NOCOUNT ON
	SELECT e.EventName, e.Content
	FROM [Events] as e INNER JOIN EventCorrelations AS ec ON e.Id = ec.EventId 
	INNER JOIN @tvpEvents as t 	ON e.EventName = t.EventName AND ec.PropertyName = t.PropertyName AND ec.PropertyValue = t.PropertyValue 
	Group by e.Id, e.EventName, e.Content
	ORDER BY e.Id
END