using Autodesk.Max;
using BabylonExport.Entities;
using GLTFExport.Entities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Max2Babylon
{
    public class KittyHawkMaterialExporter : IGLTFMaterialExporter
    {
        readonly ClassIDWrapper class_ID = new ClassIDWrapper(0x53196aaa, 0x57b6ad6a);

        ClassIDWrapper IMaterialExporter.MaterialClassID => class_ID;
        
        public KittyHawkMaterialExporter() { }

        // getValueByNameUsingParamBlock2Internal(*pMtl,_T("DiffuseTex"),TYPE_FILENAME,&Filename,TV);

        GLTF gltf;
        IIGameMaterial maxGameMaterial;
        Func<string, string, string> tryWriteImageFunc;

        GLTFMaterial IGLTFMaterialExporter.ExportGLTFMaterial(GLTF gltf, IIGameMaterial maxGameMaterial, Func<string, string, string> tryWriteImageFunc)
        {
            this.gltf = gltf;
            this.maxGameMaterial = maxGameMaterial;
            this.tryWriteImageFunc = tryWriteImageFunc;

            GLTFMaterial gltfMaterial = new GLTFMaterial();

            gltfMaterial.name = maxGameMaterial.MaterialName;
            gltfMaterial.id = maxGameMaterial.MaxMaterial.GetGuid().ToString();
                        
            ProcessParamBlocks(gltfMaterial, maxGameMaterial);

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

        void ProcessParamBlock_PARAMS2(GLTFMaterial material, IIParamBlock2 paramBlock)
        {

        }

        void ProcessParamBlock_TEXTURES(GLTFMaterial material, IIParamBlock2 paramBlock)
        {
            string texName;

            int numParams = paramBlock.NumParams;
            for (int i = 0; i < numParams; ++i)
            {
                short paramId = paramBlock.IndextoID(i);
                IParamDef def = paramBlock.GetParamDef(paramId);
                ParamType2 type = paramBlock.GetParameterType(paramId);
                
                switch (def.IntName.ToUpperInvariant())
                {
                    case "BASECOLORTEX":
                        if (def.Type != ParamType2.Filename)
                            continue;
                        if (type != ParamType2.Filename)
                            continue;// todo: throw warning
                        if (def.AssetTypeId != Autodesk.Max.MaxSDK.AssetManagement.AssetType.BitmapAsset)
                            continue;

                        string baseColorTex = paramBlock.GetStr(paramId, 0, 0);
                        texName = ValidateAndOutputTexture(baseColorTex);
                        if (texName == null)
                            continue;

                        material.SetBaseColorTexture(gltf, texName);

                        break;
                    case "OCCLUSIONROUGHNESSMETALLICTEX":
                        if (type != ParamType2.Point4)
                            continue;// todo: throw warning

                        string occRoughMetalTex = paramBlock.GetStr(paramId, 0, 0);
                        texName = ValidateAndOutputTexture(occRoughMetalTex);
                        if (texName == null)
                            continue;

                        // same texture, different channels
                        // todo: re-use GLTFImage and GLTFTexture
                        material.SetMetallicRoughnessTexture(gltf, texName);
                        material.SetOcclusionTexture(gltf, texName);

                        break;
                    case "NORMALTEX":
                        if (type != ParamType2.Float)
                            continue;// todo: throw warning

                        string normalTex = paramBlock.GetStr(paramId, 0, 0);
                        texName = ValidateAndOutputTexture(normalTex);
                        if (texName == null)
                            continue;

                        material.SetNormalTexture(gltf, texName);

                        break;
                }
            }
        }

        void ProcessParamBlock_FLAGS(GLTFMaterial material, IIParamBlock2 paramBlock)
        {
            
        }

        string ValidateAndOutputTexture(string sourceTexturePath)
        {
            string textureName = Path.GetFileName(sourceTexturePath);
            string validExtension = tryWriteImageFunc(sourceTexturePath, textureName);

            if (validExtension == null)
                return null;

            textureName = Path.ChangeExtension(textureName, validExtension);

            return textureName;
        }
    }

    static class GLTFMaterialExtensions
    {
        public static class Defaults
        {
            public static readonly float[] BaseColorFactor = new float[] { 1, 1, 1, 1 };
            public static readonly float[] EmissiveFactor = new float[] { 0, 0, 0 };
            public const float RoughnessFactor = 1.0f;
            public const float MetallicFactor = 1.0f;
        }

        #region GLTFMaterial helper functions
        // Pretty much all material parameters are optional, which means a lot of variables (nested) are null or have null parents before we set them.
        // These functions exist to make this a bit easier to read and use the gltf spec defaults when variables have to be initialized.
        // In addition, we try to minimize what we export by setting variables to null when they're defaults.

        #region Textures

        public static GLTFTextureInfo AddBasicTextureInfo(GLTF gltf, string texName)
        {
            GLTFImage image = gltf.AddImage();
            image.uri = texName;
            image.FileExtension = Path.GetExtension(texName);

            GLTFTexture texture = gltf.AddTexture(null, image);
            texture.name = texName;

            GLTFTextureInfo textureInfo = new GLTFTextureInfo();
            textureInfo.index = texture.index;

            return textureInfo;
        }

        public static void SetBaseColorTexture(this GLTFMaterial material, GLTF gltf, string texName)
        {
            // it's not so easy to undo the output writing, so don't allow this
            if (material.pbrMetallicRoughness.baseColorTexture != null)
                throw new InvalidOperationException("Base color texture already set.");

            if (string.IsNullOrWhiteSpace(texName))
            {
                if (material.pbrMetallicRoughness == null || material.pbrMetallicRoughness.baseColorTexture == null)
                    return;

                material.pbrMetallicRoughness.baseColorTexture = null;
            }

            if (material.pbrMetallicRoughness == null)
                material.pbrMetallicRoughness = new GLTFPBRMetallicRoughness();

            material.pbrMetallicRoughness.baseColorTexture = AddBasicTextureInfo(gltf, texName);
        }

        public static void SetMetallicRoughnessTexture(this GLTFMaterial material, GLTF gltf, string texName)
        {
            // it's not so easy to undo the output writing, so don't allow this
            if (material.pbrMetallicRoughness.metallicRoughnessTexture != null)
                throw new InvalidOperationException("Metallic/Roughness texture already set.");

            if (string.IsNullOrWhiteSpace(texName))
            {
                if (material.pbrMetallicRoughness == null || material.pbrMetallicRoughness.metallicRoughnessTexture == null)
                    return;

                material.pbrMetallicRoughness.metallicRoughnessTexture = null;
            }

            if (material.pbrMetallicRoughness == null)
                material.pbrMetallicRoughness = new GLTFPBRMetallicRoughness();

            material.pbrMetallicRoughness.metallicRoughnessTexture = AddBasicTextureInfo(gltf, texName);
        }

        public static void SetEmissiveTexture(this GLTFMaterial material, GLTF gltf, string texName)
        {
            // it's not so easy to undo the output writing, so don't allow this
            if (material.emissiveTexture != null)
                throw new InvalidOperationException("Metallic/Roughness texture already set.");

            if (string.IsNullOrWhiteSpace(texName))
                return;

            material.emissiveTexture = AddBasicTextureInfo(gltf, texName);
        }

        public static void SetOcclusionTexture(this GLTFMaterial material, GLTF gltf, string texName)
        {
            // it's not so easy to undo the output writing, so don't allow this
            if (material.occlusionTexture != null)
                throw new InvalidOperationException("Metallic/Roughness texture already set.");

            if (string.IsNullOrWhiteSpace(texName))
                return;

            material.occlusionTexture = AddBasicTextureInfo(gltf, texName);
        }

        public static void SetNormalTexture(this GLTFMaterial material, GLTF gltf, string texName)
        {
            // it's not so easy to undo the output writing, so don't allow this
            if (material.normalTexture != null)
                throw new InvalidOperationException("Metallic/Roughness texture already set.");

            if (string.IsNullOrWhiteSpace(texName))
                return;

            material.normalTexture = AddBasicTextureInfo(gltf, texName);
        }

        #endregion

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
    }


    //public class KittyHawkMaterial : Autodesk.Max.Plugins.MtlBase
    //{
    //
    //}
}