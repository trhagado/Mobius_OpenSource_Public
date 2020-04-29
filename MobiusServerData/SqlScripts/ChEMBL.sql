SELECT distinct md.chembl_id,  cp.*
from 
 chembl_26.compound_properties cp 
 left join chembl_26.molecule_dictionary md
 on cp.molregno = md.molregno;

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