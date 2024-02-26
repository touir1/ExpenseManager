-- expenses database
CREATE TABLE public."MQ_USR_Users"
(
    "USR_Id" integer NOT NULL,
    "USR_FirstName" character varying(255),
    "USR_LastName" character varying(255),
    "USR_Email" character varying(255),
    "USR_FamilyId" integer,
    "USR_IsDeleted" boolean,
    PRIMARY KEY ("USR_Id")
);

ALTER TABLE IF EXISTS public."MQ_USR_Users"
    OWNER to postgres;