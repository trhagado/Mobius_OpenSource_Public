using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class AnnotationVo
    {
        [DataMember] public long rslt_id; //                  number (12)   not null,
        [DataMember] public long rslt_grp_id; //              number (12)   not null,
        [DataMember] public string ext_cmpnd_id_txt; //       varchar2 (32),
        [DataMember] public int ext_cmpnd_id_nbr; //          number (12), 
        [DataMember] public int src_db_id; //                 number (12)
        [DataMember] public int mthd_vrsn_id; //              number (12), 
        [DataMember] public long rslt_typ_id; //               number (12), 
        [DataMember] public string rslt_val_prfx_txt = ""; // varchar2 (2), 
        [DataMember] public double rslt_val_nbr; //           number, 
        [DataMember] public int uom_id; //                    number (12), 
        [DataMember] public string rslt_val_txt = ""; //      varchar2 (4000), 
        [DataMember] public DateTime rslt_val_dt; //          date,
        [DataMember] public string cmnt_txt = ""; //          varchar2 (500), 
        [DataMember] public string dc_lnk = "";
        [DataMember] public string chng_op_cd = "";  //       varchar2 (1)  not null, 
        [DataMember] public string chng_usr_id = ""; //       varchar2 (12)  not null, 
        [DataMember] public DateTime crt_dt; //               date          not null, 
        [DataMember] public DateTime updt_dt; //              date          not null, 
    }
}
