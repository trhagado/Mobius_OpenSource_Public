using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Services.Util.TypeConversionUtil
{
    internal class SourceTypeTargetTypePair
    {
        private static Dictionary<Type, Dictionary<Type, SourceTypeTargetTypePair>> knownTypePairs =
            new Dictionary<Type, Dictionary<Type, SourceTypeTargetTypePair>>();

        private Type _sourceType;
        internal Type SourceType
        {
            get
            {
                return _sourceType;
            }
        }
        private Type _targetType;
        internal Type TargetType
        {
            get
            {
                return _targetType;
            }
        }

        private SourceTypeTargetTypePair()
        {
        }

        internal static SourceTypeTargetTypePair GetTypePair(Type sourceType, Type targetType)
        {
            SourceTypeTargetTypePair typePair = null;

            Dictionary<Type, SourceTypeTargetTypePair> sourceTypeDict;
            if (knownTypePairs.ContainsKey(sourceType))
            {
                sourceTypeDict = knownTypePairs[sourceType];
            }
            else
            {
                sourceTypeDict = new Dictionary<Type, SourceTypeTargetTypePair>();
                knownTypePairs.Add(sourceType, sourceTypeDict);
            }

            if (sourceTypeDict.ContainsKey(targetType))
            {
                typePair = sourceTypeDict[targetType];
            }
            else
            {
                typePair = new SourceTypeTargetTypePair(sourceType, targetType);
                sourceTypeDict.Add(targetType, typePair);
            }

            return typePair;
        }

        private SourceTypeTargetTypePair(Type sourceType, Type targetType)
        {
            if (sourceType == null ||
                targetType == null)
            {
                throw new NullReferenceException("Neither source nor target type may be null.");
            }

            _sourceType = sourceType;
            _targetType = targetType;
        }
    }
}
