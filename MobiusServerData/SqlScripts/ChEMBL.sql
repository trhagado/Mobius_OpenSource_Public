SELECT 
 table_name, 
 table_rows, 
 avg_row_length,
 data_length, 
 table_comment
FROM INFORMATION_SCHEMA.Tables
where table_schema = 'chembl_26';

SELECT *
FROM INFORMATION_SCHEMA.COLUMNS
where table_name = 'molecule_dictionary'
ORDER BY ORDINAL_POSITION;

SELECT *
			  FROM INFORMATION_SCHEMA.COLUMNS
				WHERE 
           table_schema = 'CHEMBL_26' 
           AND table_name = 'compound_structures'
				ORDER BY ORDINAL_POSITION;

SELECT data_type, count(*)
FROM INFORMATION_SCHEMA.COLUMNS
group by data_type;

SELECT /*+ first_rows */ CHEMBL_ID,
	MOLSTRUCTURE,
	MOLFORMULA,
	MOLWEIGHT,
	MOLSMILES
FROM (
	SELECT CHEMBL_ID,
		MOLFILE MOLSTRUCTURE,
		'ChEMBL' MOLDATABASES,
		MOLFORMULA,
		MOLWEIGHT,
		MOLSMILES
	FROM (
		SELECT md.chembl_id,
			cs.molfile,
			cp.full_molformula molformula,
			cp.full_mwt molweight,
			cs.molregno,
			cs.canonical_smiles molsmiles,
			cs.standard_inchi,
			cs.standard_inchi_key
		FROM CHEMBL_26.molecule_dictionary md
		RIGHT JOIN CHEMBL_26.compound_structures cs ON cs.molregno = md.molregno
		RIGHT JOIN CHEMBL_26.compound_properties cp ON cp.molregno = md.molregno
		) SQ_22
	) T1
WHERE ((1 = 1))
	AND CHEMBL_ID IN ('CHEMBL25');

                    
select 
		md.chembl_id,  
		cp.full_mwt molweight,
		cp.full_molformula molformula,
		cs.canonical_smiles molsmiles
	from 
	 chembl_owner.molecule_dictionary md, 
	 chembl_owner.compound_structures cs, 
	 chembl_owner.compound_properties cp
	where 
	 cs.molregno = md.molregno and
	 css.molregno = md.molregno and
	 cp.molregno (+) = md.molregno;
     
 SELECT /*+ first_rows */ CHEMBL_ID,
	FULL_MWT,
	MW_FREEBASE,
	ALOGP,
	HBA,
	HBD,
	PSA,
	RTB,
	RO3_PASS,
	NUM_RO5_VIOLATIONS,
	CX_MOST_APKA,
	CX_MOST_BPKA,
	CX_LOGP,
	CX_LOGD,
	MOLECULAR_SPECIES,
	AROMATIC_RINGS,
	HEAVY_ATOMS,
	QED_WEIGHTED,
	MW_MONOISOTOPIC,
	FULL_MOLFORMULA,
	HBA_LIPINSKI,
	HBD_LIPINSKI,
	NUM_LIPINSKI_RO5_VIOLATIONS
FROM (
	SELECT md.chembl_id,
		cp.*
	FROM CHEMBL_26.compound_properties cp
	LEFT JOIN CHEMBL_26.molecule_dictionary md ON cp.molregno = md.molregno
	) T1
WHERE ((1 = 1))
	AND CHEMBL_ID IN ('CHEMBL25');

SELECT distinct md.chembl_id,  cp.*
from 
 chembl_26.compound_properties cp 
 left join chembl_26.molecule_dictionary md
 on cp.molregno = md.molregno
where chembl_id = 'CHEMBL25'

CREATE TABLE `compound_properties` ( // from MySQL Workbench
  `molregno` bigint NOT NULL COMMENT 'Foreign key to compounds table (compound structure)',
  `mw_freebase` decimal(9,2) DEFAULT NULL COMMENT 'Molecular weight of parent compound',
  `alogp` decimal(9,2) DEFAULT NULL COMMENT 'Calculated ALogP',
  `hba` mediumint DEFAULT NULL COMMENT 'Number hydrogen bond acceptors',
  `hbd` mediumint DEFAULT NULL COMMENT 'Number hydrogen bond donors',
  `psa` decimal(9,2) DEFAULT NULL COMMENT 'Polar surface area',
  `rtb` mediumint DEFAULT NULL COMMENT 'Number rotatable bonds',
  `ro3_pass` varchar(3) DEFAULT NULL COMMENT 'Indicates whether the compound passes the rule-of-three (mw < 300, logP < 3 etc)',
  `num_ro5_violations` tinyint DEFAULT NULL COMMENT 'Number of violations of Lipinski''s rule-of-five, using HBA and HBD definitions',
  `cx_most_apka` decimal(9,2) DEFAULT NULL COMMENT 'The most acidic pKa calculated using ChemAxon v17.29.0',
  `cx_most_bpka` decimal(9,2) DEFAULT NULL COMMENT 'The most basic pKa calculated using ChemAxon v17.29.0',
  `cx_logp` decimal(9,2) DEFAULT NULL COMMENT 'The calculated octanol/water partition coefficient using ChemAxon v17.29.0',
  `cx_logd` decimal(9,2) DEFAULT NULL COMMENT 'The calculated octanol/water distribution coefficient at pH7.4 using ChemAxon v17.29.0',
  `molecular_species` varchar(50) DEFAULT NULL COMMENT 'Indicates whether the compound is an acid/base/neutral',
  `full_mwt` decimal(9,2) DEFAULT NULL COMMENT 'Molecular weight of the full compound including any salts',
  `aromatic_rings` mediumint DEFAULT NULL COMMENT 'Number of aromatic rings',
  `heavy_atoms` mediumint DEFAULT NULL COMMENT 'Number of heavy (non-hydrogen) atoms',
  `qed_weighted` decimal(3,2) DEFAULT NULL COMMENT 'Weighted quantitative estimate of drug likeness (as defined by Bickerton et al., Nature Chem 2012)',
  `mw_monoisotopic` decimal(11,4) DEFAULT NULL COMMENT 'Monoisotopic parent molecular weight',
  `full_molformula` varchar(100) DEFAULT NULL COMMENT 'Molecular formula for the full compound (including any salt)',
  `hba_lipinski` mediumint DEFAULT NULL COMMENT 'Number of hydrogen bond acceptors calculated according to Lipinski''s original rules (i.e., N + O count))',
  `hbd_lipinski` mediumint DEFAULT NULL COMMENT 'Number of hydrogen bond donors calculated according to Lipinski''s original rules (i.e., NH + OH count)',
  `num_lipinski_ro5_violations` tinyint DEFAULT NULL COMMENT 'Number of violations of Lipinski''s rule of five using HBA_LIPINSKI and HBD_LIPINSKI counts',
  PRIMARY KEY (`molregno`),
  KEY `idx_cp_hbd` (`hbd`),
  KEY `idx_cp_rtb` (`rtb`),
  KEY `idx_cp_mw` (`mw_freebase`),
  KEY `idx_cp_ro5` (`num_ro5_violations`),
  KEY `idx_cp_hba` (`hba`),
  KEY `idx_cp_alogp` (`alogp`),
  KEY `idx_cp_psa` (`psa`),
  CONSTRAINT `fk_cmpdprop_molregno` FOREIGN KEY (`molregno`) REFERENCES `molecule_dictionary` (`molregno`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Table storing calculated physicochemical properties for compounds, now calculated with RDKit and ChemAxon software (note all but FULL_MWT and FULL_MOLFORMULA are calculated on the parent structure)';
;

  SELECT molregno
    FROM CHEMBL_OWNER.compound_struct_symyx
WHERE FlexMatch ( ctab , 'IZgcrE8AnCwQh^xipBGYI28I3I01dqky8R1W0t1ZHKqUlpP$D0nqf$H3BZI6XI0Cyh3K1qVuI90Hd$Bb5aZ1H^uE3$CGADYzwra7g$xmvhCz2xLQgZOKw7eyYjoyUOg7liEZlwV^vfVuWbqGBiXRnDxvaSpGgyOjJOEVMilU6qodrnCnamy^v3KBJ1ccMvGN0ayikwmeqXjwcMbRgF6GSOKc3VNCSNrCt0FN5o7eawSF7J186ekGebBSxpCyyC3ppPInsSilOfxhKGl652hKFtAG$HycIekfuRZNtJ7eq8sOZZ8j78c70oyTTIzAzrqmaoT28k7vmejGoxU2T^sknhAHV2gA9k2palX$zH^tAPB72O^wA4LXF^D5cB' , 'TAU' ) = 1
;

SELECT molregno
FROM CHEMBL_OWNER.compound_struct_symyx
WHERE sss ( ctab , 'DZgpW00AXBwQ4V6R8oXvWBTQldALf9gxwTg9UsyqWoenoNXYGM3xeFvhyP4Vqt6Q9p2CfRH$lcgcEmkpHUugHMc6mtG$8H38T$vFRrbYWkazWoiFgbklw7eZYjoyUOg7liEZlwV^vfVuWbqGBiXRnDxvaSpGgyOjJOEVMilU6qodrnCnamy^v3KBJ1ccMvGN0ayikwmeqXjwcMbRgF6GSOKc3VNCSNrCt0FN5o7eawSF7J186ekGebBSxpCyyC3ppPInsSilOfxhKGl652hKFtAG$HycIekfuRZNtJ7eq8sOZZ8j78c70oyTTIzAzrqmaoT28k7vmejGoxU2T^sknhAHV2gA9k2palX$zH^tAPB72O^wA4LXF^D5cB' ) = 1
;

/* FS */
SELECT DISTINCT T1.CHEMBL_ID
FROM
  (SELECT CHEMBL_ID,
    chime(ctab) MOLSTRUCTURE,
    'ChEMBL' MOLDATABASES,
    ctab
  FROM
    (SELECT md.chembl_id,
      css.ctab,
      cp.full_mwt molweight,
      cp.full_molformula molformula,
      cs.canonical_smiles molsmiles
    FROM CHEMBL_OWNER.compound_struct_symyx css,
      CHEMBL_OWNER.molecule_dictionary md,
      CHEMBL_OWNER.compound_structures cs,
      CHEMBL_OWNER.compound_properties cp
    WHERE cs.molregno   = md.molregno
    AND css.molregno    = md.molregno
    AND cp.molregno (+) = md.molregno
    ) SQ_33
  ) T1
WHERE FlexMatch ( T1.ctab , 'IZgcrE8AnCwQh^xipBGYI28I3I01dqky8R1W0t1ZHKqUlpP$D0nqf$H3BZI6XI0Cyh3K1qVuI90Hd$Bb5aZ1H^uE3$CGADYzwra7g$xmvhCz2xLQgZOKw7eyYjoyUOg7liEZlwV^vfVuWbqGBiXRnDxvaSpGgyOjJOEVMilU6qodrnCnamy^v3KBJ1ccMvGN0ayikwmeqXjwcMbRgF6GSOKc3VNCSNrCt0FN5o7eawSF7J186ekGebBSxpCyyC3ppPInsSilOfxhKGl652hKFtAG$HycIekfuRZNtJ7eq8sOZZ8j78c70oyTTIzAzrqmaoT28k7vmejGoxU2T^sknhAHV2gA9k2palX$zH^tAPB72O^wA4LXF^D5cB' , 'TAU' ) = 1
;

/* SS */
SELECT DISTINCT T1.CHEMBL_ID
FROM
  (SELECT CHEMBL_ID,
    chime(ctab) MOLSTRUCTURE,
    'ChEMBL' MOLDATABASES,
    ctab
  FROM
    (SELECT md.chembl_id,
      css.ctab,
      cp.full_mwt molweight,
      cp.full_molformula molformula,
      cs.canonical_smiles molsmiles
    FROM CHEMBL_OWNER.compound_struct_symyx css,
      CHEMBL_OWNER.molecule_dictionary md,
      CHEMBL_OWNER.compound_structures cs,
      CHEMBL_OWNER.compound_properties cp
    WHERE cs.molregno   = md.molregno
    AND css.molregno    = md.molregno
    AND cp.molregno (+) = md.molregno
    ) SQ_11
  ) T1
WHERE sss ( T1.ctab , 'DZgpW00AXBwQ4V6R8oXvWBTQldALf9gxwTg9UsyqWoenoNXYGM3xeFvhyP4Vqt6Q9p2CfRH$lcgcEmkpHUugHMc6mtG$8H38T$vFRrbYWkazWoiFgbklw7eZYjoyUOg7liEZlwV^vfVuWbqGBiXRnDxvaSpGgyOjJOEVMilU6qodrnCnamy^v3KBJ1ccMvGN0ayikwmeqXjwcMbRgF6GSOKc3VNCSNrCt0FN5o7eawSF7J186ekGebBSxpCyyC3ppPInsSilOfxhKGl652hKFtAG$HycIekfuRZNtJ7eq8sOZZ8j78c70oyTTIzAzrqmaoT28k7vmejGoxU2T^sknhAHV2gA9k2palX$zH^tAPB72O^wA4LXF^D5cB' ) = 1
;

SELECT distinct
		md.chembl_id,  
		cp.*
	from 
	 chembl_26.molecule_dictionary md, 
	 chembl_26.compound_properties cp
	where 
	 cp.molregno (+) = md.molregno;