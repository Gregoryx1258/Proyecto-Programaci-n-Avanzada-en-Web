
CREATE DATABASE Connect4DB;
GO
USE Connect4DB;
GO

CREATE TABLE Players (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Identification INT NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    Score INT DEFAULT 0,
    Wins INT DEFAULT 0,
    Losses INT DEFAULT 0,
    Draws INT DEFAULT 0
);

CREATE TABLE Games (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Player1Id INT NOT NULL,
    Player2Id INT NOT NULL,
    GridJson NVARCHAR(MAX) NOT NULL,
    CurrentTurnId INT NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    WinnerId INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    FOREIGN KEY (Player1Id) REFERENCES Players(Id),
    FOREIGN KEY (Player2Id) REFERENCES Players(Id),
    FOREIGN KEY (CurrentTurnId) REFERENCES Players(Id),
    FOREIGN KEY (WinnerId) REFERENCES Players(Id)
);

SELECT * FROM Players

INSERT INTO Players (Name, Identification, Score, Wins, Losses, Draws)
VALUES ('María Jiménez', '123456789', 0, 0, 0, 0),
       ('Carlos Rodríguez', '987654321', 0, 0, 0, 0);