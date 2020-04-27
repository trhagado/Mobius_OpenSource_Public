using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Mobius.SpotfireComOps;

namespace Mobius.SpotfireDocument
{

	/// <summary>
	/// Base class for all document classes
	/// Maintains links between document nodes and their parents 
	/// and other nodes that are referenced but now owned by the node
	/// Note that members of this class are not serialized
	/// </summary>

	public class NodeMsx
	{
		[XmlIgnore]
		public NodeMsx Owner; // node that owns us

		// Convenience getters for commonly accessed document properties.
		// These are/should not be serialized

		[XmlIgnore]
		public AnalysisApplicationMsx App
		{
			get
			{
				if (_app == null) _app = GetAncestor<AnalysisApplicationMsx>(); // move up the tree until we find a defined App
				return _app;
			}
		}
		private AnalysisApplicationMsx _app = null;

		[XmlIgnore] 
		public DocumentMsx App_Document => App?.Document;

		[XmlIgnore] 
		public DataManagerMsx Doc_DataManager => App_Document?.DataManager;

		[XmlIgnore] 
		public DataTableCollectionMsx Doc_Tables => Doc_DataManager?.TableCollection;

		[XmlIgnore]
		public PageCollectionMsx Doc_Pages => App_Document?.Pages;

		[XmlIgnore]
		public VisualMsx Doc_ActiveVisual => App_Document?.ActiveVisualReference; 


		/// <summary>
		/// Get ancestor node of the specified type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>

		public T GetAncestor<T>() where T : NodeMsx
		{
			for (NodeMsx owner = this.Owner; owner != null; owner = owner.Owner)
			{
				T t = owner as T;
				if (t != null)
				{
					return t;
				}
			}
			return default(T);
		}

/// <summary>
/// Set the Owner and App of a child node from its parent
/// </summary>
/// <param name="owner"></param>

		public void SetChildOwnerAndApp(NodeMsx child)
		{
			if (child == null) return;

			child.SetOwner(this);
			child.SetApp(this.App);
			return;
		}

		/// <summary>
		/// SetOwner
		/// </summary>
		/// <param name="app"></param>

		public void SetOwner(NodeMsx owner)
		{
			Owner = owner;
			return;
		}

		/// <summary>
		/// SetApp
		/// </summary>
		/// <param name="app"></param>

		public void SetApp(AnalysisApplicationMsx app)
		{
			_app = app;
			return;
		}
		
		// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// Secondary references for all nodes include the parent node and root app node
		/// These also include references to other non-owned nodes
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// Each class is responsible for providing a UpdatePostDeserializationSecondaryReferences method to
		/// update these for the class.
		/// </summary>
		/// <param name="child">The child node to be updated from this parent node</param>

		public void UpdatePreSerializationSecondaryReferences(NodeMsx child)
		{
			if (child == null) return;

			child.Owner = this; // link to parent

			child._app = this._app; // link to top level analysis node

			child.UpdatePreSerializationSecondaryReferences();

			return;
		}


		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public virtual void UpdatePreSerializationSecondaryReferences()
		{
			return;
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// Secondary references for all nodes include the parent node and root app node
		/// These also include references to other non-owned nodes
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// Each class is responsible for providing a UpdatePostDeserializationSecondaryReferences method to
		/// update these for the class.
		/// </summary>
		/// <param name="child">The child node to be updated from this parent node</param>

		public void UpdatePostDeserializationSecondaryReferences(NodeMsx child)
		{
			if (child == null) return;

			child.Owner = this; // link to parent

			child._app = this._app; // link to top level analysis node

			child.UpdatePostDeserializationSecondaryReferences();

			return;
		}

		/// <summary>
		/// Update secondary object xxxMsx class object references from
		/// Guid Ids following deserialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public virtual void UpdatePostDeserializationSecondaryReferences()
		{
			return;
		}

		/// <summary>
		/// Do a quick validation on a node to be sure the key document links are defined
		/// </summary>

		public void ValidateNode()
		{
			AssertMx.IsNotNull(App, "App");
			AssertMx.IsNotNull(App?.Document, "App.Document");
			AssertMx.IsNotNull(App?.Document?.DataManager, "App.Document.DataManager");
			AssertMx.IsNotNull(App?.Document?.DataManager?.TableCollection, "App.Document.TableCollection");
			AssertMx.IsNotNull(App?.Document?.Pages, "App.Document.Pages");
		}

	}
}
