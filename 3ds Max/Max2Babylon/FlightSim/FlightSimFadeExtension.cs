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

namespace Max2Babylon.FlightSimExtension
{
    [DataContract]
    class GLTFExtensionAsoboFade : GLTFProperty
    {
        [DataMember(Name = "fade_objects")]
        public List<GLTFExtensionFade> fades { get; set; }
    }
    [DataContract]
    class GLTFExtensionFade : GLTFProperty
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
                GLTFExtensionAsoboFade fade = new GLTFExtensionAsoboFade();
                List<GLTFExtensionFade> fadeObjects = new List<GLTFExtensionFade>();
                fade.fades = fadeObjects;
                Guid guid = Guid.Empty;
                Guid.TryParse(babylonMesh.id, out guid);
                IINode maxNode = Tools.GetINodeByGuid(guid);
                foreach (IINode node in maxNode.NodeTree())
                {
                    IObject obj = node.ObjectRef;
                    if (new ClassIDWrapper(obj.ClassID).Equals(SphereFadeClassID))
                    {
                        GLTFExtensionFade fadeSphere = new GLTFExtensionFade();
                        GLTFExtensionAsoboFadeSphereParams sphereParams = new GLTFExtensionAsoboFadeSphereParams();
                        float radius = GetGizmoParameter(node,"SphereGizmo", "radius");
                        fadeSphere.Translation = GetTranslation(node,maxNode);
                        sphereParams.radius = radius;
                        fadeSphere.Type = "sphere";
                        fadeSphere.Params = sphereParams;
                        fadeObjects.Add(fadeSphere);
                    }
                }
                if(fadeObjects.Count>0)
                {
                    return fade;
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

        private float[] GetTranslation(IINode node,IINode renderedNode)
        {
            float[] res = new float[3];
            string mxs = $"(maxOps.getNodeByHandle {node.Handle}).center * inverse (maxOps.getNodeByHandle {renderedNode.Handle}).transform";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            var r=  mxsRetVal.P;
            res[0] = r.X;
            res[1] = r.Z;
            res[2] = r.Y;
            return res;
        }
    }
}
