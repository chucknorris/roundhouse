CREATE OR REPLACE FUNCTION ufn_GetDate 
( fake IN varchar2 )
RETURN timestamp
IS
  v_Today timestamp;
BEGIN
  SELECT SYSDATE INTO v_Today FROM dual;
RETURN v_Today;
END
;
