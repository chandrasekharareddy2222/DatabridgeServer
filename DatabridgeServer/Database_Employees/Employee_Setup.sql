USE DatabridgeDB;
/*-----------------------------------------------------------
        Database: DatabridgeDB
        Server :localhost\SQLEXPRESS
------------------------------------------------------------*/



/*----------------------------------------------------------
                 TABLE: Department
------------------------------------------------------------*/

CREATE TABLE Department
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
        REFERENCES Department(DeptId)
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
    SELECT @DeptId = DeptId FROM Department WHERE DeptName = @DeptName;

    IF @DeptId IS NULL
    BEGIN
        INSERT INTO Department (DeptName) VALUES (@DeptName);
        SET @DeptId = SCOPE_IDENTITY();
    END

    /* Insert Employee */
    INSERT INTO Employee (EmpName, DeptId)
    VALUES (@EmpName, @DeptId);

    /* SUCCESS - Return ONLY the message (No columns, no joins) */
    SELECT 'Employee inserted successfully' AS Message;

END;




exec SP_AddEmployee 'tharun','hr';



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
    JOIN Department d ON e.DeptId = d.DeptId;
END;
EXEC SP_GetAllEmployeesFull;



/*----------------------------------------------------------
           To Get Employee Details(GET)
-----------------------------------------------------------*/
go
CREATE OR ALTER PROCEDURE SP_GetEmployeeById
(
    @EmpId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        e.EmpName,
        d.DeptName
    FROM Employee e
    INNER JOIN Department d
        ON e.DeptId = d.DeptId
    WHERE e.EmpId = @EmpId;
END;





exec SP_GetEmployeeById 142



/*----------------------------------------------------------------
        To Update The Employee Name (POST)
------------------------------------------------------------------*/
go
CREATE OR ALTER PROCEDURE SP_UpdateEmployeeName
(
    @EmpId INT,
    @EmpName VARCHAR(50)
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

    /* Employee exists check */
    IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmpId = @EmpId)
    BEGIN
        SELECT 'Employee not found' AS Message;
        RETURN;
    END

    /* Duplicate name check */
    IF EXISTS (
        SELECT 1 
        FROM Employee 
        WHERE EmpName = @EmpName AND EmpId <> @EmpId
    )
    BEGIN
        SELECT 'Employee with this name already exists' AS Message;
        RETURN;
    END

    /* Update only EmpName */
    UPDATE Employee
    SET EmpName = @EmpName
    WHERE EmpId = @EmpId;

    /* Success message only */
    SELECT 'Employee updated successfully' AS Message;
END;

-- Update only the employee name by EmpId
EXEC SP_UpdateEmployeeName 1, 'SeemaSimran';




/*-----------------------------------------------------------------
        To Delete The Employees Details (PUT)
-------------------------------------------------------------------*/
go

CREATE or alter  PROCEDURE SP_DeleteEmployee
(
    @EmpId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    /* NULL validation */
    IF (@EmpId IS NULL OR @EmpId <= 0)
    BEGIN
        SELECT 
            @EmpId AS EmpId,
            'EmpId is required and must be greater than 0' AS Message;
        RETURN;
    END

    /* Check if Employee exists */
    IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmpId = @EmpId)
    BEGIN
        SELECT 
            @EmpId AS EmpId,
            'Employee not found' AS Message;
        RETURN;
    END

    /* Delete Employee */
    DELETE FROM Employee WHERE EmpId = @EmpId;

    /* Success response */
    SELECT 
        @EmpId AS EmpId,
        'Employee deleted successfully' AS Message;
END;

exec SP_DeleteEmployee 66

SELECT * FROM Department;
SELECT * FROM Employee;


