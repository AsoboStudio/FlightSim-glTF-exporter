using Autodesk.Max;
using Babylon2GLTF;
using BabylonExport.Entities;
using GLTFExport.Entities;
using System;
using System.Runtime.Serialization;
using Utilities;

namespace Max2Babylon.FlightSimExtension
{
    [DataContract]
    class GLTFExtensionAsoboMacroLight : GLTFProperty
    {
        //[DataMember(Name = "macro_type")]  public string macroLightType { get; set; }
        [DataMember(Name = "color", IsRequired = true)] public float[] color { get; set; }
        [DataMember(Name = "intensity", IsRequired = true)] public float intensity { get; set; }
        [DataMember(Name = "cone_angle", IsRequired = true)] public float coneAngle { get; set; }
        [DataMember(Name = "has_simmetry", IsRequired = true)] public bool hasSimmetry { get; set; }
        [DataMember(Name = "is_beacon", IsRequired = true)] public bool isBeacon { get; set; }
    }


    class FlightSimLightExtension : IBabylonExtensionExporter
    {
        readonly MaterialUtilities.ClassIDWrapper MacroLightOmniClassID = new MaterialUtilities.ClassIDWrapper(0x3eb36fbb, 0x36275949);
        readonly MaterialUtilities.ClassIDWrapper MacroLightSpotClassID = new MaterialUtilities.ClassIDWrapper(0x451a77a6, 0x232b0194);

        //public static Dictionary<string, string> MacroLight = new Dictionary<string, string>()
        //{
        //    {"White light spot (car front)", "white_light_spot"},
        //    {"Red light spot (car rear)", "red_light_spot"},
        //    {"Orange beacon/flashing lights (security cars)", "orange_beacon"},
        //    {"Red beacon / flashing lights (Fire trucks)", "red_beacon"},
        //    {"Blue beacon / flashing lights (Fire trucks)", "blue_beacon"}
        //};
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

        public object ExportGLTFExtension<T>(T babylonObject, ExportParameters parameters, ref GLTF gltf, ILoggingProvider logger)
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
                    if (new MaterialUtilities.ClassIDWrapper(obj.ClassID).Equals(MacroLightOmniClassID))
                    {
                        logger.RaiseError($"{maxNode.NodeName} is type of MacroLightOmni and it is DEPRECATED, use MACROLIGHT");
                        return null;
                    }
                    else if (new MaterialUtilities.ClassIDWrapper(obj.ClassID).Equals(MacroLightSpotClassID))
                    {

                        //string macroLightValue = maxNode.GetStringProperty("flightsim_macro_light_type", FlightSimLightExtension.MacroLight.Keys.ElementAt(0));
                        //string macroLightType = FlightSimLightExtension.MacroLight[macroLightValue];
                        //macroLightExt.macroLightType = macroLightType;

                        //return macroLightExt;

                        macroLightExt.color = GetColor(maxNode);
                        macroLightExt.intensity = GetIntensity(maxNode);
                        macroLightExt.coneAngle = GetConeAngle(maxNode);
                        macroLightExt.hasSimmetry = GetHasSimmetry(maxNode);
                        macroLightExt.isBeacon = GetIsBeacon(maxNode);
                        return macroLightExt;
                    }
                }
            }
            return null;
        }

        #region Parameters
        private float[] GetColor(IINode node)
        {
            string mxs = String.Empty;
            mxs = $"(maxOps.getNodeByHandle {node.Handle}).Color";

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 || MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif
            var r = mxsRetVal.Clr.ToArray();

            return r;
        }


        private float GetIntensity(IINode node)
        {
            string mxs = String.Empty;
            mxs = $"(maxOps.getNodeByHandle {node.Handle}).Intensity";

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 || MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif
            var r = mxsRetVal.F;
            return r;
        }

        private float GetConeAngle(IINode node)
        {
            string mxs = String.Empty;
            mxs = $"(maxOps.getNodeByHandle {node.Handle}).ConeAngle";

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 || MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif
            var r = mxsRetVal.F;
            return r;
        }

        private bool GetHasSimmetry(IINode node)
        {
            string mxs = String.Empty;
            mxs = $"(maxOps.getNodeByHandle {node.Handle}).HasSimmetry";

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 || MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif
            var r = mxsRetVal.B;
            return r;
        }

        private bool GetIsBeacon(IINode node)
        {
            string mxs = String.Empty;
            mxs = $"(maxOps.getNodeByHandle {node.Handle}).IsBeacon";

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 || MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif
            var r = mxsRetVal.B;
            return r;
        }
        #endregion

        #endregion

    }
}
