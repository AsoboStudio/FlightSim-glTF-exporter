using Autodesk.Max;
using GLTFExport.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;

namespace Max2Babylon
{
    #region Serializable glTF Objects

    [DataContract]
    class GLTFNormalTextureInfo : GLTFTextureInfo
    {
        [DataMember(EmitDefaultValue = false)]
        public float? scale { get; set; }
    }

    [DataContract]
    class GLTFOcclusionTextureInfo : GLTFTextureInfo
    {
        [DataMember(EmitDefaultValue = false)]
        public float? strength { get; set; }
    }

    [DataContract]
    class GLTFExtensionAsoboMaterialDecal : GLTFProperty // use GLTFChildRootProperty if you want to add a name
    {
        public const string SerializedName = "ASOBO_material_blend_gbuffer";
        [DataMember(EmitDefaultValue = false)] public float? baseColorBlendFactor;
        [DataMember(EmitDefaultValue = false)] public float? metallicBlendFactor;
        [DataMember(EmitDefaultValue = false)] public float? roughnessBlendFactor;
        [DataMember(EmitDefaultValue = false)] public float? normalBlendFactor;
        [DataMember(EmitDefaultValue = false)] public float? emissiveBlendFactor;
        [DataMember(EmitDefaultValue = false)] public float? occlusionBlendFactor;
    }

    [DataContract]
    class GLTFExtensionAsoboMaterialLayer : GLTFProperty
    {
        public const string SerializedName = "ASOBO_material_distance_field_layer";
        [DataMember(EmitDefaultValue = false)] public float[] layerColor;
        [DataMember(EmitDefaultValue = false)] public GLTFTextureInfo layerColorTexture;
        [DataMember(EmitDefaultValue = false)] public GLTFTextureInfo distanceFieldLayerMaskTexture;
    }

    [DataContract]
    class GLTFExtensionAsoboAlphaModeDither : GLTFProperty
    {
        public const string SerializedName = "ASOBO_material_alphamode_dither";
        //[DataMember(EmitDefaultValue = false)] public string alphaMode;
    }

    [DataContract]
    class GLTFExtensionAsoboMaterialDetail
    {
        public const string SerializedName = "ASOBO_material_detail_map";

        [DataMember(EmitDefaultValue = false)]
        public float? UVScale { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public float[] UVOffset { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public GLTFTextureInfo detailColorTexture { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public GLTFNormalTextureInfo detailNormalTexture { get; set; }

        public static class Defaults
        {
            public static readonly float UVScale = 1;
            public static readonly float[] UVOffset = new float[] { 0, 0 };
            public static readonly float NormalScale = 1;
        }
    }

    static class GLTFExtensionHelper
    {
        public static string Name_MSFT_texture_dds = "MSFT_texture_dds";
    }

    #endregion

    #region Serializable Extras

    public static class KittyGLTFExtras
    {
        public static string Name_ASOBO_material_code = "ASOBO_material_code";

        [DataContract]
        public class MaterialCode
        {
            public enum Code
            {
                Windshield,
                Porthole,
                Glass
            }

            public static MaterialCode AsoboWindshield = new MaterialCode(Code.Windshield);
            public static MaterialCode AsoboPorthole = new MaterialCode(Code.Porthole);
            public static MaterialCode AsoboGlass = new MaterialCode(Code.Glass);

            // name is the serialized name, dont change
            [DataMember(EmitDefaultValue=false)]
            string ASOBO_material_code;

            public MaterialCode(Code code)
            {
                ASOBO_material_code = code.ToString();
            }
        }
    }

    #endregion

    public class KittyHawkMaterialExporter : IGLTFMaterialExporter
    {
        enum MaterialType
        {
            Standard,
            Decal,
            Windshield,
            Porthole,
            Glass
        }

        readonly ClassIDWrapper class_ID = new ClassIDWrapper(0x53196aaa, 0x57b6ad6a);

        ClassIDWrapper IMaterialExporter.MaterialClassID => class_ID;
        
        public KittyHawkMaterialExporter() { }

        BabylonExporter exporter;
        GLTF gltf;
        IIGameMaterial maxGameMaterial;
        Func<string, string, string> tryWriteImageFunc;
        Action<string, Color> raiseMessageAction;
        Action<string> raiseWarningAction;
        Action<string> raiseErrorAction;

        Dictionary<string, GLTFImage> srcTextureExportCache = new Dictionary<string, GLTFImage>();
        Dictionary<string, string> dstTextureExportCache = new Dictionary<string, string>();

        void RaiseMessage(string message) { RaiseMessage(message, Color.CornflowerBlue); }
        void RaiseMessage(string message, Color color) { raiseMessageAction?.Invoke(message, color); }
        void RaiseWarning(string message) { raiseWarningAction?.Invoke(message); }
        void RaiseError(string message) { raiseErrorAction?.Invoke(message); }

        GLTFMaterial IGLTFMaterialExporter.ExportGLTFMaterial(BabylonExporter exporter, GLTF gltf, IIGameMaterial maxGameMaterial, 
            Func<string, string, string> tryWriteImageFunc, 
            Action<string, Color> raiseMessageAction, 
            Action<string> raiseWarningAction, 
            Action<string> raiseErrorAction)
        {
            // if the gltf class instance is different, this is a new export
            if (this.gltf != gltf)
            {
                srcTextureExportCache.Clear();
                dstTextureExportCache.Clear();
            }

            // save parameters
            this.exporter = exporter;
            this.gltf = gltf;
            this.maxGameMaterial = maxGameMaterial;
            this.tryWriteImageFunc = tryWriteImageFunc;
            this.raiseMessageAction = raiseMessageAction;
            this.raiseWarningAction = raiseWarningAction;
            this.raiseErrorAction = raiseErrorAction;

            GLTFMaterial gltfMaterial = new GLTFMaterial();
            gltfMaterial.name = maxGameMaterial.MaterialName;
            gltfMaterial.id = maxGameMaterial.MaxMaterial.GetGuid().ToString();

            ProcessMaterialProperties(gltfMaterial, maxGameMaterial);

            return gltfMaterial;

            // to get an overview, for debug purposes
            /*
            int numProps = material.IPropertyContainer.NumberOfProperties;
            IIGameProperty[] properties = new IIGameProperty[numProps];
            for (int i = 0; i < numProps; ++i)
            {
                IIGameProperty property = material.IPropertyContainer.GetProperty(i);
                properties[i] = property;
            }

            int numParamBlocks = material.MaxMaterial.NumParamBlocks;
            IIParamBlock2[] paramBlocks = new IIParamBlock2[numParamBlocks];
            for(int i = 0; i < numParamBlocks; ++i)
            {
                IIParamBlock2 paramBlock = material.MaxMaterial.GetParamBlock(i);
                paramBlocks[i] = paramBlock;
            }
            */
        }

        void ProcessMaterialProperties(GLTFMaterial material, IIGameMaterial maxMaterial)
        {
            #region Helper Variables

            int int_out = 0;
            float float_out = 0.0f;
            string string_out = ""; // passing in a null string causes a null-reference exception, even though strings are immutable anyway
            IPoint4 point4_out = Loader.Global.Point4.Create();
            IPoint3 p3_out = Loader.Global.Point3.Create();

            GLTFImage image;
            GLTFTextureInfo info;

            // Define some variables for the GetPropertyValue parameters.
            // http://help.autodesk.com/view/3DSMAX/2017/ENU/?guid=__cpp_ref_class_i_game_property_html
            //
            // The time to retrieve the value, defaulted to the static frame.
            // set to 0 in the exporter with Loader.Global.IGameInterface.SetStaticFrame
            int param_t = 0;
            //
            // The flag indicating if percent fraction value (TYPE_PCNT_FRAC) should be converted (0.1 to 10), default:false 
            bool param_p = false;

            #endregion

            // Iterate over all properties and find what we want by string.
            // This way, it doesn't matter in which param rollout the property is defined.
            // This gives us a little more flexibility in the MaxScript material definition.
            //
            // Some parameters only have to be set if we have a texture:
            // - normal scale
            // - occlusion strength
            // Some parameters only have to be set if we have a specific alphaMode:
            // - alphaCutoff
            //
            // Thus, two for loops!
            // 1. process textures and alphamode
            // 2. the remaining parameters
            
            int numProps = maxMaterial.IPropertyContainer.NumberOfProperties;

            // cache some extension property values (conditional exports)

            float[] layerColor = new float[] { 1, 1, 1, 1 };
            string layerColorTexPath = null;
            string layerColorMaskPath = null;

            float detailUVScale = GLTFExtensionAsoboMaterialDetail.Defaults.UVScale;
            float[] detailUVOffset = new float[] { GLTFExtensionAsoboMaterialDetail.Defaults.UVOffset[0], GLTFExtensionAsoboMaterialDetail.Defaults.UVOffset[1] };
            float detailNormalScale = GLTFExtensionAsoboMaterialDetail.Defaults.NormalScale;
            string detailColorTexPath = null;
            string detailNormalTexPath = null;

            #region Material Type (Standard, Decal, Windshield, ...)
            // - Standard
            // - GBuffer Blend
            // - Windshield

            // only create if needed
            GLTFExtensionAsoboMaterialDecal decalExtensionObject = null;
            GLTFExtensions materialExtensions = new GLTFExtensions();
            GLTFExtensions materialExtras = new GLTFExtensions();

            // material flag is checked for setting specific defaults and other special cases
            // e.g. windshield is always using AlphaMode.BLEND for compatibility with gltf viewers (it's ignored engine side)
            MaterialType materialType = MaterialType.Standard;

            for (int i = 0; i < numProps; ++i)
            {
                IIGameProperty property = maxMaterial.IPropertyContainer.GetProperty(i);

                if (property == null)
                    continue;

                IParamDef paramDef = property.MaxParamBlock2?.GetParamDef(property.ParamID);
                string propertyName = property.Name.ToUpperInvariant();

                switch (propertyName)
                {
                    case "MATERIALTYPE":
                        {
                            if (!property.GetPropertyValue(ref int_out, param_t))
                                RaiseError("Could not retrieve MATERIALTYPE property.");
                            switch(int_out)
                            {
                                case 1:
                                    materialType = MaterialType.Standard;
                                    break;
                                case 2:
                                    materialType = MaterialType.Decal;
                                    decalExtensionObject = new GLTFExtensionAsoboMaterialDecal();
                                    break;
                                case 3:
                                    materialType = MaterialType.Windshield;
                                    materialExtras.Add(KittyGLTFExtras.Name_ASOBO_material_code, KittyGLTFExtras.MaterialCode.Code.Windshield.ToString());
                                    break;
                                case 4:
                                    materialType = MaterialType.Porthole;
                                    materialExtras.Add(KittyGLTFExtras.Name_ASOBO_material_code, KittyGLTFExtras.MaterialCode.Code.Porthole.ToString());
                                    break;
                                case 5:
                                    materialType = MaterialType.Glass;
                                    materialExtras.Add(KittyGLTFExtras.Name_ASOBO_material_code, KittyGLTFExtras.MaterialCode.Code.Glass.ToString());
                                    break;
                                default:
                                    materialType = MaterialType.Standard;
                                    break;
                            }
                            RaiseMessage(string.Format("Exporting Material Type: \"{0}\"", materialType.ToString()));
                            break;
                        }
                }
            }

            #endregion

            #region Decal Extension properties
            for (int i = 0; i < numProps; ++i)
            {
                IIGameProperty property = maxMaterial.IPropertyContainer.GetProperty(i);

                if (property == null)
                    continue;

                IParamDef paramDef = property.MaxParamBlock2?.GetParamDef(property.ParamID);
                string propertyName = property.Name.ToUpperInvariant();

                if (decalExtensionObject != null)
                {
                    switch (propertyName)
                    {
                        case "DECALCOLORFACTOR":
                            {
                                if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                                {
                                    RaiseError("Could not retrieve DECALCOLORFACTOR property.");
                                    continue;
                                }
                                decalExtensionObject.SetBaseColorBlendFactor(float_out);
                                break;
                            }
                        case "DECALROUGHNESSFACTOR":
                            {
                                if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                                {
                                    RaiseError("Could not retrieve DECALROUGHNESSFACTOR property.");
                                    continue;
                                }
                                decalExtensionObject.SetRoughnessBlendFactor(float_out);
                                break;
                            }
                        case "DECALMETALFACTOR":
                            {
                                if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                                {
                                    RaiseError("Could not retrieve DECALMETALFACTOR property.");
                                    continue;
                                }
                                decalExtensionObject.SetMetallicBlendFactor(float_out);
                                break;
                            }
                        case "DECALNORMALFACTOR":
                            {
                                if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                                {
                                    RaiseError("Could not retrieve DECALNORMALFACTOR property.");
                                    continue;
                                }
                                decalExtensionObject.SetNormalBlendFactor(float_out);
                                break;
                            }
                        case "DECALEMISSIVEFACTOR":
                            {
                                if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                                {
                                    RaiseError("Could not retrieve DECALEMISSIVEFACTOR property.");
                                    continue;
                                }
                                decalExtensionObject.SetEmissiveBlendFactor(float_out);
                                break;
                            }
                        case "DECALOCCLUSIONFACTOR":
                            {
                                if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                                {
                                    RaiseError("Could not retrieve DECALOCCLUSIONFACTOR property.");
                                    continue;
                                }
                                decalExtensionObject.SetOcclusionBlendFactor(float_out);
                                break;
                            }
                    }
                }
            }
            #endregion

            #region Detail Map Extension Properties
            if (materialType == MaterialType.Standard || materialType == MaterialType.Windshield)
            {
                for (int i = 0; i < numProps; ++i)
                {
                    IIGameProperty property = maxMaterial.IPropertyContainer.GetProperty(i);

                    if (property == null)
                        continue;

                    IParamDef paramDef = property.MaxParamBlock2?.GetParamDef(property.ParamID);
                    string propertyName = property.Name.ToUpperInvariant();

                    switch (propertyName)
                    {
                        case "DETAILCOLORTEX":
                            {
                                detailColorTexPath = GetImagePath(paramDef, property, param_t, "DETAILCOLORTEX");
                                break;
                            }
                        case "DETAILNORMALTEX":
                            {
                                detailNormalTexPath = GetImagePath(paramDef, property, param_t, "DETAILNORMALTEX");
                                break;
                            }
                        case "DETAILUVSCALE":
                            {
                                if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                                {
                                    RaiseError("Could not retrieve DETAILUVSCALE property.");
                                    continue;
                                }
                                detailUVScale = float_out;
                                break;
                            }
                        case "DETAILUVOFFSETX":
                            {
                                if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                                {
                                    RaiseError("Could not retrieve DETAILUVOFFSETX property.");
                                    continue;
                                }
                                detailUVOffset[0] = float_out;
                                break;
                            }
                        case "DETAILUVOFFSETY":
                            {
                                if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                                {
                                    RaiseError("Could not retrieve DETAILUVOFFSETY property.");
                                    continue;
                                }
                                detailUVOffset[1] = float_out;
                                break;
                            }
                        case "DETAILNORMALSCALE":
                            {
                                if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                                {
                                    RaiseError("Could not retrieve DETAILNORMALSCALE property.");
                                    continue;
                                }
                                detailNormalScale = float_out;
                                break;
                            }
                    }
                }
            }
            #endregion

            #region Textures & AlphaMode

            for (int i = 0; i < numProps; ++i)
            {
                IIGameProperty property = maxMaterial.IPropertyContainer.GetProperty(i);
                
                if (property == null)
                    continue;

                IParamDef paramDef = property.MaxParamBlock2?.GetParamDef(property.ParamID);
                string propertyName = property.Name.ToUpperInvariant();
                
                switch (propertyName)
                {
                    case "ALPHAMODE":
                        {
                            if (!property.GetPropertyValue(ref int_out, param_t))
                            {
                                RaiseError("Could not retrieve ALPHAMODE property.");
                                continue;
                            }

                            // overrides for specific material types
                            if (materialType == MaterialType.Decal || materialType == MaterialType.Windshield || materialType == MaterialType.Glass)
                                material.SetAlphaMode(GLTFMaterial.AlphaMode.BLEND.ToString());
                            else if (materialType == MaterialType.Porthole)
                                material.SetAlphaMode(GLTFMaterial.AlphaMode.OPAQUE.ToString());
                            else
                            {
                                int alphaMode = int_out - 1;
                                if (alphaMode == 3)
                                {
                                    alphaMode = (int)GLTFMaterial.AlphaMode.BLEND;
                                    GLTFExtensionAsoboAlphaModeDither ditherExtensionObject = new GLTFExtensionAsoboAlphaModeDither();
                                    material.extensions.Add(GLTFExtensionAsoboAlphaModeDither.SerializedName, ditherExtensionObject);
                                }
                                else if(alphaMode > 3 || alphaMode < 0)
                                {
                                    alphaMode = (int)GLTFMaterial.AlphaMode.OPAQUE;
                                    RaiseWarning("Unknown alpha mode: exporting OPAQUE");
                                }
                                else material.SetAlphaMode(((GLTFMaterial.AlphaMode)(alphaMode)).ToString());
                            }
                            break;
                        }
                    case "BASECOLORTEX":
                        {
                            string_out = GetImagePath(paramDef, property, param_t, "BASECOLORTEX");
                            image = ExportImage(string_out);
                            if (image == null)
                                continue;

                            info = CreateTextureInfo(image);
                            material.SetBaseColorTexture(info);

                            break;
                        }
                    case "LAYERCOLORTEX":
                        {
                            layerColorTexPath = GetImagePath(paramDef, property, param_t, "LAYERCOLORTEX");
                            break;
                        }
                    case "LAYERMASKTEX":
                        {
                            layerColorMaskPath = GetImagePath(paramDef, property, param_t, "LAYERMASKTEX");
                            break;
                        }
                    case "OCCLUSIONROUGHNESSMETALLICTEX":
                        {
                            string_out = GetImagePath(paramDef, property, param_t, "OCCLUSIONROUGHNESSMETALLICTEX");
                            image = ExportImage(string_out);
                            if (image == null)
                                continue;

                            info = CreateTextureInfo(image);
                            material.SetMetallicRoughnessTexture(info);

                            // same image & sampler, so can use same GLTFTexture
                            int textureIndex = info.index;
                            info = CreateTextureInfo<GLTFOcclusionTextureInfo>(textureIndex);
                            material.SetOcclusionTexture(info);

                            break;
                        }
                    case "NORMALTEX":
                        {
                            string_out = GetImagePath(paramDef, property, param_t, "NORMALTEX");
                            image = ExportImage(string_out);
                            if (image == null)
                                continue;

                            info = CreateTextureInfo<GLTFNormalTextureInfo>(image);
                            material.SetNormalTexture(info);

                            break;
                        }
                    case "EMISSIVETEX":
                        {
                            string_out = GetImagePath(paramDef, property, param_t, "EMISSIVETEX");
                            image = ExportImage(string_out);
                            if (image == null)
                                continue;

                            info = CreateTextureInfo(image);
                            material.SetEmissiveTexture(info);

                            break;
                        }
                }
            }

            #endregion

            #region The Other Parameters

            for (int i = 0; i < numProps; ++i)
            {
                IIGameProperty property = maxMaterial.IPropertyContainer.GetProperty(i);

                if (property == null)
                    continue;

                string propertyName = property.Name.ToUpperInvariant();
                switch (propertyName)
                {
                    case "BASECOLOR":
                        {
                            if (!property.GetPropertyValue(point4_out, param_t))
                            {
                                RaiseError("Could not retrieve BASECOLOR property.");
                                continue;
                            }
                            material.SetBaseColorFactor(point4_out.X, point4_out.Y, point4_out.Z);
                            material.SetBaseColorFactorAlpha(point4_out.W);
                            break;
                        }
                    case "LAYERCOLOR":
                        {
                            if (!property.GetPropertyValue(point4_out, param_t))
                            {
                                RaiseError("Could not retrieve LAYERCOLOR property.");
                                continue;
                            }
                            for (int c = 0; c < 4; ++c)
                                layerColor[c] = point4_out[c];

                            break;
                        }
                    case "EMISSIVE":
                        {
                            if (!property.GetPropertyValue(point4_out, param_t))
                            {
                                RaiseError("Could not retrieve EMISSIVE property.");
                                continue;
                            }
                            material.SetEmissiveFactor(point4_out.X, point4_out.Y, point4_out.Z);
                            break;
                        }
                    case "ROUGHNESS":
                        {
                            if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                            {
                                RaiseError("Could not retrieve ROUGHNESS property.");
                                continue;
                            }
                            material.SetRoughnessFactor(float_out);
                            break;
                        }
                    case "METALLIC":
                        {
                            if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                            {
                                RaiseError("Could not retrieve METALLIC property.");
                                continue;
                            }
                            material.SetMetallicFactor(float_out);
                            break;
                        }
                    case "NORMALSCALE":
                        {
                            if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                            {
                                RaiseError("Could not retrieve NORMALSCALE property.");
                                continue;
                            }

                            // only set if normal texture is defined
                            GLTFNormalTextureInfo normalTexture = material.normalTexture as GLTFNormalTextureInfo;
                            if (normalTexture == null)
                                continue;

                            material.SetNormalScale(float_out);
                            break;
                        }
                    //case "OCCLUSIONSTRENGTH":
                    //    {
                    //        if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                    //            RaiseError("Could not retrieve OCCLUSIONSTRENGTH property.");
                    //
                    //        // only set if occlusion texture is defined
                    //        GLTFOcclusionTextureInfo occlusionTexture = material.occlusionTexture as GLTFOcclusionTextureInfo;
                    //        if (occlusionTexture == null)
                    //            continue;
                    //
                    //        material.SetOcclusionStrength(float_out);
                    //        break;
                    //    }
                    case "ALPHACUTOFF":
                        {
                            if (!property.GetPropertyValue(ref float_out, param_t, param_p))
                            {
                                RaiseError("Could not retrieve ALPHACUTOFF property.");
                                continue;
                            }

                            // only set if alphamode == mask
                            if (material.alphaMode == GLTFMaterial.AlphaMode.MASK.ToString())
                            {
                                material.SetAlphaCutoff(float_out);
                            }
                            break;
                        }
                    case "DOUBLESIDED":
                        {
                            if (!property.GetPropertyValue(ref int_out, param_t))
                            {
                                RaiseError("Could not retrieve DOUBLESIDED property.");
                                continue;
                            }

                            material.SetDoubleSided(int_out > 0);
                            break;
                        }
                }
            }

            #endregion
            
            #region Process Extension Objects

            // custom layer extension, only export if we have a layer mask texture
            GLTFExtensionAsoboMaterialLayer layerExtensionObject = null;
            if (!string.IsNullOrWhiteSpace(layerColorMaskPath))
            {
                layerExtensionObject = new GLTFExtensionAsoboMaterialLayer();

                layerExtensionObject.layerColor = layerColor;

                image = ExportImage(layerColorMaskPath, true);
                if (image != null)
                {
                    info = CreateTextureInfo(image);
                    layerExtensionObject.distanceFieldLayerMaskTexture = info;
                }

                if (!string.IsNullOrWhiteSpace(layerColorTexPath))
                {
                    image = ExportImage(layerColorTexPath);
                    if (image != null)
                    {
                        info = CreateTextureInfo(image);
                        layerExtensionObject.layerColorTexture = info;
                    }
                }
            }

            // detail map extension, only if we have a detail color and/or detail normal map
            GLTFExtensionAsoboMaterialDetail detailExtensionObject = null;
            if(!string.IsNullOrWhiteSpace(detailColorTexPath) || !string.IsNullOrWhiteSpace(detailNormalTexPath))
            {
                detailExtensionObject = new GLTFExtensionAsoboMaterialDetail();
                if (!string.IsNullOrWhiteSpace(detailColorTexPath))
                {
                    image = ExportImage(detailColorTexPath);
                    if (image != null)
                    {
                        info = CreateTextureInfo(image);
                        detailExtensionObject.detailColorTexture = info;
                    }
                }
                if (!string.IsNullOrWhiteSpace(detailNormalTexPath))
                {
                    image = ExportImage(detailNormalTexPath);
                    if (image != null)
                    {
                        info = CreateTextureInfo<GLTFNormalTextureInfo>(image);
                        detailExtensionObject.detailNormalTexture = (GLTFNormalTextureInfo)info;

                        if(detailNormalScale != GLTFExtensionAsoboMaterialDetail.Defaults.NormalScale)
                            detailExtensionObject.detailNormalTexture.scale = detailNormalScale;
                    }
                }

                if (detailUVScale != GLTFExtensionAsoboMaterialDetail.Defaults.UVScale)
                    detailExtensionObject.UVScale = detailUVScale;

                if (detailUVOffset[0] != GLTFExtensionAsoboMaterialDetail.Defaults.UVOffset[0]
                    && detailUVOffset[1] != GLTFExtensionAsoboMaterialDetail.Defaults.UVOffset[1])
                {
                    detailExtensionObject.UVOffset = new float[2];
                    detailUVOffset.CopyTo(detailExtensionObject.UVOffset, 0);
                }
            }

            #endregion

            #region Post-processing

            // add used extensions to dictionaries
            if (decalExtensionObject != null)
                materialExtensions.Add(GLTFExtensionAsoboMaterialDecal.SerializedName, decalExtensionObject);

            if(layerExtensionObject != null)
                materialExtensions.Add(GLTFExtensionAsoboMaterialLayer.SerializedName, layerExtensionObject);

            if (detailExtensionObject != null)
                materialExtensions.Add(GLTFExtensionAsoboMaterialDetail.SerializedName, detailExtensionObject);

            if (materialExtensions.Count > 0)
            {
                material.extensions = materialExtensions;

                // set all extensions as used but not required
                foreach (var pair in material.extensions)
                {
                    if (!gltf.extensionsUsed.Contains(pair.Key))
                        gltf.extensionsUsed.Add(pair.Key);
                }
            }

            if(materialExtras.Count > 0)
            {
                material.extras = materialExtras;
            }

            #endregion
        }

        /* ProcessParamBlocks
        void ProcessParamBlocks(GLTFMaterial gltfMaterial, IIGameMaterial material)
        {
            int numTexMaps = material.NumberOfTextureMaps;
            int numParamBlocks = material.MaxMaterial.NumParamBlocks;
            for (int i = 0; i < numParamBlocks; ++i)
            {
                IIParamBlock2 paramBlock = material.MaxMaterial.GetParamBlock(i);
                switch(paramBlock.LocalName.ToUpperInvariant())
                {
                    case "SHADER":
                        ProcessParamBlock_SHADER(gltfMaterial, paramBlock);
                        break;
                    case "PARAMS":
                        ProcessParamBlock_PARAMS(gltfMaterial, paramBlock);
                        break;
                    case "PARAMS2":
                        ProcessParamBlock_PARAMS2(gltfMaterial, paramBlock);
                        break;
                    case "TEXTURES":
                        ProcessParamBlock_TEXTURES(gltfMaterial, paramBlock);
                        break;
                    case "FLAGS":
                        ProcessParamBlock_FLAGS(gltfMaterial, paramBlock);
                        break;
                }
            }
        }
        */
        /* ProcessParamBlock_SHADER
        void ProcessParamBlock_SHADER(GLTFMaterial material, IIParamBlock2 paramBlock)
        {
            int numParams = paramBlock.NumParams;
            for(int i = 0; i < numParams; ++i)
            {
                short paramId = paramBlock.IndextoID(i);
                IParamDef def = paramBlock.GetParamDef(paramId);
                ParamType2 type = paramBlock.GetParameterType(paramId);
                switch (def.IntName.ToUpperInvariant())
                {
                    case "CODE":
                        if (type != ParamType2.String)
                            continue;// todo: throw warning
                        string code = paramBlock.GetStr(paramId, 0, 0);
                        if (code.ToUpperInvariant() == "STANDARD")
                        {
                            material.alphaMode = GLTFMaterial.AlphaMode.OPAQUE.ToString();
                        }
                        else if (code.ToUpperInvariant() == "GLASS")
                        {
                            material.alphaMode = GLTFMaterial.AlphaMode.BLEND.ToString();
                            material.SetBaseColorFactorAlpha(0.4f);
                        }
                        break;
                }
            }
        }
        */
        /* ProcessParamBlock_PARAMS
        void ProcessParamBlock_PARAMS(GLTFMaterial material, IIParamBlock2 paramBlock)
        {
            int numParams = paramBlock.NumParams;
            for (int i = 0; i < numParams; ++i)
            {
                short paramId = paramBlock.IndextoID(i);
                IParamDef def = paramBlock.GetParamDef(paramId);
                ParamType2 type = paramBlock.GetParameterType(paramId);
                switch (def.IntName.ToUpperInvariant())
                {
                    case "BASECOLOR":
                        if (type != ParamType2.Point4)
                            continue;// todo: throw warning

                        IPoint4 baseColor = paramBlock.GetPoint4(paramId, 0, 0);
                        material.SetBaseColorFactor(baseColor.X, baseColor.Y, baseColor.Z);

                        break;
                    case "EMISSIVE":
                        if (type != ParamType2.Point4)
                            continue;// todo: throw warning

                        IPoint4 emissive = paramBlock.GetPoint4(paramId, 0, 0);
                        material.SetEmissiveFactor(emissive.X, emissive.Y, emissive.Z);

                        break;
                    case "ROUGHNESS":
                        if (type != ParamType2.Float)
                            continue;// todo: throw warning

                        float roughness = paramBlock.GetFloat(paramId, 0, 0);
                        material.SetRoughnessFactor(roughness);

                        break;
                    case "METALLIC":
                        if (type != ParamType2.Float)
                            continue;// todo: throw warning

                        float metallic = paramBlock.GetFloat(paramId, 0, 0);
                        material.SetMetallicFactor(metallic);

                        break;
                    case "NORMALSCALE":
                        if (type != ParamType2.Float)
                            continue;// todo: throw warning
                        float normalscale = paramBlock.GetFloat(paramId, 0, 0);
                        // todo, but should move to texture info

                        break;
                }
            }
        }
        */
        /* ProcessParamBlock_TEXTURES
        void ProcessParamBlock_TEXTURES(GLTFMaterial material, IIParamBlock2 paramBlock)
        {
            string texPath;
            GLTFImage image;
            GLTFTextureInfo info;
            
            int numParams = paramBlock.NumParams;
            for (int i = 0; i < numParams; ++i)
            {
                short paramId = paramBlock.IndextoID(i);
                IParamDef def = paramBlock.GetParamDef(paramId);
                
                switch (def.IntName.ToUpperInvariant())
                {
                    case "BASECOLORTEX":
                        {
                            if (def.Type != ParamType2.Filename)
                                continue; // todo: throw warning
                            if (def.AssetTypeId != Autodesk.Max.MaxSDK.AssetManagement.AssetType.BitmapAsset)
                                continue; // todo: throw warning

                            texPath = paramBlock.GetStr(paramId, 0, 0);

                            image = ExportImage(texPath);
                            if (image == null)
                                continue;

                            info = CreateTextureInfo(image);

                            material.SetBaseColorTexture(info);

                            break;
                        }
                    case "OCCLUSIONROUGHNESSMETALLICTEX":
                        {
                            if (def.Type != ParamType2.Filename)
                                continue; // todo: throw warning
                            if (def.AssetTypeId != Autodesk.Max.MaxSDK.AssetManagement.AssetType.BitmapAsset)
                                continue; // todo: throw warning

                            texPath = paramBlock.GetStr(paramId, 0, 0);

                            image = ExportImage(texPath);
                            if (image == null)
                                continue;

                            info = CreateTextureInfo(image);
                            material.SetMetallicRoughnessTexture(info);

                            // same image & sampler, so same GLTFTexture
                            int textureIndex = info.index;
                            info = CreateTextureInfo<GLTFOcclusionTextureInfo>(textureIndex);
                            material.SetOcclusionTexture(info);

                            break;
                        }
                    case "NORMALTEX":

                        {
                            if (def.Type != ParamType2.Filename)
                                continue; // todo: throw warning
                            if (def.AssetTypeId != Autodesk.Max.MaxSDK.AssetManagement.AssetType.BitmapAsset)
                                continue; // todo: throw warning

                            texPath = paramBlock.GetStr(paramId, 0, 0);

                            image = ExportImage(texPath);
                            if (image == null)
                                continue;

                            info = CreateTextureInfo<GLTFNormalTextureInfo>(image);

                            material.SetNormalTexture(info);

                            break;
                        }
                }
            }
        }
        */

        string GetImagePath(IParamDef paramDef, IIGameProperty property, int param_t, string debugName)
        {
            if (paramDef.Type != ParamType2.Filename)
                RaiseError(debugName + " is not a Filename property.");
            if (paramDef.AssetTypeId != Autodesk.Max.MaxSDK.AssetManagement.AssetType.BitmapAsset)
                RaiseError(debugName + " AssetTypeId is not of type BitmapAsset.");
            string string_out = property.MaxParamBlock2?.GetStr(property.ParamID, param_t, 0);

            if (string.IsNullOrWhiteSpace(string_out))
                return null;

            var path = Loader.Global.MaxSDK.Util.Path.Create();
            path.SetPath(string_out);
            string_out = path.ConvertToAbsolute.String;

            if (string.IsNullOrWhiteSpace(string_out))
                return null;

            return string_out;
        }

        GLTFImage ExportImage(string sourceTexturePath, bool allowDDS = false)
        {
            if (string.IsNullOrWhiteSpace(sourceTexturePath))
                return null;

            if (srcTextureExportCache.TryGetValue(sourceTexturePath, out GLTFImage info))
            {
                return info;
            }

            string textureName = Path.GetFileName(sourceTexturePath);
            string ext = Path.GetExtension(sourceTexturePath);

            if (string.IsNullOrWhiteSpace(ext))
                return null;

            ext = ext.Substring(1); // remove the period

            // if dds, export as-is
            if(allowDDS && ext.ToUpperInvariant() == "DDS")
            {
                if (exporter.exportParameters.writeTextures)
                {
                    var destTexturePath = Path.Combine(gltf.OutputFolder, textureName);
                    File.Copy(sourceTexturePath, destTexturePath, true);
                }
            }
            else
            {
                string previousExtension = ext; // substring removes '.'
                string validExtension = tryWriteImageFunc(sourceTexturePath, textureName);

                if (validExtension == null)
                {
                    RaiseWarning("Texture has an invalid extension: " + sourceTexturePath);
                    return null;
                }

                if (previousExtension.ToUpperInvariant() != validExtension.ToUpperInvariant())
                {
                    string message = string.Format("Exported texture {0} was changed from '{1}' to '{2}'", sourceTexturePath, previousExtension, validExtension);
                    RaiseMessage(message);
                }

                textureName = Path.ChangeExtension(textureName, validExtension);
                ext = validExtension;
            }

            if (dstTextureExportCache.TryGetValue(textureName, out string otherTexturePath))
            {
                RaiseWarning("Texture with the exported name already exists and will be re-used or overwritten!");
                RaiseWarning("-> You have referenced a texture with the same name in different folders.");
                RaiseWarning("-- -> Texture: " + textureName);
                RaiseWarning("-- -> Material: " + maxGameMaterial.MaterialName);
                RaiseWarning("-- -> This texture path: " + sourceTexturePath);
                RaiseWarning("-- -> Other texture path: " + otherTexturePath);
            }
            else dstTextureExportCache.Add(textureName, sourceTexturePath);

            info = gltf.AddImage();
            info.uri = textureName;
            info.FileExtension = ext;

            srcTextureExportCache.Add(sourceTexturePath, info);

            return info;
        }
        
        GLTFTextureInfo CreateTextureInfo(GLTFImage image, GLTFSampler sampler = null)
        {
            return CreateTextureInfo<GLTFTextureInfo>(image, sampler);
        }
        T CreateTextureInfo<T>(GLTFImage image, GLTFSampler sampler = null) where T : GLTFTextureInfo, new()
        {
            GLTFTexture texture;
            if (image.FileExtension.ToUpperInvariant() == "DDS")
            {
                // texture object without image
                texture = gltf.AddTexture(null, sampler);

                // texture object for dds extension image reference
                GLTFTexture ddsTexture = new GLTFTexture();
                ddsTexture.sampler = sampler?.index;
                ddsTexture.source = image?.index;


                texture.extensions = new GLTFExtensions();
                texture.extensions.Add(GLTFExtensionHelper.Name_MSFT_texture_dds, ddsTexture);

                if (!gltf.extensionsUsed.Contains(GLTFExtensionHelper.Name_MSFT_texture_dds))
                    gltf.extensionsUsed.Add(GLTFExtensionHelper.Name_MSFT_texture_dds);
            }
            else
            {
                texture = gltf.AddTexture(image, sampler);
            }
            texture.name = image.uri;
            return CreateTextureInfo<T>(texture);
        }
        T CreateTextureInfo<T>(int textureIndex) where T : GLTFTextureInfo, new()
        {
            return CreateTextureInfo<T>(gltf.TexturesList[textureIndex]);
        }
        T CreateTextureInfo<T>(GLTFTexture texture) where T : GLTFTextureInfo, new()
        {
            return new T { index = texture.index };
        }
    }
    
    static class KittyClassExtensions
    {
        public static class Defaults
        {
            public static readonly float[] BaseColorFactor = new float[] { 1, 1, 1, 1 };
            public static readonly float[] EmissiveFactor = new float[] { 0, 0, 0 };
            public const float MetallicFactor = 1.0f;
            public const float RoughnessFactor = 1.0f;

            public const float NormalScale = 1.0f;
            public const float OcclusionStrength = 1.0f;

            public const GLTFMaterial.AlphaMode AlphaMode = GLTFMaterial.AlphaMode.OPAQUE;
            public const float AlphaCutoff = 0.5f;
            public const bool DoubleSided = false;

            public const float BaseColorBlendFactor = 1.0f;
            public const float MetallicBlendFactor = 1.0f;
            public const float RoughnessBlendFactor = 1.0f;
            public const float NormalBlendFactor = 1.0f;
            public const float EmissiveBlendFactor = 1.0f;
            public const float OcclusionBlendFactor = 1.0f;
        }

        #region GLTFMaterial helper functions
        // Pretty much all material parameters are optional, which means a lot of variables (nested) are null or have null parents before we set them.
        // These functions exist to make this a bit easier to read and use the gltf spec defaults when variables have to be initialized.
        // In addition, we try to minimize what we export by setting variables to null when they're defaults.

        #region Textures

        public static void SetBaseColorTexture(this GLTFMaterial material, GLTFTextureInfo info)
        {
            // it's not so easy to undo the output writing, so don't allow this
            if (material.pbrMetallicRoughness != null && material.pbrMetallicRoughness.baseColorTexture != null)
                throw new InvalidOperationException("Base color texture already set.");

            if (material.pbrMetallicRoughness == null)
                material.pbrMetallicRoughness = new GLTFPBRMetallicRoughness();

            material.pbrMetallicRoughness.baseColorTexture = info;
        }
        public static void SetMetallicRoughnessTexture(this GLTFMaterial material, GLTFTextureInfo info)
        {
            // it's not so easy to undo the output writing, so don't allow this
            if (material.pbrMetallicRoughness != null && material.pbrMetallicRoughness.metallicRoughnessTexture != null)
                throw new InvalidOperationException("Metallic/roughness texture already set.");

            if (material.pbrMetallicRoughness == null)
                material.pbrMetallicRoughness = new GLTFPBRMetallicRoughness();

            material.pbrMetallicRoughness.metallicRoughnessTexture = info;
        }
        public static void SetEmissiveTexture(this GLTFMaterial material, GLTFTextureInfo info)
        {
            // it's not so easy to undo the output writing, so don't allow this
            if (material.emissiveTexture != null)
                throw new InvalidOperationException("Emissive texture already set.");

            material.emissiveTexture = info ?? throw new ArgumentNullException(nameof(info));
        }
        public static void SetOcclusionTexture(this GLTFMaterial material, GLTFTextureInfo info)
        {
            // it's not so easy to undo the output writing, so don't allow this
            if (material.occlusionTexture != null)
                throw new InvalidOperationException("Occlusion texture already set.");

            material.occlusionTexture = info ?? throw new ArgumentNullException(nameof(info));
        }
        public static void SetNormalTexture(this GLTFMaterial material, GLTFTextureInfo info)
        {
            // it's not so easy to undo the output writing, so don't allow this
            if (material.normalTexture != null)
                throw new InvalidOperationException("Normal texture already set.");

            material.normalTexture = info ?? throw new ArgumentNullException(nameof(info));
        }

        #endregion

        #region Factors

        public static void SetBaseColorFactorAlpha(this GLTFMaterial gltfMaterial, float alpha)
        {
            if (alpha == Defaults.BaseColorFactor[3])
            {
                if(gltfMaterial.pbrMetallicRoughness == null || gltfMaterial.pbrMetallicRoughness.baseColorFactor == null)
                    return;

                if(gltfMaterial.pbrMetallicRoughness.baseColorFactor[0] == Defaults.BaseColorFactor[0]
                    && gltfMaterial.pbrMetallicRoughness.baseColorFactor[1] == Defaults.BaseColorFactor[1]
                    && gltfMaterial.pbrMetallicRoughness.baseColorFactor[2] == Defaults.BaseColorFactor[2])
                {
                    gltfMaterial.pbrMetallicRoughness.baseColorFactor = null;
                    return;
                }
            }

            if (gltfMaterial.pbrMetallicRoughness == null)
                gltfMaterial.pbrMetallicRoughness = new GLTFPBRMetallicRoughness();

            if (gltfMaterial.pbrMetallicRoughness.baseColorFactor == null)
                gltfMaterial.pbrMetallicRoughness.baseColorFactor = new float[] {
                    Defaults.BaseColorFactor[0], Defaults.BaseColorFactor[1], Defaults.BaseColorFactor[2], alpha };
            else
                gltfMaterial.pbrMetallicRoughness.baseColorFactor[3] = alpha;
        }
        public static void SetBaseColorFactor(this GLTFMaterial gltfMaterial, float r, float g, float b)
        {
            if (r == Defaults.BaseColorFactor[0] && g == Defaults.BaseColorFactor[1] && b == Defaults.BaseColorFactor[2])
            {
                if (gltfMaterial.pbrMetallicRoughness == null || gltfMaterial.pbrMetallicRoughness.baseColorFactor == null)
                    return;

                if (gltfMaterial.pbrMetallicRoughness.baseColorFactor[3] == 1.0f)
                {
                    gltfMaterial.pbrMetallicRoughness.baseColorFactor = null;
                    return;
                }
            }

            if (gltfMaterial.pbrMetallicRoughness == null)
                gltfMaterial.pbrMetallicRoughness = new GLTFPBRMetallicRoughness();

            if (gltfMaterial.pbrMetallicRoughness.baseColorFactor == null)
                gltfMaterial.pbrMetallicRoughness.baseColorFactor = new float[] { r, g, b, Defaults.BaseColorFactor[3] };
            else
            {
                gltfMaterial.pbrMetallicRoughness.baseColorFactor[0] = r;
                gltfMaterial.pbrMetallicRoughness.baseColorFactor[1] = g;
                gltfMaterial.pbrMetallicRoughness.baseColorFactor[2] = b;
            }
        }
        public static void SetEmissiveFactor(this GLTFMaterial gltfMaterial, float r, float g, float b)
        {
            if (r == Defaults.EmissiveFactor[0] && g == Defaults.EmissiveFactor[1] && b == Defaults.EmissiveFactor[2])
            {
                gltfMaterial.emissiveFactor = null;
                return;
            }

            if (gltfMaterial.emissiveFactor == null)
            {
                gltfMaterial.emissiveFactor = new float[] { r, g, b };
            }
            else
            {
                gltfMaterial.emissiveFactor[0] = r;
                gltfMaterial.emissiveFactor[1] = g;
                gltfMaterial.emissiveFactor[2] = b;
            }
        }
        public static void SetRoughnessFactor(this GLTFMaterial gltfMaterial, float roughness)
        {
            if (roughness == Defaults.RoughnessFactor)
            {
                if (gltfMaterial.pbrMetallicRoughness == null)
                    return;

                gltfMaterial.pbrMetallicRoughness.roughnessFactor = null;
                return;
            }

            if (gltfMaterial.pbrMetallicRoughness == null)
                gltfMaterial.pbrMetallicRoughness = new GLTFPBRMetallicRoughness();
            
            gltfMaterial.pbrMetallicRoughness.roughnessFactor = roughness;
        }
        public static void SetMetallicFactor(this GLTFMaterial gltfMaterial, float metallic)
        {
            if (metallic == Defaults.MetallicFactor)
            {
                if (gltfMaterial.pbrMetallicRoughness == null)
                    return;

                gltfMaterial.pbrMetallicRoughness.metallicFactor = null;
                return;
            }

            if (gltfMaterial.pbrMetallicRoughness == null)
                gltfMaterial.pbrMetallicRoughness = new GLTFPBRMetallicRoughness();

            gltfMaterial.pbrMetallicRoughness.metallicFactor = metallic;
        }

        #endregion

        public static void SetNormalScale(this GLTFMaterial gltfMaterial, float normalScale)
        {
            GLTFNormalTextureInfo normalTexture = (GLTFNormalTextureInfo)gltfMaterial.normalTexture;
            normalTexture.scale = normalScale == Defaults.NormalScale ? null : (float?)normalScale;
        }
        public static void SetOcclusionStrength(this GLTFMaterial gltfMaterial, float occlusionStrength)
        {
            GLTFOcclusionTextureInfo normalTexture = (GLTFOcclusionTextureInfo)gltfMaterial.normalTexture;
            normalTexture.strength = occlusionStrength == Defaults.OcclusionStrength ? null : (float?)occlusionStrength;
        }

        public static void SetAlphaCutoff(this GLTFMaterial gltfMaterial, float alphaCutoff)
        {
            gltfMaterial.alphaCutoff = alphaCutoff == Defaults.AlphaCutoff ? null : (float?)alphaCutoff;
        }
        public static void SetAlphaMode(this GLTFMaterial gltfMaterial, string alphaMode)
        {
            gltfMaterial.alphaMode = alphaMode == Defaults.AlphaMode.ToString() ? null : alphaMode;
        }
        public static void SetDoubleSided(this GLTFMaterial gltfMaterial, bool doubleSided)
        {
            gltfMaterial.doubleSided = doubleSided;
        }

        #endregion

        #region Decal Extension helper functions
        
        public static void SetBaseColorBlendFactor(this GLTFExtensionAsoboMaterialDecal decalMaterial, float factor)
        {
            if (factor == Defaults.BaseColorBlendFactor)
                decalMaterial.baseColorBlendFactor = null;
            else decalMaterial.baseColorBlendFactor = factor;
        }
        public static void SetRoughnessBlendFactor(this GLTFExtensionAsoboMaterialDecal decalMaterial, float factor)
        {
            if (factor == Defaults.RoughnessBlendFactor)
                decalMaterial.roughnessBlendFactor = null;
            else decalMaterial.roughnessBlendFactor = factor;
        }
        public static void SetMetallicBlendFactor(this GLTFExtensionAsoboMaterialDecal decalMaterial, float factor)
        {
            if (factor == Defaults.MetallicBlendFactor)
                decalMaterial.metallicBlendFactor = null;
            else decalMaterial.metallicBlendFactor = factor;
        }
        public static void SetNormalBlendFactor(this GLTFExtensionAsoboMaterialDecal decalMaterial, float factor)
        {
            if (factor == Defaults.NormalBlendFactor)
                decalMaterial.normalBlendFactor = null;
            else decalMaterial.normalBlendFactor = factor;
        }
        public static void SetEmissiveBlendFactor(this GLTFExtensionAsoboMaterialDecal decalMaterial, float factor)
        {
            if (factor == Defaults.EmissiveBlendFactor)
                decalMaterial.emissiveBlendFactor = null;
            else decalMaterial.emissiveBlendFactor = factor;
        }
        public static void SetOcclusionBlendFactor(this GLTFExtensionAsoboMaterialDecal decalMaterial, float factor)
        {
            if (factor == Defaults.OcclusionBlendFactor)
                decalMaterial.occlusionBlendFactor = null;
            else decalMaterial.occlusionBlendFactor = factor;
        }

        #endregion
    }

    //public class KittyHawkMaterial : Autodesk.Max.Plugins.MtlBase
    //{
    //
    //}
}