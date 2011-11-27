CREATE SEQUENCE sample_items_sequence
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
	
    create table SampleItems (
       id integer DEFAULT nextval('sample_items_sequence'::regclass) NOT NULL,
       name VARCHAR(255) null,
       firstname VARCHAR(255) null,
       lastname VARCHAR(255) null
    )
