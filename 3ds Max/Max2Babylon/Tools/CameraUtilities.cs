using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;

namespace Max2Babylon
{
    public static class CameraUtilities
    {
        public static ICameraObject GetGenCameraFromNode(this IINode iNode)
        {
            IObject obj = iNode.EvalWorldState(Loader.Core.Time, false).Obj;
            if (obj.CanConvertToType(Loader.GenCamera) == 1)
            {
                return (ICameraObject)obj;
            }
            return null;
        }
    }
}
