using System;
using System.Runtime.Serialization;
using Babylon2GLTF;
using BabylonExport.Entities;
using GLTFExport.Entities;
using Utilities;

namespace Max2Babylon.FlightSimExtension
{
    [DataContract]
    class GLTFExtensionGlobalFadeScale : GLTFProperty
    {
        [DataMember(EmitDefaultValue = false)] public float? scale;
    }

    class FlightSimGlobalFadeScaleExtension : IBabylonExtensionExporter
    {
        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return "ASOBO_scene_fade_scale";
        }

        public ExtendedTypes GetExtendedType()
        {
            return new ExtendedTypes(typeof(GLTFScene));
        }

        public bool ExportBabylonExtension<T>(T babylonObject, ref BabylonScene babylonScene, BabylonExporter exporter)
        {
            // just skip this extension is ment only for GLTF
            return false;
        }

        public object ExportGLTFExtension<T1,T2>(T1 babylonObject, ref T2 gltfObject,  ref GLTF gltf, GLTFExporter exporter,ExtensionInfo extInfo)
        {
            var babylonScene = babylonObject as BabylonScene;
            if (babylonScene != null)
            {
                GLTFExtensionGlobalFadeScale fadeScale = new GLTFExtensionGlobalFadeScale();
                float fadeGlobalScale = Loader.Core.RootNode.GetFloatProperty("flightsim_fade_globalscale", 1);
                fadeScale.scale = fadeGlobalScale;

                if(fadeScale.scale!=1.0f)
                {
                    return fadeScale;
                }
            }
            return null;
        }
        #endregion

    }
}
