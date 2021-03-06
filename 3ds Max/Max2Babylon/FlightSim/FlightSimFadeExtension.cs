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
using Max2Babylon.FlightSim;
using Utilities;

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
        [DataMember(EmitDefaultValue = false,Name = "type", IsRequired = true)] public string Type { get; set; }
        [DataMember(EmitDefaultValue = false, Name = "translation")] public object Translation;
        [DataMember(Name = "params", IsRequired = true)] public object Params { get; set; }
    }

    [DataContract]
    class GLTFExtensionAsoboFadeSphereParams : GLTFProperty
    {
        [DataMember(EmitDefaultValue = false)] public float? radius;
    }


    class FlightSimFadeExtension : IBabylonExtensionExporter
    {
        readonly MaterialUtilities.ClassIDWrapper SphereFadeClassID = new MaterialUtilities.ClassIDWrapper(0x794b56ca, 0x172623ba);

        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return "ASOBO_fade_object";
        }

        public ExtendedTypes GetExtendedType()
        {
            return new ExtendedTypes(typeof(GLTFMesh));
        }

        public bool ExportBabylonExtension<T>(T babylonObject, ref BabylonScene babylonScene, BabylonExporter exporter)
        {
            // just skip this extension is ment only for GLTF
            return false;
        }

        public object ExportGLTFExtension<T1,T2>(T1 babylonObject, ref T2 gltfObject, ref GLTF gltf, GLTFExporter exporter,ExtensionInfo extInfo)
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
                foreach (IINode node in maxNode.DirectChildren())
                {
                    IObject obj = node.ObjectRef;
                    if (new MaterialUtilities.ClassIDWrapper(obj.ClassID).Equals(SphereFadeClassID))
                    {
                        GLTFExtensionFade fadeSphere = new GLTFExtensionFade();
                        GLTFExtensionAsoboFadeSphereParams sphereParams = new GLTFExtensionAsoboFadeSphereParams();
                        float radius = FlightSimExtensionUtility.GetGizmoParameterFloat(node,"SphereGizmo", "radius");
                        fadeSphere.Translation = FlightSimExtensionUtility.GetTranslation(node,maxNode);
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

    }
}
