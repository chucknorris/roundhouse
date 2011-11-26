--DROP FUNCTION IF EXISTS ufn_CountTimmy;

CREATE OR REPLACE FUNCTION  ufn_CountTimmy (in i integer) 
RETURNS integer AS 
$$
BEGIN
SELECT COUNT(*) INTO i FROM Timmy;
return i;

END;
$$
LANGUAGE plpgsql VOLATILE;
