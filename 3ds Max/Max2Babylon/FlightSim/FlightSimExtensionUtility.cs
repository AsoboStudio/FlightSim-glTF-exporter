using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;

namespace Max2Babylon.FlightSim
{
    static class FlightSimExtensionUtility
    {
        public static float GetGizmoParameter(IINode node, string gizmoClass, string paramName)
        {
            string mxs = $"(maxOps.getNodeByHandle {node.Handle}).{gizmoClass}.{paramName}";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            var r=  mxsRetVal.F;
            return r;
        }

        public static float[] GetTranslation(IINode node,IINode renderedNode)
        {
            float[] res = new float[3];
            string mxs = $"((maxOps.getNodeByHandle {node.Handle}).center * inverse (maxOps.getNodeByHandle {renderedNode.Handle}).transform) as string";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            if (!string.IsNullOrEmpty(mxsRetVal.S))
            {
                float[] r = PointStringToVector3(mxsRetVal.S);

                //var o = new BabylonVector3(r[0],r[1],r[2]);
                //float f = (float)-Math.Sqrt(2) / 2;
                //var q = new BabylonQuaternion(0,f,f,0);

                //var m = q.Rotate(o);

                res[0] = r[0];
                res[1] = r[2];
                res[2] = -r[1];
            }
            
            return res;
        }

        public static float[] GetRotation(IINode node,IINode renderedNode)
        {
            float[] res = new float[4];
            string mxs = $"((maxOps.getNodeByHandle {node.Handle}).rotation * inverse (maxOps.getNodeByHandle {renderedNode.Handle}).transform) as string";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);

            if (!string.IsNullOrEmpty(mxsRetVal.S))
            {
                float[] r = QuaternionStringToVector4(mxsRetVal.S);

                //var v = new BabylonVector3(0,0,1);
                //var o = new BabylonQuaternion(r[0],r[1],r[2],r[3]);
                //float f = (float)-Math.Sqrt(2) / 2;
                //var q = new BabylonQuaternion(0,f,f,0);

                //var m = q.MultiplyWith(o);
                //var p = m.Rotate(v);

                //babylon to GLTF
                res[0] = r[0];
                res[1] = r[2];
                res[2] = -r[1];
                res[3] = -r[3];
            }

            return res;
        }

        public static bool IsDefaultRotation(float[] rotation)
        {
            if (rotation[0] == 0 && rotation[1] == 0 && rotation[2] == 0 && rotation[3] == 1) return true;
            return false;
        }

        public static float[] PointStringToVector3(string pointString)
        {
            string[] mxsRes = pointString.Substring(1, pointString.Length - 2).Split(',');
            float[] result = new float[mxsRes.Length];
            for (int i = 0; i < mxsRes.Length; i++)
            {
                float r = 0;
                float.TryParse(mxsRes[i], out r);
                result[i] = r;
            }

            return result;
        }

        public static float[] QuaternionStringToVector4(string quaternionString)
        {
            quaternionString = quaternionString.Replace("quat ", "");
            quaternionString = quaternionString.Substring(1, quaternionString.Length - 2);
            string[] mxsRes = quaternionString.Split(' ');
            float[] result = new float[mxsRes.Length];
            for (int i = 0; i < mxsRes.Length; i++)
            {
                float r = 0;
                float.TryParse(mxsRes[i], out r);
                result[i] = r;
            }

            return result;
        }
    }
}
