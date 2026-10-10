/*
 RaceDay Database Schema
 SQL Server / SSMS
 Matches RaceDay_ERD.png
*/

IF DB_ID('RaceDayDB') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END;
GO

CREATE DATABASE RaceDayDB;
GO
USE RaceDayDB;
GO

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    PhoneNumber VARCHAR(30) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETDATE(),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Organiser','Participant'))
);
GO

CREATE TABLE Events (
    EventID INT IDENTITY(1,1) CONSTRAINT PK_Events PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName VARCHAR(150) NOT NULL,
    Description VARCHAR(500) NULL,
    EventDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    Location VARCHAR(150) NOT NULL,
    EventType VARCHAR(20) NOT NULL,
    Status VARCHAR(20) NOT NULL CONSTRAINT DF_Events_Status DEFAULT 'Upcoming',
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserID) REFERENCES Users(UserID),
    CONSTRAINT CK_Events_EventType CHECK (EventType IN ('Running','Walking','Cycling')),
    CONSTRAINT CK_Events_Status CHECK (Status IN ('Upcoming','Open','Completed','Cancelled'))
);
GO

CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) CONSTRAINT PK_Categories PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL CONSTRAINT DF_Categories_EntryFee DEFAULT 0,
    MaxParticipants INT NOT NULL,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT UQ_Categories_Event_CategoryName UNIQUE (EventID, CategoryName),
    CONSTRAINT UQ_Categories_Category_Event UNIQUE (CategoryID, EventID),
    CONSTRAINT CK_Categories_Distance CHECK (DistanceKm > 0),
    CONSTRAINT CK_Categories_EntryFee CHECK (EntryFee >= 0),
    CONSTRAINT CK_Categories_MaxParticipants CHECK (MaxParticipants > 0)
);
GO

CREATE TABLE Enrolments (
    EnrolmentID INT IDENTITY(1,1) CONSTRAINT PK_Enrolments PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    ParticipantID INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL CONSTRAINT DF_Enrolments_Date DEFAULT GETDATE(),
    Status VARCHAR(20) NOT NULL CONSTRAINT DF_Enrolments_Status DEFAULT 'Confirmed',
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryID, EventID)
        REFERENCES Categories(CategoryID, EventID),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    CONSTRAINT UQ_Enrolments_Event_Participant UNIQUE (EventID, ParticipantID),
    CONSTRAINT CK_Enrolments_Status CHECK (Status IN ('Confirmed','Cancelled','Completed'))
);
GO

CREATE TABLE Results (
    ResultID INT IDENTITY(1,1) CONSTRAINT PK_Results PRIMARY KEY,
    EnrolmentID INT NOT NULL CONSTRAINT UQ_Results_Enrolment UNIQUE,
    FinishTime TIME NULL,
    Position INT NULL,
    Status VARCHAR(20) NOT NULL CONSTRAINT DF_Results_Status DEFAULT 'Finished',
    RecordedAt DATETIME2 NOT NULL CONSTRAINT DF_Results_RecordedAt DEFAULT GETDATE(),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID),
    CONSTRAINT CK_Results_Position CHECK (Position IS NULL OR Position > 0),
    CONSTRAINT CK_Results_Status CHECK (Status IN ('Finished','DidNotFinish','Disqualified'))
);
GO

CREATE TABLE Routes (
    RouteID INT IDENTITY(1,1) CONSTRAINT PK_Routes PRIMARY KEY,
    EventID INT NOT NULL CONSTRAINT UQ_Routes_Event UNIQUE,
    RouteName VARCHAR(150) NOT NULL,
    DistanceKm DECIMAL(6,2) NULL,
    RouteUrl VARCHAR(500) NULL,
    Description VARCHAR(500) NULL,
    CONSTRAINT FK_Routes_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT CK_Routes_Distance CHECK (DistanceKm IS NULL OR DistanceKm > 0)
);
GO

INSERT INTO Users (FullName, Email, PasswordHash, Role, PhoneNumber)
VALUES
('Thabo Mokoena','thabo.organiser@raceday.co.za','HASH_ORGANISER_1','Organiser','0825551001'),
('Lerato Dlamini','lerato.organiser@raceday.co.za','HASH_ORGANISER_2','Organiser','0835551002'),
('Sipho Nkosi','sipho.participant@raceday.co.za','HASH_PARTICIPANT_1','Participant','0845551003'),
('Naledi Maseko','naledi.participant@raceday.co.za','HASH_PARTICIPANT_2','Participant','0855551004');
GO

INSERT INTO Events (OrganiserID, EventName, Description, EventDate, StartTime, Location, EventType, Status)
VALUES
(1,'Soweto Sunrise Run','Community road running event through Soweto.','2026-10-18','06:30','Soweto, Johannesburg','Running','Open'),
(1,'Mpumalanga Charity Walk','Family-friendly charity walking event.','2026-11-07','07:00','Mbombela, Mpumalanga','Walking','Open'),
(2,'Cape Town Coastal Cycle','Road cycling event along the Cape Town coast.','2026-12-06','06:00','Cape Town, Western Cape','Cycling','Upcoming');
GO

INSERT INTO Categories (EventID, CategoryName, DistanceKm, EntryFee, MaxParticipants)
VALUES
(1,'10 km Open Run',10.00,120.00,1000),
(1,'5 km Fun Run',5.00,80.00,1500),
(2,'10 km Charity Walk',10.00,100.00,800),
(2,'5 km Family Walk',5.00,60.00,1200),
(3,'60 km Road Race',60.00,350.00,600),
(3,'30 km Social Ride',30.00,220.00,800);
GO

INSERT INTO Routes (EventID, RouteName, DistanceKm, RouteUrl, Description)
VALUES
(1,'Soweto Sunrise Route',10.00,'https://example.org/routes/soweto-sunrise','10 km road route starting and finishing near the event village.'),
(2,'Mbombela Charity Route',10.00,'https://example.org/routes/mbombela-charity','Scenic 10 km walking route suitable for families and charity participants.'),
(3,'Cape Town Coastal Route',60.00,'https://example.org/routes/cape-town-coastal','60 km coastal road cycling route with controlled race sections.');
GO

INSERT INTO Enrolments (EventID, CategoryID, ParticipantID, Status)
VALUES
(1,1,3,'Confirmed'),
(1,2,4,'Confirmed'),
(2,3,3,'Confirmed'),
(3,6,4,'Confirmed');
GO

INSERT INTO Results (EnrolmentID, FinishTime, Position, Status)
VALUES
(1,'00:52:31',18,'Finished'),
(2,'00:31:45',42,'Finished');
GO

SELECT * FROM Users;
SELECT * FROM Events;
SELECT * FROM Categories;
SELECT * FROM Enrolments;
SELECT * FROM Results;
SELECT * FROM Routes;
GO
