using Mobius.ComOps;

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace Mobius.Data
{
	/// <summary>
	/// List of molecules
	/// </summary>

	public class MoleculeList : IEnumerable<MoleculeListItem>, IEnumerable
	{
		public string Name = "";
		public List<MoleculeListItem> ItemList = new List<MoleculeListItem>();

		/// <summary>
		/// Indexer
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>

		public MoleculeListItem this[int index]    // Indexer declaration  
		{	get => ItemList[index]; }

		public void Add(MoleculeListItem item) => ItemList.Add(item);

		public int Count { get => ItemList.Count; }

		/// <summary>
		/// Returns an IEnumerator to interate through list
		/// </summary>
		/// <returns></returns>

		IEnumerator<MoleculeListItem> IEnumerable<MoleculeListItem>.GetEnumerator()
		{
			return ItemList.GetEnumerator();
		}

		/// <summary>
		/// Returns an IEnumerator to interate through MapList
		/// </summary>
		/// <returns></returns>

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ItemList.GetEnumerator();
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <returns></returns>

		public string Serialize()
		{
			XmlMemoryStreamTextWriter mstw = new XmlMemoryStreamTextWriter();
			Serialize(mstw.Writer);
			return mstw.GetXmlAndClose();
		}

		/// <summary>
		/// Serialize
		/// </summary>
		/// <returns></returns>

		public void Serialize(
			XmlTextWriter tw)
		{
			tw.WriteStartElement("StructureList"); // keep name as StructureList for compatibility with existing serialized objects
			XmlUtil.WriteAttributeIfDefined(tw, "Name", Name);

			foreach (MoleculeListItem sli in ItemList)
			{
				tw.WriteStartElement("StructureListItem");
				XmlUtil.WriteAttributeIfDefined(tw, "Name", sli.Name);

				string chime = sli.Molecule?.ChimeString;
				XmlUtil.WriteAttributeIfDefined(tw, "ChimeString", chime);

				string helm = sli.Molecule?.HelmString;
				XmlUtil.WriteAttributeIfDefined(tw, "HelmString", helm);


				XmlUtil.WriteAttributeIfDefined(tw, "UpdateDate", sli.UpdateDate.ToString());
				XmlUtil.WriteAttributeIfDefined(tw, "StructureType", sli.MoleculeType);
				tw.WriteEndElement();
			}

			tw.WriteEndElement();
			return;
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="serializedForm"></param>
		/// <returns></returns>

		public static MoleculeList Deserialize(
				string serializedForm)
		{
			XmlMemoryStreamTextReader mstr = new XmlMemoryStreamTextReader(serializedForm);

			XmlTextReader tr = mstr.Reader;
			tr.Read();
			tr.MoveToContent();
			if (!Lex.Eq(tr.Name, "StructureList"))
				throw new Exception("MoleculeList.Deserialize - No \"StructureList\" element found");

			if (tr.IsEmptyElement) return new MoleculeList(); // if nothing there return empty StructureItem list

			MoleculeList strList = Deserialize(mstr.Reader);
			mstr.Close();
			return strList;
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="tr"></param>
		/// <returns></returns>

		public static MoleculeList Deserialize(
			XmlTextReader tr)
		{
			Enum iEnum = null;
			string txt;

			MoleculeList strList = new MoleculeList();

			if (tr.IsEmptyElement) // all done if no MoleculeItems
				return strList;

			strList.Name = XmlUtil.GetAttribute(tr, "Name");

			while (true) // loop on list of molecule items
			{
				tr.Read(); // move to next MoleculeListItem
				tr.MoveToContent();

				if (Lex.Eq(tr.Name, "StructureListItem")) // start or end of StructureItem
				{
					if (tr.NodeType == XmlNodeType.EndElement) continue; // end of current StructureItem
				}

				else if (tr.NodeType == XmlNodeType.EndElement) break; // done with all StructureItems

				else throw new Exception("MoleculeList.Deserialize - Unexpected element: " + tr.Name);

				MoleculeListItem sli = new MoleculeListItem();

				sli.Name = XmlUtil.GetAttribute(tr, "Name");

				string chime = XmlUtil.GetAttribute(tr, "ChimeString");
				if (Lex.IsDefined(chime))
					sli.Molecule = new MoleculeMx(MoleculeFormat.Chime, chime);

				string helm = XmlUtil.GetAttribute(tr, "HelmString");
				if (Lex.IsDefined(helm))
				{
					if (sli.Molecule == null)
						sli.Molecule = new MoleculeMx(MoleculeFormat.Helm, helm);
					sli.Molecule.HelmString = helm;
				}

				string dateString = XmlUtil.GetAttribute(tr, "UpdateDate");
				if (Lex.IsDefined(dateString)) DateTime.TryParse(dateString, out sli.UpdateDate);

				sli.MoleculeType = XmlUtil.GetAttribute(tr, "StructureType");

				strList.Add(sli);
			}

			return strList;
		}
	}

	/// <summary>
	/// Item in list of molecules
	/// </summary>

	public class MoleculeListItem
	{
		public string Name = "";
		public MoleculeMx Molecule = null;
		public DateTime UpdateDate = new DateTime();
		public string MoleculeType = "";

		public Bitmap FormattedBitmap = null;
		public int HeightInLines = 0; // for structure
	}

}
