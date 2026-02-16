USE DatabridgeDB;
/*-----------------------------------------------------------
        Database: DatabridgeDB
        Server :localhost\SQLEXPRESS
------------------------------------------------------------*/

--SELECT * FROM IT_Sector;
--SELECT * FROM Employee;


/*----------------------------------------------------------
                 TABLE: Department
------------------------------------------------------------*/

CREATE TABLE IT_Sector
(
    DeptId INT IDENTITY(1,1) PRIMARY KEY,
    DeptName VARCHAR(50) NOT NULL UNIQUE
);
/*----------------------------------------------------------
                 TABLE: Employee
------------------------------------------------------------*/
CREATE TABLE Employee
(
    EmpId INT IDENTITY(1,1) PRIMARY KEY,
    EmpName VARCHAR(50) NOT NULL,
    DeptId INT NOT NULL,

    CONSTRAINT FK_Employee_Department
        FOREIGN KEY (DeptId)
        REFERENCES IT_Sector(DeptId)
); 
/*========================================================
             TVP format uploading data table
=========================================================*/
-- Create a type that matches your Excel columns
CREATE TYPE EmployeeImportType AS TABLE 
(
    EmpName VARCHAR(100),
    DeptName VARCHAR(100)
);
GO
/*========================================================
      deleting data multiple or single table (format (TVP))
=========================================================*/
CREATE TYPE EmpIdTableType AS TABLE
(
    EmpId INT NOT NULL
);

/*---------------------------------------------------------
        To Insert Employee Details (POST)
-----------------------------------------------------------*/
Go
CREATE OR ALTER PROCEDURE SP_AddEmployee
(
    @EmpName  VARCHAR(50),
    @DeptName VARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    IF (@EmpName IS NULL OR LTRIM(RTRIM(@EmpName)) = '')
    BEGIN
        SELECT 'EmpName cannot be NULL or empty' AS Message;
        RETURN;
    END
    IF (@EmpName LIKE '%[0-9]%')
    BEGIN
        SELECT 'EmpName must not contain numbers' AS Message;
        RETURN;
    END
    IF (@DeptName IS NULL OR LTRIM(RTRIM(@DeptName)) = '')
    BEGIN
        SELECT 'DeptName cannot be NULL or empty' AS Message;
        RETURN;
    END
    IF (@DeptName LIKE '%[0-9]%')
    BEGIN
        SELECT 'DeptName must not contain numbers' AS Message;
        RETURN;
    END
    DECLARE @DeptId INT;
    DECLARE @EmpId INT;
    IF EXISTS (SELECT 1 FROM Employee WHERE EmpName = @EmpName)
    BEGIN
        SELECT 'Employee already exists' AS Message;
        RETURN;
    END
    SELECT @DeptId = DeptId FROM IT_Sector WHERE DeptName = @DeptName;

    IF @DeptId IS NULL
    BEGIN
        INSERT INTO IT_Sector (DeptName) VALUES (@DeptName);
        SET @DeptId = SCOPE_IDENTITY();
    END
    INSERT INTO Employee (EmpName, DeptId)
    VALUES (@EmpName, @DeptId);
    SELECT 'Employee inserted successfully' AS Message;
END;
--exec SP_AddEmployee 'SeemaSimran','hr';
--exec SP_AddEmployee 'Deepak','dotnet';
--exec SP_AddEmployee 'tharun','hr';
--exec SP_AddEmployee 'manisha','python';
--exec SP_AddEmployee 'SeemaSimran','hr';
--exec SP_AddEmployee 'ramu','suport';


/*-----------------------------------------------------------
        To Fetch All The Details(GET)
------------------------------------------------------------*/
go
CREATE OR ALTER PROCEDURE SP_GetAllEmployeesFull
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        e.EmpId,
        e.EmpName,
        d.DeptId,
        d.DeptName
    FROM Employee e
    JOIN IT_Sector d ON e.DeptId = d.DeptId;
END;
--EXEC SP_GetAllEmployeesFull;


/*----------------------------------------------------------
           To Get Employee Details(GET)
-----------------------------------------------------------*/

GO
CREATE OR alter PROCEDURE SP_GetEmployeeById
(
    @EmpId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmpId = @EmpId)
    BEGIN
        SELECT 'Employee not found' AS Message;
        RETURN;
    END
    SELECT 
        e.EmpName,
        d.DeptName
    FROM Employee e
    INNER JOIN IT_Sector d
        ON e.DeptId = d.DeptId
    WHERE e.EmpId = @EmpId;
END;

--exec SP_GetEmployeeById 255
--EXEC SP_GetEmployeeById 3

/*----------------------------------------------------------------
        To Update The Employee Name (POST)
------------------------------------------------------------------*/
go
CREATE OR ALTER PROCEDURE SP_UpdateEmployeeName
(
    @EmpId INT,
    @EmpName VARCHAR(50),
    @DeptName VARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    IF (@EmpId IS NULL OR @EmpId <= 0)
    BEGIN
        SELECT 'EmpId is required and must be greater than 0' AS Message;
        RETURN;
    END
    IF (@EmpName IS NULL OR LTRIM(RTRIM(@EmpName)) = '')
    BEGIN
        SELECT 'EmpName cannot be NULL or empty' AS Message;
        RETURN;
    END
    IF (@EmpName NOT LIKE '[A-Za-z]%')
    BEGIN
        SELECT 'EmpName must start with an alphabet' AS Message;
        RETURN;
    END
    IF (@DeptName IS NULL OR LTRIM(RTRIM(@DeptName)) = '')
    BEGIN
        SELECT 'DeptName cannot be NULL or empty' AS Message;
        RETURN;
    END
    IF (@DeptName NOT LIKE '[A-Za-z]%')
    BEGIN
        SELECT 'DeptName must start with an alphabet' AS Message;
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmpId = @EmpId)
    BEGIN
        SELECT 'Employee not found' AS Message;
        RETURN;
    END
    DECLARE @DeptId INT;
    SELECT @DeptId = DeptId FROM IT_Sector WHERE DeptName = @DeptName;
    IF @DeptId IS NULL
    BEGIN
        INSERT INTO IT_Sector (DeptName)
        VALUES (@DeptName);
        SET @DeptId = SCOPE_IDENTITY();
    END
    UPDATE Employee
    SET EmpName = @EmpName,
        DeptId = @DeptId
    WHERE EmpId = @EmpId;

    SELECT 'Employee updated successfully' AS Message;
END;

--EXEC SP_UpdateEmployeeName 3, 'Punjuri','dev';

/*-----------------------------------------------------------------
        To Delete The Employees Details (PUT)
-------------------------------------------------------------------*/
go
CREATE OR ALTER PROCEDURE SP_DeleteEmployee
(
    @EmpId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    IF (@EmpId IS NULL OR @EmpId <= 0)
    BEGIN
        SELECT 'EmpId is required and must be greater than 0' AS Message;
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmpId = @EmpId)
    BEGIN
        SELECT 'Employee not found' AS Message;
        RETURN;
    END
    DELETE FROM Employee WHERE EmpId = @EmpId;
    SELECT 'Employee deleted successfully' AS Message;
END;

--exec SP_DeleteEmployee 66

/*-----------------------------------------------------------
        To Bulk Insert Employee Details (POST)
------------------------------------------------------------*/
go
CREATE OR ALTER PROCEDURE SP_BulkImportEmployees
(
    @Employees EmployeeImportType READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TotalReceived INT;
    DECLARE @InsertedCount INT = 0;
    DECLARE @SkippedCount INT = 0;
    SELECT @TotalReceived = COUNT(*) FROM @Employees;
    SELECT DISTINCT 
        LTRIM(RTRIM(EmpName)) AS EmpName, 
        LTRIM(RTRIM(DeptName)) AS DeptName
    INTO #ProcessingQueue
    FROM @Employees;
    INSERT INTO IT_Sector (DeptName)
    SELECT DISTINCT P.DeptName
    FROM #ProcessingQueue P
    WHERE NOT EXISTS (SELECT 1 FROM IT_Sector D WHERE D.DeptName = P.DeptName);
    INSERT INTO Employee (EmpName, DeptId)
    SELECT 
        P.EmpName, 
        D.DeptId
    FROM #ProcessingQueue P
    INNER JOIN IT_Sector D ON P.DeptName = D.DeptName
    WHERE NOT EXISTS (
        SELECT 1 
        FROM Employee E 
        WHERE E.EmpName = P.EmpName
    );
    SET @InsertedCount = @@ROWCOUNT;
    DROP TABLE #ProcessingQueue;
    SELECT 
        @TotalReceived AS TotalRowsReceived,
        @InsertedCount AS SuccessfullyInserted,
        @SkippedCount  AS [Skipped (Due to Duplicates)],
        'Bulk import completed' AS Message;
END;

/*================================================
        Deleting multiple
=================================================*/

GO
CREATE OR ALTER PROCEDURE SP_DeleteMultipleEmployees
(
    @EmpIds EmpIdTableType READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM @EmpIds)
    BEGIN
        SELECT 'Warning: No Employee IDs were provided.' AS Message;
        RETURN;
    END
    BEGIN TRY
        DECLARE @TotalRequested INT =
        (
            SELECT COUNT(*) FROM @EmpIds
        );
        IF @TotalRequested = 1
        BEGIN
            DECLARE @SingleId INT =
            (
                SELECT TOP (1) EmpId FROM @EmpIds
            );

            IF NOT EXISTS
            (
                SELECT 1
                FROM Employee AS E
                WHERE E.EmpId = @SingleId
            )
            BEGIN
                SELECT 'Employee not found or already deleted.' AS Message;
                RETURN;
            END
        END
        DELETE E
        FROM Employee AS E
        INNER JOIN @EmpIds AS T
            ON E.EmpId = T.EmpId;

        DECLARE @DeletedCount INT = @@ROWCOUNT;
        IF @DeletedCount = 0
        BEGIN
            SELECT 'No matching employees found to delete.' AS Message;
            RETURN;
        END

        IF @TotalRequested = 1
            SELECT 'Employee deleted successfully.' AS Message;
        ELSE
            SELECT CAST(@DeletedCount AS VARCHAR(10))
                   + ' employee(s) deleted successfully.' AS Message;
    END TRY
    BEGIN CATCH
        SELECT 'Error: ' + ERROR_MESSAGE() AS Message;
    END CATCH
END;
GO


--DECLARE @EmpIds EmpIdTableType;

--INSERT INTO @EmpIds (EmpId)
--VALUES 
--(334),
--(734),
--(934);

--EXEC SP_DeleteMultipleEmployees @EmpIds;

