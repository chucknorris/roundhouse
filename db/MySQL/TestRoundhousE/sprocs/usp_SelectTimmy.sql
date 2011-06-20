DROP PROCEDURE IF EXISTS usp_SelectTimmy;
;
delimiter //
;
CREATE PROCEDURE usp_SelectTimmy()
BEGIN
  SELECT * FROM Timmy;
END//
;
delimiter ;
;