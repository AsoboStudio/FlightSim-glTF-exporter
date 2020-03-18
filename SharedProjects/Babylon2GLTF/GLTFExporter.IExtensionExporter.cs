using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabylonExport.Entities;
using GLTFExport.Entities;
using Utilities;

namespace Babylon2GLTF
{
    public class BabylonExtendTypes
    {
        public Type babylonType;
        public Type gltfType;

        public BabylonExtendTypes(Type babylonType, Type gltfType)
        {
            this.babylonType = babylonType;
            this.gltfType = gltfType;
        }

        public BabylonExtendTypes(Type gltfType)
        {
            this.gltfType = gltfType;
        }
    }

    public interface IBabylonExtensionExporter
    {
        string GetGLTFExtensionName();
        BabylonExtendTypes GetExtendedType();
        object ExportGLTFExtension<T>(T babylonObject,ExportParameters parameters,GLTF gltf,ILoggingProvider logger);
        bool ExportBabylonExtension<T>(T babylonObject,ExportParameters parameters,ref BabylonScene babylonScene,ILoggingProvider logger);
    }

    public interface IBabylonMaterialExtensionExporter: IBabylonExtensionExporter
    {

    }




}
