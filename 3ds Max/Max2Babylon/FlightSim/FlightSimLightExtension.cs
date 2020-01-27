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

namespace Max2Babylon.FlightSimExtension
{
    [DataContract]
    class GLTFExtensionAsoboMacroLight : GLTFProperty
    {
        [DataMember(Name = "macro_type")]
        public string macroLightType { get; set; }
    }


    class FlightSimLightExtension : IBabylonExtensionExporter
    {
        readonly ClassIDWrapper MacroLightClassID = new ClassIDWrapper(0x3eb36fbb, 0x36275949);

        public static string[] MacroLightType =
        {
            "Faro 1",
            "Poisson Sampling",
            "ESM",
            "Blurred ESM"
        }; 
        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return "ASOBO_macro_light";
        }

        public Type GetGLTFExtendedType()
        {
            return typeof(GLTFNode);
        }

        public object ExportBabylonExtension<T>(T babylonObject)
        {
            var babylonLight = babylonObject as BabylonNode;
            if (babylonLight != null)
            {
                GLTFExtensionAsoboMacroLight macroLightExt = new GLTFExtensionAsoboMacroLight();
                
                Guid guid = Guid.Empty;
                Guid.TryParse(babylonLight.id, out guid);
                IINode maxNode = Tools.GetINodeByGuid(guid);
                IObject obj = maxNode.ObjectRef;
                if (new ClassIDWrapper(obj.ClassID).Equals(MacroLightClassID))
                {
                    string macroLightType = maxNode.GetStringProperty("flightsim_macro_light_type", FlightSimLightExtension.MacroLightType[0]);
                    macroLightExt.macroLightType = macroLightType;

                    return macroLightExt;
                }
            }
            return null;
        }
        #endregion

    }
}
