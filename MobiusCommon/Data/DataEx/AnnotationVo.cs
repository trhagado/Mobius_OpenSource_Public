using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Data
{
	/// <summary>
	/// Generic Data Warehouse value object
	/// </summary>

	public class AnnotationVo
	{
		public long rslt_id; //                  number (12)   not null,
		public long rslt_grp_id; //              number (12)   not null,
		public string ext_cmpnd_id_txt; //       varchar2 (32),
		public int ext_cmpnd_id_nbr; //          number (12), 
		public int src_db_id; //                 number (12)
		public int mthd_vrsn_id; //              number (12), 
		public long rslt_typ_id; //               number (12), 
		public string rslt_val_prfx_txt = ""; // varchar2 (2), 
		public double rslt_val_nbr; //           number, 
		public int uom_id; //                    number (12), 
		public string rslt_val_txt = ""; //      varchar2 (4000), 
		public DateTime rslt_val_dt; //          date,
		public string cmnt_txt = ""; //          varchar2 (500), 
		public string dc_lnk = "";
		public string chng_op_cd = "";  //       varchar2 (1)  not null, 
		public string chng_usr_id = ""; //       varchar2 (12)  not null, 
		public DateTime crt_dt; //               date          not null, 
		public DateTime updt_dt; //              date          not null, 

		public AnnotationVo Clone()
		{
			return (AnnotationVo)this.MemberwiseClone();
		}
	}

}
