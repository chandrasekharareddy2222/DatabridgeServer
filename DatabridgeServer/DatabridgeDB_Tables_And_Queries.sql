/* ===========================
   DEPARTMENT TABLE
=========================== */
IF OBJECT_ID('dbo.Department', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Department (
        DeptID INT IDENTITY(1,1) PRIMARY KEY,
        DeptName VARCHAR(100) NOT NULL UNIQUE
    );
END;
GO

/* ===========================
   SEED DEPARTMENTS
=========================== */
IF NOT EXISTS (SELECT 1 FROM Department)
BEGIN
    INSERT INTO Department (DeptName)
    VALUES 
    ('Computer Science'),
    ('Information Technology'),
    ('Electronics'),
    ('Mechanical');
END;
GO

/* ===========================
   STUDENT TABLE
=========================== */
IF OBJECT_ID('dbo.Student', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student (
        StudentID INT IDENTITY(1,1) PRIMARY KEY,
        StudentName VARCHAR(100) NOT NULL,
        Age INT NOT NULL,
        DeptID INT NOT NULL,
        CONSTRAINT FK_Student_Department
            FOREIGN KEY (DeptID)
            REFERENCES Department(DeptID)
    );
END;
GO

/* ===========================
   INSERT STUDENT (POST)
=========================== */
CREATE OR ALTER PROCEDURE dbo.InsertStudentDetails
    @StudentName VARCHAR(100),
    @Age INT,
    @DeptName VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DeptID INT;

    IF @Age <= 18 OR @Age >= 60
    BEGIN
        RAISERROR ('Age must be between 18 and 60.', 16, 1);
        RETURN;
    END

    SELECT @DeptID = DeptID
    FROM Department
    WHERE DeptName = @DeptName;

    IF @DeptID IS NULL
    BEGIN
        RAISERROR ('Department does not exist.', 16, 1);
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM Student
        WHERE UPPER(StudentName) = UPPER(@StudentName)
    )
    BEGIN
        RAISERROR ('Student already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO Student (StudentName, Age, DeptID)
    VALUES (@StudentName, @Age, @DeptID);
END;
GO

/* ===========================
   GET ALL STUDENTS
=========================== */
CREATE OR ALTER PROCEDURE dbo.GetAllStudents
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.StudentID,
        s.StudentName,
        s.Age,
        d.DeptName
    FROM Student s
    INNER JOIN Department d
        ON s.DeptID = d.DeptID;
END;
GO


/* ===========================
   GET STUDENT BY ID
=========================== */
CREATE OR ALTER PROCEDURE dbo.GetStudentById
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.StudentName,
        s.Age,
        d.DeptName
    FROM Student s
    INNER JOIN Department d
        ON s.DeptID = d.DeptID
    WHERE s.StudentID = @StudentId;
END;
GO

/* ===========================
   UPDATE STUDENT
=========================== */
CREATE OR ALTER PROCEDURE dbo.UpdateStudent
    @StudentId INT,
    @StudentName VARCHAR(100),
    @Age INT,
    @DeptName VARCHAR(100),
    @RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DeptID INT;

    SELECT @DeptID = DeptID
    FROM Department
    WHERE DeptName = @DeptName;

    IF @DeptID IS NULL
    BEGIN
        RAISERROR ('Department does not exist.', 16, 1);
        SET @RowsAffected = 0;
        RETURN;
    END

    UPDATE Student
    SET
        StudentName = @StudentName,
        Age = @Age,
        DeptID = @DeptID
    WHERE StudentID = @StudentId;

    SET @RowsAffected = @@ROWCOUNT;
END;
GO



EXEC dbo.UpdateStudent
    @StudentId = 1,
    @StudentName = 'naniqq',
    @Age = 44,
    @DeptName = 'Mechanical';

    SELECT * FROM Student WHERE StudentID = 1;


/* ===========================
   DELETE STUDENT
=========================== */
CREATE OR ALTER PROCEDURE dbo.DeleteStudent
    @StudentId INT,
    @RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Student
    WHERE StudentID = @StudentId;

    SET @RowsAffected = @@ROWCOUNT;
END;
GO

/* ===========================
   TABLE TYPE FOR BULK DELETE
=========================== */
IF TYPE_ID(N'dbo.StudentIdTableType') IS NULL
BEGIN
    CREATE TYPE dbo.StudentIdTableType AS TABLE
    (
        StudentID INT PRIMARY KEY
    );
END;
GO




/* ===========================
   ENHANCED BULK DELETE SP
   - Deletes multiple students using TVP
   - Returns deleted row count
   - Returns missing StudentIDs
=========================== */
CREATE OR ALTER PROCEDURE dbo.DeleteStudentsBatchEnhanced
    @StudentIds dbo.StudentIdTableType READONLY,
    @RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    /* ---------------------------
       STEP 1: Identify missing IDs
    --------------------------- */
    ;WITH MissingIDs AS
    (
        SELECT t.StudentID
        FROM @StudentIds t
        LEFT JOIN dbo.Student s
            ON t.StudentID = s.StudentID
        WHERE s.StudentID IS NULL
    )
    SELECT * INTO #MissingIDsTemp FROM MissingIDs;

    /* ---------------------------
       STEP 2: Delete existing IDs
    --------------------------- */
    DELETE s
    FROM dbo.Student s
    INNER JOIN @StudentIds t
        ON s.StudentID = t.StudentID;

    /* ---------------------------
       STEP 3: Return number of rows deleted
    --------------------------- */
    SET @RowsAffected = @@ROWCOUNT;

    /* ---------------------------
       STEP 4: Return missing IDs if any
    --------------------------- */
    IF EXISTS (SELECT 1 FROM #MissingIDsTemp)
    BEGIN
        SELECT StudentID AS MissingID
        FROM #MissingIDsTemp;
    END

    DROP TABLE #MissingIDsTemp;
END;
GO

/* ===========================
   QUICK VERIFICATION
=========================== */


-- Table variable to hold multiple student IDs for bulk deletion
DECLARE @IdsToDelete dbo.StudentIdTableType;

-- Add IDs to delete (include some missing to test missing IDs return)
INSERT INTO @IdsToDelete (StudentID)
VALUES (1), (2), (999);  -- 999 does NOT exist

DECLARE @DeletedCount INT;

-- Execute Enhanced Bulk Delete
EXEC dbo.DeleteStudentsBatchEnhanced
    @StudentIds = @IdsToDelete,
    @RowsAffected = @DeletedCount OUTPUT;

-- Show number of rows actually deleted
SELECT @DeletedCount AS DeletedRows;

-- Any missing IDs will be returned automatically by the SP
-- (SP returns a result set with column MissingID)






/* ===========================
   QUICK VERIFICATION
=========================== */
SELECT * FROM Department;
SELECT * FROM Student;