CREATE OR REPLACE PROCEDURE usp_GetDate 
  (v_date OUT timestamp)
AS
BEGIN
  SELECT ufn_GetDate('0') INTO v_date FROM dual;
END
;