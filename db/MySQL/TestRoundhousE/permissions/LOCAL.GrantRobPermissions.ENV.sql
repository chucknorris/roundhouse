DROP USER IF EXISTS 'rob'@'%';

CREATE USER 'rob'@'%' IDENTIFIED BY 'RHr0x0r!';

Grant All Privileges on {{DatabaseName}}.* to 'rob'@'%';
