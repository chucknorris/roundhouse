CREATE OR REPLACE FUNCTION rh_createuser() RETURNS void AS $$
DECLARE 
    t_exists integer;
BEGIN
    SELECT INTO t_exists COUNT(*) FROM pg_roles where lower(rolname) = lower('rob');
    IF t_exists = 0 THEN
      EXECUTE 'CREATE USER rob WITH PASSWORD ''RHr0x0r!'';';
    END IF;
END;
$$ LANGUAGE 'plpgsql';
SELECT rh_createuser();
DROP FUNCTION rh_createuser();

GRANT ALL PRIVILEGES ON DATABASE {{DatabaseName}} to rob;