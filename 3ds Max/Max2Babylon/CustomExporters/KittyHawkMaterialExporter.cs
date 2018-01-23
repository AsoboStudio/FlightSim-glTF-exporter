using Autodesk.Max;
using BabylonExport.Entities;
using GLTFExport.Entities;

namespace Max2Babylon
{
    public class KittyHawkMaterialExporter : IGLTFMaterialExporter
    {
        readonly ClassIDWrapper class_ID = new ClassIDWrapper(0x53196aaa, 0x57b6ad6a);

        ClassIDWrapper IMaterialExporter.MaterialClassID => class_ID;

        //bool IMaterialExporter.IsBabylonExporter => true;
        //
        //bool IMaterialExporter.IsGltfExporter => false;

        public KittyHawkMaterialExporter() { }

        // getValueByNameUsingParamBlock2Internal(*pMtl,_T("DiffuseTex"),TYPE_FILENAME,&Filename,TV);

        GLTFMaterial IGLTFMaterialExporter.ExportGLTFMaterial(IIGameMaterial material)
        {
            GLTFMaterial gltfMaterial = new GLTFMaterial();

            gltfMaterial.name = material.MaterialName;
            gltfMaterial.id = material.MaxMaterial.GetGuid().ToString();

            int numProps = material.IPropertyContainer.NumberOfProperties;
            IIGameProperty[] properties = new IIGameProperty[numProps];
            for (int i = 0; i < numProps; ++i)
            {
                IIGameProperty property = material.IPropertyContainer.GetProperty(i);
                properties[i] = property;
            }

            int numParamBlocks = material.MaxMaterial.NumParamBlocks;
            IIParamBlock2[] paramBlocks = new IIParamBlock2[numParamBlocks];
            for (int i = 0; i < numParamBlocks; ++i)
            {
                IIParamBlock2 paramBlock = material.MaxMaterial.GetParamBlock(i);
                paramBlocks[i] = paramBlock;
            }
            
            ProcessParamBlocks(gltfMaterial, material);

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

        void ProcessParamBlocks(GLTFMaterial babylonMaterial, IIGameMaterial material)
        {
            int numParamBlocks = material.MaxMaterial.NumParamBlocks;
            for (int i = 0; i < numParamBlocks; ++i)
            {
                IIParamBlock2 paramBlock = material.MaxMaterial.GetParamBlock(i);
                switch(paramBlock.LocalName.ToUpperInvariant())
                {
                    case "SHADER":
                        ProcessParamBlock_SHADER(babylonMaterial, paramBlock);
                        break;
                    case "PARAMS":
                        ProcessParamBlock_PARAMS(babylonMaterial, paramBlock);
                        break;
                    case "PARAMS2":
                        ProcessParamBlock_PARAMS2(babylonMaterial, paramBlock);
                        break;
                    case "TEXTURES":
                        ProcessParamBlock_TEXTURES(babylonMaterial, paramBlock);
                        break;
                    case "FLAGS":
                        ProcessParamBlock_FLAGS(babylonMaterial, paramBlock);
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
                            material.SetBaseColorFactorAlpha(0.3f);
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
            int numParams = paramBlock.NumParams;
            for (int i = 0; i < numParams; ++i)
            {
                short paramId = paramBlock.IndextoID(i);
                IParamDef def = paramBlock.GetParamDef(paramId);
                ParamType2 type = paramBlock.GetParameterType(paramId);
                switch (def.IntName.ToUpperInvariant())
                {
                    case "BASECOLORTEX":
                        if (type != ParamType2.Bitmap)
                            continue;// todo: throw warning

                        IPBBitmap baseColor = paramBlock.GetBitmap(paramId, 0, 0);
                        // todo
                        break;
                    case "OCCLUSIONROUGHNESSMETALLICTEX":
                        if (type != ParamType2.Point4)
                            continue;// todo: throw warning

                        IPBBitmap orm = paramBlock.GetBitmap(paramId, 0, 0);
                        // todo
                        break;
                    case "NORMALTEX":
                        if (type != ParamType2.Float)
                            continue;// todo: throw warning
                        IPBBitmap normalTex = paramBlock.GetBitmap(paramId, 0, 0);
                        // todo
                        break;
                }
            }
        }
        void ProcessParamBlock_FLAGS(GLTFMaterial material, IIParamBlock2 paramBlock)
        {
            
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

        public static void SetBaseColorFactorAlpha(this GLTFMaterial gltfMaterial, float alpha)
        {
            if (alpha == Defaults.EmissiveFactor[3])
            {
                if(gltfMaterial.pbrMetallicRoughness == null || gltfMaterial.pbrMetallicRoughness.baseColorFactor == null)
                    return;

                if(gltfMaterial.pbrMetallicRoughness.baseColorFactor[0] == Defaults.EmissiveFactor[0]
                    && gltfMaterial.pbrMetallicRoughness.baseColorFactor[1] == Defaults.EmissiveFactor[1]
                    && gltfMaterial.pbrMetallicRoughness.baseColorFactor[2] == Defaults.EmissiveFactor[2])
                {
                    gltfMaterial.pbrMetallicRoughness.baseColorFactor = null;
                    return;
                }
            }

            if (gltfMaterial.pbrMetallicRoughness == null)
                gltfMaterial.pbrMetallicRoughness = new GLTFPBRMetallicRoughness();

            if (gltfMaterial.pbrMetallicRoughness.baseColorFactor == null)
                gltfMaterial.pbrMetallicRoughness.baseColorFactor = new float[] {
                    Defaults.EmissiveFactor[0], Defaults.EmissiveFactor[1], Defaults.EmissiveFactor[2], alpha };
            else
                gltfMaterial.pbrMetallicRoughness.baseColorFactor[3] = alpha;
        }

        public static void SetBaseColorFactor(this GLTFMaterial gltfMaterial, float r, float g, float b)
        {
            if (r == Defaults.EmissiveFactor[0] && g == Defaults.EmissiveFactor[1] && b == Defaults.EmissiveFactor[2])
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
                gltfMaterial.pbrMetallicRoughness.baseColorFactor = new float[] { r, g, b, Defaults.EmissiveFactor[3] };
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