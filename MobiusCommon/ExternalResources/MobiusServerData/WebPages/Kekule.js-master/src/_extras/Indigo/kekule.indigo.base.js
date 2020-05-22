/**
 * Created by ginger on 2017/5/9.
 */

/*
 * requires /utils/kekule.utils.js
 * requires /core/kekule.common.js
 * requires /core/kekule.structures.js
 * requires /_extras/kekule.emscriptenUtils.js
 * requires /localization/
 */

(function($root){
"use strict";

/** @ignore */
var EU = Kekule.EmscriptenUtils;

/**
 * Initialization options of OpenBabel js module.
 * @private
 * @ignore
 */
var indigoInitOptions = {
	usingModulaize: true,  // whether using modularize option to build Indigo.js
	moduleName: 'IndigoModule', // the name of OpenBabl module
	moduleInitEventName: 'Indigo.Initialized',
	moduleInitCallbackName: '__$indigoInitialized$__',
	indigoAdaptFuncName: 'CreateIndigo'
};

/**
 * Namespace of Indigo related objects.
 * @namespace
 */
Kekule.Indigo = {
	/**
	 * A flag, whether auto enable InChI function when find InChI lib is already loaded.
	 */
	_autoEnabled: true,
	/** @private */
	_module: null, // a variable to store created OpenBabel module object
	/** @private */
	_indigo: null,
	/** @private */
	SCRIPT_FILE: 'indigo.js',
	/** @private */
	HELPER_SCRIPT_FILE: 'indigoAdapter.js',
	/** @private */
	_enableFuncs: [],
	isScriptLoaded: function()
	{
		return EU.isSupported(indigoInitOptions.moduleName)
				&& (typeof($root[indigoInitOptions.indigoAdaptFuncName]) !== 'undefined');
	},
	getModule: function()
	{
		if (!KI._module)
		{
			KI._module = EU.getRootModule(indigoInitOptions.moduleName);
		}
		return KI._module;
	},
	/**
	 * Returns Indigo adapter instance.
	 */
	getIndigo: function()
	{
		if (!KI._indigo)
		{
			var module = KI.getModule();
			if (module)
				KI._indigo = ($root[indigoInitOptions.indigoAdaptFuncName])(module);
		}
		return KI._indigo;
	},
	getClassCtor: function(className)
	{
		return EU.getClassCtor(className, KI.getModule());
	},
	/**
	 * Check if OpenBabel js file is successful loaded and available.
	 * @returns {Bool}
	 */
	isAvailable: function()
	{
		return KI.getModule() && KI.getIndigo();
	},

	/**
	 * Load Indigo.js lib and enable all related functions
	 */
	enable: function(callback)
	{
		if (!KI.isScriptLoaded())  // Indigo not loaded?
		{
			KI.loadIndigoScript(Kekule.$jsRoot.document, function(error){
				//Kekule.IO.registerAllInChIFormats();
				if (!error)
					KI._enableAllFunctions();
				if (callback)
					callback(error);
			});
		}
		else
		{
			KI._enableAllFunctions();
			if (callback)
				callback();
		}
	},
	_enableAllFunctions: function()
	{
		//if (KI.isScriptLoaded())
		if (EU.isModuleReady(indigoInitOptions.moduleName))
		{
			var funcs = KI._enableFuncs;
			for (var i = 0, l = funcs.length; i < l; ++i)
			{
				var func = funcs[i];
				if (func)
					func();
			}
		}
	}
};

/** @ignore */
Kekule.Indigo.getIndigoPath = function()
{
	var isMin = Kekule.scriptSrcInfo.useMinFile;
	var path = isMin? 'extra/': '_extras/Indigo/';
	path = Kekule.scriptSrcInfo.path + path;
	return path;
};
/** @ignore */
Kekule.Indigo.getIndigoScriptUrl = function()
{
	var result = KI.getIndigoPath() + KI.SCRIPT_FILE;
	var isMin = Kekule.scriptSrcInfo.useMinFile;
	if (!isMin)
		result += '.dev';
	return result;
};
Kekule.Indigo.getIndigoHelperScriptUrl = function()
{
	var result = KI.getIndigoPath() + KI.HELPER_SCRIPT_FILE;
	return result;
};
/** @ignore */
Kekule.Indigo.loadIndigoScript = function(doc, callback)
{
	if (!doc)
		doc = Kekule.$jsRoot.document;
	var done = function(error)
	{
		KI._scriptLoadedBySelf = !error;
		if (!error)
			Kekule.Indigo.getIndigo();
		if (callback)
			callback(error);
	};

	if (!KI._scriptLoadedBySelf && !KI.isScriptLoaded())
	{
		var filePath = KI.getIndigoScriptUrl();

		EU.loadScript(filePath,
			function(error){
				if (!error)
					Kekule.ScriptFileUtils.appendScriptFiles(doc, [KI.getIndigoHelperScriptUrl()], done);
				else
					done(error);
			},
			doc, indigoInitOptions);
	}
	else
	{
		done();
	}

	/*
	if (!KI._scriptLoadedBySelf && !KI.isScriptLoaded())
	{
		//console.log('load');
		//var urls = [KI.getIndigoScriptUrl(), KI.getIndigoHelperScriptUrl()];
		//Kekule.ScriptFileUtils.appendScriptFiles(doc, urls, done);
		EU.loadScript(KI.getIndigoScriptUrl(), function(){
			// when finish initialize indigo.js, load the adapter
			Kekule.ScriptFileUtils.appendScriptFiles(doc, [KI.getIndigoHelperScriptUrl()], done);
		}, doc, indigoInitOptions);
		KI._scriptLoadedBySelf = true;
	}
	else
	{
		KI._scriptLoadedBySelf = true;
		if (callback)
			callback();
	}
	*/
};

var KI = Kekule.Indigo;

/**
 * Util class to convert object between Indigo and Kekule.
 * Unfinished
 * @class
 * @ignore
 */
Kekule.Indigo.AdaptUtils = {
};

Kekule._registerAfterLoadSysProc(function() {
	if (KI._autoEnabled && KI.isScriptLoaded())
	{
		EU.ensureModuleReady(Kekule.$jsRoot.document, indigoInitOptions, KI._enableAllFunctions);
	}
});

})(this);
