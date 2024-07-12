SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create or Alter FUNCTION [dbo].[nop_splitstring_to_table]  (      @string NVARCHAR(MAX),      @delimiter CHAR(1)  )  RETURNS @output TABLE(      data NVARCHAR(MAX)  )  BEGIN      DECLARE @start INT, @end INT      SELECT @start = 1, @end = CHARINDEX(@delimiter, @string)        WHILE @start < LEN(@string) + 1 BEGIN          IF @end = 0               SET @end = LEN(@string) + 1            INSERT INTO @output (data)           VALUES(SUBSTRING(@string, @start, @end - @start))          SET @start = @end + 1          SET @end = CHARINDEX(@delimiter, @string, @start)      END      RETURN  END  