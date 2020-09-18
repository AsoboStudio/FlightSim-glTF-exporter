using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabylonExport.Entities;
using GLTFExport.Entities;
using Max2Babylon;
using Utilities;

namespace Babylon2GLTF
{
    public class ExtendedTypes
    {
        public Type babylonType;
        public Type gltfType;

        public ExtendedTypes(Type babylonType, Type gltfType)
        {
            this.babylonType = babylonType;
            this.gltfType = gltfType;
        }

        public ExtendedTypes(Type gltfType)
        {
            this.gltfType = gltfType;
        }
    }

    public class ExtensionInfo {}

    public class AnimationExtensionInfo:ExtensionInfo
    {
        public int startFrame;
        public int endFrame;

        public AnimationExtensionInfo(int startFrame, int endFrame)
        {
            this.startFrame = startFrame;
            this.endFrame = endFrame;
        }
    }

    public interface IBabylonExtensionExporter
    {
        string GetGLTFExtensionName();
        ExtendedTypes GetExtendedType();
        object ExportGLTFExtension<T1,T2>(T1 babylonObject,ref T2 gltfObject, ref GLTF gltf, GLTFExporter exporter, ExtensionInfo extInfo );
        bool ExportBabylonExtension<T>(T babylonObject,ref BabylonScene babylonScene, BabylonExporter exporter);
    }

    




}
