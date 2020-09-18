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
        [DataMember(Name = "flash_frequency", IsRequired = true)] public float flashFrequency { get; set; }
        [DataMember(Name = "flash_duration", IsRequired = true)] public float flashDuration { get; set; }
        [DataMember(Name = "flash_phase", IsRequired = true)] public float flashPhase { get; set; }
        [DataMember(Name = "rotation_speed", IsRequired = true)] public float rotationSpeed { get; set; }
        [DataMember(Name = "day_night_cycle", IsRequired = true)] public bool dayNightCycle { get; set; }
    }


    class FlightSimLightExtension : IBabylonExtensionExporter
    {
        readonly MaterialUtilities.ClassIDWrapper MacroLightOmniClassID = new MaterialUtilities.ClassIDWrapper(0x3eb36fbb, 0x36275949);
        readonly MaterialUtilities.ClassIDWrapper MacroLightSpotClassID = new MaterialUtilities.ClassIDWrapper(0x451a77a6, 0x232b0194);
        readonly MaterialUtilities.ClassIDWrapper FlightSimLightClassID = new MaterialUtilities.ClassIDWrapper(0x18a3b84e, 0x63ec33ad);
        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return "ASOBO_macro_light";
        }

        public ExtendedTypes GetExtendedType()
        {
            return new ExtendedTypes(typeof(GLTFNode));
        }
        public bool ExportBabylonExtension<T>(T babylonObject, ref BabylonScene babylonScene, BabylonExporter exporter)
        {
            // just skip this extension is ment only for GLTF
            return false;
        }

        public object ExportGLTFExtension<T1,T2>(T1 babylonObject, ref T2 gltfObject,  ref GLTF gltf, GLTFExporter exporter,ExtensionInfo extInfo)
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
                        exporter.logger?.RaiseError($"{maxNode.NodeName} is type of MacroLightOmni and it is DEPRECATED, use FlightSimLight");
                        return null;
                    }
                    else if (new MaterialUtilities.ClassIDWrapper(obj.ClassID).Equals(MacroLightSpotClassID))
                    {
                        exporter.logger?.RaiseError($"{maxNode.NodeName} is type of MacroLightSpot and it is DEPRECATED, use FlightSimLight");
                        return null;
                    }
                    else if (new MaterialUtilities.ClassIDWrapper(obj.ClassID).Equals(FlightSimLightClassID))
                    {
                        macroLightExt.color = GetColor(maxNode);
                        macroLightExt.intensity = GetIntensity(maxNode);
                        macroLightExt.coneAngle = GetConeAngle(maxNode);
                        macroLightExt.hasSimmetry = GetHasSimmetry(maxNode);
                        macroLightExt.flashFrequency = GetFlashFrequency(maxNode);
                        macroLightExt.dayNightCycle = GetDayNightCycle(maxNode);
                        macroLightExt.flashDuration = GetFlashDuration(maxNode);
                        macroLightExt.flashPhase = GetFlashPhase(maxNode);
                        macroLightExt.rotationSpeed = GetRotationSpeed(maxNode);
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
#if MAX2015 ||MAX2016 || MAX2017 || MAX2018
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
            mxs = $"(maxOps.getNodeByHandle {node.Handle}).delegate.Intensity";

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 ||MAX2016|| MAX2017 || MAX2018
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
#if MAX2015 || MAX2016|| MAX2017 || MAX2018
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
#if MAX2015 || MAX2016|| MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif
            var r = mxsRetVal.B;
            return r;
        }

        private float GetFlashFrequency(IINode node)
        {
            string mxs = String.Empty;
            mxs = $"(maxOps.getNodeByHandle {node.Handle}).FlashFrequency";

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 || MAX2016|| MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif
            var r = mxsRetVal.F;
            return r;
        }

        private float GetFlashDuration(IINode node)
        {
            string mxs = String.Empty;
            mxs = $"(maxOps.getNodeByHandle {node.Handle}).FlashDuration";

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 || MAX2016|| MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif
            var r = mxsRetVal.F;
            return r;
        }

        private float GetRotationSpeed(IINode node)
        {
            string mxs = String.Empty;
            mxs = $"(maxOps.getNodeByHandle {node.Handle}).RotationSpeed";

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 || MAX2016|| MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif
            var r = mxsRetVal.F;
            return r;
        }


        private float GetFlashPhase(IINode node)
        {
            string mxs = String.Empty;
            mxs = $"(maxOps.getNodeByHandle {node.Handle}).FlashPhase";

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 || MAX2016|| MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif
            var r = mxsRetVal.F;
            return r;
        }


        private bool GetDayNightCycle(IINode node)
        {
            string mxs = String.Empty;
            mxs = $"(maxOps.getNodeByHandle {node.Handle}).ActivationMode";

            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
#if MAX2015 || MAX2016|| MAX2017 || MAX2018
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal);
#else
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
#endif

            bool r = mxsRetVal.I == 1;
            return r;
        }
        #endregion

        #endregion

    }

}
