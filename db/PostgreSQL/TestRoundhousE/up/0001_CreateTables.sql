CREATE SEQUENCE timmy_id_sequence
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE TABLE Timmy
(
  id integer DEFAULT nextval('timmy_id_sequence'::regclass) NOT NULL,
  dude varchar(50) NULL
)