using System;
using System.Runtime.Serialization;
using Babylon2GLTF;
using BabylonExport.Entities;
using GLTFExport.Entities;

namespace FlightSimExtension
{
    public enum TangentSpaceConvention
    {
        DirectX,
        OpenGL
    }

    [DataContract]
    class GLTFExtensionNormalMapConvention : GLTFProperty
    {
        [DataMember(Name = "tanget_space_convention")] 
        public string tangentSpaceConvention { get; set; }
    }


    static class FlightSimNormalMapConvention
    {
        #region Implementation of IGLTFExtensionExporter

        public static string GetGLTFExtensionName()
        {
            return "ASOBO_normal_map_convention";
        }

        public static void AddNormalMapConvention(ref GLTF gltf,ExportParameters exportParameters)
        {
            string extensionName = GetGLTFExtensionName();
            
            if (gltf != null && gltf.asset!= null)
            {
                GLTFExtensionNormalMapConvention gltfExtensionNormalMapConvention = new GLTFExtensionNormalMapConvention();

                if (exportParameters.tangentSpaceConvention == TangentSpaceConvention.OpenGL)
                {
                    gltfExtensionNormalMapConvention.tangentSpaceConvention = TangentSpaceConvention.OpenGL.ToString();
                }
                else
                {
                    gltfExtensionNormalMapConvention.tangentSpaceConvention = TangentSpaceConvention.DirectX.ToString();
                }

                if (gltf.asset.extensions == null)
                {
                    gltf.asset.extensions = new GLTFExtensions();
                }
                gltf.asset.extensions.Add(extensionName, gltfExtensionNormalMapConvention);

                if (gltf.extensionsUsed == null)
                {
                    gltf.extensionsUsed = new System.Collections.Generic.List<string>();
                }

                gltf.extensionsUsed.Add(extensionName);


            }
            
        }
        
        #endregion

        
    }
}