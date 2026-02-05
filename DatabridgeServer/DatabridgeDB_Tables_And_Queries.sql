/* =========================================================
   File Name   : DatabridgeDB_Tables_And_Procedures.sql
   Purpose     : Database schema, master data, and procedures
   Author      : Devaraj
   Created On  : 2026-02-05
   ========================================================= */

USE DatabridgeDB;
GO

/* =========================================================
   TABLE: Department
   Purpose: Stores department master data
   ========================================================= */
CREATE TABLE Department (
    DeptID INT IDENTITY(1,1) PRIMARY KEY,
    DeptName VARCHAR(100) NOT NULL UNIQUE
);
GO

/* ---------------------------------------------------------
   Insert default departments (Master Data)
   --------------------------------------------------------- */
INSERT INTO Department (DeptName)
VALUES 
    ('Computer Science'),
    ('Information Technology'),
    ('Electronics'),
    ('Mechanical');
GO

/* =========================================================
   TABLE: Student
   Purpose: Stores student details
   ========================================================= */
CREATE TABLE Student (
    StudentID INT IDENTITY(1,1) PRIMARY KEY,
    StudentName VARCHAR(100) NOT NULL,
    Age INT NOT NULL,
    DeptID INT NOT NULL,
    CONSTRAINT FK_Student_Department
        FOREIGN KEY (DeptID) REFERENCES Department(DeptID)
);
GO

/* =========================================================
   STORED PROCEDURE: InsertStudentDetails
   Purpose:
     - Validates age
     - Validates department existence
     - Prevents duplicate student names (case-insensitive)
     - Inserts student record
   ========================================================= */
CREATE OR ALTER PROCEDURE InsertStudentDetails
(
    @StudentName VARCHAR(100),
    @Age INT,
    @DeptName VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DeptID INT;

    /* -------- Age Validation -------- */
    IF @Age <= 18 OR @Age >= 60
    BEGIN
        RAISERROR ('Age must be greater than 18 and less than 60.', 16, 1);
        RETURN;
    END

    /* -------- Department Validation -------- */
    SELECT @DeptID = DeptID
    FROM Department
    WHERE DeptName = @DeptName;

    IF @DeptID IS NULL
    BEGIN
        RAISERROR ('Department does not exist.', 16, 1);
        RETURN;
    END

    /* -------- Duplicate Student Check (Global, Case-Insensitive) -------- */
    IF EXISTS (
        SELECT 1
        FROM Student
        WHERE UPPER(StudentName) = UPPER(@StudentName)
    )
    BEGIN
        RAISERROR ('Student already exists.', 16, 1);
        RETURN;
    END

    /* -------- Insert Student -------- */
    INSERT INTO Student (StudentName, Age, DeptID)
    VALUES (@StudentName, @Age, @DeptID);
END;
GO
