/* =========================================================
   File Name   : DatabridgeDB_Tables_And_Procedures.sql
   Purpose     : Database schema, master data, and procedures
   Author      : Devaraj
   Updated On  : 2026-02-16
   ========================================================= */

USE DatabridgeDB;
GO

/* =========================================================
   DEPARTMENT TABLE
========================================================= */
IF OBJECT_ID('dbo.Department', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Department (
        DeptID INT IDENTITY(1,1) PRIMARY KEY,
        DeptName VARCHAR(100) NOT NULL UNIQUE
    );
END;
GO

/* ===========================
   SEED DEPARTMENTS (SAFE)
=========================== */
IF NOT EXISTS (SELECT 1 FROM dbo.Department)
BEGIN
    INSERT INTO dbo.Department (DeptName)
    VALUES 
    ('Computer Science'),
    ('Information Technology'),
    ('Electronics'),
    ('Mechanical');
END;
GO


/* =========================================================
   STUDENT TABLE
========================================================= */
IF OBJECT_ID('dbo.Student', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student (
        StudentID INT IDENTITY(1,1) PRIMARY KEY,
        StudentName VARCHAR(100) NOT NULL,
        Age INT NOT NULL,
        DeptID INT NOT NULL,
        CONSTRAINT FK_Student_Department
            FOREIGN KEY (DeptID)
            REFERENCES dbo.Department(DeptID)
    );
END;
GO


/* =========================================================
   INSERT STUDENT
========================================================= */
CREATE OR ALTER PROCEDURE dbo.InsertStudentDetails
(
    @StudentName VARCHAR(100),
    @Age INT,
    @DeptName VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Name Validation
    IF @StudentName LIKE '%[^A-Za-z]%'
    BEGIN
        RAISERROR('Student name must contain only alphabets.', 16, 1);
        RETURN;
    END

    -- Age Validation
    IF @Age <= 18 OR @Age >= 60
    BEGIN
        RAISERROR('Age must be between 19 and 59.', 16, 1);
        RETURN;
    END

    DECLARE @DeptID INT;

    -- Department Validation
    SELECT @DeptID = DeptID
    FROM dbo.Department
    WHERE DeptName = @DeptName;

    IF @DeptID IS NULL
    BEGIN
        RAISERROR('Department does not exist.', 16, 1);
        RETURN;
    END

    -- Duplicate Check (Case-Insensitive)
    IF EXISTS (
        SELECT 1
        FROM dbo.Student
        WHERE UPPER(StudentName) = UPPER(@StudentName)
    )
    BEGIN
        RAISERROR('Student already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Student (StudentName, Age, DeptID)
    VALUES (@StudentName, @Age, @DeptID);
END;
GO


/* =========================================================
   GET ALL STUDENTS
========================================================= */
CREATE OR ALTER PROCEDURE dbo.GetAllStudents
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.StudentID,
        s.StudentName,
        s.Age,
        d.DeptName
    FROM dbo.Student s
    INNER JOIN dbo.Department d
        ON s.DeptID = d.DeptID;
END;
GO


/* =========================================================
   GET STUDENT BY ID
========================================================= */
CREATE OR ALTER PROCEDURE dbo.GetStudentById
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.StudentName,
        s.Age,
        d.DeptName
    FROM dbo.Student s
    INNER JOIN dbo.Department d
        ON s.DeptID = d.DeptID
    WHERE s.StudentID = @StudentId;
END;
GO


/* =========================================================
   UPDATE STUDENT
========================================================= */
CREATE OR ALTER PROCEDURE dbo.UpdateStudent
(
    @StudentId INT,
    @StudentName VARCHAR(100),
    @Age INT,
    @DeptName VARCHAR(100),
    @RowsAffected INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Student WHERE StudentID = @StudentId)
    BEGIN
        SET @RowsAffected = 0;
        RETURN;
    END

    IF @StudentName LIKE '%[^A-Za-z]%'
    BEGIN
        RAISERROR('Student name must contain only alphabets.', 16, 1);
        RETURN;
    END

    IF @Age <= 18 OR @Age >= 60
    BEGIN
        RAISERROR('Age must be between 19 and 59.', 16, 1);
        RETURN;
    END

    DECLARE @DeptID INT;

    SELECT @DeptID = DeptID
    FROM dbo.Department
    WHERE DeptName = @DeptName;

    IF @DeptID IS NULL
    BEGIN
        RAISERROR('Department does not exist.', 16, 1);
        RETURN;
    END

    UPDATE dbo.Student
    SET
        StudentName = @StudentName,
        Age = @Age,
        DeptID = @DeptID
    WHERE StudentID = @StudentId;

    SET @RowsAffected = @@ROWCOUNT;
END;
GO


/* =========================================================
   DELETE STUDENT
========================================================= */
CREATE OR ALTER PROCEDURE dbo.DeleteStudent
(
    @StudentId INT,
    @RowsAffected INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Student
    WHERE StudentID = @StudentId;

    SET @RowsAffected = @@ROWCOUNT;
END;
GO


/* =========================================================
   TABLE TYPE FOR BULK DELETE
========================================================= */
IF TYPE_ID(N'dbo.StudentIdTableType') IS NULL
BEGIN
    CREATE TYPE dbo.StudentIdTableType AS TABLE
    (
        StudentID INT PRIMARY KEY
    );
END;
GO


/* =========================================================
   BULK DELETE (TVP)
========================================================= */
CREATE OR ALTER PROCEDURE dbo.DeleteStudentsBatchEnhanced
(
    @StudentIds dbo.StudentIdTableType READONLY,
    @RowsAffected INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Identify missing IDs
    ;WITH MissingIDs AS
    (
        SELECT t.StudentID
        FROM @StudentIds t
        LEFT JOIN dbo.Student s
            ON t.StudentID = s.StudentID
        WHERE s.StudentID IS NULL
    )
    SELECT * INTO #MissingIDsTemp FROM MissingIDs;

    -- Delete existing
    DELETE s
    FROM dbo.Student s
    INNER JOIN @StudentIds t
        ON s.StudentID = t.StudentID;

    SET @RowsAffected = @@ROWCOUNT;

    -- Return missing
    IF EXISTS (SELECT 1 FROM #MissingIDsTemp)
    BEGIN
        SELECT StudentID AS MissingID
        FROM #MissingIDsTemp;
    END

    DROP TABLE #MissingIDsTemp;
END;
GO
