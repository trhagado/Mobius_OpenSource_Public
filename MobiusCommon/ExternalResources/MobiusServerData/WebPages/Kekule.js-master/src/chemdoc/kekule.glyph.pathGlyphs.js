/**
 * @fileoverview
 * Implementation of glyphs defined by a series of nodes and paths.
 * @author Partridge Jiang
 */

/*
 * requires /lan/classes.js
 * requires /core/kekule.common.js
 * requires /core/kekule.structures.js
 * requires /chemdoc/kekule.glyph.base.js
 */

(function(){
"use strict";

var CU = Kekule.CoordUtils;
var CM = Kekule.CoordMode;

/**
 * Represent an node in glyph path.
 * @class
 * @augments Kekule.BaseStructureNode
 * @param {String} id Id of this node.
 * @param {String} nodeType Type of this glyph node. Value from {@link Kekule.Glyph.NodeType}.
 * @param {Hash} coord2D The 2D coordinates of node, {x, y}, can be null.
 * @param {Hash} coord3D The 3D coordinates of node, {x, y, z}, can be null.
 *
 * @property {String} nodeType Type of this glyph node.
 * @property {Hash} pathNodeParams Additional params of path node. Different glyph may requires different params.
 *   Some common ones:
 *   {
 *     useStickingOffset: Bool. Whether use a small offset to draw the end of path when this node sticking to another target.
 *     stickingOffsetRelLength: Number. Can be null to use the default one.
 *   }
 */
Kekule.Glyph.PathGlyphNode = Class.create(Kekule.BaseStructureNode,
/** @lends Kekule.Glyph.PathGlyphNode# */
{
	/** @private */
	CLASS_NAME: 'Kekule.Glyph.PathGlyphNode',
	/** @constructs */
	initialize: function(/*$super, */id, nodeType, coord2D, coord3D)
	{
		this.tryApplySuper('initialize', [id])  /* $super(id) */;
		if (coord2D)
			this.setCoord2D(coord2D);
		if (coord3D)
			this.setCoord3D(coord3D);
		this.setNodeType(nodeType || Kekule.Glyph.NodeType.LOCATION);
	},
	/** @private */
	initProperties: function()
	{
		this.defineProp('nodeType', {
			'dataType': DataType.STRING,
			'scope': Class.PropertyScope.PUBLIC
		});
		this.defineProp('pathNodeParams', {
			'dataType': DataType.HASH,
			'scope': Class.PropertyScope.PUBLISHED,
			'getter': function()
			{
				var result = this.getPropStoreFieldValue('pathNodeParams');
				if (!result)
				{
					result = {};
					this.setPropStoreFieldValue('pathNodeParams', result);
				}
				return result;
			},
			'setter': function(value)
			{
				if (!value)
					this.setPropStoreFieldValue('pathNodeParams', null);
				else
					this.setPropStoreFieldValue('pathNodeParams', Object.extend({}, value, true));
			}
		});
	},
	/** @ignore */
	initPropValues: function(/*$super*/)
	{
		this.tryApplySuper('initPropValues')  /* $super() */;
		//this.setInteractMode(Kekule.ChemObjInteractMode.UNSELECTABLE);
	},
	/** @ignore */
	getAllowCoordStickTo: function(dest)
	{
		if (!dest || !this.isSiblingWith(dest))
		{
			var p = this.getParent();
			// coord stick is controlled by parent glyph
			if (p && p.getAllowChildCoordStickTo)
				return p.getAllowChildCoordStickTo(this, dest);
		}
		// defaultly is not allowed
		return false;
	},
	/** @ignore */
	getAcceptCoordStickFrom: function(fromObj)
	{
		var p = this.getParent();
		// coord stick is controlled by parent glyph
		if (p && p.getChildAcceptCoordStickFrom)
			return p.getChildAcceptCoordStickFrom(this, fromObj);
		// defaultly is not allowed
		return false;
	},
	/** @ignore */
	notifyCoordStickTargetChanged: function(/*$super, */oldTarget, newTarget)
	{
		this.tryApplySuper('notifyCoordStickTargetChanged', [oldTarget, newTarget])  /* $super(oldTarget, newTarget) */;
		var p = this.getParent();
		if (Kekule.ObjUtils.isUnset(this.getPathNodeParams().useStickingOffset))
		{
			if (p.getChildUseCoordStickOffset)
			{
				var useOffset = p.getChildUseCoordStickOffset(this, newTarget);
				if (Kekule.ObjUtils.notUnset(useOffset))
				{
					this.getPathNodeParams().useStickingOffset = useOffset;
				}
			}
		}
		// notify parent that stick target has been changed
		if (p.notifyChildCoordStickTargetChanged)
		{
			p.notifyChildCoordStickTargetChanged(this.oldTarget, newTarget);
		}
	}
});

/**
 * Enumeration of path types.
 * @class
 */
Kekule.Glyph.NodeType = {
	/* A default node, same as location point, do not need to draw. */
	/*
	DEFAULT: 'default',
	*/
	/** Location point, do not need to draw. Default value of node type. */
	LOCATION: 'location',
	/** Control point, control the shape of glyph, do not need to draw. */
	CONTROLLER: 'controller',
	/** Do not need to draw and can not manipulate in editor. */
	HIDDEN: 'hidden'
};

/**
 * Represent control node of glyph path connector.
 * @class
 * @augments Kekule.BaseStructureNode
 * @param {String} id Id of this node.
 * @param {Hash} coord2D The 2D coordinates of node, {x, y}, can be null.
 * @param {Hash} coord3D The 3D coordinates of node, {x, y, z}, can be null.
 *
 * @property {String} nodeType Type of this glyph node.
 */
Kekule.Glyph.PathGlyphConnectorControlNode = Class.create(Kekule.BaseStructureNode,
/** @lends Kekule.Glyph.PathGlyphConnectorControlNode# */
{
	/** @private */
	CLASS_NAME: 'Kekule.Glyph.PathGlyphConnectorControlNode',
	initialize: function(/*$super, */id, coord2D, coord3D)
	{
		this.tryApplySuper('initialize', [id])  /* $super(id) */;
		if (coord2D)
			this.setCoord2D(coord2D);
		if (coord3D)
			this.setCoord3D(coord3D);
	},
	/** @ignore */
	initPropValues: function(/*$super*/)
	{
		this.tryApplySuper('initPropValues')  /* $super() */;
		this.setInteractMode(Kekule.ChemObjInteractMode.UNSELECTABLE);
	},
	/** @private */
	getAutoIdPrefix: function()
	{
		return 'cn';
	},
	/** @ignore */
	getAcceptCoordStickFrom: function(fromObj)
	{
		return false;  // ensure other node can not stick to this control node
	},
	/**
	 * Returns the parent connector object.
	 * @returns {Kekule.Glyph.PathGlyphConnector}
	 */
	getParentConnector: function()
	{
		var p = this.getParent();
		if (p && p instanceof Kekule.Glyph.PathGlyphConnector)
			return p;
		else
			return null;
	},

	/**
	 * Returns whether the position of control point is set in coordMode.
	 * @returns {Bool}
	 */
	isPositioned: function(coordMode, allowCoordBorrow)
	{
		if (!coordMode)
			coordMode = CM.COORD2D;
		return this.doCheckIsPositioned(coordMode, allowCoordBorrow);
	},
	/**
	 * Do actual work of method isPositioned.
	 * Descendants should override this method.
	 * @returns {Bool}
	 * @private
	 */
	doCheckIsPositioned: function(coordMode, allowCoordBorrow)
	{
		return true;
	},
	/**
	 * Reset the position of this control node in coordMode.
	 */
	resetPosition: function(coordMode)
	{
		if (!coordMode)
		{
			this.doResetPosition(CM.COORD2D);
			this.doResetPosition(CM.COORD3D);
		}
		else
			this.doResetPosition(coordMode);
		return this;
	},
	/**
	 * Do actual work of method resetPosition.
	 * Descendants should override this method.
	 * @private
	 */
	doResetPosition: function(coordMode)
	{
		// do nothing here
	},
});

/**
 * Enumeration of path types.
 * @class
 */
Kekule.Glyph.PathType = {
	/** A straight line, may contains arrow at beginning and ending. */
	LINE: 'L',
	/** A arc path */
	ARC: 'A'
};

/**
 * Enumeration of path end arrow types.
 * @class
 */
Kekule.Glyph.ArrowType = {
	NONE: null,
	OPEN: 'open',
	TRIANGLE: 'triangle'
};
/**
 * Enumeration of arrow location around path.
 * @class
 */
Kekule.Glyph.ArrowSide = {
	DEFAULT: 0,
	BOTH: 0,  // default
	SINGLE: 1,  // one one side of path
	REVERSED: -1   // one side but at the different side of SINGLE
};

/**
 * General connector between glyph nodes.
 * @class
 * @augments Kekule.BaseStructureConnector
 * @param {String} id Id of this connector.
 * @param {String} pathType Type of path to draw between connected nodes.
 * @param {Array} connectedObjs Objects ({@link Kekule.ChemStructureObject}) connected by connected, usually a connector connects two nodes.
 *
 * @property {String} pathType Type of path to draw between connected nodes, value from {@link Kekule.Glyph.PathType}.
 * @property {Hash} pathParams Other params to control the outlook of path. May including the following fields:
 *   {
 *     lineCount: {Int} need to draw single or multiple line in path?
 *     lineGap: {Float} gap between multiple lines, a relative value to ref length.
 *     startArrowType:
 *     startArrowSide:
 *     startArrowLength, startArrowWidth:
 *     endArrowType:
 *     endArrowSide:
 *     endArrowLength, endArrowWidth:
 *     //startOffsetPercent, endOffsetPercent: {Float} percent of total path length, determinated by the actual renderer of connector
 *     autoOffset: {Bool}
 *   }
 */
Kekule.Glyph.PathGlyphConnector = Class.create(Kekule.BaseStructureConnector,
/** @lends Kekule.Glyph.PathGlyphConnector# */
{
	/** @private */
	CLASS_NAME: 'Kekule.Glyph.PathGlyphConnector',
	/** @constructs */
	initialize: function(/*$super, */id, pathType, connectedObjs)
	{
		this.tryApplySuper('initialize', [id, connectedObjs])  /* $super(id, connectedObjs) */;
		this.setPathType(pathType);
		//this.setControlPoints([new Kekule.Glyph.PathGlyphConnectorControlNode(null, {x: 0.1, y: 0.1})]);  // test
	},
	/** @private */
	initProperties: function()
	{
		this.defineProp('pathType', {
			'dataType': DataType.STRING,
			'scope': Class.PropertyScope.PUBLISHED,
			'enumSource': Kekule.Glyph.PathType
		});
		this.defineProp('pathParams', {
			'dataType': DataType.HASH,
			'scope': Class.PropertyScope.PUBLISHED,
			'getter': function()
			{
				var result = this.getPropStoreFieldValue('pathParams');
				if (!result)
				{
					result = {};
					this.setPropStoreFieldValue('pathParams', result);
				}
				return result;
			},
			'setter': function(value)
			{
				if (!value)
					this.setPropStoreFieldValue('pathParams', null);
				else
					this.setPropStoreFieldValue('pathParams', Object.extend({}, value, true));
			}
		});
		this.defineProp('controlPoints', {
			'dataType': DataType.ARRAY,
			'scope': Class.PropertyScope.PUBLISHED,
			'getter': function(autoCreate)
			{
				var result = this.getPropStoreFieldValue('controlPoints');
				if (!result && autoCreate)
				{
					result = [];
					this.setPropStoreFieldValue('controlPoints', result);
				}
				if (result && !result.length)
				{
					this.doFillDefaultControlPoints(result);
				}
				return result;
			},
			'setter': function(value)
			{
				this.clearControlPoints();
				this.setPropStoreFieldValue('controlPoints', value);
				/*
				this._updateControlPointsOwner();
				this._updateControlPointsParent();
				*/
				this._controlPointsChanged();
			}
		});
	},
	/** @ignore */
	initPropValues: function(/*$super*/)
	{
		this.tryApplySuper('initPropValues')  /* $super() */;
		this.setInteractMode(Kekule.ChemObjInteractMode.UNSELECTABLE);
	},

	/** @ignore */
	_appendChildObj: function(/*$super, */obj)
	{
		var result = this.tryApplySuper('_appendChildObj', [obj])  /* $super(obj) */;
		// when add new child control point, try reset its position
		if (obj instanceof Kekule.Glyph.PathGlyphConnectorControlNode)
		{
			this.tryResetControlPointPosition(obj);
		}
		return result;
	},
	/** @ignore */
	doPropChanged: function(/*$super, */propName, newValue)
	{
		// reset position of all control points when connected objs are set
		if (propName === 'connectedObjs')
		{
			this.tryResetAllControlPointPositions();
		}
		return this.tryApplySuper('doPropChanged', [propName, newValue])  /* $super(propName, newValue) */;
	},

	// methods about coords
	// since connector has child control points, it must implement related coord method for the children to get abs coord
	/** @private */
	getAbsCoord2D: function(allowCoordBorrow)
	{
		return this.getAbsCoordOfMode(Kekule.CoordMode.COORD2D, allowCoordBorrow);
	},
	/** @private */
	getAbsCoord3D: function(allowCoordBorrow)
	{
		return this.getAbsCoordOfMode(Kekule.CoordMode.COORD2D, allowCoordBorrow);
	},
	/** @private */
	getAbsCoordOfMode: function(coordMode, allowCoordBorrow)
	{
		var CU = Kekule.CoordUtils;
		// coord is based on connected objects
		var sum = {'x': 0, 'y': 0};
		var count = 0;
		for (var i = 0, l = this.getConnectedObjCount(); i < l; ++i)
		{
			var obj = this.getConnectedObjAt(i);
			if (obj && obj.getAbsCoordOfMode)
			{
				var coord = obj.getAbsCoordOfMode(coordMode, allowCoordBorrow);
				if (coord)
				{
					++count;
					sum = CU.add(sum, coord);
				}
			}
		}
		return CU.divide(sum, count);
	},

	// methods about children
	/** @ignore */
	getChildSubgroupNames: function(/*$super*/)
	{
		return ['controlPoint'].concat(this.tryApplySuper('getChildSubgroupNames')  /* $super() */);
	},
	/** @ignore */
	getBelongChildSubGroupName: function(/*$super, */obj)
	{
		if (obj instanceof Kekule.Glyph.PathGlyphArcConnectorControlNode)
			return 'controlPoint';
		else
			return this.tryApplySuper('getBelongChildSubGroupName', [obj])  /* $super(obj) */;
	},
	/**
	 * Returns the count of child control points.
	 * @returns {Int}
	 */
	getControlPointCount: function()
	{
		var ps = this.getControlPoints();
		return ((ps && ps.length) || 0);
	},
	/**
	 * Returns child control point at index.
	 * @param {Int} index
	 * @returns {Kekule.Glyph.PathGlyphArcConnectorControlNode}
	 */
	getControlPointAt: function(index)
	{
		var ps = this.getControlPoints() || [];
		return ps[index];
	},
	/**
	 * Returns the index of a child control point.
	 * @param {Kekule.Glyph.PathGlyphArcConnectorControlNode} point
	 */
	indexOfControlPoint: function(point)
	{
		var ps = this.getControlPoints();
		var result = ps? ps.indexOf(point): -1;
		return result;
	},
	/**
	 * Removes the child control point at index.
	 * @param {Int} index
	 * @returns {Kekule.Glyph.PathGlyphArcConnectorControlNode} Removed point or null if nothing is removed.
	 */
	removeControlPointAt: function(index)
	{
		var result = null;
		var ps = this.getControlPoints();
		if (ps)
		{
			if (index >= 0 && index < ps.length)
			{
				result = ps.splice(index, 1);
				if (result.setOwner)
					result.setOwner(null);
				if (result.setParent)
					result.setParent(null);
				this.notifyPropSet('controlPoints', this.getControlPoints());
			}
		}
		return result;
	},
	/**
	 * Remove a child control point.
	 * @param {Kekule.Glyph.PathGlyphArcConnectorControlNode} point
	 * @returns {Kekule.Glyph.PathGlyphArcConnectorControlNode} Actually removed object.
	 */
	removeControlPoint: function(point)
	{
		var result = null;
		var ps = this.getControlPoints();
		if (ps)
		{
			var index = ps.indexOf(point);
			if (index >= 0)
			{
				ps.splice(index, 1);
				if (point.setOwner)
					point.setOwner(null);
				if (point.setParent)
					point.setParent(null);
				result = point;
				this.notifyPropSet('controlPoints', this.getControlPoints());
			}
		}
		return result;
	},
	/**
	 * Insert a control point at index.
	 * @param {Kekule.Glyph.PathGlyphArcConnectorControlNode} point
	 * @param {Int} index
	 */
	insertControlPointAt: function(point, index)
	{
		var ps = this.getControlPoints();
		if (ps && index < ps.length)
		{
			var r = Kekule.ArrayUtils.insertUniqueEx(ps, point, index);
			if (r.isInserted)
			{
				if (point.setOwner)
					point.setOwner(this.getOwner());
				if (point.setParent)
					point.setParent(this);
				this.notifyPropSet('controlPoints', this.getControlPoints());
			}
			return r.index;
		}
		return -1;
	},
	/**
	 * Insert a control point before refPoint.
	 * If refPoint is not set, point will be appended to the tail.
	 * @param {Kekule.Glyph.PathGlyphArcConnectorControlNode} point
	 * @param {Kekule.Glyph.PathGlyphArcConnectorControlNode} refPoint
	 */
	insertControlPointBefore: function(point, refPoint)
	{
		var ps = this.getControlPoints();
		if (ps)
		{
			var index = refPoint ? this.indexOfControlPoint(refPoint) : -1;
			if (index < 0)
				index = ps.length;
			return this.insertControlPointAt(point, index);
		}
		return -1;
	},

	/** @private */
	tryResetControlPointPosition: function(point)
	{
		if (point.isPositioned && point.resetPosition)
		{
			var cms = [CM.COORD2D, CM.COORD3D];
			for (var i = 0, l = cms.length; i < l; ++i)
			{
				var coordMode = cms[i];
				if (!point.isPositioned(coordMode, true))
				{
					point.resetPosition(coordMode);
				}
			}
		}
	},
	/** @private */
	tryResetAllControlPointPositions: function()
	{
		for (var i = 0, l = this.getControlPointCount(); i < l; ++i)
		{
			var p = this.getControlPointAt(i);
			this.tryResetControlPointPosition(p);
		}
	},

	/**
	 * Fill in default control points when the controlPoints property is empty while creating or loading glyph.
	 * Descendants may override this method.
	 * @private
	 */
	doFillDefaultControlPoints: function(controlPoints)
	{
		// do nothing here
	},

	/**
	 * Remove childObj from connector.
	 * @param {Variant} childObj A child control point.
	 */
	removeChildObj: function(childObj)
	{
		return this.removeControlPoint(childObj);
	},
	/*
	 * Remove child obj directly.
	 * @param {Variant} childObj A child node or connector.
	 */
	/*
	removeChild: function($super, obj)
	{
		return this.removeChildObj(obj) || $super(obj);
	},
	*/

	/**
	 * Check if childObj is a child control point of this connector.
	 * @param {Kekule.ChemObject} childObj
	 * @returns {Bool}
	 */
	hasChildObj: function(childObj)
	{
		//var ps = this.getControlPoints();
		//return ps && ps.indexOf(childObj) >= 0;
		return this.hasChild(childObj);
	},

	/**
	 * Returns next sibling node or connector to childObj.
	 * @param {Variant} childObj Node or connector.
	 * @returns {Variant}
	 */
	getNextSiblingOfChild: function(childObj)
	{
		var index = this.indexOfChild(childObj);
		++index;
		return this.getChildAt(index);
	},

	/*
	 * Get count of child objects.
	 * @returns {Int}
	 */
	/*
	getChildCount: function($super)
	{
		var ps = this.getControlPoints();
		return ((ps && ps.length) || 0) + $super();
	},
	*/
	/*
	 * Get child control point at index.
	 * @param {Int} index
	 * @returns {Variant}
	 */
	/*
	getChildAt: function($super, index)
	{
		var ps = this.getControlPoints() || [];
		return ps[index] || $super(index - ps.length);
	},
	*/
	/*
	 * Get the index of obj in children list.
	 * @param {Variant} obj
	 * @returns {Int} Index of obj or -1 when not found.
	 */
	/*
	indexOfChild: function($super, obj)
	{
		var ps = this.getControlPoints();
		var result = ps? ps.indexOf(obj): -1;
		if (result < 0)
			result = (ps? ps.length: 0) + $super(obj);
	},
	*/

	/** @private */
	_controlPointsChanged: function()
	{
		this._updateControlPointsOwner();
		this._updateControlPointsParent();
	},

	/** @private */
	_updateControlPointsOwner: function(owner)
	{
		if (!owner)
			owner = this.getOwner();
		var ps = this.getControlPoints();
		if (ps)
		{
			for (var i = 0, l = ps.length; i < l; ++i)
			{
				var p = ps[i];
				if (p.setOwner)
					p.setOwner(owner);
			}
		}
	},
	/** @private */
	_updateControlPointsParent: function(parent)
	{
		if (!parent)
			parent = this;
		var ps = this.getControlPoints();
		if (ps)
		{
			for (var i = 0, l = ps.length; i < l; ++i)
			{
				var p = ps[i];
				if (p.setParent)
					p.setParent(parent);
			}
		}
	},

	/**
	 * Clear all control points of this connector.
	 * @private
	 */
	clearControlPoints: function()
	{
		var oldPoints = this.getControlPoints();
		if (oldPoints)
			oldPoints = Kekule.ArrayUtils.clone(oldPoints);

		this.setPropStoreFieldValue('controlPoints', null);

		if (oldPoints)
		{
			for (var i = 0, l = oldPoints.length; i < l; ++i)
			{
				var p = oldPoints[i];
				if (p.setOwner)
					p.setOwner(null);
				if (p.setParent)
					p.setParent(null);
			}
		}

		this.notifyPropSet('controlPoints', null);
		return this;
	},
	/**
	 * Modify current path params.
	 * Only values in params will be changed, other values of old path params will remain unchanged.
	 * @param {Hash} params
	 * @returns {Hash} New path params.
	 */
	modifyPathParams: function(params)
	{
		if (params)
		{
			var p = this.getPathParams();
			p = Object.extend(p, params || {});
			this.setPathParams(p);
			return p;
		}
		else
			return null;
	}
});


/**
 * Control node of glyph arc path connector.
 * @class
 * @augments Kekule.Glyph.PathGlyphConnectorControlNode
 *
 * @property {String} nodeType Type of this glyph node.
 */
Kekule.Glyph.PathGlyphArcConnectorControlNode = Class.create(Kekule.Glyph.PathGlyphConnectorControlNode,
/** @lends Kekule.Glyph.PathGlyphArcConnectorControlNode# */
{
	/** @private */
	CLASS_NAME: 'Kekule.Glyph.PathGlyphArcConnectorControlNode',
	/** @private */
	initProperties: function()
	{
		this.defineProp('distanceToChord', {
			'dataType': DataType.FLOAT
		});
	},
	/** @ignore */
	doCheckIsPositioned: function(coordMode, allowCoordBorrow)
	{
		return this.getIndirectCoordRefCoords(coordMode, allowCoordBorrow) && Kekule.ObjUtils.notUnset(this.getDistanceToChord());
	},
	/** @ignore */
	doResetPosition: function(coordMode)
	{
		if (coordMode === CM.COORD2D)  // only apply to 2D glyph
		{
			var coords = this.getIndirectCoordRefCoords(coordMode, true);
			if (coords && coords[0] && coords[1])
			{
				var refLength = CU.getDistance(coords[0], coords[1]);
				this.setDistanceToChord(refLength * 0.5);  // default 1/2 of ref length
				this.notifyPropSet('coord2D', this.getCoord2D());  // notify the value of coord has been changed
			}
		}
	},

	/** @ignore */
	doGetEnableIndirectCoord: function()
	{
		return true;  // force always use relative coord
	},
	/** @ignore */
	getIndirectCoordRefCoords: function(/*$super, */coordMode, allowCoordBorrow)
	{
		var connector = this.getParentConnector();
		if (connector && connector.getConnectedObjCount() >= 2)
		{
			// use abs coord to calculate the arc, since there may be shadow node in connector
			var c1 = connector.getConnectedObjAt(0).getAbsCoordOfMode(allowCoordBorrow);
			var c2 = connector.getConnectedObjAt(1).getAbsCoordOfMode(allowCoordBorrow);
			return [c1, c2];
		}
		else
			return this.tryApplySuper('getIndirectCoordRefCoords', [coordMode])  /* $super(coordMode) */;
	},
	/** @ignore */
	calcIndirectCoordValue: function(/*$super, */coordMode, allowCoordBorrow)
	{
		if (coordMode === CM.COORD2D)
		{
			// coord 2D is determinated by distance to chord
			/*
			var ratio = this.getIndirectCoordStorageOfMode(coordMode);
			var d = ratio && ratio.distanceToChord;
			*/
			var d = this.getDistanceToChord();
			if (Kekule.ObjUtils.notUnset(d))
			{
				var refCoords = this.getIndirectCoordRefCoords();
				if (refCoords)
				{
					// use abs coord to calculate the arc, since there may be shadow node in connector
					var arcStartCoord = refCoords[0];
					var arcEndCoord = refCoords[1];
					if (arcStartCoord && arcEndCoord)
					{
						//var midCoord = CU.divide(CU.add(arcStartCoord, arcEndCoord), 2);
						var chordVector = CU.substract(arcEndCoord, arcStartCoord);
						var signX = Math.sign(chordVector.x);
						var signY = Math.sign(chordVector.y);
						if (Kekule.NumUtils.isFloatEqual(chordVector.x, 0, 1e-10))  // vertical line
						{
							result = {'x': -d * signY, 'y': 0};
						}
						else if (Kekule.NumUtils.isFloatEqual(chordVector.y, 0, 1e-10))  // horizontal line
						{
							result = {'x': 0, 'y': d * signX};
						}
						else
						{
							var chordSlope = chordVector.y / chordVector.x;
							var refSlope = -1 / chordSlope;
							var result = {'x': -signY * d / Math.sqrt(1 + Math.sqr(refSlope))};
							result.y = result.x * refSlope;
						}
						return result;
					}
				}
			}
		}

		return this.tryApplySuper('calcIndirectCoordValue', [coordMode, allowCoordBorrow])  /* $super(coordMode, allowCoordBorrow) */;
	},
	/** @ignore */
	saveIndirectCoordValue: function(/*$super, */coordMode, coordValue, oldCoordValue, allowCoordBorrow)
	{
		if (coordMode === CM.COORD2D)
		{
			// the control point of arc should always be at the middle of arc, so do this constraint
			var oldCoord = oldCoordValue;
			var refCoords = this.getIndirectCoordRefCoords();
			if (refCoords)
			{
				var arcStartCoord = refCoords[0];
				var arcEndCoord = refCoords[1];
				var baseVector = CU.substract(arcEndCoord, arcStartCoord);
				var baseAngle = Math.atan2(baseVector.y, baseVector.x);
				var refLength = CU.getDistance(arcStartCoord, arcEndCoord);

				var valueDeltaVector = CU.substract(coordValue, oldCoord);
				var valueDeltaAngle = Math.atan2(valueDeltaVector.y, valueDeltaVector.x);
				var valueDeltaLength = CU.getDistance(coordValue, oldCoord);

				var actualMovement = valueDeltaLength * Math.sin(valueDeltaAngle - baseAngle);

				var distanceToChord = this.getDistanceToChord() || 0;
				var newDistanceToChord = distanceToChord + actualMovement;
				this.setDistanceToChord(newDistanceToChord);

				return newDistanceToChord;
			}
		}

		return this.tryApplySuper('saveIndirectCoordValue', [coordMode, coordValue])  /* $super(coordMode, coordValue) */;
	}

	/* @ignore */
	/*
	doGetCoord2D: function($super, allowCoordBorrow, allowCreateNew)
	{
		// coord 2D is determinated by distance to chord
		var d = this.getDistanceToChord();
		if (Kekule.ObjUtils.notUnset(d))
		{
			var connector = this.getParentConnector();
			if (connector)
			{
				// use abs coord to calculate the arc, since there may be shadow node in connector
				var arcStartCoord = connector.getConnectedObjAt(0).getAbsCoord2D(allowCoordBorrow);
				var arcEndCoord = connector.getConnectedObjAt(1).getAbsCoord2D(allowCoordBorrow);
				if (arcStartCoord && arcEndCoord)
				{
					//var midCoord = CU.divide(CU.add(arcStartCoord, arcEndCoord), 2);
					var chordVector = CU.substract(arcEndCoord, arcStartCoord);
					var signX = Math.sign(chordVector.x);
					var signY = Math.sign(chordVector.y);
					if (Kekule.NumUtils.isFloatEqual(chordVector.x, 0, 1e-10))  // vertical line
					{
						result = {'x': - d * signY, 'y': 0};
					}
					else if (Kekule.NumUtils.isFloatEqual(chordVector.y, 0, 1e-10))  // horizontal line
					{
						result = {'x': 0, 'y': d * signX};
					}
					else
					{
						var chordSlope = chordVector.y / chordVector.x;
						var refSlope = -1 / chordSlope;
						var result = {'x': -signY * d / Math.sqrt(1 + Math.sqr(refSlope))};
						result.y = result.x * refSlope;
					}
					//console.log(d, arcStartCoord, arcEndCoord);

					return result;
				}
			}
		}

		return $super(allowCoordBorrow, allowCreateNew);
	},
	*/

	/* @ignore */
	/*
	doSetCoord2D: function($super, value)
	{
		// the control point of arc should alway be at the middle of arc, so do this constraint
		var oldCoord = this.getCoord2D();
		var connector = this.getParentConnector();
		if (connector)
		{
			var arcStartCoord = connector.getConnectedObjAt(0).getCoord2D();
			var arcEndCoord = connector.getConnectedObjAt(1).getCoord2D();
			var baseVector = CU.substract(arcEndCoord, arcStartCoord);
			var baseAngle = Math.atan2(baseVector.y, baseVector.x);

			var valueDeltaVector = CU.substract(value, oldCoord);
			var valueDeltaAngle = Math.atan2(valueDeltaVector.y, valueDeltaVector.x);
			var valueDeltaLength = CU.getDistance(value, oldCoord);

			var actualMovement = valueDeltaLength * Math.sin(valueDeltaAngle - baseAngle);
			var newDistanceToChord = (this.getDistanceToChord() || 0) + actualMovement;
			this.setDistanceToChord(newDistanceToChord);

			return;
		}

		return $super(value);
	}
	*/
});

/**
 * BaseArc shaped connector between glyph nodes.
 * @class
 * @augments Kekule.Glyph.PathGlyphConnector
 * @param {String} id Id of this connector.
 * @param {Array} connectedObjs Objects ({@link Kekule.ChemStructureObject}) connected by connected, usually a connector connects two nodes.
 */
Kekule.Glyph.PathGlyphArcConnector = Class.create(Kekule.Glyph.PathGlyphConnector,
/** @lends Kekule.Glyph.PathGlyphArcConnector# */
{
	/** @private */
	CLASS_NAME: 'Kekule.Glyph.PathGlyphArcConnector',
	/** @constructs */
	initialize: function(/*$super, */id, connectedObjs)
	{
		this.tryApplySuper('initialize', [id, Kekule.Glyph.PathType.ARC, connectedObjs])  /* $super(id, Kekule.Glyph.PathType.ARC, connectedObjs) */;
		// add control point to control the arc
		/*
		var controlPoint = new Kekule.Glyph.PathGlyphArcConnectorControlNode(null, {x: 0, y: 0});
		this.setControlPoints([controlPoint]);
		*/
		var controlPoints = this._createDefaultControlPoints();
		this.setControlPoints(controlPoints);
	},
	/**
	 * Returns the arc control point.
	 * @returns {Kekule.Glyph.PathGlyphArcConnectorControlNode}
	 */
	getControlPoint: function()
	{
		return (this.getControlPoints() || [])[0]
	},
	/** @ignore */
	doFillDefaultControlPoints: function(controlPoints)
	{
		var points = this._createDefaultControlPoints();
		Kekule.ArrayUtils.pushUnique(controlPoints, points);
	},
	_createDefaultControlPoints: function()
	{
		var controlPoint = new Kekule.Glyph.PathGlyphArcConnectorControlNode(null, {x: 0, y: 0});
		return [controlPoint];
	}
});

/**
 * A glyph defined by a series of nodes and connectors (paths).
 * @class
 * @augments Kekule.Glyph
 * @param {String} id Id of this node.
 * @param {Float} refLength ref length of editor, this value will be used to create suitable connector length.
 * @param {Hash} initialParams InitialParams used for creating connector and nodes.
 *   Can including all the fields in pathParams property of connector.
 *   Note: in initialParams, length fields(e.g. startArrowLength, endArrowLength) are based on refLength,
 *   field * refLength will be the actual length passed to connector. In private method createDefaultStructure,
 *   those length fields will be converted into actual length and passed into doCreateDefaultStructure.
 * @param {Object} coord2D The 2D coordinates of node, {x, y}, can be null.
 * @param {Object} coord3D The 3D coordinates of node, {x, y, z}, can be null.
 *
 * @property {Array} nodes All nodes in this glyph.
 * @property {Array} connectors Connectors (paths) in this glyph.
 */
Kekule.Glyph.PathGlyph = Class.create(Kekule.Glyph.Base,
/** @lends Kekule.Glyph.PathGlyph# */
{
	/** @private */
	CLASS_NAME: 'Kekule.Glyph.PathGlyph',
	/**
	 * @constructs
	 */
	initialize: function(/*$super, */id, refLength, initialParams, coord2D, coord3D)
	{
		this.tryApplySuper('initialize', [id, coord2D, coord3D])  /* $super(id, coord2D, coord3D) */;
		this.createDefaultStructure(refLength || 1, initialParams || {});
	},
	doFinalize: function(/*$super*/)
	{
		if (this.hasCtab())
			this.getCtab().finalize();
		this.tryApplySuper('doFinalize')  /* $super() */;
	},
	/** @private */
	initProperties: function()
	{
		this.defineProp('ctab', {
			'dataType': 'Kekule.StructureConnectionTable',
			'scope': Class.PropertyScope.PUBLIC,
			'getter': function(allowCreate)
			{
				if (!this.getPropStoreFieldValue('ctab'))
				{
					if (allowCreate)
						this.createCtab();
				}
				return this.getPropStoreFieldValue('ctab');
			},
			'setter': function(value)
			{
				var old = this.getPropStoreFieldValue('ctab');
				if (old)
				{
					old.finalize();
					old = null;
				}

				if (value)
				{
					value.setPropValue('parent', this, true);
					value.setOwner(this.getOwner());
				}

				this.setPropStoreFieldValue('ctab', value);
			}
		});
		// values are read from ctab
		this.defineProp('nodes', {
			'dataType': DataType.ARRAY,
			'serializable': false,
			'scope': Class.PropertyScope.PUBLIC,
			'setter': null,
			'getter': function() { return this.hasCtab()? this.getCtab().getNodes(): []; }
		});
		this.defineProp('connectors', {
			'dataType': DataType.ARRAY,
			'serializable': false,
			'scope': Class.PropertyScope.PUBLIC,
			'setter': null,
			'getter': function() { return this.hasCtab()? this.getCtab().getConnectors(): []; }
		});
	},

	/**
	 * Create default structure by ref length.
	 * @param {Float} refLength
	 * @param {Hash} initialParams
	 * @private
	 */
	createDefaultStructure: function(refLength, initialParams)
	{
		if (!refLength)
			refLength = 1;
		var actualParams = {};
		var lengthFields = [/*'lineGap',*/ 'startArrowLength', 'startArrowWidth', 'endArrowLength', 'endArrowWidth'];
		for (var field in initialParams)
		{
			if (lengthFields.indexOf(field) >= 0)
			{
				actualParams[field] = initialParams[field] * refLength;
				//console.log('transform', field, refLength, initialParams[field], actualParams[field]);
			}
			else
				actualParams[field] = initialParams[field];
		}
		return this.doCreateDefaultStructure(refLength, actualParams);
	},
	/**
	 * Do actual work of createDefaultStructure.
	 * Descendants need to override this method.
	 * @param {Float} refLength
	 * @param {Hash} initialParams
	 * @private
	 */
	doCreateDefaultStructure: function(refLength, initialParams)
	{
		// do nothing here
	},

	/** @private */
	getAutoIdPrefix: function()
	{
		return 'p';
	},
	/** @private */
	ownerChanged: function(/*$super, */newOwner, oldOwner)
	{
		if (this.hasCtab())
			this.getCtab().setOwner(newOwner);
		this.tryApplySuper('ownerChanged', [newOwner, oldOwner])  /* $super(newOwner, oldOwner) */;
	},
	/** @private */
	_removeChildObj: function(/*$super, */obj)
	{
		if (this.hasCtab())
		{
			var ctab = this.getCtab();
			if (ctab === obj)
				this.removeCtab();
			else
			{
				if (ctab.hasChildObj(obj))
					ctab.removeChildObj(obj);
			}
		}
		this.tryApplySuper('_removeChildObj', [obj])  /* $super(obj) */;
	},
	/**
	 * Returns if this fragment has no formula or ctab, or ctab has no nodes or connectors.
	 * @return {Bool}
	 */
	isEmpty: function()
	{
		return this.getCtab().isEmpty();
	},
	/** @private */
	createCtab: function()
	{
		var ctab = new Kekule.StructureConnectionTable(this.getOwner(), this);
		this.setPropStoreFieldValue('ctab', ctab);
		// install event listeners to ctab
		ctab.addEventListener('propValueSet',
			function(e)
			{
				if (e.propName == 'nodes')
				{
					this.notifyPropSet(e.propName, e.propValue);
				}
			}, this);
		ctab.setEnablePropValueSetEvent(true); // to enable propValueSet event
	},
	/**
	 * Check whether a connection table is used to represent this fragment.
	 */
	hasCtab: function()
	{
		return (!!this.getPropStoreFieldValue('ctab'));
	},
	/**
	 * Calculate the box to fit whole glyph.
	 * @param {Int} coordMode Determine to calculate 2D or 3D box. Value from {@link Kekule.CoordMode}.
	 * @param {Bool} allowCoordBorrow
	 * @returns {Hash} Box information. {x1, y1, z1, x2, y2, z2} (in 2D mode z1 and z2 will not be set).
	 */
	getContainerBox: function(/*$super, */coordMode, allowCoordBorrow)
	{
		if (this.hasCtab())
		{
			return this.getCtab().getContainerBox(coordMode, allowCoordBorrow);
		}
		else
			return this.tryApplySuper('getContainerBox', [coordMode])  /* $super(coordMode) */;
	},

	/**
	 * Get a structure node object with a specified id.
	 * @param {String} id
	 * @returns {Kekule.ChemStructureNode}
	 */
	getNodeById: function(id)
	{
		/*
		 var nodes = this.getNodes();
		 for (var i = 0, l = nodes.length; i < l; ++i)
		 {
		 if (nodes[i].getId() == id)
		 return nodes[i];
		 }
		 return null;
		 */
		return this.hasCtab()? this.getCtab().getNodeById(id): null;
	},
	/**
	 * Get a structure connector object with a specified id.
	 * @param {String} id
	 * @returns {Kekule.ChemStructureConnector}
	 */
	getConnectorById: function(id)
	{
		/*
		 var connectors = this.getConnectors();
		 for (var i = 0, l = connectors.length; i < l; ++i)
		 {
		 if (connectors[i].getId() == id)
		 return connectors[i];
		 }
		 return null;
		 */
		return this.hasCtab()? this.getCtab().getConnectorById(id): null;
	},
	/**
	 * Get a structure node or connector object with a specified id.
	 * @param {String} id
	 * @returns {Kekule.ChemStructureObject}
	 */
	getObjectById: function(id)
	{
		var node = this.getNodeById(id);
		return node? node: this.getConnectorById(id);
	},
	/**
	 * Return count of nodes.
	 * @returns {Int}
	 */
	getNodeCount: function()
	{
		return this.hasCtab()? this.getCtab().getNodeCount(): 0;
	},
	/**
	 * Get node at index.
	 * @param {Int} index
	 * @returns {Kekule.Glyph.PathGlyphNode}
	 */
	getNodeAt: function(index)
	{
		return this.hasCtab()? this.getCtab().getNodeAt(index): null;
	},
	/**
	 * Get index of node.
	 * @param {Kekule.Glyph.PathGlyphNode} node
	 * @returns {Int}
	 */
	indexOfNode: function(node)
	{
		return this.hasCtab()? this.getCtab().indexOfNode(node): -1;
	},
	/**
	 * Check if a node exists in structure.
	 * @param {Kekule.Glyph.PathGlyphNode} node Node to seek.
	 * @param {Bool} checkNestedStructure If true the nested sub groups will also be checked.
	 * @returns {Bool}
	 */
	hasNode: function(node, checkNestedStructure)
	{
		return this.hasCtab()? this.getCtab().hasNode(node, checkNestedStructure): null;
	},
	/**
	 * Add node to container. If node already in container, nothing will be done.
	 * @param {Kekule.Glyph.PathGlyphNode} node
	 */
	appendNode: function(node)
	{
		/*
		 if (this.getNodes().indexOf(node) >= 0) // already exists
		 ;// do nothing
		 else
		 {
		 var result = this.getNodes().push(node);
		 this.notifyNodesChanged();
		 return result;
		 }
		 */
		return this.doGetCtab(true).appendNode(node);
	},
	/**
	 * Insert node to index. If index is not set, node will be inserted as the first node of ctab.
	 * @param {Kekule.Glyph.PathGlyphNode} node
	 * @param {Int} index
	 */
	insertNodeAt: function(node, index)
	{
		return this.doGetCtab(true).insertNodeAt(node, index);
	},
	/**
	 * Insert node before refNode. If refNode is not set, node will be appended to the tail.
	 * @param {Kekule.Glyph.PathGlyphNode} node
	 * @param {Int} index
	 */
	insertNodeBefore: function(node, refNode)
	{
		var index = refNode? this.indexOfNode(refNode): -1;
		if (index < 0)
			return this.appendNode(node);
		else
			return this.doGetCtab(true).insertNodeAt(node, index);
	},
	/**
	 * Remove node at index in container.
	 * @param {Int} index
	 * @param {Bool} preserveLinkedConnectors Whether remove relations between this node and linked connectors.
	 */
	removeNodeAt: function(index, preserveLinkedConnectors)
	{
		/*
		 var node = this.getNodes()[index];
		 if (node)
		 {
		 // remove from connectors
		 this.removeConnectNode(node);
		 this.getNodes().removeAt(index);
		 this.notifyNodesChanged();
		 }
		 */
		if (!this.hasCtab())
			return null;
		return this.getCtab().removeNodeAt(index, preserveLinkedConnectors);
	},
	/**
	 * Remove a node in container.
	 * @param {Kekule.Glyph.PathGlyphNode} node
	 * @param {Bool} preserveLinkedConnectors Whether remove relations between this node and linked connectors.
	 */
	removeNode: function(node, preserveLinkedConnectors)
	{
		/*
		 var index = this.getNodes().indexOf(node);
		 if (index >= 0)
		 this.removeNodeAt(index);
		 */
		if (!this.hasCtab())
			return null;
		return this.getCtab().removeNode(node, preserveLinkedConnectors);
	},
	/**
	 * Replace oldNode with new one, preserve coords and all linked connectors.
	 * @param {Kekule.Glyph.PathGlyphNode} oldNode Must be direct child of current fragment (node in nested structure fragment will be ignored).
	 * @param {Kekule.Glyph.PathGlyphNode} newNode
	 */
	replaceNode: function(oldNode, newNode)
	{
		if (!this.hasCtab())
			return null;
		return this.getCtab().replaceNode(oldNode, newNode);
	},
	/**
	 * Remove all nodes.
	 */
	clearNodes: function()
	{
		if (this.hasCtab())
			return this.getCtab().clearNodes();
	},
	/**
	 * Check if child nodes has 2D coord.
	 * @param {Bool} allowCoordBorrow
	 * @returns {Bool}
	 */
	nodesHasCoord2D: function(allowCoordBorrow)
	{
		if (!this.hasCtab())
			return false;
		return this.getCtab().nodesHasCoord2D(allowCoordBorrow);
	},
	/**
	 * Check if child nodes has 3D coord.
	 * @param {Bool} allowCoordBorrow
	 * @returns {Bool}
	 */
	nodesHasCoord3D: function(allowCoordBorrow)
	{
		if (!this.hasCtab())
			return false;
		return this.getCtab().nodesHasCoord3D(allowCoordBorrow);
	},
	/**
	 * Return count of connectors.
	 * @returns {Int}
	 */
	getConnectorCount: function()
	{
		return this.hasCtab()? this.getCtab().getConnectorCount(): 0;
	},
	/**
	 * Get connector at index.
	 * @param {Int} index
	 * @returns {Kekule.Glyph.PathGlyphConnector}
	 */
	getConnectorAt: function(index)
	{
		//return this.getConnectors()[index];
		return this.hasCtab()? this.getCtab().getConnectorAt(index): null;
	},
	/**
	 * Get index of connector inside fragment.
	 * @param {Kekule.Glyph.PathGlyphConnector} connector
	 * @returns {Int}
	 */
	indexOfConnector: function(connector)
	{
		return this.hasCtab()? this.getCtab().indexOfConnector(connector): -1;
	},
	/**
	 * Check if a connector exists in structure.
	 * @param {Kekule.Glyph.PathGlyphConnector} connector Connector to seek.
	 * @param {Bool} checkNestedStructure If true the nested sub groups will also be checked.
	 * @returns {Bool}
	 */
	hasConnector: function(connector, checkNestedStructure)
	{
		return this.hasCtab()? this.getCtab().hasConnector(connector, checkNestedStructure): null;
	},
	/**
	 * Add connector to container.
	 * @param {Kekule.Glyph.PathGlyphConnector} connector
	 */
	appendConnector: function(connector)
	{
		/*
		 if (this.getConnectors().indexOf(connector) >= 0) // already exists
		 ;// do nothing
		 else
		 {
		 return this.getConnectors().push(connector);
		 this.notifyConnectorsChanged();
		 }
		 */
		return this.doGetCtab(true).appendConnector(connector);
	},
	/**
	 * Insert connector to index. If index is not set, connector will be inserted as the first connector of ctab.
	 * @param {Kekule.Glyph.PathGlyphConnector} connector
	 * @param {Int} index
	 */
	insertConnectorAt: function(connector, index)
	{
		return this.doGetCtab(true).insertConnectorAt(connector, index);
	},
	/**
	 * Insert connector before refConnector. If refConnector is not set, connector will be appended.
	 * @param {Kekule.Glyph.PathGlyphConnector} connector
	 * @param {Kekule.Glyph.PathGlyphConnector} refConnector
	 * @param {Int} index
	 */
	insertConnectorBefore: function(connector, refConnector)
	{
		var index = refConnector? this.indexOfConnector(refConnector): -1;
		if (index < 0)
			return this.appendConnector(connector);
		else
			return this.doGetCtab(true).insertConnectorAt(connector, index);
	},
	/**
	 * Remove connector at index of connectors.
	 * @param {Int} index
	 * @param {Bool} preserveConnectedObjs Whether delte relations between this connector and related nodes.
	 */
	removeConnectorAt: function(index, preserveConnectedObjs)
	{
		/*
		 var connector = this.getConnectors()[index];
		 if (connector)
		 {
		 this.getConnectors().removeAt(index);
		 this.notifyConnectorsChanged();
		 }
		 */
		if (!this.hasCtab())
			return null;
		return this.getCtab().removeConnectorAt(index, preserveConnectedObjs);
	},
	/**
	 * Remove a connector in container.
	 * @param {Kekule.Glyph.PathGlyphConnector} connector
	 * @param {Bool} preserveConnectedObjs Whether delte relations between this connector and related nodes.
	 */
	removeConnector: function(connector, preserveConnectedObjs)
	{
		/*
		 var index = this.getConnectors().indexOf(connector);
		 if (index >= 0)
		 this.removeConnectorAt(index);
		 */
		if (!this.hasCtab())
			return null;
		return this.getCtab().removeConnector(connector, preserveConnectedObjs);
	},
	/**
	 * Remove all connectors.
	 */
	clearConnectors: function()
	{
		if (this.hasCtab())
			return this.getCtab().clearConnectors();
	},

	/*
	 * Insert obj before refChild in node or connector list of ctab.
	 * If refChild is null or does not exists, obj will be append to tail of list.
	 * @param {Variant} obj A node or connector.
	 * @param {Variant} refChild Ref node or connector
	 * @return {Int} Index of obj after inserting.
	 */
	/*
	insertBefore: function(obj, refChild)
	{
		if (this.hasCtab())
			return this.getCtab().insertBefore(obj, refChild);
	},
	*/

	/**
	 * Returns nodes or connectors that should be removed cascadely with childObj.
	 * @param {Object} childObj
	 * @returns {Array}
	 * @private
	 */
	_getObjsNeedToBeCascadeRemoved: function(childObj, ignoredChildObjs)
	{
		if (this.hasCtab())
			return this.getCtab()._getObjsNeedToBeCascadeRemoved(childObj, ignoredChildObjs);
		else
			return [];
	},

	/** @ignore */
	getChildSubgroupNames: function(/*$super*/)
	{
		return ['node', 'connector'].concat(this.tryApplySuper('getChildSubgroupNames')  /* $super() */);
	},
	/** @ignore */
	getBelongChildSubGroupName: function(/*$super, */obj)
	{
		if (obj instanceof Kekule.Glyph.PathGlyphNode)
			return 'node';
		else if (obj instanceof Kekule.Glyph.PathGlyphConnector)
			return 'connector';
		else
			return this.tryApplySuper('getBelongChildSubGroupName', [obj])  /* $super(obj) */;
	},

	/**
	 * Remove childObj from connection table.
	 * @param {Variant} childObj A child node or connector.
	 * @param {Bool} cascadeRemove Whether remove related objects (e.g., bond connected to an atom).
	 * @param {Bool} freeRemoved Whether free all removed objects.
	 */
	removeChildObj: function(childObj, cascadeRemove, freeRemoved)
	{
		if (this.hasCtab())
			this.getCtab().removeChildObj(childObj, cascadeRemove, freeRemoved);
	},
	/*
	 * Remove child obj directly from connection table.
	 * @param {Variant} childObj A child node or connector.
	 */
	/*
	removeChild: function($super, obj)
	{
		return this.removeChildObj(obj) || $super(obj);
	},
	*/

	/**
	 * Check if childObj is a child node or connector of this fragment's ctab.
	 * @param {Kekule.ChemObject} childObj
	 * @returns {Bool}
	 */
	hasChildObj: function(childObj)
	{
		if (this.hasCtab())
		{
			return this.getCtab().hasChildObj(childObj);
		}
		else
			return false;
	},

	/**
	 * Returns next sibling node or connector to childObj.
	 * @param {Variant} childObj Node or connector.
	 * @returns {Variant}
	 */
	getNextSiblingOfChild: function(childObj)
	{
		if (this.hasCtab())
			return this.getCtab().getNextSiblingOfChild(childObj);
		else
			return null;
	},

	/*
	 * Get count of child objects (including both nodes and connectors).
	 * @returns {Int}
	 */
	/*
	getChildCount: function()
	{
		if (this.hasCtab())
			return this.getCtab().getChildCount();
		else
			return 0;
	},
	*/
	/*
	 * Get child object (including both nodes and connectors) at index.
	 * @param {Int} index
	 * @returns {Variant}
	 */
	/*
	getChildAt: function(index)
	{
		if (this.hasCtab())
			return this.getCtab().getChildAt(index);
		else
			return null;
	},
	*/
	/*
	 * Get the index of obj in children list.
	 * @param {Variant} obj
	 * @returns {Int} Index of obj or -1 when not found.
	 */
	/*
	indexOfChild: function(obj)
	{
		if (this.hasCtab())
			return this.getCtab().indexOfChild(obj);
		else
			return -1;
	}
	*/
	/**
	 * Returns the object that will be directly manipulated after inserting into editor.
	 * Descendants may override this methods.
	 * @returns {Kekule.ChemObject}
	 */
	getDirectManipulationTarget: function()
	{
		return null;
	}
});

})();