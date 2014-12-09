CREATE TABLE readings
(
ReadingId int IDENTITY (1, 1) NOT NULL,
DeviceTimeStamp DateTime NOT NULL,
DeviceId varchar(255) NOT NULL,
Reading int NOT NULL,
ServerTimestamp DateTime NOT NULL,
PRIMARY KEY (ReadingId))