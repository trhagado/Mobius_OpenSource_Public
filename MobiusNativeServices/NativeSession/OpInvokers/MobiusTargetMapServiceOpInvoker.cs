using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Util.TypeConversionUtil;
using Mobius.Services.Types;
using Mobius.Services.Native.ServiceOpCodes;


namespace Mobius.Services.Native.OpInvokers
{
    public class MobiusTargetMapServiceOpInvoker : IInvokeServiceOps
    {
        private static TypeConversionHelper _transHelper = new TypeConversionHelper(TypeConversionHelper.ContextRetentionMode.NoRetention);

        #region IInvokeServiceOps Members

				object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
				{
					MobiusTargetMapService op = (MobiusTargetMapService)opCode;
					switch (op)
					{
						case MobiusTargetMapService.GetKeggPathway:
							{
								string pathwayId = (string)args[0];
								Data.TargetMap mobiusTargetMap = UAL.TargetMapDao.GetKeggPathway(pathwayId);
								Types.TargetMap targetMap = _transHelper.Convert<Data.TargetMap, Types.TargetMap>(mobiusTargetMap);
								return targetMap;
							}
						case MobiusTargetMapService.GetCommonMapNames:
							{
								List<string> mapNames = UAL.TargetMapDao.GetCommonMapNames();
								return mapNames;
							}
						case MobiusTargetMapService.GetTargetNamesAndLabels:
							{
								Dictionary<string, string> targetNamesAndLabels = UAL.TargetMapDao.GetTargetNamesAndLabels();
								return targetNamesAndLabels;
							}
						case MobiusTargetMapService.GetMap:
							{
								string mapName = (string)args[0];
								Data.TargetMap mobiusMap = UAL.TargetMapDao.GetMap(mapName);
								Types.TargetMap map = _transHelper.Convert<Data.TargetMap, Types.TargetMap>(mobiusMap);
								return map;
							}
						case MobiusTargetMapService.GetMapWithCoords:
							{
								string mapName = (string)args[0];
								Data.TargetMap mobiusMap = UAL.TargetMapDao.GetMapWithCoords(mapName);
								Types.TargetMap map = _transHelper.Convert<Data.TargetMap, Types.TargetMap>(mobiusMap);
								return map;
							}
						case MobiusTargetMapService.GetMapImage:
							{
								string mapName = (string)args[0];
								byte[] mapImageBytes = UAL.TargetMapDao.GetMapImage(mapName);
								return mapImageBytes;
							}
					}
					return null;
				}

				#endregion
    }
}
