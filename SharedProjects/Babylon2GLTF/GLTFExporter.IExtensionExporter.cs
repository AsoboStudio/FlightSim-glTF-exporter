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
    public interface IBabylonExtensionExporter
    {
        string GetGLTFExtensionName();
        Type GetGLTFExtendedType();
        object ExportGLTFExtension<T>(T babylonObject,ExportParameters parameters,GLTF gltf,ILoggingProvider logger);
        bool ExportBabylonExtension<T>(T babylonObject,ExportParameters parameters,ref BabylonScene babylonScene,ILoggingProvider logger);
    }




}
