-- users database
CREATE TABLE public.USR_Users
(
    USR_Id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 ),
    USR_FirstName character varying(255),
    USR_LastName character varying(255),
    USR_Email character varying(255),
    USR_FamilyId integer,
    USR_CreatedAt timestamp without time zone,
    USR_CreatedBy integer,
    USR_LastUpdatedAt timestamp without time zone,
    USR_LastUpdatedBy integer,
	USR_IsEmailValidated boolean,
	USR_EmailValidationHash character varying(36),
    USR_IsDisabled boolean,
    PRIMARY KEY (USR_Id)
);

ALTER TABLE IF EXISTS public.USR_Users
    OWNER to postgres;

CREATE TABLE public.ATH_Authentications
(
    ATH_UserId integer,
    ATH_HashPassword character varying(4000),
	ATH_HashSalt character varying(4000),
	ATH_IsTemporaryPassword boolean,
    PRIMARY KEY (ATH_UserId),
    CONSTRAINT FK_UserId_USR_Users FOREIGN KEY (ATH_UserId)
        REFERENCES public.USR_Users (USR_Id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS public.ATH_Authentications
    OWNER to postgres;

CREATE TABLE public.APP_Applications
(
    APP_Id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 ),
    APP_Code character varying(255) NOT NULL,
    APP_Name character varying(255) NOT NULL,
    APP_Description character varying(255),
    APP_UrlPath character varying(4000),
    PRIMARY KEY (APP_Id),
    CONSTRAINT UQ_APP_CODE UNIQUE (APP_Code)
);

ALTER TABLE IF EXISTS public.APP_Applications
    OWNER to postgres;
	
CREATE TABLE public.RQA_RequestAccesses
(
    RQA_Id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 ),
    RQA_Name character varying(255),
    RQA_Description character varying(255),
    RQA_Path character varying(4000),
    RQA_Type character varying(255),
    RQA_IsAuthenticationNeeded boolean,
    RQA_ApplicationId integer,
    PRIMARY KEY (RQA_Id),
    CONSTRAINT FK_ApplicationId_APP_Applications FOREIGN KEY (RQA_ApplicationId)
        REFERENCES public.APP_Applications (APP_Id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS public.RQA_RequestAccesses
    OWNER to postgres;
	
CREATE TABLE public.RLE_Roles
(
    RLE_Id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 ),
    RLE_Code character varying(255),
    RLE_Name character varying(255),
    RLE_Description character varying(255),
    RLE_ApplicationId integer,
    PRIMARY KEY (RLE_Id),
    CONSTRAINT FK_ApplicationId_APP_Applications FOREIGN KEY (RLE_ApplicationId)
        REFERENCES public.APP_Applications (APP_Id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS public.RLE_Roles
    OWNER to postgres;
	
CREATE TABLE public.RRA_RoleRequestAccesses
(
    RRA_RoleId integer NOT NULL,
    RRA_RequestAccessId integer NOT NULL,
    PRIMARY KEY (RRA_RoleId, RRA_RequestAccessId),
    CONSTRAINT FK_RoleId_RLE_Roles FOREIGN KEY (RRA_RoleId)
        REFERENCES public.RLE_Roles (RLE_Id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT FK_RequestAccessId_RQA_RequestAccessess FOREIGN KEY (RRA_RequestAccessId)
        REFERENCES public.RQA_RequestAccesses (RQA_Id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS public.RRA_RoleRequestAccesses
    OWNER to postgres;
	
CREATE TABLE public.URR_UserRoles
(
    URR_UserId integer NOT NULL,
    URR_RoleId integer NOT NULL,
    URR_CreatedAt timestamp without time zone,
    URR_CreatedBy integer,
    PRIMARY KEY (URR_UserId, URR_RoleId),
    CONSTRAINT FK_UserId_USR_Users FOREIGN KEY (URR_UserId)
        REFERENCES public.USR_Users (USR_Id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT FK_RoleId_RLE_Roles FOREIGN KEY (URR_RoleId)
        REFERENCES public.RLE_Roles (RLE_Id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS public.URR_UserRoles
    OWNER to postgres;
