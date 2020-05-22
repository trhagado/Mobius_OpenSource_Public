/* Examine existing LSN_SALT_ISOMER_INFO table
'exact'                             (396461)
'lf.nc','Largest Frag w/o chirality (204782)
'lf' 'Largest Frag w chirality'     (158482)
'tautomer'                           (15394)
*/

select type, count(*)
from mbs_owner.LSN_SALT_ISOMER_INFO
group by type


select *
from mbs_owner.LSN_SALT_ISOMER_INFO
where type = 'tautomer'

