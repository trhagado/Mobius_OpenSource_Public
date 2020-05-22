/**
 * @fileoverview
 * A file to store string constants for widgets in English.
 * @author Partridge Jiang
 */

/** @ignore */
Kekule.LOCAL_RES = true;

Kekule.Localization.setCurrModule('widget');

/* @ignore */
//Kekule.WidgetTexts = {};
/** @ignore */
//Kekule.WidgetTexts.en = {
Kekule.Localization.addResource('en', 'WidgetTexts', {
	// Dialog
	CAPTION_OK: 'OK',
	CAPTION_CANCEL: 'Cancel',
	CAPTION_YES: 'Yes',
	CAPTION_NO: 'No',
	CAPTION_BROWSE_COLOR: 'Browse colors',

	HINT_BROWSE_COLOR: 'Browse more colors',

	S_COLOR_UNSET: '(unset)',
	S_COLOR_DEFAULT: '(default)',
	S_COLOR_MIXED: '(mixed)',
	S_COLOR_TRANSPARENT: '(transparent)',

	S_OBJECT_UNSET: '(none)',

	// property editors
	S_ITEMS: 'item(s)',
	S_OBJECT: 'object',
	S_VALUE_UNSET: '(Unset)',

	// General
	CAPTION_MENU: 'Menu',
	HINT_MENU: 'Open menu',

	// Object inspector
	S_INSPECT_NONE: '(none)',
	S_INSPECT_OBJECTS: '({0} objects)',
	S_INSPECT_ID_OBJECT: '{0}: {1}',
	S_INSPECT_ANONYMOUS_OBJECT: '({0})',

	CAPTION_TOGGLE_TEXTWRAP: 'Toggle text wrapping',
	CAPTION_INC_TEXT_SIZE: 'Increase text size',
	CAPTION_DEC_TEXT_SIZE: 'Decrease text size',
	HINT_TOGGLE_TEXTWRAP: 'Toggle the state of text wrapping',
	HINT_INC_TEXT_SIZE: 'Increase text size',
	HINT_DEC_TEXT_SIZE: 'Decrease text size',
	HINT_CHOOSE_FONT_FAMILY: 'Choose font family',

	// Page Navigator
	CAPTION_FIRST_PAGE: 'First',
	CAPTION_LAST_PAGE: 'Last',
	CAPTION_PREV_PAGE: 'Prev',
	CAPTION_NEXT_PAGE: 'Next',
	HINT_FIRST_PAGE: 'First page',
	HINT_LAST_PAGE: 'Last page',
	HINT_PREV_PAGE: 'Previous page',
	HINT_NEXT_PAGE: 'Next page',
	HINT_CURR_PAGE: 'Current page',

	// Data table
	MSG_RETRIEVING_DATA: 'Loading data...',
	CAPTION_DATATABLE_EDIT: 'Edit',
	CAPTION_DATATABLE_DELETE: 'Delete',
	CAPTION_DATATABLE_INSERT: 'Insert',
	HINT_DATATABLE_EDIT: 'Edit',
	HINT_DATATABLE_DELETE: 'Delete',
	HINT_DATATABLE_INSERT: 'Insert',

	// widget grid
	CAPTION_ADD_CELL: '+',
	HINT_ADD_CELL: 'Add new cell',
	CAPTION_REMOVE_CELL: 'Remove',
	HINT_REMOVE_CELL: 'Remove cell',

	// configurator
	CAPTION_CONFIG: 'Settings...',
	HINT_CONFIG: 'Change settings',

	// image type titles
	TITLE_IMG_FORMAT_PNG: 'PNG format image',
	TITLE_IMG_FORMAT_JPG: 'Jpeg format image',
	TITLE_IMG_FORMAT_GIF: 'GIF format image',
	TITLE_IMG_FORMAT_SVG: 'SVG format image'
});

/* @ignore */
//Kekule.ChemWidgetTexts = {};
/** @ignore */
// Predefined strings for chem widgets.
//Kekule.ChemWidgetTexts.en = {
Kekule.Localization.addResource('en', 'ChemWidgetTexts', {
	// ChemDisplayer / ChemViewer
	CAPTION_CLEAROBJS: 'Clear',
	CAPTION_LOADFILE: 'Load...',
	CAPTION_LOADDATA: 'Load...',
	CAPTION_SAVEFILE: 'Save...',
	CAPTION_ZOOMIN: 'Zoom In',
	CAPTION_ZOOMOUT: 'Zoom Out',
	CAPTION_RESETZOOM: 'Reset Zoom',
	CAPTION_RESETVIEW: 'Reset',
	CAPTION_ROTATE: 'Rotate',
	CAPTION_ROTATELEFT: 'Left Rotate',
	CAPTION_ROTATERIGHT: 'Right Rotate',
	CAPTION_ROTATEX: 'X Rotate',
	CAPTION_ROTATEY: 'Y Rotate',
	CAPTION_ROTATEZ: 'Z Rotate',
	CAPTION_MOL_DISPLAY_TYPE: 'Molecule Display Style',
	CAPTION_SKELETAL: 'Skeletal',
	CAPTION_CONDENSED: 'Condensed',
	CAPTION_WIRE: 'Wire Frame',
	CAPTION_STICKS: 'Sticks',
	CAPTION_BALLSTICK: 'Ball Stick',
	CAPTION_SPACEFILL: 'Space Fill',
	CAPTION_HIDEHYDROGENS: 'Show/hide hydrogens',
	CAPTION_OPENEDITOR: 'Edit...',
	CAPTION_EDITOR_DONE: 'Done',
	CAPTION_EDITOR_CANCEL: 'Cancel',

	CAPTION_EDIT_OBJ: 'Edit',

	HINT_CLEAROBJS: 'Clear objects',
	HINT_LOADFILE: 'Load from file',
	HINT_LOADDATA: 'Load data',
	HINT_SAVEFILE: 'Save to file',
	HINT_ZOOMIN: 'Zoom in',
	HINT_ZOOMOUT: 'Zoom out',
	HINT_RESETZOOM: 'Reset zoom',
	HINT_RESETVIEW: 'Reset zoom and rotation',
	HINT_ROTATE: 'Rotate',
	HINT_ROTATELEFT: 'Rotate in anticlockwise direction',
	HINT_ROTATERIGHT: 'Rotate in clockwise direction',
	HINT_ROTATEX: 'Rotate around X axis',
	HINT_ROTATEY: 'Rotate around Y axis',
	HINT_ROTATEZ: 'Rotate around Z axis',
	HINT_MOL_DISPLAY_TYPE: 'Change molecule display style',
	HINT_SKELETAL: 'Show molecule in skeletal style',
	HINT_CONDENSED: 'Show molecule in condensed style',
	HINT_WIRE: 'Show molecule in wire frame style',
	HINT_STICKS: 'Show molecule in sticks style',
	HINT_BALLSTICK: 'Show molecule in ball-stick style',
	HINT_SPACEFILL: 'Show molecule in space-fill style',
	HINT_HIDEHYDROGENS: 'Show/hide hydrogen atoms in model',
	HINT_OPENEDITOR: 'Open an editor to modify displayed object',
	HINT_EDITOR_DONE: 'Save the modification',
	HINT_EDITOR_CANCEL: 'Discard the modification',

	// chem editor
	CAPTION_NEWDOC: 'New',
	CAPTION_UNDO: 'Undo',
	CAPTION_REDO: 'Redo',
	CAPTION_COPY: 'Copy',
	CAPTION_CUT: 'Cut',
	CAPTION_PASTE: 'Paste',
	CAPTION_TOGGLE_SELECT: 'Toggle select',
	CAPTION_CLONE_SELECTION: 'Clone selection',
	CAPTION_TOGGLE_INSPECTOR: 'Object inspector',
	CAPTION_MANIPULATE: 'Select',
	CAPTION_MANIPULATE_MARQUEE: 'Marquee select',
	CAPTION_MANIPULATE_LASSO: 'Lasso select',
	CAPTION_MANIPULATE_BRUSH: 'Brush select',
	CAPTION_MANIPULATE_ANCESTOR: 'Select molecule',
	CAPTION_CLIENT_DRAGSCROLL: 'Scroll',
	CAPTION_ERASE: 'Erase',
	CAPTION_TRACK_INPUT: 'Track',
	CAPTION_MOL_BOND: 'Bond',
	CAPTION_MOL_BOND_SINGLE: 'Single bond',
	CAPTION_MOL_BOND_DOUBLE: 'Double bond',
	CAPTION_MOL_BOND_TRIPLE: 'Triple bond',
	CAPTION_MOL_BOND_QUAD: 'Quad bond',
	CAPTION_MOL_BOND_EXPLICIT_AROMATIC: 'Aromatic bond',
	CAPTION_MOL_BOND_WEDGEUP: 'Wedge up bond',
	CAPTION_MOL_BOND_WEDGEUP_INVERTED: 'Wedge up bond 2',
	CAPTION_MOL_BOND_WEDGEDOWN: 'Wedge down bond',
	CAPTION_MOL_BOND_WEDGEDOWN_INVERTED: 'Wedge down bond 2',
	CAPTION_MOL_BOND_CLOSER: 'Outer bond',
	CAPTION_MOL_BOND_WAVY: 'Wavy bond',
	CAPTION_MOL_BOND_DOUBLE_EITHER: 'Double Either bond',
	CAPTION_MOL_BOND_IONIC: 'Ionic bond',
	CAPTION_MOL_BOND_COORDINATE: 'Coordinate bond',
	CAPTION_MOL_BOND_METALLIC: 'Metallic bond',
	CAPTION_MOL_BOND_HYDROGEN: 'Hydrogen bond',

	CAPTION_MOL_BOND_KEKULIZE: 'Kekulize',
	CAPTION_MOL_BOND_HUCKLIZE: 'Hucklize',

	CAPTION_MOL_ATOM: 'Atom',
	CAPTION_MOL_FORMULA: 'Formula',
	CAPTION_MOL_ATOM_AND_FORMULA: 'Atom & formula',
	CAPTION_MOL_CHARGE: 'Charge',
	CAPTION_MOL_CHARGE_CLEAR: 'Charge clear',
	CAPTION_MOL_CHARGE_POSITIVE: 'Positive charge',
	CAPTION_MOL_CHARGE_NEGATIVE: 'Negative charge',
	CAPTION_MOL_CHARGE_SINGLET: 'Singlet radical',
	CAPTION_MOL_CHARGE_DOUBLET: 'Monoradical',
	CAPTION_MOL_CHARGE_TRIPLET: 'Triplet radical',
	CAPTION_MOL_ELECTRON_LONEPAIR: 'Lone pair',
	CAPTION_TEXT_BLOCK: 'Text block',
	CAPTION_IMAGE_BLOCK: 'Image block',
	CAPTION_TEXT_IMAGE: 'Text & image',

	CAPTION_MOL_FLEXCHAIN: 'Flex chain',
	CAPTION_MOL_FLEXRING: 'Flex ring',

	CAPTION_REPOSITORY_RING: 'Rings',
	CAPTION_REPOSITORY_RING_3: 'Cyclopropane',
	CAPTION_REPOSITORY_RING_4: 'Cyclobutane',
	CAPTION_REPOSITORY_RING_5: 'Cyclopentane',
	CAPTION_REPOSITORY_RING_6: 'Cyclohexane',
	CAPTION_REPOSITORY_RING_7: 'Cycloheptane',
	CAPTION_REPOSITORY_RING_8: 'Cyclooctane',
	CAPTION_REPOSITORY_RING_AR_6: 'Benzene',
	CAPTION_REPOSITORY_RING_AR_5: 'Cyclopentadiene',
	CAPTION_REPOSITORY_CYCLOHEXANE_CHAIR1: 'Cyclohexane chair 1',
	CAPTION_REPOSITORY_CYCLOHEXANE_CHAIR2: 'Cyclohexane chair 2',
	CAPTION_REPOSITORY_CYCLOHEXANE_HARWORTH1: 'Cyclohexane Haworth 1',
	CAPTION_REPOSITORY_CYCLOHEXANE_HARWORTH2: 'Cyclohexane Haworth 2',
	CAPTION_REPOSITORY_CYCLOPENTANE_HARWORTH1: 'Cyclopentane Haworth 1',
	CAPTION_REPOSITORY_CYCLOPENTANE_HARWORTH2: 'Cyclopentane Haworth 2',

	CAPTION_REPOSITORY_METHANE: 'Methane',
	CAPTION_REPOSITORY_FISCHER1: 'Fischer projection 1',
	CAPTION_REPOSITORY_FISCHER2: 'Fischer projection 2',
	CAPTION_REPOSITORY_FISCHER3: 'Fischer projection 3',
	CAPTION_REPOSITORY_SAWHORSE_STAGGERED: 'Sawhorse staggered',
	CAPTION_REPOSITORY_SAWHORSE_ECLIPSED: 'Sawhorse eclipsed',

	CAPTION_REPOSITORY_SUBBOND_MARK: 'Substituent bond',

	CAPTION_REPOSITORY_ARROWLINE: 'Arrows & symbols',
	CAPTION_REPOSITORY_GLYPH: 'Glyphs',
	CAPTION_REPOSITORY_GLYPH_LINE: 'Line',
	CAPTION_REPOSITORY_GLYPH_OPEN_ARROW_LINE: 'Open arrow line',
	CAPTION_REPOSITORY_GLYPH_TRIANGLE_ARROW_LINE: 'Triangle arrow line',
	CAPTION_REPOSITORY_GLYPH_DI_OPEN_ARROW_LINE: 'Bidirectional open arrow line',
	CAPTION_REPOSITORY_GLYPH_DI_TRIANGLE_ARROW_LINE: 'Bidirectional triangle arrow line',
	CAPTION_REPOSITORY_GLYPH_REV_ARROW_LINE: 'Reversible arrow line',
	CAPTION_REPOSITORY_GLYPH_OPEN_ARROW_DILINE: 'Open arrow double line',
	CAPTION_REPOSITORY_GLYPH_OPEN_ARROW_ARC: 'Open arrow arc',
	CAPTION_REPOSITORY_GLYPH_SINGLE_SIDE_OPEN_ARROW_ARC: 'Single side open arrow arc',
	CAPTION_ELECTRON_PUSHING_ARROW: 'Electron pushing arrow',
	CAPTION_ELECTRON_PUSHING_ARROW_DOUBLE: 'Double electron pushing arrow',
	CAPTION_ELECTRON_PUSHING_ARROW_SINGLE: 'Single electron pushing arrow',
	/*
	CAPTION_REACTION_ARROW_NORMAL: 'Normal reaction arrow',
	CAPTION_REACTION_ARROW_REVERSIBLE: 'Reversible reaction arrow',
	CAPTION_REACTION_ARROW_RESONANCE: 'Resonance reaction arrow',
	CAPTION_REACTION_ARROW_RETROSYNTHESIS: 'Retrosynthesis arrow',
  */

	CAPTION_REPOSITORY_HEAT_SYMBOL: 'Heat symbol',
	CAPTION_REPOSITORY_ADD_SYMBOL: 'Add symbol',

	// modifiers
	CAPTION_TEXT_FORMAT: 'Text format',
	CAPTION_PICK_COLOR: 'Color',
	CAPTION_FONTNAME: 'Font name',
	CAPTION_FONTSIZE: 'Font size',
	CAPTION_TEXT_DIRECTION: 'Text direction',
	CAPTION_TEXT_DIRECTION_DEFAULT: 'Default',
	CAPTION_TEXT_DIRECTION_LTR: 'Left to right',
	CAPTION_TEXT_DIRECTION_RTL: 'Right to left',
	CAPTION_TEXT_DIRECTION_TTB: 'Top to bottom',
	CAPTION_TEXT_DIRECTION_BTT: 'Bottom to top',

	CAPTION_TEXT_HORIZONTAL_ALIGN: 'Text horizontal alignment',
	CAPTION_TEXT_VERTICAL_ALIGN: 'Text vertical alignment',
	CAPTION_TEXT_ALIGN_DEFAULT: 'Default',
	CAPTION_TEXT_ALIGN_LEADING: 'Leading',
	CAPTION_TEXT_ALIGN_TRAILING: 'Trailing',
	CAPTION_TEXT_ALIGN_CENTER: 'Center',
	CAPTION_TEXT_ALIGN_LEFT: 'Left',
	CAPTION_TEXT_ALIGN_RIGHT: 'Right',
	CAPTION_TEXT_ALIGN_TOP: 'Top',
	CAPTION_TEXT_ALIGN_BOTTOM: 'Bottom',

	CAPTION_NODE_LABEL_DISPLAY_MODE: 'Atom label display mode',
	CAPTION_NODE_LABEL_DISPLAY_MODE_DEFAULT: 'Default',
	CAPTION_NODE_LABEL_DISPLAY_MODE_SHOWN: 'Show label',
	CAPTION_NODE_LABEL_DISPLAY_MODE_HIDDEN: 'Hide label',
	CAPTION_NODE_LABEL_DISPLAY_MODE_SMART: 'Smart',

	CAPTION_ATOM_MODIFIER: 'Atom',
	CAPTION_ATOM_MODIFIER_MIXED: '[A]',
	CAPTION_BOND_MODIFIER: 'Bond',
	CAPTION_CHARGE_MODIFIER: 'Charge',
	CAPTION_GLYPH_PATH_MODIFIER: 'Path',
	CAPTION_REACTION_ARROW_AND_SEGMENT_PATH_MODIFIER: 'Reaction arrow & segment',
	CAPTION_ARC_PATH_MODIFIER: 'Arc',
	CAPTION_MULTI_ARC_PATH_MODIFIER: 'Multi-Arc',
	CAPTION_ELECTRON_PUSHING_ARROW_MODIFIER: 'Electron pushing arrow',
	CAPTION_BOND_FORMING_ELECTRON_PUSHING_ARROW_MODIFIER: 'Bond forming arrow',

	TEXT_CHARGE_POSITIVE: '+',
	TEXT_CHARGE_NEGATIVE: '-',
	TEXT_CHARGE_UNKNOWN: '<sup>+</sup>/<sub>-</sub>', //'&#x207a;&#x2044;&#x208b;',  // '+/-',
	TEXT_CHARGE_NONE: '&#8709;',

	HINT_NEWDOC: 'Create new document',
	HINT_UNDO: 'Undo',
	HINT_REDO: 'Redo',
	HINT_COPY: 'Copy selection to internal clipboard',
	HINT_CUT: 'Cut selection to internal clipboard',
	HINT_PASTE: 'Paste from internal clipboard',
	HINT_CLONE_SELECTION: 'Clone selection',
	HINT_TOGGLE_SELECT: 'Toggle select',
	HINT_TOGGLE_INSPECTOR: 'Show or hide object inspector panel',
	HINT_MANIPULATE: 'Select tool',
	HINT_MANIPULATE_MARQUEE: 'Marquee select',
	HINT_MANIPULATE_LASSO: 'Lasso select',
	HINT_MANIPULATE_BRUSH: 'Brush select',
	HINT_MANIPULATE_ANCESTOR: 'Select molecule',
	HINT_CLIENT_DRAGSCROLL: 'Scroll',
	HINT_ERASE: 'Erase tool',
	HINT_TRACK_INPUT: 'Track tool',
	HINT_MOL_BOND: 'Bond tool',
	HINT_MOL_BOND_SINGLE: 'Single bond',
	HINT_MOL_BOND_DOUBLE: 'Double bond',
	HINT_MOL_BOND_TRIPLE: 'Triple bond',
	HINT_MOL_BOND_QUAD: 'Quad bond',
	HINT_MOL_BOND_EXPLICIT_AROMATIC: 'Explicit aromatic bond',
	HINT_MOL_BOND_WEDGEUP: 'Wedge up bond',
	HINT_MOL_BOND_WEDGEUP_INVERTED: 'Wedge up bond 2',
	HINT_MOL_BOND_WEDGEDOWN: 'Wedge down bond',
	HINT_MOL_BOND_WEDGEDOWN_INVERTED: 'Wedge down bond 2',
	HINT_MOL_BOND_CLOSER: 'Outer bond',
	HINT_MOL_BOND_WAVY: 'Wavy bond',
	HINT_MOL_BOND_DOUBLE_EITHER: 'Double either bond',
	HINT_MOL_BOND_IONIC: 'Ionic bond',
	HINT_MOL_BOND_COORDINATE: 'Coordinate bond',
	HINT_MOL_BOND_METALLIC: 'Metallic bond',
	HINT_MOL_BOND_HYDROGEN: 'Hydrogen bond',

	HINT_MOL_BOND_KEKULIZE: 'Kekulize, convert explicit aromatic bonds to compartmental single and double ones',
	HINT_MOL_BOND_HUCKLIZE: 'Hucklize, mark bonds on aromatic rings as explicit aromatic ones',

	HINT_MOL_ATOM: 'Atom tool',
	HINT_MOL_FORMULA: 'Formula tool',
	HINT_MOL_ATOM_AND_FORMULA: 'Atom and formula tool',
	HINT_MOL_CHARGE: 'Charge tool',
	HINT_MOL_CHARGE_CLEAR: 'Clear charge and radical',
	HINT_MOL_CHARGE_POSITIVE: 'Positive charge',
	HINT_MOL_CHARGE_NEGATIVE: 'Negative charge',
	HINT_MOL_CHARGE_SINGLET: 'Singlet radical',
	HINT_MOL_CHARGE_DOUBLET: 'Monoradical',
	HINT_MOL_CHARGE_TRIPLET: 'Triplet radical',
	HINT_MOL_ELECTRON_LONEPAIR: 'Lone pair electrons',
	HINT_TEXT_BLOCK: 'Text block tool',
	HINT_IMAGE_BLOCK: 'Image block tool',
	HINT_TEXT_IMAGE: 'Text and image tool',

	HINT_MOL_FLEXCHAIN: 'Flex carbon chain',
	HINT_MOL_FLEXRING: 'Flex carbon ring',

	HINT_REPOSITORY_RING: 'Ring structures tool',
	HINT_REPOSITORY_RING_3: 'Cyclopropane',
	HINT_REPOSITORY_RING_4: 'Cyclobutane',
	HINT_REPOSITORY_RING_5: 'Cyclopentane',
	HINT_REPOSITORY_RING_6: 'Cyclohexane',
	HINT_REPOSITORY_RING_7: 'Cycloheptane',
	HINT_REPOSITORY_RING_8: 'Cyclooctane',
	HINT_REPOSITORY_RING_AR_6: 'Benzene',
	HINT_REPOSITORY_RING_AR_5: 'Cyclopentadiene',
	HINT_REPOSITORY_CYCLOHEXANE_CHAIR1: 'Cyclohexane chair 1',
	HINT_REPOSITORY_CYCLOHEXANE_CHAIR2: 'Cyclohexane chair 2',
	HINT_REPOSITORY_CYCLOHEXANE_HARWORTH1: 'Cyclohexane Haworth 1',
	HINT_REPOSITORY_CYCLOHEXANE_HARWORTH2: 'Cyclohexane Haworth 2',
	HINT_REPOSITORY_CYCLOPENTANE_HARWORTH1: 'Cyclopentane Haworth 1',
	HINT_REPOSITORY_CYCLOPENTANE_HARWORTH2: 'Cyclopentane Haworth 2',

	HINT_REPOSITORY_SUBBOND_MARK: 'Substituent bond',

	HINT_REPOSITORY_METHANE: 'Methane',
	HINT_REPOSITORY_FISCHER1: 'Fischer projection with one chiral center',
	HINT_REPOSITORY_FISCHER2: 'Fischer projection with two chiral centers',
	HINT_REPOSITORY_FISCHER3: 'Fischer projection with three chiral centers',
	HINT_REPOSITORY_SAWHORSE_STAGGERED: 'Sawhorse staggered',
	HINT_REPOSITORY_SAWHORSE_ECLIPSED: 'Sawhorse eclipsed',

	HINT_REPOSITORY_ARROWLINE: 'Arrows and symbols tool',
	HINT_REPOSITORY_GLYPH: 'Glyphs',
	HINT_REPOSITORY_GLYPH_LINE: 'Line',
	HINT_REPOSITORY_GLYPH_OPEN_ARROW_LINE: 'Open arrow line',
	HINT_REPOSITORY_GLYPH_TRIANGLE_ARROW_LINE: 'Triangle arrow line',
	HINT_REPOSITORY_GLYPH_DI_OPEN_ARROW_LINE: 'Bidirectional open arrow line',
	HINT_REPOSITORY_GLYPH_DI_TRIANGLE_ARROW_LINE: 'Bidirectional triangle arrow line',
	HINT_REPOSITORY_GLYPH_REV_ARROW_LINE: 'Reversible arrow line',
	HINT_REPOSITORY_GLYPH_OPEN_ARROW_DILINE: 'Open arrow double line',
	HINT_REPOSITORY_GLYPH_OPEN_ARROW_ARC: 'Open arrow arc',
	HINT_REPOSITORY_GLYPH_SINGLE_SIDE_OPEN_ARROW_ARC: 'Single side open arrow arc',
	HINT_REPOSITORY_HEAT_SYMBOL: 'Heat symbol',
	HINT_REPOSITORY_ADD_SYMBOL: 'Add symbol',
	HINT_ELECTRON_PUSHING_ARROW: 'Electron pushing arrow',
	/*
	HINT_REACTION_ARROW_NORMAL: 'Normal reaction arrow',
	HINT_REACTION_ARROW_REVERSIBLE: 'Reversible reaction arrow',
	HINT_REACTION_ARROW_RESONANCE: 'Resonance reaction arrow',
	HINT_REACTION_ARROW_RETROSYNTHESIS: 'Retrosynthesis arrow',
	*/

	// modifiers
	HINT_NODE_LABEL_DISPLAY_MODE: 'Atom label display mode',

	HINT_TEXT_FORMAT: 'Set text format',
	HINT_FONTNAME: 'Set font name',
	HINT_FONTSIZE: 'Set font size',
	HINT_PICK_COLOR: 'Select color',
	HINT_TEXT_DIRECTION: 'Set text direction',
	HINT_TEXT_HORIZONTAL_ALIGN: 'Set text horizontal alignment',
	HINT_TEXT_VERTICAL_ALIGN: 'Set text vertical alignment',

	HINT_CHARGE_NONE: 'No charge',

	HINT_ATOM_MODIFIER: 'Set atom',
	HINT_BOND_MODIFIER: 'Set bond',
	HINT_CHARGE_MODIFIER: 'Set charge',
	HINT_GLYPH_PATH_MODIFIER: 'Set path properties',
	HINT_REACTION_ARROW_AND_SEGMENT_PATH_MODIFIER: 'Set reaction arrow and segment properties',
	HINT_ARC_PATH_MODIFIER: 'Set arc properties',
	HINT_MULTI_ARC_PATH_MODIFIER: 'Set multi-arc properties',
	HINT_ELECTRON_PUSHING_ARROW_MODIFIER: 'Set electron pushing arrow properties',
	HINT_BOND_FORMING_ELECTRON_PUSHING_ARROW_MODIFIER: 'Set bond forming arrow properties',

	HINT_USE_ATOM_CUSTOM_COLOR: '(use atom custom color)',

	// load / save data dialog
	CAPTION_LOADDATA_DIALOG: 'Load data',
	CAPTION_LOADDATA_DIALOG_APPENDMODE: 'Append to current',
	CAPTION_SAVEDATA_DIALOG: 'Save data',
	CAPTION_DATA_FORMAT: 'Data format:',
	CAPTION_DATA_SRC: 'Input or paste data below:',
	CAPTION_LOADDATA_FROM_FILE: 'Load from file',

	// Choose file format dialog
	CAPTION_CHOOSEFILEFORMAT: 'Choose file format',
	CAPTION_SELECT_FORMAT: 'Select format:',
	CAPTION_PREVIEW_FILE_CONTENT: 'Preview: ',

	// file name
	S_DEF_SAVE_FILENAME: 'Unnamed',

	// Atom edit list
	CAPTION_ATOMLIST_PERIODIC_TABLE: 'more...',
	CAPTION_RGROUP: 'Sub group',
	CAPTION_VARIABLE_ATOM: 'Variable Atom (list)',
	CAPTION_VARIABLE_NOT_ATOM: 'Variable Atom (not list)',
	CAPTION_PSEUDOATOM: 'Pseudoatom',
	CAPTION_DUMMY_ATOM: 'Dummy Atom',
	CAPTION_HETERO_ATOM: 'Hetero Atom',
	CAPTION_ANY_ATOM: 'Any Atom',

	// Structure node selector
	CAPTION_ATOM: 'Atom',
	CAPTION_SUBGROUP: 'Subgroup',

	// Periodic table dialog in editor
	CAPTION_PERIODIC_TABLE_DIALOG: 'Periodic table',
	CAPTION_PERIODIC_TABLE_DIALOG_SEL_ELEM: 'Select element',
	CAPTION_PERIODIC_TABLE_DIALOG_SEL_ELEMS: 'Select elements',

	// Text block editor
	CAPTION_TEXTBLOCK_INIT: 'Enter text here',

	// Glyph arrow param setter panel
	CAPTION_WIDTH: 'Width',
	CAPTION_LENGTH: 'Length',
	// Glyph path param setter panel
	CAPTION_PATH_INDEX: 'Path {0}',
	HINT_PATH_INDEX: 'Path {0}',
	// Reaction arrow preset selector
	CAPTION_REACTION_ARROW_UNSET: '(Unset)',
	CAPTION_REACTION_ARROW_CUSTOM: 'Custom',
	CAPTION_REACTION_ARROW_NORMAL: 'Normal',
	CAPTION_REACTION_ARROW_RESONANCE: 'Resonance',
	CAPTION_REACTION_ARROW_REVERSIBLE: 'Reversible',
	CAPTION_REACTION_ARROW_RETROSYNTHESIS: 'Retrosynthesis',
	HINT_REACTION_ARROW_UNSET: 'Unset',
	HINT_REACTION_ARROW_CUSTOM: 'Custom',
	HINT_REACTION_ARROW_NORMAL: 'Normal arrow',
	HINT_REACTION_ARROW_RESONANCE: 'Resonance arrow',
	HINT_REACTION_ARROW_REVERSIBLE: 'Reversible arrow',
	HINT_REACTION_ARROW_RETROSYNTHESIS: 'Retrosynthesis arrow',
	// Glyph line param setter panel
	CAPTION_LINE_COUNT: 'Line count',
	CAPTION_LINE_GAP: 'Line gap',
	// Path glyph param setter panel
	CAPTION_LINE: 'Line',
	CAPTION_STARTING_ARROW: 'Starting arrow',
	CAPTION_ENDING_ARROW: 'Ending arrow',
	CAPTION_REACTION_ARROW_TYPE: 'Reaction arrow type',
	CAPTION_ELECTRON_PUSHING_ARROW_TYPE: 'Arrow type',
	//CAPTION_MULTI_ARROW_SIDE_RELATION: 'Arrow sides relation',
	CAPTION_OPPOSITE_ARROW_SIDES: 'Opposite arrow sides',
	CAPTION_SAME_ARROW_SIDES: 'Same arrow sides',
	// electron pushing arrow panel
	CAPTION_ELECTRON_PUSHING_ARROW_1: 'Single electron pushing arrow',
	CAPTION_ELECTRON_PUSHING_ARROW_1_ABBR: 'Single',
	HINT_ELECTRON_PUSHING_ARROW_1: 'Single electron pushing arrow',
	CAPTION_ELECTRON_PUSHING_ARROW_2: 'Double electron pushing arrow',
	CAPTION_ELECTRON_PUSHING_ARROW_2_ABBR: 'Double',
	HINT_ELECTRON_PUSHING_ARROW_2: 'Double electron pushing arrow',
	CAPTION_BOND_FORMING_ELECTRON_PUSHING_ARROW_1: 'Bond forming electron pushing arrow',
	HINT_BOND_FORMING_ELECTRON_PUSHING_ARROW_1: 'Bond forming electron pushing arrow',

	// Periodic table
	LEGEND_CAPTION: 'Legend',
	LEGEND_ELEM_SYMBOL: 'Symbol',
	LEGEND_ELEM_NAME: 'name',
	LEGEND_ATOMIC_NUM: 'atomic number',
	LEGEND_ATOMIC_WEIGHT: 'atomic weight',

	// ChemObjInserter
	CAPTION_2D: '2D',
	CAPTION_3D: '3D',
	CAPTION_AUTOSIZE: 'Autosize',
	CAPTION_AUTOFIT: 'Autofit',
	CAPTION_SHOWSIZEINFO: 'Show size info',
	CAPTION_LABEL_SIZE: 'Size: ',
	CAPTION_BACKGROUND_COLOR: 'Background color: ',
	CAPTION_WIDTH_HEIGHT: 'width: {0}, height: {1}',
	PLACEHOLDER_WIDTH: 'width',
	PLACEHOLDER_HEIGHT: 'height',
	HINT_AUTOSIZE: 'Whether graph size is determined by object automatically',
	HINT_AUTOFIT: 'Whether object is zoomed to fullfill the whole graph',


	//HINT_SHOWSIZEINFO: 'Whether show width and height of context',

	// misc
	S_VALUE_DEFAULT: '(Default)'
});

// error messages
//Object.extend(Kekule.ErrorMsg.en, {
Kekule.Localization.addResource('en', 'ErrorMsg', {
	WIDGET_CLASS_NOT_FOUND: 'Widget class not found',
	WIDGET_CAN_NOT_BIND_TO_ELEM: 'Widget {0} can not be binded to element <{1}>',
	LOAD_CHEMDATA_FAILED: 'Failed to load data',
	FILE_API_NOT_SUPPORTED: 'File operations are not supported by your current web browser. Please update it.',
	DRAW_BRIDGE_NOT_SUPPORTED: 'It seems that your web browser is not modern enough to support the drawing function. Please update it.',

	// widget/operations/kekule.operations.js
	COMMAND_NOT_REVERSIBLE: 'Command can not be undone',

	// page navigator
	PAGE_INDEX_OUTOF_RANGE: 'Page index out of range',

	// DataSet
	FETCH_DATA_TIMEOUT: 'Time out to fetch data',

	// displayer
	RENDER_TYPE_CHANGE_NOT_ALLOWED: 'It is not allowed to change render type',

	// viewer
	CAN_NOT_CREATE_EDITOR: 'Creating editor failed',

	// editor operations
	CAN_NOT_SET_COORD_OF_CLASS: 'Can not set coordinate of instance of {0}.',
	CAN_NOT_SET_DIMENSION_OF_CLASS: 'Can not set dimension of instance of {0}.',
	/*
	CAN_NOT_MERGE_CONNECTOR_WITH_DIFF_NODECOUNT: 'Can not merge connectors with different number of connected objects.',
	CAN_NOT_MERGE_CONNECTOR_WITH_NODECOUNT_NOT_TWO: 'Can not merge connectors connected with more than 2 objects.',
	CAN_NOT_MERGE_CONNECTOR_CONNECTED_WITH_CONNECTOR: 'Can not merge a connector connected with another connector.'
	*/
	CAN_NOT_MERGE_CONNECTORS: 'Specified connectors can not be merged.',

	NOT_A_VALID_ATOM: 'Not a valid atom',
	INVALID_ATOM_SYMBOL: 'Invalid atom symbol',
	INVALID_OR_EMPTY_IMAGE: 'Image is empty or invalidate'

	//WIDGET_UNAVAILABLE_FOR_PLACEHOLDER: 'Widget of this placeholder is unavailable'
});
