/* MobiusDDL.sql - Mobius data definitions for basic required Mobius tables */

/**************************/
/*** Table: MBS_SEQUENCES ***/
/* Used to simulate Oracle Sequences in MySql */
/**************************/

  CREATE TABLE MBS_OWNER.MBS_SEQUENCES
  (
     NAME VARCHAR(256) NOT NULL UNIQUE,   -- multiple counters can be stored in this table, this is its id
     VALUE BIGINT                            -- current value of the counter
  );
  
  /* Create the sequences */
  
  INSERT INTO MBS_OWNER.MBS_SEQUENCES VALUES ('MBS_USR_OBJ_SEQ', 1); 
  INSERT INTO MBS_OWNER.MBS_SEQUENCES VALUES ('MBS_ADW_RSLT_SEQ', 1); 
  INSERT INTO MBS_OWNER.MBS_SEQUENCES VALUES ('UCDB_DB_ID_SEQ', 1); 
  INSERT INTO MBS_OWNER.MBS_SEQUENCES VALUES ('MBS_LOG_SEQ', 1); 
  INSERT INTO MBS_OWNER.MBS_SEQUENCES VALUES ('MBS_SPOTFIRE_SQL_SEQ', 1); 
  INSERT INTO MBS_OWNER.MBS_SEQUENCES VALUES ('UCDB_CMPND_ID_SEQ', 1); 
  INSERT INTO MBS_OWNER.MBS_SEQUENCES VALUES ('UCDB_ALIAS_ID_SEQ', 1); 
  INSERT INTO MBS_OWNER.MBS_SEQUENCES VALUES ('UCDB_DB_MODEL_ID_SEQ', 1); 

  /* Get the current value and increment */

  UPDATE MBS_OWNER.MBS_SEQUENCES 
  SET value = LAST_INSERT_ID(value) + 1
  WHERE name = 'sequenceName';
  
  SELECT LAST_INSERT_ID(); -- gets value BEFORE update (must add one to get current value)

/**************************/
/*** Table: MBS_USR_OBJ ***/
/**************************/

  CREATE TABLE MBS_OWNER.MBS_USR_OBJ (
	OBJ_ID BIGINT NOT NULL PRIMARY KEY, 
	OBJ_TYP_ID BIGINT NOT NULL, 
	OWNR_ID VARCHAR(32) NOT NULL, 
	OBJ_NM VARCHAR(256) NOT NULL, 
	OBJ_DESC_TXT VARCHAR(4000), 
	FLDR_TYP_ID BIGINT, 
	FLDR_NM VARCHAR(100) NOT NULL, 
	ACS_LVL_ID BIGINT, 
	OBJ_ITM_CNT BIGINT NOT NULL, 
	OBJ_CNTNT LONGTEXT, 
	CRT_DT DATE NOT NULL, 
	UPD_DT DATE NOT NULL, 
	CHNG_OP_CD VARCHAR(1) NOT NULL, 
	CHNG_USR_ID VARCHAR(32) NOT NULL, 
	ACL VARCHAR(4000));
    
CREATE UNIQUE INDEX MBS_USR_OBJ_U01 ON MBS_OWNER.MBS_USR_OBJ(OBJ_TYP_ID, OWNR_ID, FLDR_NM, OBJ_NM);
CREATE INDEX MBS_USR_OBJ_A01 ON MBS_OWNER.MBS_USR_OBJ (ACS_LVL_ID, OBJ_TYP_ID, OWNR_ID); 
CREATE INDEX MBS_USR_OBJ_A02 ON MBS_OWNER.MBS_USR_OBJ (FLDR_NM);

/**************************/
/* Table: MBS_ADW_RSLT */
/**************************/

  CREATE TABLE MBS_OWNER.MBS_ADW_RSLT ( 
    RSLT_ID BIGINT NOT NULL PRIMARY KEY, 
	RSLT_GRP_ID BIGINT NOT NULL, 
	EXT_CMPND_ID_TXT VARCHAR(32) NOT NULL, 
	EXT_CMPND_ID_NBR BIGINT, 
	SRC_DB_ID BIGINT, 
	MTHD_VRSN_ID BIGINT, 
	RSLT_TYP_ID BIGINT, 
	RSLT_VAL_PRFX_TXT VARCHAR(2), 
	RSLT_VAL_NBR DOUBLE, 
	RSLT_VAL_TXT VARCHAR(4000), 
	RSLT_VAL_DT DATE, 
	UOM_ID BIGINT, 
	CMNT_TXT VARCHAR(500), 
	DC_LNK VARCHAR(500), 
	STS_ID BIGINT NOT NULL, 
	STS_DT DATE NOT NULL, 
	CHNG_OP_CD VARCHAR(1) NOT NULL, 
	CHNG_USR_ID VARCHAR(32) NOT NULL, 
	CRT_DT DATE NOT NULL, 
	UPDT_DT DATE NOT NULL);

  CREATE INDEX MBS_ADW_RSLT_A01 ON MBS_OWNER.MBS_ADW_RSLT (MTHD_VRSN_ID);
  CREATE INDEX MBS_ADW_RSLT_A02 ON MBS_OWNER.MBS_ADW_RSLT (EXT_CMPND_ID_NBR);
  CREATE INDEX MBS_ADW_RSLT_A03 ON MBS_OWNER.MBS_ADW_RSLT (EXT_CMPND_ID_TXT); 
  CREATE INDEX MBS_ADW_RSLT_A04 ON MBS_OWNER.MBS_ADW_RSLT (MTHD_VRSN_ID, EXT_CMPND_ID_NBR); 

/**************************/
 /* Table MBS_LOG */
/**************************/

  CREATE TABLE MBS_OWNER.MBS_LOG (
   	OBJ_ID BIGINT NOT NULL PRIMARY KEY, 
	OBJ_TYP_ID BIGINT NOT NULL, 
	OWNR_ID VARCHAR(32) NOT NULL, 
	OBJ_NM VARCHAR(256) NOT NULL, 
	OBJ_DESC_TXT VARCHAR(4000), 
	FLDR_TYP_ID BIGINT, 
	FLDR_NM VARCHAR(100), 
	ACS_LVL_ID BIGINT, 
	OBJ_ITM_CNT BIGINT, 
	OBJ_CNTNT LONGTEXT, 
	CRT_DT DATE NOT NULL, 
	UPD_DT DATE NOT NULL, 
	CHNG_OP_CD VARCHAR(1) NOT NULL, 
	CHNG_USR_ID VARCHAR(32) NOT NULL);

  CREATE INDEX MBS_LOG_A01 ON MBS_OWNER.MBS_LOG (CRT_DT); 

/**************************/
/* Table CMN_ASSY_ATRBTS */
/**************************/

CREATE TABLE MBS_OWNER.CMN_ASSY_ATRBTS 
(
  ID integer NOT NULL 
, ASSY_DB VARCHAR(32) NOT NULL 
, ASSY_ID_NBR integer 
, ASSY_ID_TXT varchar(32) NOT NULL 
, ASSY_NM varchar(256) 
, ASSY_DESC varchar(4000) 
, ASSY_SRC varchar(256) 
, ASSY_TYP varchar(32) 
, ASSY_MODE varchar(32) 
, ASSY_STS varchar(32) 
, ASSY_SUM_LVL integer 
, RSLT_TYP varchar(32) 
, RSLT_TYP_NM varchar(256) 
, RSLT_TYP_ID_NBR integer 
, RSLT_TYP_ID_TXT varchar(32) 
, TOP_LVL_RSLT CHAR(1) 
, REMPPD CHAR(1) 
, MLTPLXD CHAR(1) 
, RVWD CHAR(1) 
, PRFLNG_ASSY CHAR(1) 
, CMPDS_ASSYD integer 
, RSLT_CNT integer 
, ASSY_UPDT_DT DATE 
, ASSY_GENE_CNT integer 
, GENE_ID integer 
, GENE_SYMBL varchar(32) 
, GENE_DESC varchar(256) 
, GENE_FMLY varchar(32) 
, ASSN_SRC varchar(32) 
, ASSN_CNFLCT varchar(256) 
, RSLT_UOM varchar(32) 
, CONC_UOM varchar(32) 
, X Double
, Y Double 
, Z Double
, ASSY_ID_REFDB integer 
, RSLT_TYP_ID_REFDB integer 
) ;

CREATE UNIQUE INDEX CAA_PK ON MBS_OWNER.CMN_ASSY_ATRBTS (ID ASC);
CREATE INDEX AEGA_IDX1 ON MBS_OWNER.CMN_ASSY_ATRBTS (GENE_ID ASC); 
CREATE INDEX AEGA_IDX2 ON MBS_OWNER.CMN_ASSY_ATRBTS (GENE_SYMBL ASC);
CREATE INDEX AEGA_IDX3 ON MBS_OWNER.CMN_ASSY_ATRBTS (ASSY_DB ASC, ASSY_ID_NBR ASC, RSLT_TYP ASC); 
CREATE INDEX AEGA_IDX4 ON MBS_OWNER.CMN_ASSY_ATRBTS (ASSY_DB ASC, ASSY_ID_NBR ASC, RSLT_TYP_ID_NBR ASC); 
CREATE INDEX AEGA_IDX5 ON MBS_OWNER.CMN_ASSY_ATRBTS (GENE_FMLY ASC, ASSY_DB ASC, ASSY_ID_NBR ASC, RSLT_TYP_ID_NBR ASC);
CREATE INDEX AEGA_IDX6 ON MBS_OWNER.CMN_ASSY_ATRBTS (ASSY_TYP ASC, ASSY_DB ASC, ASSY_ID_NBR ASC, RSLT_TYP_ID_NBR ASC); 
CREATE INDEX AEGA_IDX7 ON MBS_OWNER.CMN_ASSY_ATRBTS (RSLT_TYP ASC, ASSY_DB ASC, ASSY_ID_NBR ASC, RSLT_TYP_ID_NBR ASC);
CREATE INDEX BMV_REFDB_RSLT_TYP_IDX ON MBS_OWNER.CMN_ASSY_ATRBTS (ASSY_ID_REFDB ASC, RSLT_TYP_ID_REFDB ASC);

/*************************************/

GRANT mbs_user_role to mobius_reviewer;

/

/**********************************************************************/
/* Database function and associated table of SQL to allow metatable 
/* queries to be executed from Spotfire via Information links  
/**********************************************************************/

create or replace function  
 get_mobius_sql (sql_name varchar) 
  return SYS_REFCURSOR
as
 sql_cursor SYS_REFCURSOR;
 sql_str clob := 'select assay_id, id_nbr, nm_txt
 from ngr_ss_owner.ngr_assay@prd868_link
 where id_nbr = 8958';
begin

 select sql
 into sql_str
 from mbs_owner.mbs_spotfire_sql
 where NAME = sql_name;
   
 open sql_cursor for sql_str;
 return sql_cursor;
end;

GRANT ALL ON MBS_OWNER.get_mobius_sql TO MBS_USER, MBS_USER_ROLE;

select get_mobius_sql('NGR_ASSAY')
from dual

select assay_id, dmnsn_typ_id, id_nbr, id_txt, nm_txt, dscrptn_txt, crtd_dt
 from ngr_ss_owner.ngr_assay@prd868_link
 where id_nbr = 8958

/*** Create table of Mobius metatable sql ***/

ALTER TABLE MBS_OWNER.MBS_SPOTFIRE_SQL
 DROP PRIMARY KEY CASCADE;

DROP TABLE MBS_OWNER.MBS_SPOTFIRE_SQL CASCADE CONSTRAINTS;

CREATE TABLE MBS_OWNER.MBS_SPOTFIRE_SQL
(
  NAME     varchar(256)                   NOT NULL,
  SQL      CLOB,
  OWNR_ID  varchar(32),
  CRT_DT   DATE                                 DEFAULT SYSDATE
)

select ROWID, mss.* /* select from table allowing update */ 
FROM mbs_owner.MBS_SPOTFIRE_SQL mss

GRANT ALL ON MBS_OWNER.MBS_SPOTFIRE_SQL TO MBS_USER, MBS_USER_ROLE;


/*** Sequence for MBS_METATABLE_SQL ***/

CREATE SEQUENCE MBS_OWNER.MBS_METATABLE_SQL_SEQ
  START WITH 1
  MAXVALUE 999999999999999999999999999
  MINVALUE 1
  NOCYCLE
  NOCACHE
  NOORDER;


GRANT SELECT ON MBS_OWNER.MBS_USR_OBJ_SEQ TO MBS_USER, MBS_USER_ROLE;

/**********************************************************************/
/* Tables are linked through PRD857.MBS_USER via synonyms for the 
/* following reasons:
/*  1. Same schema name needs to be referenced in 2 or more instances.
/*  2. Public synonyms can't be referenced via schema name & so can't be
/*     resolved to a instance.
/*  3. Development tables belonging to DEV857.MBS_OWNER must be accessed.
/**********************************************************************/

/* Create link from prd857.mbs_user to dev857.mbs_owner

DROP DATABASE LINK dev857_link 

CREATE DATABASE LINK dev857_link 
CONNECT TO mbs_owner 
IDENTIFIED BY ad64_mbsu1
USING 'dev857'

/* Hypothesis tracker structures */

CREATE DATABASE LINK prd586_link
CONNECT TO atlsm_browser
IDENTIFIED BY QAS34R5
USING 'prd586'

CREATE SYNONYM atlsm_samples FOR atlsm_browser.atlsm_samples@prd586_link


DROP SYNONYM iht_structure

CREATE SYNONYM iht_structure FOR mbs_owner.iht_structure@dev857_link

SELECT * FROM iht_structure

/* Promiscuity table */

DROP SYNONYM PROMISCUITY

CREATE SYNONYM PROMISCUITY FOR mbs_owner.PROMISCUITY@dev857_link

SELECT * FROM PROMISCUITY

/* Pubchem Assay data */

DROP SYNONYM MBS_PBCHM_RSLT

CREATE SYNONYM MBS_PBCHM_RSLT FOR mbs_owner.MBS_PBCHM_RSLT@dev857_link

SELECT * FROM MBS_PBCHM_RSLT

/* Create synonyms for prd121 gsm_owner tables so we can properly access the current public synonyms */

DROP SYNONYM gsm_neat_stock

CREATE SYNONYM gsm_neat_stock FOR gsm_neat_stock@prd121_link

SELECT * FROM gsm_neat_stock

===

DROP SYNONYM gsm_solubilized_stock

CREATE SYNONYM gsm_solubilized_stock FOR gsm_solubilized_stock@prd121_link

SELECT * FROM gsm_solubilized_stock

===

DROP SYNONYM gsm_tstst

CREATE SYNONYM gsm_tstst FOR gsm_tstst@prd121_link

SELECT * FROM gsm_tstst

===

DROP SYNONYM gsm_tstst_stk

CREATE SYNONYM gsm_tstst_stk FOR gsm_tstst_stk@prd121_link

SELECT * FROM gsm_tstst_stk

===

DROP SYNONYM sam_lot

CREATE SYNONYM sam_lot FOR sam_lot@prd121_link

SELECT * FROM sam_lot

===

DROP SYNONYM sam_submission

CREATE SYNONYM sam_submission FOR sam_submission@prd121_link

SELECT * FROM sam_submission

/* Create synonyms for lmd tables to separate from cmd tables with same owner */

CREATE SYNONYM lmd_cos_testset FOR cos_owner.cos_testset@prd204_link

CREATE SYNONYM lmd_cos_composite FOR cos_owner.cos_composite@prd204_link

CREATE SYNONYM lmd_eng_kit_subset FOR inv_owner.eng_kit_subset@prd204_link

CREATE SYNONYM lmd_eng_kit_design_item FOR inv_owner.eng_kit_design_item@
      prd204_link

CREATE SYNONYM lmd_submission FOR lmd_owner.lmd_submission@prd204_link

CREATE DATABASE LINK link_prd465
CONNECT TO mobius_user
IDENTIFIED BY mobius001
USING 'prd465'

/*************************************************************/
/* Create Related-Structure table */
/*************************************************************/

CREATE TABLE MBS_RLTD_STRCT
(
  mbs_rltd_strct_id NUMBER(12),
  ly_srl_NBR NUMBER(12),
  rltd_ly_srl_nbr NUMBER(12),
  rltn_typ_id NUMBER(12),
  score NUMBER,
  crt_dt DATE,
  updt_dt DATE)
  

/*************************************************************/
/* Temp key table */
/*************************************************************/
  
create global temporary table TempKeyList1 (
 rowPos integer,
 intKey integer,
 stringKey varchar(256))
on commit preserve rows

truncate table tempkeylist1

insert into tempkeylist1 (intkey) 
 select lilly_nbr
 from LILLY_OWNER.lilly_moltable@prd867_link
 where lilly_nbr between 1 and 10000
