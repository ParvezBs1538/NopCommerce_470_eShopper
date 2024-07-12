CREATE PROCEDURE `nop_splitstring_to_table`(string LONGTEXT,      delimiter CHAR(1))
SWL_return:
BEGIN
-- This procedure was converted on Wed Aug 03 13:31:31 2022 using Ispirer SQLWays 10.22.7 Build 7072 64bit Licensed to asdasdas - sadasdasd - Algeria - Ispirer SQLWays Toolkit 10 Microsoft SQL Server to MySQL Database Migration Demo License (1 month, 20220903).
   DECLARE start INT; 
   DECLARE end INT;      
   DROP TEMPORARY TABLE IF EXISTS nop_splitstring_to_table_output;
   CREATE TEMPORARY TABLE IF NOT EXISTS nop_splitstring_to_table_output
   (      
      data LONGTEXT  
   );
   SET start = 1;
   SET end = LOCATE(delimiter,string);        
   WHILE start < CHAR_LENGTH(RTRIM(string))+1 DO          
      IF end = 0 then               
         SET end = CHAR_LENGTH(RTRIM(string))+1;
      end if;            
      INSERT INTO nop_splitstring_to_table_output(data)           VALUES(SUBSTRING(string, start,end -start));          
      SET start = end+1;          
      SET end = LOCATE(delimiter,string,start);
   END WHILE;      
   LEAVE SWL_return;  
END
