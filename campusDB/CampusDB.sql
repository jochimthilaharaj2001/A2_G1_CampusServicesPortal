USE CampusServicePortalDB;
SELECT * FROM Roles;
SELECT * FROM Users;
SELECT * FROM Students;
SELECT * FROM RefreshTokens;
SELECT * FROM StudentMasterList;

USE CampusServicePortalDB;
GO

INSERT INTO StudentMasterList (IndexNumber, FullName, Faculty, DegreeProgram, EnrollmentYear, IsRegistered, CreatedAt)
VALUES (
    'UT011690', 
    'Jochim Thilaharaj', 
    'Faculty of Engineering', -- Or your target faculty name
    'BSE', 
    2025, 
    0, 
    GETUTCDATE()
);
GO
