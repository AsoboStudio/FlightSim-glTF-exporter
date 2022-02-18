using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Utilities;

namespace Max2Babylon
{
    public static class CameraUtilities
    {
        public static ICameraObject GetGenCameraFromNode(this IINode iNode, ILoggingProvider logger)
        {
            ICameraObject result = null;
            IObject obj = iNode.EvalWorldState(Loader.Core.Time, false).Obj;
            try
            {
                result = (ICameraObject)obj;
            }
            catch (Exception)
            {
                logger.RaiseWarning($"Camera type format of node {iNode.Name} is not supported");
            }
            return result;
        }
    }
}
