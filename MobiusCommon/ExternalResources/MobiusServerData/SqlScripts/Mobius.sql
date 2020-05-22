/* User object types */

PUBLIC enum UserObjectType
{
    NONE          = 0,
    CnList        = 1,
    QUERY         = 2,
    Structure     = 3,
    UserParameter = 4,
    CalcField     = 5,
    Annotation    = 6,
    CondFormat    = 7,
    MetaRename    = 8  // renamed metadata
    Folder        = 9, // USER-created folder
    Alert         = 10, // NEW-data alert
    MultiTable    = 11, // metatable based ON a multiple-TABLE QUERY
    UserDatabase  = 12, // user-created compound database with associated annotation & computed data
    ImportState   = 13, // import state for user data
    BackgroundExport = 14,
    Link          = 15, // hyperlink or a link to an existing user object
    UserGroup     = 16,
    SpotfireLink  = 17, // link to Spotfire analysis that retrieves data associated with a query
}

/* Access levels */

public enum UserObjectAccess 
{
    None          = 0,
    Private       = 1,
    Public        = 2,
    ACL           = 3 // controlled via an Access Control List
}

/****************** Utility queries *******************/

select count(*)
from mbs_owner.mbs_usr_obj uo
where obj_typ_id = 5;

/

select ROWID, uo.*
from mbs_owner.mbs_usr_obj uo
where obj_id in (914176);
/

select ROWID, uo.* 
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 10
and ownr_id in ('A113347') /* Margolis, Brandon */
order by obj_id desc;
/

/* About 33 queries have references to old Smrf.NodeXL network view */
select ROWID, uo.* 
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 2
and upper(obj_cntnt) like '%NETWORKPROPERTIES%'
/

insert into mbs_owner.mbs_usr_obj
/
select *
from mbs_owner.mbs_usr_obj  
where obj_typ_id=4
and ownr_id in ('C049476')
order by upper(obj_nm);
/
insert into mbs_owner.mbs_usr_obj
select *
from mbs_owner.mbs_usr_obj_archv  
where obj_typ_id = 2
and ownr_id in ('C049476')
and upper(obj_nm) like 'CF%'
and obj_id not in (select obj_id from mbs_owner.mbs_usr_obj)
/

where obj_id in (235547)
and crt_dt > '16-nov-2015'
order by obj_id desc
/

select *
from mbs_owner.mbs_usr_obj_archv  
where obj_id in (117924);
/

select ROWID, uo.* /* search by object name */
FROM mbs_owner.MBS_USR_OBJ uo
where obj_nm like 'UCDB%';
and ownr_id in ('C049476')
order by upper(obj_nm);
/ 
and ownr_id in ('C049476')


select * /* lookup userid by user name */
FROM mbs_owner.MBS_USR_OBJ uo
where 
 obj_typ_id = 4 and
 obj_nm='NameAddress' and
 upper(obj_desc_txt) like upper('%mergott%');

select *
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id != 40
and ownr_id in ('MOBIUS_MAINT');

select ROWID, uo.*
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 6
and obj_cntnt like '%BASICCOMPPROP%'

select * from ( /* count queries by user */
 select ownr_id, count(*) count
 FROM mbs_owner.MBS_USR_OBJ uo
 where obj_typ_id = 2
  group by ownr_id
) order by count desc

xxxupdate mbs_owner.MBS_USR_OBJ
set obj_typ_id = 107
where obj_typ_id = 7 and crt_dt < '1-JAN-2016'

where obj_typ_id = 13
and 

and crt_dt > '1-jan-2014'
order by crt_dt desc

select ROWID, uo.*
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 2
and lower(obj_nm) like 'metap2mousepd%'

where obj_typ_id = 2
and obj_nm = 'Ricks slow ngr query'

and lower(obj_nm) like '08-19-09%'

order by crt_dt desc

select ROWID, uo.*
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 4
and obj_nm like 'Privilege%'
and ownr_id = 'RH80997'

and crt_dt > '1-jan-2014'
order by crt_dt desc



select ROWID, uo.*
FROM mbs_owner.MBS_USR_OBJ uo
where obj_nm like 'jsi_smiles%' 

where obj_typ_id = 4
and obj_nm in ( 'NameAddress', 'PrivilegeLogon')
and ownr_id in (
'C165030',
'C158253',
'C159365',
'YSX0386',
'C161388',
'YS10445',
'YS10550',
'C162576',
'YS10584',
'C150619',
'C160736',
'YS10550'
)
order by ownr_id, obj_nm

and obj_nm = 'NameAddress'
and obj_desc_txt not like '%|AM|%'

 
and obj_nm like 'Summarized Target Results Analysis%' 

and ownr_id = 'RX23376'
order by crt_dt desc

obj_id in(399197, 273806, 399195) 

 (318299, 318295) 

(235620, 231925, 235547, 235551, 235558)


(366800, 366801, 366802, 366803, 366804, 366775, 367220, 367221, 367222)

select count(*)
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 2 and ownr_id = 'YE76006'

 and obj_desc_txt like '\\%'

select ROWID, uo.*
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 4 

 
(obj_nm = 'SharePointSiteUrl' or obj_nm = 'DefaultExportFolder')
and ownr_id = 'A113347'

select ROWID, uo.* /* STB model query */
FROM mbs_owner.MBS_USR_OBJ uo
where obj_id = 358879

select ROWID, uo.*  
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 10 and obj_nm = 'Alert_339766'

select ROWID, uo.*  
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 4 and ownr_id = 'A113347'


select uo2.obj_id, uo2.ownr_id, uo.* /* find user objects in folders owned by other users */ 
FROM mbs_owner.MBS_USR_OBJ uo,
 mbs_owner.MBS_USR_OBJ uo2
where uo.FLDR_NM like 'FOLDER%'
 and uo2.obj_typ_id = 9 
 and uo.fldr_nm = concat('FOLDER_', uo2.obj_id)
 and uo.ownr_id != uo2.ownr_id
order by uo.crt_dt

////////////////////

select ROWID, uo.*  
FROM mbs_owner.MBS_USR_OBJ uo
where obj_id in (290187,
290097,
288966,
288958,
288943,
288886,
288879,
276729)


where crt_dt >= '1-oct-2012'
and obj_typ_id = 6

select count(*)
from mbs_owner.mbs_adw_rslt
where mthd_vrsn_id = 286333

select count(*)
from mbs_owner.mbs_adw_rslt
where mthd_vrsn_id = 286360

///////////////////////////////

delete 
FROM mbs_owner.MBS_USR_OBJ uo
where obj_id = 723808

where obj_typ_id = 2
 and lower(obj_nm) like lower('%nav1.7 new main%')
 
select ROWID, uo.*  
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 6
order by obj_id desc

and ownr_id = 'RB86799' 
and obj_nm like '%Address%'

 and lower(obj_nm) like '%false positive%'
 
and ownr_id = 'C049476' 
order by ownr_id, crt_dt 

and ownr_id = 'C049476' 
and ownr_id = 'C033073' 
and ownr_id = 'RZ97441' 
and ownr_id = 'YS09036' 

and obj_nm = 'fragment false positive risk'

select *
from  mbs_owner.MBS_USR_OBJ uo
 where obj_typ_id = 4
 and obj_nm = 'LastUse'
and not exists (
select *
 from  mbs_owner.MBS_USR_OBJ uo2
 where uo2.ownr_id = uo.ownr_id
 and obj_typ_id = 4
 and obj_nm in ('NameAddress', 'PrivilegeLogon'))
 
order by ownr_id 


order by obj_nm

and ownr_id = 'C049476'

select ROWID, uo.*
from  mbs_owner.MBS_USR_OBJ uo
 where obj_typ_id = 2
 and lower(obj_nm) like lower('0 mGlur5 Antagonist Effort 4%')

select ROWID, uo.*  
FROM mbs_owner.MBS_USR_OBJ uo
where obj_id in (177995)

where fldr_nm = 'FOLDER_313698'

where obj_typ_id = 9
and ownr_id = 'RB92426'

order by obj_id
 

and obj_cntnt like '%"CfSummaryPivot"%'


where lower(obj_nm) like '%another slow%'


/****************** Session Information ******************/

SELECT * /* get Oracle version */
FROM sys.V_$VERSION

SELECT COUNT (UNIQUE sid) /* count number of sessions */
FROM sys.V_$SESSION_CONNECT_INFO
WHERE osuser='mobius'

SELECT * /* show sessions */
FROM sys.V_$SESSION_CONNECT_INFO
WHERE osuser='mobius'
ORDER BY sid

/****************** Get query execution plan ******************/

delete plan_table

explain plan for /* put SQL here */

select * from plan_table

select /* format plan */
  substr (lpad(' ', level-1) || operation || ' (' || options || ')',1,30 )
      "Operation",
  object_name
      "Object"
from
  plan_table
start with id = 0
connect by prior id=parent_id;

/****************** Basic instance status ******************/

SELECT * FROM V$INSTANCE

/****************** Lock Checking ******************/

select * from dba_waiters

SELECT LPAD(' ',DECODE(l.xidusn,0,3,0)) || l.oracle_username "User Name",
o.owner, o.object_name, o.object_type
FROM v$locked_object l, dba_objects o
WHERE l.object_id = o.object_id
ORDER BY o.object_id, 1 desc

SELECT a.sid,a.serial#, a.username,c.os_user_name,a.terminal,
b.object_id,substr(b.object_name,1,40) object_name
from v$session a, dba_objects b, v$locked_object c
where a.sid = c.session_id
and b.object_id = c.object_id



/****************** Standard queries ******************/

SELECT * /* count by object type */
FROM mbs_owner.MBS_USR_OBJ
WHERE obj_typ_id = 5
and ownr_id = 'C049476'
and fldr_nm <> 'FOLDER_49431'

SELECT obj_typ_id,COUNT(*) count /* group by object type */
FROM mbs_owner.MBS_USR_OBJ
GROUP BY obj_typ_id
order by obj_typ_id, count desc

SELECT COUNT(*) /* number of registered users */
FROM mbs_owner.MBS_USR_OBJ
WHERE obj_typ_id = 4 and obj_nm = 'NameAddress'

SELECT ownr_id /* list of recent users */
FROM mbs_owner.MBS_USR_OBJ
WHERE obj_nm = 'LastUse'
and obj_desc_txt >= '20090000'
order by ownr_id

select ROWID,uo.*  /* UserParameter / Preference */
FROM mbs_owner.MBS_USR_OBJ uo
WHERE obj_typ_id = 4 and ownr_id = 'RX63131'

obj_nm = 'RemoveLeadingZerosFromCids'

select ROWID, uo.* /* Get updatable email address for a user */
FROM mbs_owner.MBS_USR_OBJ uo
WHERE ownr_id = 'C049476'
order by crt_dt desc

SELECT ownr_id /* list of all users within a time period */
FROM mbs_owner.MBS_USR_OBJ
where obj_typ_id = 4 and obj_nm = 'LastUse' and obj_desc_txt > 2010

/****************** Utility queries ************c******/

select ROWID, uo.* /* find queries that reference a specific table */  
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 2
and obj_cntnt like '%STAR_55186%'

select ROWID, uo.*  
FROM mbs_owner.MBS_USR_OBJ uo
where obj_typ_id = 9
and ownr_id = 'RB92426'

select folder_owner, count(*) count from (
select t2.ownr_id folder_owner, t1.ownr_id obj_owner, t1.* /* find user objects in folders owned by someone else */
from 
 mbs_owner.MBS_USR_OBJ t1,
 mbs_owner.MBS_USR_OBJ t2
where 
 t1.fldr_nm like 'FOLDER_%'
 and t1.obj_typ_id = 2
 and t2.obj_typ_id = 9
 and t2.obj_id = to_number(substr(t1.fldr_nm, 8))
 and t1.ownr_id <> 'ALERTMONITOR' 
 and t2.ownr_id <> 'ALERTMONITOR' 
 and t1.ownr_id <> t2.ownr_id
) group by folder_owner
order by folder_owner desc


select t2.ownr_id folder_owner, t1.ownr_id obj_owner, t1.* /* find user objects in folders owned by someone else */
from 
 mbs_owner.MBS_USR_OBJ t1,
 mbs_owner.MBS_USR_OBJ t2
where 
 t1.fldr_nm like 'FOLDER_%'
 and t2.obj_typ_id = 9
 and t2.obj_id = to_number(substr(t1.fldr_nm, 8))
 and t1.ownr_id <> 'ALERTMONITOR' 
 and t2.ownr_id <> 'ALERTMONITOR' 
 and t1.ownr_id <> t2.ownr_id
 and t2.ownr_id = 'YE05426'
order by t1.upd_dt 
  
 and lower(t1.obj_nm) like 'tnks scaffold definitions%'

select * from (
select obj_typ_id, upper(fldr_nm), upper(obj_nm), count(*) count
FROM mbs_owner.MBS_USR_OBJ
group by obj_typ_id, upper(fldr_nm), upper(obj_nm) 
) where count > 1

SELECT ROWID,uo.* /* utility select (updatable) */
FROM mbs_owner.MBS_USR_OBJ uo
where obj_id in (309887)

order by upd_dt desc

where obj_id between 277420 and 277440

SELECT ROWID,uo.* /* utility select (updatable) */
FROM mbs_owner.MBS_USR_OBJ uo
where lower(obj_nm) like '%--glp-1%'

where ownr_id = 'A040337'
order by upd_dt desc

xxxdelete FROM mbs_owner.MBS_USR_OBJ uo
where obj_id in (304085)

where lower(obj_nm) like '%alertannot%'

where obj_typ_id = 16

and (fldr_nm = 'FOLDER_145035' or fldr_nm = 'FOLDER_149455')
order by crt_dt desc 

where obj_id in (287713)

where obj_typ_id = 4
and obj_nm = 'Favorites'
and ownr_id like 'YE%'

SELECT ROWID,uo.* /* utility select (updatable) */
FROM mbs_owner.MBS_USR_OBJ uo
where fldr_nm = 'FOLDER_304392'

xxxupdate mbs_owner.MBS_USR_OBJ 
set fldr_nm = 'FOLDER_235857'
where fldr_nm = 'FOLDER_304392'

where ownr_id in ('RB37306', 'RB90757')
and obj_typ_id = 9

and fldr_nm = 'FOLDER_49430'

SELECT ROWID,uo.* /* utility select (updatable) */
FROM mbs_owner.MBS_USR_OBJ uo
where obj_nm like 'NewDataQueryForQuery%'
order by obj_nm

update mbs_owner.MBS_USR_OBJ /* change the owner and parent folder for a user object */
set ownr_id = 'RX32785', fldr_nm = 'FOLDER_284799'
where obj_id = 170904

xxxupdate mbs_owner.MBS_USR_OBJ uo
set fldr_nm = ' '
where obj_nm like 'NewDataQueryForQuery%'

xxxdelete from mbs_owner.MBS_USR_OBJ
where obj_id = 217756

where ownr_id = 'C049476'
order by crt_dt desc

and obj_typ_id = 4

and lower(obj_nm) in ('cf11')

SELECT ROWID,uo.* /* utility select (updatable) */
FROM mbs_owner.MBS_USR_OBJ uo
where ownr_id = 'C049476'
and lower(obj_nm) = 'test2a'

where ownr_id = 'RB37306'
where obj_id = 170580

and obj_typ_id = 12x


order by crt_dt desc

where ownr_id = 'C049476'
and obj_typ_id = 2

SELECT fldr_nm, uo.*
FROM mbs_owner.MBS_USR_OBJ uo
where ownr_id = 'C049476'
order by uo.fldr_nm

SELECT fldr_nm, count(*) cnt /* count of object types by folder */
FROM mbs_owner.MBS_USR_OBJ uo
where ownr_id = 'C049476'
group by fldr_nm
order by cnt desc

SELECT ROWID,uo.* /* find user objects where the parent folder is missing */
FROM mbs_owner.MBS_USR_OBJ uo
where fldr_nm like 'FOLDER_%'
 and ownr_id = 'C049476'
 and not exists (
  select * from MBS_USR_OBJ
  where fldr_typ_id = 256 and obj_id = substr(uo.fldr_nm,8))


SELECT ROWID,uo.* /* utility select (updatable) */
FROM mbs_owner.MBS_USR_OBJ uo
where fldr_nm = 'INSILICO_DATA' and obj_id >= 211500
order by crt_dt desc

where obj_id = 211500

where obj_nm like 'Toxfire study vs%'

where obj_id = 97626
order by obj_id


order by obj_id

where obj_nm like 'CF%'
and ownr_id = 'C049476'

update mbs_owner.MBS_USR_OBJ
set obj_id = 49430
where obj_id in (99999999)

xxxdelete FROM mbs_owner.MBS_USR_OBJ uo
      /* delete alerts associated with queries with no criteria */
where obj_id in (
45790, 46244, 72349, 107852, 116587, 128605, 132349, 163143, 168194,
185390, 215026, 224527, 265776, 273185, 274525, 275008, 276814, 276816, 276818
      , 277632)
and obj_typ_id = 10

WHERE obj_typ_id = 6
and ownr_id = 'C089562'
order by obj_id desc

WHERE obj_typ_id = 9
and ownr_id = 'C089562'


where obj_id = 143266

WHERE obj_typ_id = 6
and ownr_id = 'C089562'
order by obj_id desc

WHERE ownr_id = 'C049476'
and obj_typ_id = 2

where obj_id = 153769

WHERE ownr_id = 'YE76433'
ORDER BY upd_dt DESC

where obj_nm like 'HTMC Invent%'

WHERE ownr_id = 'C049476'
and obj_typ_id = 6
and obj_itm_cnt > 0

where obj_typ_id = 13
and ownr_id = 'A515161' /* Jim Durbin */

WHERE ownr_id = 'C049476'
order by obj_id desc


WHERE lower (obj_nm) LIKE lower('%glp1 sec%')

where obj_id = 113711

WHERE ownr_id = 'C069066' /* Dustin Mergott */
order by obj_id desc

where obj_typ_id = 13

WHERE ownr_id = 'C054438'
order by upd_dt desc

WHERE lower (obj_nm) LIKE '%ikka_obs_pred%'

ORDER BY upd_dt DESC

SELECT ROWID,uo.*  /* select TRH UserLibrary user objects */
FROM mbs_owner.MBS_USR_OBJ uo
WHERE obj_typ_id = 12 and ownr_id = 'C049476'

-delete /* delete TRH UserLibrary user objects */
FROM mbs_owner.MBS_USR_OBJ
where obj_typ_id = 14

 and ownr_id = 'C049476'

delete /* delete TRH ImportState user objects */
FROM mbs_owner.MBS_USR_OBJ
where obj_typ_id = 13 and ownr_id = 'C049476'

update mbs_owner.MBS_USR_OBJ
set obj_id = 92381 where obj_id = 92379

update mbs_owner.MBS_USR_OBJ
set fldr_nm = 'FOLDER_262028'
where ownr_id = 'C049476' and fldr_nm = 'FOLDER_49430'

WHERE ownr_id = 'C049476' AND obj_typ_id=6
and fldr_typ_id = 0

WHERE ownr_id = 'RX65379'
ORDER BY upd_dt DESC

WHERE ownr_id = 'C049476'
WHERE LOWER(obj_nm) LIKE LOWER('RARg%')
WHERE fldr_typ_id = 256

SELECT fldr_typ_id, COUNT(*)
FROM mbs_owner.MBS_USR_OBJ uo
GROUP BY fldr_typ_id

WHERE upd_dt > '19-jul-2007'

WHERE obj_id=58158

WHERE ownr_id = 'C049476'
ORDER BY upd_dt DESC

AND obj_nm = 'test'

WHERE obj_typ_id = 4

SELECT ROWID,uo.*
FROM mbs_owner.MBS_USR_OBJ uo


WHERE ownr_id = 'C042334'
AND obj_typ_id = 1

AND obj_typ_id = 10


WHERE ownr_id = 'C049476'
AND obj_typ_id = 4
ORDER BY upd_dt DESC


WHERE fldr_nm = 'FOLDER_33985'


SELECT ROWID,uo.*
FROM mbs_owner.MBS_USR_OBJ uo
WHERE obj_id IN (38134,38140)

WHERE obj_nm LIKE 'QAList1'
WHERE ownr_id = 'C049476'
GROUP BY obj_typ_id

WHERE obj_typ_id = 9

WHERE obj_id = 33985


WHERE ownr_id = 'RZ97441'
ORDER BY obj_typ_id

WHERE obj_typ_id=2 AND LOWER(obj_cntnt) LIKE '%msimilar%'

WHERE obj_id = 33985

SELECT ROWID,uo.*
FROM mbs_owner.MBS_USR_OBJ uo
where ownr_id = 'ALERTMONITOR'
and obj_desc_txt like '167217%'
order by crt_dt


WHERE obj_nm = 'ALERT_167217'

WHERE ownr_id = 'RB89698'
ORDER BY upd_dt DESC

ORDER BY upd_dt DESC

WHERE obj_typ_id=2 AND LENGTH(obj_cntnt) = 0


SELECT /* count by object type & access type */
 obj_typ_id,acs_lvl_id,COUNT(*)
FROM mbs_owner.MBS_USR_OBJ
GROUP BY obj_typ_id,acs_lvl_id

-DELETE /* delete user objects */
FROM mbs_owner.MBS_USR_OBJ
WHERE obj_id IN (118561)

/* See how many private/public objects of each type */
SELECT obj_typ_id,fldr_nm,acs_lvl_id,COUNT(*)
FROM mbs_owner.MBS_USR_OBJ
WHERE obj_typ_id IN (2)
AND acs_lvl_id = 2
AND ownr_id NOT IN ('C049476','C033073','RZ97441','RXX8590')
AND LOWER(obj_nm) <> 'current'
AND fldr_nm <> 'DEFAULT_FOLDER'
GROUP BY obj_typ_id,fldr_nm,acs_lvl_id

/* See how users have saved public objects of each type */
SELECT COUNT(*)
FROM (
 SELECT COUNT(*)
 FROM mbs_owner.MBS_USR_OBJ
 WHERE obj_typ_id IN (2)
  AND acs_lvl_id = 2
  AND ownr_id NOT IN ('C049476','C033073','RZ97441','RXX8590')
 GROUP BY ownr_id)

SELECT obj_typ_id,acs_lvl_id, COUNT(*) /* all user objects by type & access */
FROM mbs_owner.MBS_USR_OBJ uo
WHERE obj_nm <> 'Current' AND obj_nm <> 'Criteria List' AND obj_nm <>
      'Previous'
GROUP BY obj_typ_id,acs_lvl_id

SELECT obj_typ_id,acs_lvl_id, COUNT(*)
      /* nulled user objects by type and access */
FROM mbs_owner.MBS_USR_OBJ uo
WHERE LENGTH(obj_cntnt) = 0
AND obj_nm <> 'Current' AND obj_nm <> 'Criteria List' AND obj_nm <> 'Previous'
GROUP BY obj_typ_id,acs_lvl_id

SELECT ROWID,uo.*
FROM mbs_owner.MBS_USR_OBJ uo
WHERE obj_typ_id=1 AND LENGTH(obj_cntnt) = 0
AND obj_nm <> 'Current' AND obj_nm <> 'Criteria List' AND obj_nm <> 'Previous'
AND acs_lvl_id = 1
ORDER BY obj_id

SELECT ROWID,uo.* /* find all corrupted public objects */
FROM mbs_owner.MBS_USR_OBJ uo
WHERE LENGTH(obj_cntnt) = 0
AND obj_nm <> 'Current' AND obj_nm <> 'Criteria List' AND obj_nm <> 'Previous'
AND acs_lvl_id = 2
ORDER BY upd_dt

xxxupdate mbs_owner.MBS_USR_OBJ uo
SET obj_nm = obj_nm || ' (UNAVAILABLE, Will Be Restored)'
WHERE LENGTH(obj_cntnt) = 0
AND obj_nm <> 'Current' AND obj_nm <> 'Criteria List' AND obj_nm <> 'Previous'
AND acs_lvl_id = 2

SELECT COUNT(*)
FROM mbs_owner.MBS_USR_OBJ uo
WHERE obj_typ_id=2 AND LENGTH(obj_cntnt) = 0
AND ownr_id='C049476' /* TRH */

SELECT obj_typ_id,COUNT(*)
FROM mbs_owner.MBS_USR_OBJ uo
WHERE LENGTH(obj_cntnt) >= 0
AND obj_nm <> 'Current' AND obj_nm <> 'Criteria List' AND obj_nm <> 'Previous'
AND acs_lvl_id = 2
GROUP BY obj_typ_id


AND crt_dt > '14-jul-2006'
AND obj_id >= 28957
ORDER BY crt_dt

AND ownr_id='RZ97441' /* JES */
ORDER BY obj_id

ORDER BY crt_dt DESC

SELECT ROWID,uo.*
FROM mbs_owner.MBS_USR_OBJ uo
WHERE obj_nm = 'Alert_23621'

WHERE obj_typ_id=10
AND ownr_id='C049476'

WHERE LOWER(obj_nm) LIKE '%herg-model list%'

WHERE obj_typ_id=2
AND fldr_nm = 'DEFAULT_FOLDER'

WHERE LOWER(obj_nm) LIKE '%administrator%'


WHERE obj_typ_id=10


WHERE obj_nm = 'Data Alert Query 6'

WHERE ownr_id = 'ALERTMONITOR'

WHERE obj_typ_id=10

WHERE obj_id=22293

WHERE ownr_id = 'ALERTMONITOR'
ORDER BY upd_dt DESC


ORDER BY upd_dt DESC


WHERE obj_nm = 'RestoreWindowsAtStartup'

SELECT COUNT(*) FROM mbs_owner.MBS_USR_OBJ WHERE obj_typ_id=4 AND obj_nm =
      'NameAddress'

WHERE ownr_id = 'C049476'
AND obj_typ_id=1
AND acs_lvl_id=2
ORDER BY upd_dt DESC

xxxDELETE
FROM mbs_owner.MBS_USR_OBJ
WHERE obj_id IN (66918,66800,66787,66786,66778,66777)

WHERE fldr_nm LIKE 'KINESIS%'
AND ownr_id = 'C049476'
AND obj_typ_id=1
AND acs_lvl_id=2
AND upd_dt >'6-feb-06'

AND obj_desc_txt LIKE '||||%'

AND ownr_id = 'RC91044'

WHERE obj_nm = 'NameAddress'
ORDER BY upd_dt DESC

WHERE obj_typ_id = 8

SELECT ownr_id,COUNT(*) COUNT /* count number of queries by user */
FROM mbs_owner.MBS_USR_OBJ
WHERE obj_typ_id = 2
GROUP BY ownr_id
ORDER BY COUNT DESC

/* Usage log */

SELECT COUNT(*)
FROM mbs_owner.MBS_LOG;

SELECT *
FROM mbs_owner.MBS_LOG
where obj_desc_txt = 'QueryRESTful';


select distinct ownr_id 
from mbs_owner.MBS_LOG  
where obj_desc_txt = 'TableStats'
 and obj_cntnt like '%ANNOTATION_85633%'
 and crt_dt between '1-jan-2013' and '20-jun-2013'


SELECT ownr_id, count(*) count
FROM mbs_owner.MBS_LOG
where obj_desc_txt = 'QueryRESTful'
and crt_dt >= '1-apr-2013'
group by ownr_id
order by count desc


SELECT *
FROM mbs_owner.MBS_LOG
where crt_dt >= '28-apr-2013'
and ownr_id = 'KNMOBTST'

SELECT l.*,length(obj_cntnt)
FROM mbs_owner.MBS_LOG l
WHERE crt_dt > '19-apr-2013'
ORDER BY crt_dt desc

select obj_desc_txt Event, count(*)
FROM mbs_owner.MBS_LOG 
WHERE crt_dt between '20-mar-2013' and '20-apr-2013'
group by obj_desc_txt
order by obj_desc_txt

SELECT obj_desc_txt, length(obj_cntnt) "l", obj_cntnt, to_char(obj_cntnt)
FROM mbs_owner.MBS_LOG l
WHERE crt_dt between '20-mar-2013' and '20-apr-2013'
and length(obj_cntnt) > 0
and obj_desc_txt = 'QueryGridAnd'
ORDER BY crt_dt desc

where crt_dt > '12-jun-2009'
ORDER BY upd_dt

/* delete /* get rid of old data */
FROM mbs_owner.MBS_LOG
WHERE crt_dt < '1-jan-2011'

/********************************/
/*** Annotation Table Queries ***/
/********************************/

SELECT  COUNT(*) 
FROM mbs_owner.MBS_ADW_RSLT
WHERE mthd_vrsn_id = 580347
and sts_id = 1
and rslt_typ_id = 1508632258


select ROWID, uo.* /* find annot */
FROM mbs_owner.MBS_USR_OBJ uo
where ownr_id = 'C049476'
and obj_typ_id = 6
order by obj_id desc

SELECT ROWID,r.*
FROM mbs_owner.MBS_ADW_RSLT r
WHERE mthd_vrsn_id = 700037
and sts_id = 1
order by updt_dt


SELECT sts_id, count(*) 
FROM mbs_owner.MBS_ADW_RSLT r
group by sts_id


xxxdelete 
FROM mbs_owner.MBS_ADW_RSLT
WHERE mthd_vrsn_id = 615946

WHERE rslt_grp_id = 535345676

and rslt_typ_id = 44155024
and rslt_val_txt like '%,'

SELECT *
FROM mbs_owner.MBS_ADW_RSLT
WHERE mthd_vrsn_id = 231766
and ext_cmpnd_id_nbr = 3116230
and rslt_grp_id = 535345803
and rslt_typ_id = 277219346
and sts_id = 1

order by updt_dt


/* Formatted on 11/30/2015 12:01:50 PM (QP5 v5.240.12305.39446) */
SELECT ext_cmpnd_id_nbr,
       mthd_vrsn_id,
       rslt_typ_id,
       rslt_val_prfx_txt,
       rslt_val_nbr,
       rslt_val_txt,
       rslt_val_dt,
       dc_lnk,
       rslt_id,
       rslt_grp_id
  FROM mbs_owner.mbs_adw_rslt
 WHERE ext_cmpnd_id_nbr IN (1,2,3) and sts_id = 1 and ((MTHD_VRSN_ID = 235547 and RSLT_TYP_ID in (287681128)) or (MTHD_VRSN_ID = 235551 and RSLT_TYP_ID in (287681136)) or (MTHD_VRSN_ID = 235558 and RSLT_TYP_ID in (287681145)) or (MTHD_VRSN_ID = 235569 and RSLT_TYP_ID in (287681155,287681156,287681157)))
 
where lower(rslt_val_txt) like '%retreat.d51%'

where rslt_id = 249783450

xxxuxpxdxaxtxe mbs_owner.MBS_ADW_RSLT
set rslt_val_txt = replace (rslt_val_txt, 'retreat.d51', 'Amazon.d50')
where lower(rslt_val_txt) like '%retreat.d51%'
and rslt_id = 249783450

select obj_id, ownr_id, obj_nm
from mbs_owner.mbs_usr_obj
where obj_id in (
select distinct(mthd_vrsn_id)
from mbs_owner.MBS_ADW_RSLT
where lower(rslt_val_txt) like '%retreat.d51%'
)

SELECT *
FROM mbs_owner.MBS_ADW_RSLT

WHERE mthd_vrsn_id = 81515

AND sbstnc_id >= 2519459

SELECT /*+ first_rows */ *
FROM mbs_owner.MBS_ADW_RSLT
WHERE mthd_vrsn_id = 277056

select * from mbs_owner.mbs_adw_rslt
where mthd_vrsn_id = 234035 and (ext_cmpnd_id_txt = '00000010' or
      ext_cmpnd_id_txt = '00000099')

 and ext_cmpnd_id_nbr = 02562023
order by sts_dt

xxxdelete /* delete bogus rows containing a vertical tab & comma */
FROM mbs_owner.MBS_ADW_RSLT
WHERE mthd_vrsn_id = 109683
and rslt_typ_id = 44155024
and rslt_val_txt like '%,'

select * /* look for invalid values ending in a comma */
FROM mbs_owner.MBS_ADW_RSLT
WHERE mthd_vrsn_id = 109683
and rslt_val_txt like '%,'

select mthd_vrsn_id, count(*) /* look for invalid dup results by method */
from (
SELECT * FROM (
 SELECT ext_cmpnd_id_txt, mthd_vrsn_id,rslt_typ_id,rslt_grp_id, COUNT(*) COUNT
 FROM mbs_owner.MBS_ADW_RSLT
 WHERE sts_id = 1
 and mthd_vrsn_id = 109683
 GROUP BY ext_cmpnd_id_txt, mthd_vrsn_id, rslt_typ_id, rslt_grp_id)
WHERE COUNT > 1
)
group by mthd_vrsn_id

SELECT * FROM ( /* get details on invalid dup results */
 SELECT ext_cmpnd_id_txt, mthd_vrsn_id,rslt_typ_id,rslt_grp_id, COUNT(*) COUNT
 FROM mbs_owner.MBS_ADW_RSLT
 WHERE sts_id = 1
 and mthd_vrsn_id = 109683
 GROUP BY ext_cmpnd_id_txt, mthd_vrsn_id, rslt_typ_id, rslt_grp_id)
WHERE COUNT > 1


update (
      /* Fix multiple instances of a result type for a result group by keeping just the type with the largest result id */
SELECT *
FROM mbs_owner.MBS_ADW_RSLT r
WHERE mthd_vrsn_id = 109683
 and sts_id = 1
 /* and ext_cmpnd_id_nbr = 2916090 */
 and rslt_id <>
    (select max(rslt_id)
     from mbs_owner.MBS_ADW_RSLT r2
     where sts_id = 1
     and r2.ext_cmpnd_id_nbr = r.ext_cmpnd_id_nbr
     and r2.mthd_vrsn_id = r.mthd_vrsn_id
     and r2.rslt_typ_id = r.rslt_typ_id
     and r2.rslt_grp_id = r.rslt_grp_id))
 set sts_id = 2, sts_dt = sysdate

SELECT /*+ first_rows */ *
FROM mbs_owner.MBS_ADW_RSLT
WHERE mthd_vrsn_id = 127156
and rslt_typ_id = 59352516
and rslt_val_txt like '%,%'


SELECT COUNT(UNIQUE(mthd_vrsn_id)) /* total method count */
FROM mbs_owner.MBS_ADW_RSLT

SELECT COUNT(*)
FROM mbs_owner.MBS_USR_OBJ
WHERE obj_typ_id=6
AND obj_id = 3608

SELECT *
FROM mbs_owner.MBS_USR_OBJ
where obj_id = 93226

WHERE obj_typ_id=6

SELECT /*+ first_rows */ rslt_typ_id, sts_id, crt_dt, rslt_val_nbr,
      rslt_val_txt, rslt_val_dt
FROM mbs_owner.MBS_ADW_RSLT r
WHERE mthd_vrsn_id = 61087
AND sbstnc_id = 2778319
ORDER BY r.crt_dt DESC

SELECT /*+ first_rows */ rslt_typ_id, sts_id, crt_dt, rslt_val_nbr,
      rslt_val_txt, rslt_val_dt
FROM mbs_owner.MBS_ADW_RSLT r
WHERE mthd_vrsn_id = 77673
AND sbstnc_id = 1
ORDER BY r.crt_dt DESC

SELECT COUNT(*) /* find rows with all null values */
FROM mbs_owner.MBS_ADW_RSLT r

UPDATE mbs_owner.MBS_ADW_RSLT
SET rslt_val_nbr = to_number(rslt_val_txt)
WHERE mthd_vrsn_id = 69636 and rslt_typ_id in (31365756, 31365757)

SELECT /* get number of rows per method */
 mthd_vrsn_id, COUNT(*) count, uo.obj_nm, uo.OWNR_ID
FROM mbs_owner.MBS_ADW_RSLT r, mbs_owner.MBS_USR_OBJ uo
WHERE uo.obj_id (+) = r.mthd_vrsn_id
GROUP BY mthd_vrsn_id, uo.obj_nm, uo.OWNR_ID
ORDER BY count desc

SELECT /* get number of rows per method */
 rslt_typ_id, COUNT(*)
FROM mbs_owner.MBS_ADW_RSLT
where mthd_vrsn_id = 241149
GROUP BY rslt_typ_id

SELECT *
FROM
 (SELECT mthd_vrsn_id, COUNT(*) COUNT
 FROM mbs_owner.MBS_ADW_RSLT
 GROUP BY mthd_vrsn_id) r,
 mbs_owner.MBS_USR_OBJ uo
WHERE uo.obj_id (+) = r.mthd_vrsn_id
ORDER BY COUNT DESC

SELECT
      /* find annotation data without any associated annotation user object (delete eventually) */
r.mthd_vrsn_id, COUNT(*)
FROM mbs_owner.MBS_ADW_RSLT r
WHERE NOT EXISTS
 (SELECT *
  FROM mbs_owner.MBS_USR_OBJ o
  WHERE o.obj_id = r.mthd_vrsn_id)
GROUP BY r.mthd_vrsn_id
ORDER BY COUNT(*) DESC

SELECT /*+first_rows */ *

xxxDELETE /*+rule*/
FROM mbs_owner.MBS_ADW_RSLT r
WHERE NOT EXISTS
 (SELECT *
  FROM mbs_owner.MBS_USR_OBJ o
  WHERE o.obj_id = r.mthd_vrsn_id)
AND ROWNUM < 100

xxxDELETE /*+rule*/
FROM mbs_owner.MBS_ADW_RSLT
WHERE mthd_vrsn_id = 58158
AND ROWNUM < 1000

xxxDELETE /* selectively remove annotation table data */
FROM mbs_owner.MBS_ADW_RSLT
WHERE (rslt_val_nbr IS NULL OR rslt_val_nbr = -2147483648)
 AND rslt_val_txt IS NULL
 AND (rslt_val_dt IS NULL OR rslt_val_dt = '1-jan-0001')
 AND dc_lnk IS NULL
 AND cmnt_txt IS NULL

WHERE mthd_vrsn_id = 58158
AND sbstnc_id >= 2519459

SELECT mthd_vrsn_id,COUNT(*)
      /* find annotation rows with null values grouped by method */
FROM mbs_owner.MBS_ADW_RSLT
WHERE (rslt_val_nbr IS NULL OR rslt_val_nbr = -2147483648)
 AND rslt_val_txt IS NULL
 AND (rslt_val_dt IS NULL OR rslt_val_dt = '1-jan-0001')
 AND dc_lnk IS NULL
 AND cmnt_txt IS NULL
GROUP BY mthd_vrsn_id

SELECT COUNT(*) /* find annotation rows with null values */
FROM mbs_owner.MBS_ADW_RSLT
WHERE (rslt_val_nbr IS NULL OR rslt_val_nbr = -2147483648)
 AND rslt_val_txt IS NULL
 AND (rslt_val_dt IS NULL OR rslt_val_dt = '1-jan-0001')
 AND dc_lnk IS NULL
 AND cmnt_txt IS NULL

 AND mthd_vrsn_id = 44934

SELECT COUNT(*) /* find annotation rows with null values */
FROM mbs_owner.MBS_ADW_RSLT

SELECT COUNT(*) /* see how many rows are not active */
FROM mbs_owner.MBS_ADW_RSLT
where sts_id <> 1

select /* get row count for each table */
 mthd_vrsn_id, count(distinct rslt_grp_id) count
from mbs_owner.mbs_adw_rslt
where
 sts_id = 1
 /* and mthd_vrsn_id = 132967 */
group by mthd_vrsn_id
order by count desc

select /* get latest update date for each table */
 mthd_vrsn_id, max(updt_dt)
from mbs_owner.mbs_adw_rslt
where
 updt_dt <= sysdate and
 sts_id = 1
 /* and mthd_vrsn_id = 132967 */
group by mthd_vrsn_id



/* Computed Properties */

SELECT COUNT(*)
FROM mbs_owner.MBS_CPDW_RSLT

SELECT *
FROM mbs_owner.MBS_CPDW_RSLT

SELECT * /* get GSAT inventory summary */
FROM mbs_owner.MBS_CPDW_RSLT
WHERE mthd_vrsn_id = 20 AND rslt_typ_id = 20
AND cmpnd_id = 2520526

SELECT MAX(updt_dt)
FROM mbs_owner.MBS_CPDW_RSLT
WHERE mthd_vrsn_id = 20 AND rslt_typ_id = 20

/
select *
from mbs_owner.mbs_spotfire_sql
/

/*************************************************************/
/* Misc test queries */
/*************************************************************/

select 
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
                from mbs_owner.mbs_adw_rslt
                where ext_cmpnd_id_nbr in (11,12,13,14) and sts_id = 1
and MTHD_VRSN_ID = 235569         

with mar as (
select 
                        ext_cmpnd_id_nbr,
                        mthd_vrsn_id,
                        rslt_typ_id,
                        rslt_val_prfx_txt,
                        rslt_val_nbr,
                        rslt_val_txt,
                        rslt_val_dt,
                        dc_lnk,
                        rslt_id,
                        rslt_grp_id,
                        sts_id
                from mbs_owner.mbs_adw_rslt
)
select *
from mar 
where MTHD_VRSN_ID = 235547 and ext_cmpnd_id_nbr in (1,2,3,4) and sts_id = 1
union all                 
select *
from mar 
where MTHD_VRSN_ID = 275383 and ext_cmpnd_id_nbr in (1,2,3,4) and sts_id = 1
union all
select *
from mar 
where MTHD_VRSN_ID = 235558 and ext_cmpnd_id_nbr in (1,2,3,4) and sts_id = 1
union all
select *
from mar 
where MTHD_VRSN_ID = 235569 and ext_cmpnd_id_nbr in (1,2,3,4) and sts_id = 1

select max(dc_lnk) from mbs_owner.mbs_adw_rslt

select mthd_vrsn_id, count(*)
from mbs_owner.mbs_adw_rslt
group by mthd_vrsn_id

SELECT /*+ first_rows */ EXT_CMPND_ID_NBR,
    MTHD_VRSN_ID,
    RSLT_TYP_ID,
    RSLT_VAL
FROM (
    SELECT t0.EXT_CMPND_ID_NBR EXT_CMPND_ID_NBR,
        t0.rslt_grp_id,
        t0.rslt_id,
        t0.rslt_typ_id,
        t0.rslt_val_prfx_txt,
        t0.rslt_val_nbr,
        t0.rslt_val_txt,
        t0.rslt_val_dt,
        t0.dc_lnk,
        NULL MTHD_VRSN_ID,
        NULL RSLT_TYP_ID,
        NULL RSLT_VAL
    FROM (
        SELECT EXT_CMPND_ID_NBR,
            rslt_grp_id,
            rslt_id,
            rslt_typ_id,
            rslt_val_prfx_txt,
            rslt_val_nbr,
            rslt_val_txt,
            rslt_val_dt,
            dc_lnk
        FROM MBS_OWNER.mbs_adw_rslt
        WHERE 1 = 1
            AND sts_id = 1
        ) t0
    ) T1
WHERE (T1.EXT_CMPND_ID_NBR = 00000004)
    AND EXT_CMPND_ID_NBR IN (4)