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
    class GLTFExtensionAsoboMacroLight : GLTFProperty
    {
        [DataMember(Name = "macro_type")]
        public string macroLightType { get; set; }
    }


    class FlightSimLightExtension : IBabylonExtensionExporter
    {
        readonly ClassIDWrapper MacroLightOmniClassID = new ClassIDWrapper(0x3eb36fbb, 0x36275949);
        readonly ClassIDWrapper MacroLightSpotClassID = new ClassIDWrapper(0x451a77a6, 0x232b0194);

        public static Dictionary<string, string> MacroLight = new Dictionary<string, string>()
        {
            {"White light spot (car front)", "white_light_spot"},
            {"Red light spot (car rear)", "red_light_spot"},
            {"Orange beacon/flashing lights (security cars)", "orange_beacon"},
            {"Red beacon / flashing lights (Fire trucks)", "red_beacon"},
            {"Blue beacon / flashing lights (Fire trucks)", "blue_beacon"}
        };
        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return "ASOBO_macro_light";
        }

        public BabylonExtendTypes GetExtendedType()
        {
            return new BabylonExtendTypes(typeof(GLTFNode));
        }
        public bool ExportBabylonExtension<T>(T babylonObject, ExportParameters parameters, ref BabylonScene babylonScene, ILoggingProvider logger)
        {
            // just skip this extension is ment only for GLTF
            return false;
        }

        public object ExportGLTFExtension<T>(T babylonObject, ExportParameters parameters, GLTF gltf, ILoggingProvider logger)
        {
            var babylonLight = babylonObject as BabylonNode;
            if (babylonLight != null)
            {
                GLTFExtensionAsoboMacroLight macroLightExt = new GLTFExtensionAsoboMacroLight();
                
                Guid guid = Guid.Empty;
                Guid.TryParse(babylonLight.id, out guid);
                IINode maxNode = Tools.GetINodeByGuid(guid);
                if (maxNode != null)
                {  
                    IObject obj = maxNode.ObjectRef;
                    if (new ClassIDWrapper(obj.ClassID).Equals(MacroLightOmniClassID) || new ClassIDWrapper(obj.ClassID).Equals(MacroLightSpotClassID) )
                    {
                        string macroLightValue = maxNode.GetStringProperty("flightsim_macro_light_type", FlightSimLightExtension.MacroLight.Keys.ElementAt(0));
                        string macroLightType = FlightSimLightExtension.MacroLight[macroLightValue];
                        macroLightExt.macroLightType = macroLightType;

                        return macroLightExt;
                    }
                }
            }
            return null;
        }
        

        #endregion

    }
}
