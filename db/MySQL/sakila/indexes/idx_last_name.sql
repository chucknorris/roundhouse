
DELIMITER $$
;
DROP PROCEDURE IF EXISTS powerup_drop_index_if_exists $$
;
CREATE PROCEDURE powerup_drop_index_if_exists(in tableschema varchar(128), in theTable varchar(128), in theIndexName varchar(128) )
BEGIN
 IF((SELECT COUNT(*) AS index_exists FROM information_schema.statistics WHERE TABLE_SCHEMA = tableschema and table_name =
theTable AND index_name = theIndexName) > 0) THEN
   SET @s = CONCAT('DROP INDEX ' , theIndexName , ' ON ' , theTable);
   PREPARE stmt FROM @s;
   EXECUTE stmt;
 END IF;
END $$
;
DELIMITER ;
;
CALL powerup_drop_index_if_exists('sakila', 'customer', 'idx_last_name');
DROP PROCEDURE IF EXISTS powerup_drop_index_if_exists;

CREATE INDEX idx_last_name ON customer(
    last_name
);