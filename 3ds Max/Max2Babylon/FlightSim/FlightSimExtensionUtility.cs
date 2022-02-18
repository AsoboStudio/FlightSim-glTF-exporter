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
        public static float GetGizmoParameterFloat(IINode node, string gizmoClass, string paramName, bool IsSubClass = true)
        {
            string mxs = String.Empty;
            if (!IsSubClass)
            {
                mxs = $"(maxOps.getNodeByHandle {node.Handle}).{paramName}";
            }
            else
            {
                mxs = $"(maxOps.getNodeByHandle {node.Handle}).{gizmoClass}.{paramName}";
            }
            
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            ScriptsUtilities.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
            var r=  mxsRetVal.F;
            return r;
        }

        public static bool GetGizmoParameterBoolean(IINode node, string gizmoClass, string paramName, bool IsSubClass = true)
        {
            string mxs = String.Empty;
            if (!IsSubClass)
            {
                mxs = $"(maxOps.getNodeByHandle {node.Handle}).{paramName}";
            }
            else
            {
                mxs = $"(maxOps.getNodeByHandle {node.Handle}).{gizmoClass}.{paramName}";
            }

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            ScriptsUtilities.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
            var r = mxsRetVal.B;
            return r;
        }

        public static float[] GetTranslation(IINode node,IINode renderedNode)
        {
            float[] res = new float[3];
            //IPoint3 translation =  node.GetNodeTM(0, Tools.Forever).Trans; //position relative to parent, translation

            IObject obj = node.ObjectRef;
            IBox3 bbox = obj.GetWorldBoundBox(0, node, Loader.Core.ActiveViewExp);
            IPoint3 bboxCenter = bbox.Center;
            IMatrix3 inverted = renderedNode.GetNodeTM(0, Tools.Forever);
            inverted.Invert();
            IPoint3 bboxCenterInRenderNodeSpace = inverted.PointTransform(bboxCenter);
            

            res[0] = bboxCenterInRenderNodeSpace.X;
            res[1] = bboxCenterInRenderNodeSpace.Z;
            res[2] = -bboxCenterInRenderNodeSpace.Y;
            
            return res;
        }

        public static float[] GetRotation(IINode node,IINode renderedNode)
        {
            float[] res = new float[4];
            IMatrix3 nodeTm =  node.GetNodeTM(0, Tools.Forever);
            IMatrix3 inverted = renderedNode.GetNodeTM(0, Tools.Forever);
            inverted.Invert();
            nodeTm = nodeTm.Multiply(inverted);

            IPoint3 p = Loader.Global.Point3.Create(0, 0, 0);
            IQuat q = Loader.Global.IdentQuat;
            IPoint3 s = Loader.Global.Point3.Create(0, 0, 0);
            Loader.Global.DecomposeMatrix(nodeTm,p,q,s);

            q.Normalize();

            res[0] = q[0];
            res[1] = q[2];
            res[2] = -q[1];
            res[3] = -q[3];

            return res;
        }

        public static bool IsDefaultRotation(float[] rotation)
        {
            if (rotation[0] == 0 && rotation[1] == 0 && rotation[2] == 0 && (rotation[3] == 1.0f || rotation[3]==-1.0f )) return true;
            return false;
        }
    }
}
