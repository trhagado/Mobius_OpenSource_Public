/*******************************/
/****** Annotation table SQL ***/
/*******************************/
/
select *
from mbs_owner.mbs_usr_obj  
where obj_typ_id=6
and ownr_id = 'C049476'
/

SELECT obj_id,
  obj_typ_id,
  ownr_id,
  obj_nm,
  obj_desc_txt,
  fldr_typ_id,
  fldr_nm,
  acs_lvl_id,
  obj_itm_cnt,
  obj_cntnt,
  crt_dt,
  upd_dt,
  acl
FROM MBS_OWNER.mbs_usr_obj
WHERE obj_id=875639;
/

/* SQL for the DWS service for the Mobius Annotation Tables on PRD857 */

/************************************************************************/
/* Get list of all annotation tables that current user account has access to */
/* The table_def_xml value will have to be parsed by the service user to get the column information for the table */
/* The particular userId = C049476 should be replaced with a parameter representing the current DWS userID */
/************************************************************************/

select 
 obj_id mthd_vrsn_id, /* annotation table id */
 ownr_id, /* table owner */
 obj_nm table_name, /* table name */
 acs_lvl_id, /* access level: 1 = private, 2 = public, 3 = ACL */
 acl, /* access control list if acs_lvl_id = 3 */
 obj_cntnt table_def_xml, /* XML describing table and columns */
 crt_dt, /* Table creation date */
 upd_dt /* Table update date */
from mbs_owner.mbs_usr_obj  
where obj_typ_id=6 /* annotation table or user structure table */
and fldr_nm not like 'USERDATABASE%' /* filter out user structure tables */
and (
 ownr_id = 'C049476' /* tables owned by current user id */
 or acs_lvl_id = 2 /* public access */
 or (acs_lvl_id = 3 and acl like '%"C049476"%') /* acl giving access to current user ID */
 or (acs_lvl_id = 3 and acl like '%"Public"%')) /* acl with public access */
/

/************************************************************************/
/* Select all data for a particular annotation table */
/* The particular mthd_vrsn_id  = 235569 should be replaced with a parameter in DWS */
/************************************************************************/

select 
 rslt_id, /* result id  (primary key) */
 rslt_grp_id, /* result group (i.e. all rows with the same rslt_group_id should be pivoted into a single annotation table row */
 ext_cmpnd_id_nbr lsn, /* LSN (numeric) */
 ext_cmpnd_id_txt lsn_text, /* LSN (string) */
 mthd_vrsn_id, /* annotation table id */
 rslt_typ_id, /* result type id */ 
 rslt_val_prfx_txt, /* value prefix (i.e < or >) */
 rslt_val_nbr, /* numeric value if a rslt_type_id is a numeric type */
 rslt_val_txt, /* string value  if a rslt_type_id is a text type */
 rslt_val_dt, /* date value  if a rslt_type_id is a date type */
 dc_lnk, /* hyperlink */
 crt_dt, /* database insert date */
 updt_dt /* database update date */
from mbs_owner.mbs_adw_rslt
where sts_id = 1
 and mthd_vrsn_id = 87569;
/

/************************************************************************/
/* Select all data for a list of lsns */
/* The particular LSN list, ext_cmpnd_id_nbr in (1,2), should be replaced with a parameter in DWS */
/************************************************************************/

select 
 rslt_id, /* result id  (primary key) */
 rslt_grp_id, /* result group (i.e. all rows with the same rslt_group_id should be pivoted into a single annotation table row */
 ext_cmpnd_id_nbr lsn, /* LSN (numeric) */
 ext_cmpnd_id_txt lsn_text, /* LSN (string) */
 mthd_vrsn_id, /* annotation table id */
 rslt_typ_id, /* result type id */ 
 rslt_val_prfx_txt, /* value prefix (i.e < or >) */
 rslt_val_nbr, /* numeric value if a rslt_type_id is a numeric type */
 rslt_val_txt, /* string value  if a rslt_type_id is a text type */
 rslt_val_dt, /* date value  if a rslt_type_id is a date type */
 dc_lnk, /* hyperlink */
 crt_dt, /* database insert date */
 updt_dt /* database update date */
from mbs_owner.mbs_adw_rslt
where sts_id = 1
 and ext_cmpnd_id_nbr in (1)

/

/* Number of data rows */
select count(*)
from mbs_owner.mbs_adw_rslt
where sts_id = 1
/
/* Clear the in-mem Oracle row caches for the table */
select count(*)
FROM MBS_OWNER.mbs_adw_rslt
WHERE dc_lnk = 'xxx'
/

/* Get number of rows for each annot table (mthd_vrsn_id) */
select * from 
(
select mthd_vrsn_id, count(mthd_vrsn_id) count 
 FROM MBS_OWNER.mbs_adw_rslt
 WHERE sts_id = 1
 group by mthd_vrsn_id)
order by mthd_vrsn_id
/ 

select count(*) /* count rows for method */  
 FROM MBS_OWNER.mbs_adw_rslt
 WHERE mthd_vrsn_id IN (580347)
  AND sts_id = 1
  
select *  
FROM MBS_OWNER.mbs_adw_rslt
WHERE mthd_vrsn_id IN (670771)
 AND sts_id = 1


select count(*) from ( /* count lsns for method */
 SELECT  ext_cmpnd_id_nbr, count(*)
  FROM MBS_OWNER.mbs_adw_rslt
 WHERE mthd_vrsn_id IN (580347)
  AND sts_id = 1
 group by  ext_cmpnd_id_nbr  
)
 
select count(*) from ( /* count rslt_grp_ids for method */
 SELECT  rslt_grp_id, count(*)
  FROM MBS_OWNER.mbs_adw_rslt
 WHERE mthd_vrsn_id IN (580347)
  AND sts_id = 1
 group by rslt_grp_id  
)

select count(*) from ( /* exercise index A05 */
 SELECT  ext_cmpnd_id_nbr, sts_id, mthd_vrsn_id, rslt_typ_id, count(*)
  FROM MBS_OWNER.mbs_adw_rslt
 group by ext_cmpnd_id_nbr, sts_id, mthd_vrsn_id, rslt_typ_id  
)                           

select count(*) from ( /* exercise index A06 */
 SELECT  mthd_vrsn_id, sts_id, rslt_typ_id, count(*)
  FROM MBS_OWNER.mbs_adw_rslt
 group by  mthd_vrsn_id, sts_id, rslt_typ_id  
)                           
                              
select count(*) /* full mbs_adw_rslt table scan */
FROM MBS_OWNER.mbs_adw_rslt
where rslt_val_txt is null or rslt_val_txt <> 'xxx'

select count(*) /* full scan for mthd_vrsn_id = 580347 */
FROM MBS_OWNER.mbs_adw_rslt
where  mthd_vrsn_id IN (580347)
 and (rslt_val_txt is null or rslt_val_txt <> 'xxx')
 
/************************************************************************/
/* Temp Queries */
/************************************************************************/
/
update MBS_OWNER.mbs_usr_obj set ownr_id = 'YS08363' where obj_id = 272884
/

select /*+ hint */ 
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
				from MBS_OWNER.mbs_adw_rslt
				WHERE mthd_vrsn_id IN (801537,821018,801226,823623) AND 
        sts_id = 1 AND 
        ext_cmpnd_id_nbr IN ('03479500','03479065','03478838','03475213','03474157','03472774','03470748','03470327','03467567','03464504','03462511','03461912','03461756','03460235','03456586','03456579','03454922','03453652','03450598','03450130','03449299','03448824','03448572','03444273','03444036','03443997','03443821','03443707','03443483','03442564','03442133','03442130','03441297','03441295','03441039','03441034','03441033','03441032','03441030','03440316','03440315','03439485','03439366','03439235','03439076','03438804','03437692','03437691','03437689','03437688')
/        