create table Rfids(
  Id integer identity(1,1) primary key,
  Tag nvarchar(20) null)

create nonclustered index IX_Rfid_Tag on [dbo].[Rfids]([Tag])
  INCLUDE([Id])
GO

create table RfidEvents(
  Id integer identity(1,1) primary key,
  Rfid int not null,
  EventType nvarchar(10) not null,
  StartUtc datetime not null,
  EndUtc datetime)

ALTER TABLE [RfidEvents]
  ADD CONSTRAINT [FK_RfidEvents_Rfid] FOREIGN KEY ([Rfid])
  REFERENCES [dbo].[Rfids] ([Id])
  ON DELETE CASCADE
GO
