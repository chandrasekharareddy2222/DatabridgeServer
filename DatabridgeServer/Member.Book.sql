CREATE TABLE Books
(
   Bookid INT IDENTITY(1,1) PRIMARY KEY ,
   Bookname VARCHAR(30)
);

CREATE TABLE Members
(
   Memberid INT identity(1,1) PRIMARY KEY,
   MemberName VARCHAR(50),
   MemberAge INT,
   Bookid INT,
   CONSTRAINT FK_Members_Books
   FOREIGN KEY (Bookid) REFERENCES Books(Bookid)
);
go
 CREATE OR ALTER PROCEDURE InsertMemberandBooks
(
    @BookName   VARCHAR(50),
    @MemberName VARCHAR(50),
    @MemberAge  INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BookId INT;

    -- Validate Age
    IF (@MemberAge < 1 OR @MemberAge > 80)
    BEGIN
        SELECT 'ERROR' AS Status, 'Invalid member age' AS Message;
        RETURN;
    END

    -- Insert or get Book
    IF NOT EXISTS (SELECT 1 FROM Books WHERE BookName = @BookName)
    BEGIN
        INSERT INTO Books (BookName)
        VALUES (@BookName);

        SET @BookId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        SELECT @BookId = BookId FROM Books WHERE BookName = @BookName;
    END

    -- Check duplicate member-book
    IF EXISTS (
        SELECT 1 FROM Members
        WHERE MemberName = @MemberName AND BookId = @BookId
    )
    BEGIN
        SELECT 'DUPLICATE' AS Status,
               'This member is already assigned to this book' AS Message;
        RETURN;
    END

    -- Insert Member
    INSERT INTO Members (MemberName, MemberAge, BookId)
    VALUES (@MemberName, @MemberAge, @BookId);

    -- Success response
    SELECT 'SUCCESS' AS Status,
           'Member added successfully' AS Message;
END;


Exec InsertMemberandBooks
@Bookname='Game of Thrones',
@MemberName ='Ajay',
@MemberAge =90;

SELECT * FROM Books
SELECT * FROM Members

Select m.Memberid,m.MemberName,M.MemberAge,b.Bookname
From Members m
Join Books b
ON m.Bookid=b.Bookid
WHERE m.Bookid=2;








