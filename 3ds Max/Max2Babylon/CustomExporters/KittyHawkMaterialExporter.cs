using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    public class KittyHawkMaterialExporter : IMaterialExporter
    {
        readonly ClassIDWrapper class_ID = new ClassIDWrapper(0x53196aaa, 0x57b6ad6a);

        ClassIDWrapper IMaterialExporter.MaterialClassID => class_ID;

        bool IMaterialExporter.IsBabylonExporter => true;

        bool IMaterialExporter.IsGltfExporter => false;

        public KittyHawkMaterialExporter() { }

        // getValueByNameUsingParamBlock2Internal(*pMtl,_T("DiffuseTex"),TYPE_FILENAME,&Filename,TV);

        BabylonMaterial IMaterialExporter.ExportBabylonMaterial(IIGameMaterial material)
        {
            BabylonPBRMetallicRoughnessMaterial babylonMaterial = new BabylonPBRMetallicRoughnessMaterial();

            babylonMaterial.name = material.MaterialName;
            babylonMaterial.id = material.MaxMaterial.GetGuid().ToString();

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

            ProcessParamBlocks(babylonMaterial, material);

            return babylonMaterial;

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

        void ProcessParamBlocks(BabylonPBRMetallicRoughnessMaterial babylonMaterial, IIGameMaterial material)
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

        void ProcessParamBlock_SHADER(BabylonPBRMetallicRoughnessMaterial material, IIParamBlock2 paramBlock)
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

                        }
                        else if (code.ToUpperInvariant() == "GLASS")
                        {
                            material.alpha = 0.3f;
                        }
                        break;
                }
            }
        }

        void ProcessParamBlock_PARAMS(BabylonPBRMetallicRoughnessMaterial material, IIParamBlock2 paramBlock)
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
                        material.baseColor = new[] { baseColor.X, baseColor.Y, baseColor.Z };

                        break;
                    case "EMISSIVE":
                        if (type != ParamType2.Point4)
                            continue;// todo: throw warning

                        IPoint4 emissive = paramBlock.GetPoint4(paramId, 0, 0);
                        material.emissive = new[] { emissive.X, emissive.Y, emissive.Z };

                        break;
                    case "ROUGHNESS":
                        if (type != ParamType2.Float)
                            continue;// todo: throw warning

                        float roughness = paramBlock.GetFloat(paramId, 0, 0);
                        material.roughness = roughness;

                        break;
                    case "METALLIC":
                        if (type != ParamType2.Float)
                            continue;// todo: throw warning

                        float metallic = paramBlock.GetFloat(paramId, 0, 0);
                        material.metallic = metallic;

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
        void ProcessParamBlock_PARAMS2(BabylonPBRMetallicRoughnessMaterial material, IIParamBlock2 paramBlock)
        {

        }
        void ProcessParamBlock_TEXTURES(BabylonPBRMetallicRoughnessMaterial material, IIParamBlock2 paramBlock)
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
        void ProcessParamBlock_FLAGS(BabylonPBRMetallicRoughnessMaterial material, IIParamBlock2 paramBlock)
        {

        }
    }


    //public class KittyHawkMaterial : Autodesk.Max.Plugins.MtlBase
    //{
    //
    //}
}