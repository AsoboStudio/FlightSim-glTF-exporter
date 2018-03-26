﻿using System;
using System.Collections.Generic;
using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        readonly List<IIGameMaterial> referencedMaterials = new List<IIGameMaterial>();
        Dictionary<ClassIDWrapper, IMaterialExporter> materialExporters;

        private void ExportMaterial(IIGameMaterial materialNode, BabylonScene babylonScene)
        {
            var name = materialNode.MaterialName;
            var id = materialNode.MaxMaterial.GetGuid().ToString();

            RaiseMessage(name, 1);

            // --- prints ---
            #region prints
            {
                RaiseVerbose("materialNode.MaterialClass=" + materialNode.MaterialClass, 2);
                RaiseVerbose("materialNode.NumberOfTextureMaps=" + materialNode.NumberOfTextureMaps, 2);

                var propertyContainer = materialNode.IPropertyContainer;
                RaiseVerbose("propertyContainer=" + propertyContainer, 2);
                if (propertyContainer != null)
                {
                    RaiseVerbose("propertyContainer.NumberOfProperties=" + propertyContainer.NumberOfProperties, 3);
                    for (int i = 0; i < propertyContainer.NumberOfProperties; i++)
                    {
                        var prop = propertyContainer.GetProperty(i);
                        if (prop != null)
                        {
                            RaiseVerbose("propertyContainer.GetProperty(" + i + ")=" + prop.Name, 3);
                            switch (prop.GetType_)
                            {
                                case PropType.StringProp:
                                    string propertyString = "";
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyString, 0)=" + prop.GetPropertyValue(ref propertyString, 0), 4);
                                    RaiseVerbose("propertyString=" + propertyString, 4);
                                    break;
                                case PropType.IntProp:
                                    int propertyInt = 0;
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyInt, 0)=" + prop.GetPropertyValue(ref propertyInt, 0), 4);
                                    RaiseVerbose("propertyInt=" + propertyInt, 4);
                                    break;
                                case PropType.FloatProp:
                                    float propertyFloat = 0;
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyFloat, 0)=" + prop.GetPropertyValue(ref propertyFloat, 0, true), 4);
                                    RaiseVerbose("propertyFloat=" + propertyFloat, 4);
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyFloat, 0)=" + prop.GetPropertyValue(ref propertyFloat, 0, false), 4);
                                    RaiseVerbose("propertyFloat=" + propertyFloat, 4);
                                    break;
                                case PropType.Point3Prop:
                                    IPoint3 propertyPoint3 = Loader.Global.Point3.Create(0, 0, 0); 
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyPoint3, 0)=" + prop.GetPropertyValue(propertyPoint3, 0), 4);
                                    RaiseVerbose("propertyPoint3=" + Point3ToString(propertyPoint3), 4);
                                    break;
                                case PropType.Point4Prop:
                                    IPoint4 propertyPoint4 = Loader.Global.Point4.Create(0,0,0,0);
                                    RaiseVerbose("prop.GetPropertyValue(ref propertyPoint4, 0)=" + prop.GetPropertyValue(propertyPoint4, 0), 4);
                                    RaiseVerbose("propertyPoint4=" + Point4ToString(propertyPoint4), 4);
                                    break;
                                case PropType.UnknownProp:
                                default:
                                    RaiseVerbose("Unknown property type", 4);
                                    break;
                            }
                        }
                        else
                        {
                            RaiseVerbose("propertyContainer.GetProperty(" + i + ") IS NULL", 3);
                        }
                    }
                }
            }
            #endregion

            if (materialNode.SubMaterialCount > 0)
            {
                var babylonMultimaterial = new BabylonMultiMaterial { name = name, id = id };

                var guids = new List<string>();

                for (var index = 0; index < materialNode.SubMaterialCount; index++)
                {
                    var subMat = materialNode.GetSubMaterial(index);

                    if (subMat != null)
                    {
                        guids.Add(subMat.MaxMaterial.GetGuid().ToString());

                        if (!referencedMaterials.Contains(subMat))
                        {
                            referencedMaterials.Add(subMat);
                            ExportMaterial(subMat, babylonScene);
                        }
                    }
                    else
                    {
                        guids.Add(Guid.Empty.ToString());
                    }
                }

                babylonMultimaterial.materials = guids.ToArray();

                babylonScene.MultiMaterialsList.Add(babylonMultimaterial);
                return;
            }

            // check custom exporters first, to allow custom exporters of supported material classes
            materialExporters.TryGetValue(new ClassIDWrapper(materialNode.MaxMaterial.ClassID), out IMaterialExporter materialExporter);
            var stdMat = materialNode.MaxMaterial.GetParamBlock(0).Owner as IStdMat2;

            if (isBabylonExported && materialExporter != null && materialExporter is IBabylonMaterialExporter)
            {
                IBabylonMaterialExporter babylonMaterialExporter = materialExporter as IBabylonMaterialExporter;
                BabylonMaterial babylonMaterial = babylonMaterialExporter.ExportBabylonMaterial(materialNode);
                if (babylonMaterial == null)
                {
                    string message = string.Format("Custom Babylon material exporter failed to export | Exporter: '{0}' | Material Name: '{1}' | Material Class: '{2}'",
                        babylonMaterialExporter.GetType().ToString(), materialNode.MaterialName, materialNode.MaterialClass);
                    RaiseWarning(message, 2);
                }
                else babylonScene.MaterialsList.Add(babylonMaterial);
            }
            else if (isGLTFExported && materialExporter != null && materialExporter is IGLTFMaterialExporter)
            {
                // add a basic babylon material to the list to forward the max material reference
                var babylonMaterial = new BabylonMaterial
                {
                    maxGameMaterial = materialNode,
                    name = name,
                    id = id
                };
                babylonScene.MaterialsList.Add(babylonMaterial);
            }
            else if (stdMat != null)
            {
                var isSelfIllumColor = materialNode.MaxMaterial.GetSelfIllumColorOn(0, false);

                var babylonMaterial = new BabylonStandardMaterial()
                {
                    maxGameMaterial = materialNode,
                    name = name,
                    id = id,
                    ambient = materialNode.MaxMaterial.GetAmbient(0, false).ToArray(),
                    diffuse = materialNode.MaxMaterial.GetDiffuse(0, false).ToArray(),
                    specular = materialNode.MaxMaterial.GetSpecular(0, false).Scale(materialNode.MaxMaterial.GetShinStr(0, false)),
                    specularPower = materialNode.MaxMaterial.GetShininess(0, false) * 256,
                    emissive =
                        isSelfIllumColor
                            ? materialNode.MaxMaterial.GetSelfIllumColor(0, false).ToArray()
                            : materialNode.MaxMaterial.GetDiffuse(0, false).Scale(materialNode.MaxMaterial.GetSelfIllum(0, false)), // compute the pre-multiplied emissive color
                    alpha = 1.0f - materialNode.MaxMaterial.GetXParency(0, false)
                };

                // If Self-Illumination color checkbox is checked
                // Then self-illumination is assumed to be pre-multiplied
                // Otherwise self-illumination needs to be multiplied with diffuse
                // linkEmissiveWithDiffuse attribute tells the Babylon engine to perform such multiplication
                babylonMaterial.linkEmissiveWithDiffuse = !isSelfIllumColor;
                // useEmissiveAsIllumination attribute tells the Babylon engine to use pre-multiplied emissive as illumination
                babylonMaterial.useEmissiveAsIllumination = isSelfIllumColor;

                // Store the emissive value (before multiplication) for gltf
                babylonMaterial.selfIllum = materialNode.MaxMaterial.GetSelfIllum(0, false);

                babylonMaterial.backFaceCulling = !stdMat.TwoSided;
                babylonMaterial.wireframe = stdMat.Wire;

                // Textures
                BabylonFresnelParameters fresnelParameters;

                babylonMaterial.ambientTexture = ExportTexture(stdMat, 0, out fresnelParameters, babylonScene);                // Ambient
                babylonMaterial.diffuseTexture = ExportTexture(stdMat, 1, out fresnelParameters, babylonScene);                // Diffuse
                if (fresnelParameters != null)
                {
                    babylonMaterial.diffuseFresnelParameters = fresnelParameters;
                }

                babylonMaterial.specularTexture = ExportTexture(stdMat, 2, out fresnelParameters, babylonScene);               // Specular
                babylonMaterial.emissiveTexture = ExportTexture(stdMat, 5, out fresnelParameters, babylonScene);               // Emissive
                if (fresnelParameters != null)
                {
                    babylonMaterial.emissiveFresnelParameters = fresnelParameters;
                    if (babylonMaterial.emissive[0] == 0 &&
                        babylonMaterial.emissive[1] == 0 &&
                        babylonMaterial.emissive[2] == 0 &&
                        babylonMaterial.emissiveTexture == null)
                    {
                        babylonMaterial.emissive = new float[] { 1, 1, 1 };
                    }
                }

                babylonMaterial.opacityTexture = ExportTexture(stdMat, 6, out fresnelParameters, babylonScene, false, true);   // Opacity
                if (fresnelParameters != null)
                {
                    babylonMaterial.opacityFresnelParameters = fresnelParameters;
                    if (babylonMaterial.alpha == 1 &&
                         babylonMaterial.opacityTexture == null)
                    {
                        babylonMaterial.alpha = 0;
                    }
                }

                babylonMaterial.bumpTexture = ExportTexture(stdMat, 8, out fresnelParameters, babylonScene);                   // Bump
                babylonMaterial.reflectionTexture = ExportTexture(stdMat, 9, out fresnelParameters, babylonScene, true);       // Reflection
                if (fresnelParameters != null)
                {
                    if (babylonMaterial.reflectionTexture == null)
                    {
                        RaiseWarning("Fallout cannot be used with reflection channel without a texture", 2);
                    }
                    else
                    {
                        babylonMaterial.reflectionFresnelParameters = fresnelParameters;
                    }
                }

                // Constraints
                if (babylonMaterial.diffuseTexture != null)
                {
                    babylonMaterial.diffuse = new[] { 1.0f, 1.0f, 1.0f };
                }

                if (babylonMaterial.emissiveTexture != null)
                {
                    babylonMaterial.emissive = new float[] { 0, 0, 0 };
                }

                if (babylonMaterial.opacityTexture != null && babylonMaterial.diffuseTexture != null &&
                    babylonMaterial.diffuseTexture.name == babylonMaterial.opacityTexture.name &&
                    babylonMaterial.diffuseTexture.hasAlpha && !babylonMaterial.opacityTexture.getAlphaFromRGB)
                {
                    // This is a alpha testing purpose
                    babylonMaterial.opacityTexture = null;
                    babylonMaterial.diffuseTexture.hasAlpha = true;
                    RaiseWarning("Opacity texture was removed because alpha from diffuse texture can be use instead", 2);
                    RaiseWarning("If you do not want this behavior, just set Alpha Source = None on your diffuse texture", 2);
                }

                babylonScene.MaterialsList.Add(babylonMaterial);
            }
            // TODO - Find another way to detect if material is physical
            else if (materialNode.MaterialClass.ToLower() == "physical material" || // English
                     materialNode.MaterialClass.ToLower() == "physisches material" || // German // TODO - check if translation is ok
                     materialNode.MaterialClass.ToLower() == "matériau physique") // French
            {
                var propertyContainer = materialNode.IPropertyContainer;

                var babylonMaterial = new BabylonPBRMetallicRoughnessMaterial
                {
                    maxGameMaterial = materialNode,
                    name = name,
                    id = id
                };

                // --- Global ---

                // Alpha
                //var alphaFromXParency = 1.0f - materialNode.MaxMaterial.GetXParency(0, false);
                var alphaFromPropertyContainer = 1.0f - propertyContainer.GetFloatProperty(17);
                //RaiseMessage("alphaFromXParency=" + alphaFromXParency, 2);
                //RaiseMessage("alphaFromPropertyContainer=" + alphaFromPropertyContainer, 2);
                babylonMaterial.alpha = alphaFromPropertyContainer;

                babylonMaterial.baseColor = materialNode.MaxMaterial.GetDiffuse(0, false).ToArray();

                babylonMaterial.metallic = propertyContainer.GetFloatProperty(6);

                babylonMaterial.roughness = propertyContainer.GetFloatProperty(4);
                var invertRoughness = propertyContainer.GetBoolProperty(5);
                if (invertRoughness)
                {
                    // Inverse roughness
                    babylonMaterial.roughness = 1 - babylonMaterial.roughness;
                }

                // Self illumination is computed from emission color, luminance, temperature and weight
                babylonMaterial.emissive = materialNode.MaxMaterial.GetSelfIllumColorOn(0, false)
                                                ? materialNode.MaxMaterial.GetSelfIllumColor(0, false).ToArray()
                                                : materialNode.MaxMaterial.GetDiffuse(0, false).Scale(materialNode.MaxMaterial.GetSelfIllum(0, false));

                // --- Textures ---

                babylonMaterial.baseTexture = ExportBaseColorAlphaTexture(materialNode, babylonMaterial.baseColor, babylonMaterial.alpha, babylonScene, name);

                babylonMaterial.metallicRoughnessTexture = ExportMetallicRoughnessTexture(materialNode, babylonMaterial.metallic, babylonMaterial.roughness, babylonScene, name, invertRoughness);

                var normalMapAmount = propertyContainer.GetFloatProperty(91);
                babylonMaterial.normalTexture = ExportPBRTexture(materialNode, 30, babylonScene, normalMapAmount);

                babylonMaterial.emissiveTexture = ExportPBRTexture(materialNode, 17, babylonScene);

                // Use diffuse roughness map as ambient occlusion
                babylonMaterial.occlusionTexture = ExportPBRTexture(materialNode, 6, babylonScene);

                // Constraints
                if (babylonMaterial.baseTexture != null)
                {
                    babylonMaterial.baseColor = new[] { 1.0f, 1.0f, 1.0f };
                    babylonMaterial.alpha = 1.0f;
                }

                if (babylonMaterial.alpha != 1.0f || (babylonMaterial.baseTexture != null && babylonMaterial.baseTexture.hasAlpha))
                {
                    babylonMaterial.transparencyMode = (int)BabylonPBRMetallicRoughnessMaterial.TransparencyMode.ALPHABLEND;
                }

                if (babylonMaterial.emissiveTexture != null)
                {
                    babylonMaterial.emissive = new[] { 1.0f, 1.0f, 1.0f };
                }

                if (babylonMaterial.metallicRoughnessTexture != null)
                {
                    babylonMaterial.metallic = 1.0f;
                    babylonMaterial.roughness = 1.0f;
                }

                babylonScene.MaterialsList.Add(babylonMaterial);
            }
            else
            {
                RaiseWarning("Unsupported material type: " + materialNode.MaterialClass, 2);
            }
        }

        // -------------------------
        // --------- Utils ---------
        // -------------------------

        private string ColorToString(IColor color)
        {
            if (color == null)
            {
                return "";
            }

            return "{ r=" + color.R + ", g=" + color.G + ", b=" + color.B + " }";
        }

        private string Point3ToString(IPoint3 point)
        {
            if (point == null)
            {
                return "";
            }

            return "{ x=" + point.X + ", y=" + point.Y + ", z=" + point.Z + " }";
        }

        private string Point4ToString(IPoint4 point)
        {
            if (point == null)
            {
                return "";
            }

            return "{ x=" + point.X + ", y=" + point.Y + ", z=" + point.Z + ", w=" + point.W + " }";
        }
    }
}
