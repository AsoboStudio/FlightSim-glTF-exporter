using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Max;
using Babylon2GLTF;
using BabylonExport.Entities;
using GLTFExport.Entities;

namespace Max2Babylon.KittyHawkExtension
{
    [DataContract]
    class GLTFExtensionAsoboFade : GLTFProperty
    {
        [DataMember(EmitDefaultValue = false,Name = "type")] public string Type { get; set; }
        [DataMember(EmitDefaultValue = false, Name = "translation")] public object Translation;
        [DataMember(Name = "params")] public object Params { get; set; }
    }

    [DataContract]
    class GLTFExtensionAsoboFadeSphereParams : GLTFProperty
    {
        [DataMember(EmitDefaultValue = false)] public float? radius;
    }


    class FlightSimFadeExtension : IBabylonExtensionExporter
    {
        readonly ClassIDWrapper SphereFadeClassID = new ClassIDWrapper(0x794b56ca, 0x172623ba);

        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return "ASOBO_fade_object";
        }

        public Type GetGLTFExtendedType()
        {
            return typeof(GLTFMesh);
        }

        public object ExportBabylonExtension<T>(T babylonObject)
        {
            var babylonMesh = babylonObject as BabylonMesh;
            if (babylonMesh != null)
            {
                List<GLTFExtensionAsoboFade> fadeObjects = new List<GLTFExtensionAsoboFade>();
                Guid guid = Guid.Empty;
                Guid.TryParse(babylonMesh.id, out guid);
                IINode maxNode = Tools.GetINodeByGuid(guid);
                foreach (IINode node in maxNode.NodeTree())
                {
                    IObject obj = node.ObjectRef;
                    if (new ClassIDWrapper(obj.ClassID).Equals(SphereFadeClassID))
                    {
                        GLTFExtensionAsoboFade fadeSphere = new GLTFExtensionAsoboFade();
                        GLTFExtensionAsoboFadeSphereParams sphereParams = new GLTFExtensionAsoboFadeSphereParams();
                        float radius = GetGizmoParameter(node,"SphereGizmo", "radius");
                        fadeSphere.Translation = GetTranslation(node);
                        sphereParams.radius = radius;
                        fadeSphere.Type = "sphere";
                        fadeSphere.Params = sphereParams;
                        fadeObjects.Add(fadeSphere);
                    }
                }
                if(fadeObjects.Count>0)
                {
                    return fadeObjects;
                }
            }
            return null;
        }
        #endregion

        private float GetGizmoParameter(IINode node, string gizmoClass, string paramName)
        {
            string mxs = $"(maxOps.getNodeByHandle {node.Handle}).{gizmoClass}.{paramName}";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            var r=  mxsRetVal.F;
            return r;
        }

        private float[] GetTranslation(IINode node)
        {
            float[] res = new float[3];
            string mxs = $"(maxOps.getNodeByHandle {node.Handle}).transform.pos * inverse (maxOps.getNodeByHandle {node.Handle}).parent.transform";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            var r=  mxsRetVal.P;
            res[0] = r.X;
            res[1] = r.Y;
            res[2] = r.Z;
            return res;
        }
    }
}
