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

                IParamDef paramDef = property.MaxParamBlock2?.GetParamDef(property.ParamID);
                string propertyName = property.Name.ToUpperInvariant();
                
                switch (propertyName)
                {
                    case "WIPERANIMSTATE1":
                        {
                            exporter.ExportFloatAnimation(property.Name, animations,
                                 key => new[]{ material.IPropertyContainer.GetFloatProperty(property.Name, key, 0) });
                            break;
                        }
                    default:
                        break;
                    //case "EMISSIVEFACTOR":
                    //    {
                    //        exporter.ExportColor4Animation(property.Name, animations,
                    //            key => material.IPropertyContainer.GetPoint4Property(property.Name, key).ToArray());
                    //        break;
                    //    }
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
            
            if (babylonMaterial!= null && gltfAnimation!=null && FlightSimMaterialUtilities.class_ID.Equals(new MaterialUtilities.ClassIDWrapper(babylonMaterial.maxGameMaterial.MaxMaterial.ClassID)))
            {
                GLTFMaterial gltfMaterial;
                if (!exporter.materialToGltfMaterialMap.TryGetValue(babylonMaterial, out gltfMaterial)) return gltfObject;


                int samlerIndex = AddParameterSamplerAnimation(babylonMaterial, gltfAnimation ,exporter,gltf,animationExtensionInfo.startFrame,animationExtensionInfo.endFrame);
                GLTFExtensionAsoboPropertyAnimation gltfExtension = new GLTFExtensionAsoboPropertyAnimation();
                List<GLTFAnimationPropertyChannel> animationPropertyChannels = new List<GLTFAnimationPropertyChannel>();

                //TODO: for each animated property in the materials
                //TODO: this should be generated based on the property itself
                GLTFAnimationPropertyChannel propertyChannel = new GLTFAnimationPropertyChannel();
                propertyChannel.sampler = samlerIndex;
                
                propertyChannel.target = $"material/{gltfMaterial.index}/wiper1AnimationState";

                animationPropertyChannels.Add(propertyChannel);
               
                gltfExtension.channels = animationPropertyChannels.ToArray();
                return gltfExtension;
            }

            return null;
        }

        private int AddParameterSamplerAnimation(BabylonMaterial babylonMaterial, GLTFAnimation gltfAnimation, GLTFExporter exporter, GLTF gltf, int startFrame, int endFrame)
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

            return samplerList.Count -1;
        }

      
    }
}
