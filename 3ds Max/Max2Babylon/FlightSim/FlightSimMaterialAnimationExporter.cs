using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Babylon2GLTF;
using BabylonExport.Entities;
using GLTFExport.Entities;
using GLTFExport.Tools;

namespace Max2Babylon.FlightSim
{   
    public class FlightSimMaterialAnimationExporter : IBabylonMaterialExtensionExporter
    {
        public MaterialUtilities.ClassIDWrapper MaterialClassID
        {
            get { return FlightSimMaterialUtilities.class_ID; }
        }
        public string GetGLTFExtensionName()
        {
            return FlightSimAsoboPropertyAnimationExtension.SerializedName;
        }

        public ExtendedTypes GetExtendedType()
        {
            ExtendedTypes extendType = new ExtendedTypes(typeof(BabylonMaterial),typeof(GLTFAnimation));
            return extendType;
        }


        public bool ExportBabylonExtension<T>(T babylonObject, ref BabylonScene babylonScene, BabylonExporter exporter)
        {
            var materialNode = babylonObject as Autodesk.Max.IIGameMaterial;
            bool isGLTFExported = exporter.exportParameters.outputFormat == "gltf";
            

            if (isGLTFExported && FlightSimMaterialUtilities.class_ID.Equals(new MaterialUtilities.ClassIDWrapper(materialNode.MaxMaterial.ClassID) ))
            {
                var id = materialNode.MaxMaterial.GetGuid().ToString();
                // add a basic babylon material to the list to forward the max material reference
                var babylonMaterial = new BabylonMaterial(id)
                {
                    maxGameMaterial = materialNode,
                    name = materialNode.MaterialName
                };

                babylonMaterial= ExportMaterialAnimations(babylonMaterial, materialNode,exporter);
                babylonScene.MaterialsList.Add(babylonMaterial);
                return true;
            }

            return false;
        }

        private BabylonMaterial ExportMaterialAnimations(BabylonMaterial babylonMaterial, IIGameMaterial material, BabylonExporter exporter)
        {
            var animations = new List<BabylonAnimation>();

            int numProps =  material.IPropertyContainer.NumberOfProperties;
            for (int i = 0; i < numProps; ++i)
            {
                IIGameProperty property = material.IPropertyContainer.GetProperty(i);

                if (property == null)
                    continue;
				
                if(!property.IsPropAnimated )
                    continue;

                IParamDef paramDef = property.MaxParamBlock2?.GetParamDef(property.ParamID);
                string propertyName = property.Name.ToUpperInvariant();

                switch (propertyName)
                {                    
                    case "BASECOLOR":
                        {
                            if(property.IGameControl.GetMaxControl(IGameControlType.Point4).NumKeys > 0)
                            {
                                exporter.ExportColor4Animation(property.Name, animations,
                                key => material.IPropertyContainer.GetPoint4Property(property.Name, key).ToArray());
                            }
                            break;
                        }
                    case "EMISSIVE":
                        {
                            if(property.IGameControl.GetMaxControl(IGameControlType.Point3).NumKeys > 0)
                            {
                                exporter.ExportColor3Animation(property.Name, animations,
                                key => material.IPropertyContainer.GetPoint3Property(property.Name, key).ToArray());
                            }
                            break;
                        }
                    case "WIPERANIMSTATE1":
                    case "WIPERANIMSTATE2":
                    case "WIPERANIMSTATE3":
                    case "WIPERANIMSTATE4":
                    case "METALLIC":
                    case "ROUGHNESS":
                    case "UVOFFSETU":
                    case "UVOFFSETV":
                    case "UVTILINGU":
                    case "UVTILINGV":
                    case "UVROTATION":
                        {
                            if(property.IGameControl.GetMaxControl(IGameControlType.Float).NumKeys > 0)
                            {
                                exporter.ExportFloatAnimation(property.Name, animations,
                                 key => new[] { material.IPropertyContainer.GetFloatProperty(property.Name, key, 0) });
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            babylonMaterial.animations = animations.ToArray();
            return babylonMaterial;
        }

        public object ExportGLTFExtension<T1,T2>(T1 babylonObject, ref T2 gltfObject, ref GLTF gltf, GLTFExporter exporter,ExtensionInfo info)
        {
            var babylonMaterial = babylonObject as BabylonMaterial;
            var gltfAnimation = gltfObject as GLTFAnimation;
            AnimationExtensionInfo animationExtensionInfo = info as AnimationExtensionInfo;            
            
            if(babylonMaterial== null) return null;

            if ( gltfAnimation!=null && FlightSimMaterialUtilities.class_ID.Equals(new MaterialUtilities.ClassIDWrapper(babylonMaterial.maxGameMaterial.MaxMaterial.ClassID)))
            {
                GLTFMaterial gltfMaterial;
                if (!exporter.materialToGltfMaterialMap.TryGetValue(babylonMaterial, out gltfMaterial)) return gltfObject;
                
                int samlerIndex = gltfAnimation.SamplerList.Count;
                
                AddParameterSamplerAnimation(babylonMaterial, gltfAnimation ,exporter,gltf,animationExtensionInfo.startFrame,animationExtensionInfo.endFrame);
                
                
                GLTFExtensionAsoboPropertyAnimation gltfExtension = new GLTFExtensionAsoboPropertyAnimation();
                
                //in case more material are animated in the same animation group
                // the extension used on the animation need to be stored
                if (gltfAnimation.extensions != null && gltfAnimation.extensions.Count > 0)
                {
                    foreach (KeyValuePair<string, object> ext in gltfAnimation.extensions)
                    {
                        if(ext.Key == FlightSimAsoboPropertyAnimationExtension.SerializedName)
                        {
                            gltfExtension = (GLTFExtensionAsoboPropertyAnimation)ext.Value; 
                        }
                    }
                }
                
                List<GLTFAnimationPropertyChannel> matAnimationsPropertyChannels = new List<GLTFAnimationPropertyChannel>();

                if (gltfExtension.channels != null)
                {
                    matAnimationsPropertyChannels.AddRange(gltfExtension.channels);
                }

                IIGameProperty property = babylonMaterial.maxGameMaterial.IPropertyContainer.QueryProperty("materialType");
               
                int material_value = 0;
                if (!property.GetPropertyValue(ref material_value,0)) return null;

                MaterialType materialType = FlightSimMaterialHelper.GetMaterialType(material_value);

                foreach (BabylonAnimation anim in babylonMaterial.animations)
                {
                    GLTFAnimationPropertyChannel propertyChannel = new GLTFAnimationPropertyChannel();
                    propertyChannel.sampler = samlerIndex;

                    if (materialType == MaterialType.Windshield && anim.property == "wiperAnimState1")
                    {                        
                        propertyChannel.target = $"materials/{gltfMaterial.index}/extensions/{GLTFExtensionAsoboWindshield.SerializedName}/wiper1State";
                    }
                    else if (materialType == MaterialType.Windshield && anim.property == "wiperAnimState2")
                    {                        
                        propertyChannel.target = $"materials/{gltfMaterial.index}/extensions/{GLTFExtensionAsoboWindshield.SerializedName}/wiper2State";
                    }
                    else if (materialType == MaterialType.Windshield && anim.property == "wiperAnimState3")
                    {                        
                        propertyChannel.target = $"materials/{gltfMaterial.index}/extensions/{GLTFExtensionAsoboWindshield.SerializedName}/wiper3State";
                    }
                    else if (materialType == MaterialType.Windshield && anim.property == "wiperAnimState4")
                    {                        
                        propertyChannel.target = $"materials/{gltfMaterial.index}/extensions/{GLTFExtensionAsoboWindshield.SerializedName}/wiper4State";
                    }                    
                    else if(materialType == MaterialType.Standard && anim.property == "emissive")
                    {
                        propertyChannel.target = $"materials/{gltfMaterial.index}/emissiveFactor";
                    }
                    else if (materialType == MaterialType.Standard && anim.property == "baseColor")
                    {
                        propertyChannel.target = $"materials/{gltfMaterial.index}/pbrMetallicRoughness/baseColorFactor";
                    }
                    else if (materialType == MaterialType.Standard && anim.property == "roughness")
                    {
                        propertyChannel.target = $"materials/{gltfMaterial.index}/pbrMetallicRoughness/roughnessFactor";
                    }
                    else if (materialType == MaterialType.Standard && anim.property == "metallic")
                    {
                        propertyChannel.target = $"materials/{gltfMaterial.index}/pbrMetallicRoughness/metallicFactor";
                    }
                    else if (materialType != MaterialType.EnvironmentOccluder && anim.property == "UVOffsetU")
                    {
                        propertyChannel.target = $"materials/{gltfMaterial.index}/extensions/{GLTFExtensionAsoboMaterialUVOptions.SerializedName}/UVOffsetU";
                    }
                    else if (materialType != MaterialType.EnvironmentOccluder && anim.property == "UVOffsetV")
                    {
                        propertyChannel.target = $"materials/{gltfMaterial.index}/extensions/{GLTFExtensionAsoboMaterialUVOptions.SerializedName}/UVOffsetV";
                    }
                    else if (materialType != MaterialType.EnvironmentOccluder && anim.property == "UVTilingU")
                    {
                        propertyChannel.target = $"materials/{gltfMaterial.index}/extensions/{GLTFExtensionAsoboMaterialUVOptions.SerializedName}/UVTilingU";
                    }
                    else if (materialType != MaterialType.EnvironmentOccluder && anim.property == "UVTilingV")
                    {
                        propertyChannel.target = $"materials/{gltfMaterial.index}/extensions/{GLTFExtensionAsoboMaterialUVOptions.SerializedName}/UVTilingV";
                    }
                    else if (materialType != MaterialType.EnvironmentOccluder && anim.property == "UVRotation")
                    {
                        propertyChannel.target = $"materials/{gltfMaterial.index}/extensions/{GLTFExtensionAsoboMaterialUVOptions.SerializedName}/UVRotation";
                    }

                    if (propertyChannel.isValid())
                    {
                        matAnimationsPropertyChannels.Add(propertyChannel);
                        samlerIndex += 1;
                    } 
                    
                }                
                
                gltfExtension.channels = matAnimationsPropertyChannels.ToArray();
                
                return gltfExtension;
            }

            return null;
        }

        private void AddParameterSamplerAnimation(BabylonMaterial babylonMaterial, GLTFAnimation gltfAnimation, GLTFExporter exporter, GLTF gltf, int startFrame, int endFrame)
        {
            var samplerList = gltfAnimation.SamplerList;
            // Combine babylon animations from .babylon file and cached ones
            var babylonAnimations = new List<BabylonAnimation>();
            if (babylonMaterial.animations != null)
            {
                babylonAnimations.AddRange(babylonMaterial.animations);
            }

            if (babylonAnimations.Count > 0)
            {
                if (babylonAnimations.Count > 0)
                {
                    exporter.logger.RaiseMessage("GLTFExporter.Animation | Export animations of node named: " + babylonMaterial.name, 2);
                }


                foreach (BabylonAnimation babylonAnimation in babylonAnimations)
                {

                    var babylonAnimationKeysInRange = babylonAnimation.keys.Where(key => key.frame >= startFrame && key.frame <= endFrame);
                    if (babylonAnimationKeysInRange.Count() <= 0)
                        continue;
                    
                    string target_path = babylonAnimation.property;

                    // --- Input ---
                    var accessorInput = exporter._createAndPopulateInput(gltf, babylonAnimation, startFrame, endFrame);
                    if (accessorInput == null)
                        continue;

                    // --- Output ---
                    GLTFAccessor accessorOutput = FlightSimAsoboPropertyAnimationExtension._createAccessorOfProperty(target_path, gltf);
                    if (accessorOutput == null)
                        continue;

                    // Populate accessor
                    int numKeys = 0;
                    foreach (var babylonAnimationKey in babylonAnimationKeysInRange)
                    {
                        numKeys++;

                        // copy data before changing it in case animation groups overlap
                        float[] outputValues = new float[babylonAnimationKey.values.Length];
                        babylonAnimationKey.values.CopyTo(outputValues, 0);

                        // Store values as bytes
                        foreach (var outputValue in outputValues)
                        {
                            accessorOutput.bytesList.AddRange(BitConverter.GetBytes(outputValue));
                        }
                    };
                    accessorOutput.count = numKeys;
                    if (accessorOutput.count == 0)
                    {
                        exporter.logger.RaiseWarning(String.Format("GLTFExporter.Animation | No frames to export in material animation \"{1}\" of node named \"{0}\". This will cause an error in the output gltf.", babylonMaterial.name, babylonAnimation.name));
                    }

                    // Animation sampler
                    var gltfAnimationSampler = new GLTFAnimationSampler
                    {
                        input = accessorInput.index,
                        output = accessorOutput.index
                    };
                    gltfAnimationSampler.index = samplerList.Count;
                    samplerList.Add(gltfAnimationSampler);
                }
            }

        }

      
    }
}
