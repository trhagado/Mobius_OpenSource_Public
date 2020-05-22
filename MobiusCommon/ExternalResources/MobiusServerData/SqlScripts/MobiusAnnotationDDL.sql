/* MobiusAnnotationDDL.sql - Mobius data definition stuff */

/
  CREATE TABLE "MBS_OWNER"."MBS_ADW_RSLT" 
   (	"RSLT_ID" NUMBER(12,0) NOT NULL ENABLE, 
	"RSLT_GRP_ID" NUMBER(12,0) NOT NULL ENABLE, 
	"EXT_CMPND_ID_TXT" VARCHAR2(32 BYTE) NOT NULL ENABLE, 
	"EXT_CMPND_ID_NBR" NUMBER(12,0), 
	"SRC_DB_ID" NUMBER(12,0), 
	"MTHD_VRSN_ID" NUMBER(12,0), 
	"RSLT_TYP_ID" NUMBER(12,0), 
	"RSLT_VAL_PRFX_TXT" VARCHAR2(2 BYTE), 
	"RSLT_VAL_NBR" NUMBER, 
	"RSLT_VAL_TXT" VARCHAR2(4000 BYTE), 
	"RSLT_VAL_DT" DATE, 
	"UOM_ID" NUMBER(12,0), 
	"CMNT_TXT" VARCHAR2(500 BYTE), 
	"DC_LNK" VARCHAR2(500 BYTE), 
	"STS_ID" NUMBER(12,0) NOT NULL ENABLE, 
	"STS_DT" DATE NOT NULL ENABLE, 
	"CHNG_OP_CD" VARCHAR2(1 BYTE) NOT NULL ENABLE, 
	"CHNG_USR_ID" VARCHAR2(12 BYTE) NOT NULL ENABLE, 
	"CRT_DT" DATE NOT NULL ENABLE, 
	"UPDT_DT" DATE NOT NULL ENABLE, 
	 CONSTRAINT "MBS_ADW_RSLT_PK" PRIMARY KEY ("RSLT_ID")
  USING INDEX PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 131072 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_INDEX_T01"  ENABLE
   ) PCTFREE 10 PCTUSED 40 INITRANS 1 MAXTRANS 255 
 NOCOMPRESS LOGGING
  STORAGE(INITIAL 131072 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_DATA_T01" ;

   COMMENT ON COLUMN "MBS_OWNER"."MBS_ADW_RSLT"."EXT_CMPND_ID_TXT" IS 'Contains the Serial Number (GSM), Biosel identifier, or alpha-numeric compound identifier from a structure database.';
   COMMENT ON COLUMN "MBS_OWNER"."MBS_ADW_RSLT"."EXT_CMPND_ID_NBR" IS 'Contains the numeric substance identifer. Serial Number for GSM or Biosel identifier.';
   COMMENT ON COLUMN "MBS_OWNER"."MBS_ADW_RSLT"."STS_ID" IS 'May be used as a foreign key column for a look-up table in the future.';
/

  CREATE INDEX "MBS_OWNER"."MBS_ADW_RSLT_A01" ON "MBS_OWNER"."MBS_ADW_RSLT" ("MTHD_VRSN_ID") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_DATA_T01" ;
/
  CREATE INDEX "MBS_OWNER"."MBS_ADW_RSLT_A02" ON "MBS_OWNER"."MBS_ADW_RSLT" ("EXT_CMPND_ID_NBR") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_DATA_T01" ;
/  
  CREATE INDEX "MBS_OWNER"."MBS_ADW_RSLT_A03" ON "MBS_OWNER"."MBS_ADW_RSLT" ("EXT_CMPND_ID_TXT") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_DATA_T01" ;
/

  CREATE INDEX "MBS_OWNER"."MBS_ADW_RSLT_A04" ON "MBS_OWNER"."MBS_ADW_RSLT" ("MTHD_VRSN_ID", "EXT_CMPND_ID_NBR") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_DATA_T01" ;

/* PRD857 uses mbs_index_t01 */

   CREATE INDEX "MBS_OWNER"."MBS_ADW_RSLT_A01" ON "MBS_OWNER"."MBS_ADW_RSLT" ("EXT_CMPND_ID_TXT", "MTHD_VRSN_ID", "RSLT_TYP_ID", "RSLT_VAL_NBR") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 131072 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_INDEX_T01" ;



/**********************************************************************/
/* Database function and associated table of SQL to allow metatable 
/* queries to be executed from Spotfire via Information links  
/**********************************************************************/
/
ANALYZE TABLE MBS_ADW_RSLT ESTIMATE STATISTICS sample 10 percent
/
ANALYZE TABLE MBS_ADW_RSLT COMPUTE STATISTICS
/
SELECT /*+ no_index (mbs_adw_rslt mbs_adw_rslt_a02 mbs_adw_rslt_a04) */
	ext_cmpnd_id_nbr,
	mthd_vrsn_id,
	rslt_typ_id,
	rslt_val_prfx_txt,
	rslt_val_nbr,
	rslt_val_txt,
	rslt_val_dt,
	dc_lnk,
	rslt_id,
	rslt_grp_id
FROM MBS_OWNER.mbs_adw_rslt
WHERE mthd_vrsn_id IN (511477)
	AND sts_id = 1
	AND ext_cmpnd_id_nbr IN (1,2,3,4,5)
/  
select *
from mbs_adw_rslt 
where rslt_id = 3650321791
/
where mthd_vrsn_id > 423711
/
CREATE INDEX "MBS_OWNER"."MBS_ADW_RSLT_A08" ON "MBS_OWNER"."MBS_ADW_RSLT" (
EXT_CMPND_ID_NBR,
STS_ID, 
MTHD_VRSN_ID,
RSLT_TYP_ID,
rslt_grp_id,
rslt_id,
rslt_val_prfx_txt,
rslt_val_nbr,
rslt_val_txt,
rslt_val_dt,
dc_lnk) 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 131072 NEXT 131072 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_DATA_T01" ;
/  
CREATE INDEX "MBS_OWNER"."MBS_ADW_RSLT_A11" ON "MBS_OWNER"."MBS_ADW_RSLT" ("MTHD_VRSN_ID") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_DATA_T01" ;
/  
CREATE INDEX "MBS_OWNER"."MBS_ADW_RSLT_A12" ON "MBS_OWNER"."MBS_ADW_RSLT" (ext_cmpnd_id_nbr) 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_DATA_T01" ;
/  
CREATE INDEX "MBS_OWNER"."MBS_ADW_RSLT_A13" ON "MBS_OWNER"."MBS_ADW_RSLT" ("MTHD_VRSN_ID", ext_cmpnd_id_nbr) 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_DATA_T01" ;
/
/  
CREATE INDEX "MBS_OWNER"."MBS_ADW_RSLT_A14" ON "MBS_OWNER"."MBS_ADW_RSLT" (ext_cmpnd_id_nbr, "MTHD_VRSN_ID") 
  PCTFREE 10 INITRANS 2 MAXTRANS 255 COMPUTE STATISTICS 
  STORAGE(INITIAL 65536 NEXT 1048576 MINEXTENTS 1 MAXEXTENTS 2147483645
  PCTINCREASE 0 FREELISTS 1 FREELIST GROUPS 1
  BUFFER_POOL DEFAULT)
  TABLESPACE "MBS_DATA_T01" ;

/
//drop table MBS_ADW_RSLT

/
/* create table as select (CTAS) in desired order (doesn't seem to work) */
CREATE TABLE MBS_ADW_RSLT
as 
 select * 
 from mbs_owner.mbs_usr_obj@prd857_link
/ 
 order by mthd_vrsn_id, ext_cmpnd_id_nbr,	rslt_grp_id, rslt_id)
/
insert /*+ APPEND */ into mbs_owner.mbs_adw_rslt 
  select * from mbs_owner.mbs_adw_rslt@prd857_link
  where mthd_vrsn_id = 708070 
  order by ext_cmpnd_id_nbr, rslt_grp_id, rslt_id
/  
//truncate table MBS_ADW_RSLT drop storage
/
select count(*)
from MBS_ADW_RSLT
/
//drop index MBS_ADW_RSLT_A08
/
//delete from mbs_usr_obj
/
insert /*+ APPEND */ into mbs_owner.mbs_usr_obj 
  (select * from mbs_owner.mbs_usr_obj@prd857_link
  where obj_id in(708065,708070,511477,700577,708071,493853,444368,659450,675092,423711,300935,298752,530065))
/  
select min(rslt_id), max(rslt_id) 
from MBS_ADW_RSLT 
/
where mthd_vrsn_id in(708065,708070,511477,700577,708071,493853,444368,659450,675092,423711,300935,298752,530065)
/
select count(*)
from MBS_ADW_RSLT   
where rslt_id between 1310251601 and 4331096190
/
create or replace function  
 get_mobius_sql (sql_name varchar2) 
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
  NAME     VARCHAR2(256 BYTE)                   NOT NULL,
  SQL      CLOB,
  OWNR_ID  VARCHAR2(32 BYTE),
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
 stringKey varchar2(256))
on commit preserve rows

truncate table tempkeylist1

insert into tempkeylist1 (intkey) 
 select lilly_nbr
 from LILLY_OWNER.lilly_moltable@prd867_link
 where lilly_nbr between 1 and 10000
