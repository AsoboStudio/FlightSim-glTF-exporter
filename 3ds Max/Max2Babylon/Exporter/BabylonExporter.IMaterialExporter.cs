using System;
using System.Collections.Generic;
using System.Drawing;
using Autodesk.Max;
using Babylon2GLTF;
using BabylonExport.Entities;
using GLTFExport.Entities;
using Utilities;

namespace Max2Babylon
{
    public interface IBabylonMaterialExtensionExporter: IBabylonExtensionExporter
    {
        MaterialUtilities.ClassIDWrapper MaterialClassID { get; }
    }

    
}