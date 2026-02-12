USE DatabridgeDB;
/*-----------------------------------------------------------
        Database: DatabridgeDB
        Server :localhost\SQLEXPRESS
------------------------------------------------------------*/



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


/*---------------------------------------------------------
        To Insert The Details (POST)
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

    /* EmpName: Check for NULL or Empty */
    IF (@EmpName IS NULL OR LTRIM(RTRIM(@EmpName)) = '')
    BEGIN
        SELECT 'EmpName cannot be NULL or empty' AS Message;
        RETURN;
    END

    /* EmpName: Check if it contains numbers 
       (User requirement: "without numbers") */
    IF (@EmpName LIKE '%[0-9]%')
    BEGIN
        SELECT 'EmpName must not contain numbers' AS Message;
        RETURN;
    END

    /* DeptName: Check for NULL or Empty */
    IF (@DeptName IS NULL OR LTRIM(RTRIM(@DeptName)) = '')
    BEGIN
        SELECT 'DeptName cannot be NULL or empty' AS Message;
        RETURN;
    END

    /* DeptName: Check if it contains numbers */
    IF (@DeptName LIKE '%[0-9]%')
    BEGIN
        SELECT 'DeptName must not contain numbers' AS Message;
        RETURN;
    END

    DECLARE @DeptId INT;
    DECLARE @EmpId INT;

    /* Check if Employee already exists */
    IF EXISTS (SELECT 1 FROM Employee WHERE EmpName = @EmpName)
    BEGIN
        SELECT 'Employee already exists' AS Message;
        RETURN;
    END

    /* Get or Insert Department */
    SELECT @DeptId = DeptId FROM IT_Sector WHERE DeptName = @DeptName;

    IF @DeptId IS NULL
    BEGIN
        INSERT INTO IT_Sector (DeptName) VALUES (@DeptName);
        SET @DeptId = SCOPE_IDENTITY();
    END

    /* Insert Employee */
    INSERT INTO Employee (EmpName, DeptId)
    VALUES (@EmpName, @DeptId);

     /* SUCCESS - Return ONLY the message (No columns, no joins) */
    SELECT 'Employee inserted successfully' AS Message;

END;

exec SP_AddEmployee 'suresh','python';

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

Go
CREATE OR Alter PROCEDURE SP_GetEmployeeById
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
        To Update The Employee Name By Getting EmpId (POST)
------------------------------------------------------------------*/
Go
CREATE OR ALTER PROCEDURE SP_UpdateEmployeeName
(
    @EmpId INT,
    @EmpName VARCHAR(50),
    @DeptName VARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    /* Validation */
    IF (@EmpId IS NULL OR @EmpId <= 0)
    BEGIN
        SELECT 'EmpId is required and must be greater than 0' AS Message;
        RETURN;
    END

    -- Validate EmpName
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

    -- Validate DeptName
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

    -- Check Employee exists
    IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmpId = @EmpId)
    BEGIN
        SELECT 'Employee not found' AS Message;
        RETURN;
    END

    DECLARE @DeptId INT;

    -- Check if Dept exists
    SELECT @DeptId = DeptId FROM IT_Sector WHERE DeptName = @DeptName;

    -- If Dept does not exist, create new one
    IF @DeptId IS NULL
    BEGIN
        INSERT INTO IT_Sector (DeptName)
        VALUES (@DeptName);

        SET @DeptId = SCOPE_IDENTITY();
    END

    -- Update Employee
    UPDATE Employee
    SET EmpName = @EmpName,
        DeptId = @DeptId
    WHERE EmpId = @EmpId;

    SELECT 'Employee updated successfully' AS Message;
END;

--EXEC SP_UpdateEmployeeName 34, 'Punjuri','hr';

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

--SELECT * FROM IT_Sector;
--SELECT * FROM Employee;
