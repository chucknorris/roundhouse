DROP FUNCTION IF EXISTS ufn_CountTimmy
;
delimiter //
;
CREATE FUNCTION  ufn_CountTimmy (i INT) RETURNS int
BEGIN
SELECT COUNT(*) INTO i FROM Timmy;
return i;
END//
;
delimiter ;
;