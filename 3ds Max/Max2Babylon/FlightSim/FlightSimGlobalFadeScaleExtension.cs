using System;
using System.Runtime.Serialization;
using Babylon2GLTF;
using BabylonExport.Entities;
using GLTFExport.Entities;

namespace Max2Babylon.FlightSimExtension
{
    [DataContract]
    class GLTFExtensionGlobalFadeScale : GLTFProperty
    {
        [DataMember(EmitDefaultValue = false)] public float? radius;
    }

    class FlightSimGlobalFadeScaleExtension : IBabylonExtensionExporter
    {
        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return "ASOBO_fade_global_scale";
        }

        public Type GetGLTFExtendedType()
        {
            return typeof(GLTFScene);
        }

        public object ExportBabylonExtension<T>(T babylonObject)
        {
            var babylonScene = babylonObject as BabylonScene;
            if (babylonScene != null)
            {
                GLTFExtensionGlobalFadeScale fadeScale = new GLTFExtensionGlobalFadeScale();
                float fadeGlobalScale = Loader.Core.RootNode.GetFloatProperty("flightsim_fade_globalscale", 1);
                fadeScale.radius = fadeGlobalScale;

                if(fadeScale.radius!=1.0f)
                {
                    return fadeScale;
                }
            }
            return null;
        }
        #endregion

    }
}
