using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Babylon2GLTF;
using BabylonExport.Entities;
using GLTFExport.Entities;
using Max2Babylon.FlightSim;
using Utilities;
using Max2Babylon;

namespace Max2Babylon.FlightSimExtension
{
    [DataContract]
    public class GLTFCameraChannel : GLTFProperty
    {
        [DataMember(IsRequired = true)]
        public int sampler { get; set; }

        [DataMember(IsRequired = true)]
        public GLTFChannelCameraTarget target { get; set; }
    }

    [DataContract]
    public class GLTFChannelCameraTarget : GLTFProperty
    {
        [DataMember(EmitDefaultValue = false)]
        public int? camera { get; set; }

        [DataMember(IsRequired = true)]
        public string path { get; set; }
    }


     public class FlightSimCameraPropertyAnimationExporter : IBabylonExtensionExporter
     {

        public string GetGLTFExtensionName()
        {
            return FlightSimAsoboPropertyAnimationExtension.SerializedName;
        }

        public ExtendedTypes GetExtendedType()
        {
            ExtendedTypes extendType = new ExtendedTypes(typeof(BabylonCamera),typeof(GLTFAnimation));
            return extendType;
        }

        public object ExportGLTFExtension<T1,T2>(T1 babylonObject, ref T2 gltfObject, ref GLTF gltf, GLTFExporter exporter, ExtensionInfo info)
        {
            var babylonCamera = babylonObject as BabylonCamera;
            var gltfAnimation = gltfObject as GLTFAnimation;
            AnimationExtensionInfo animationExtensionInfo = info as AnimationExtensionInfo;

            if (babylonCamera == null) return null;

            Guid guid = Guid.Empty;
            Guid.TryParse(babylonCamera.id, out guid);
            IINode camNode = Tools.GetINodeByGuid(guid);

            var maxCamera =  CameraUtilities.GetGenCameraFromNode(camNode, exporter.logger);

            if(maxCamera== null)
            {
                exporter.logger.RaiseError($"Impossible to export {FlightSimAsoboPropertyAnimationExtension.SerializedName} for {camNode.Name}, camera node must be a TargetCamera type");
                return null;
            }

            if (babylonCamera != null && gltfAnimation != null && FlightSimCameraUtilities.class_ID.Equals(new MaterialUtilities.ClassIDWrapper(maxCamera.ClassID)))
            {               

                GLTFNode gltfNode = null;  
                GLTFCamera gltfCamera =null;
                if (!exporter.nodeToGltfNodeMap.TryGetValue(babylonCamera, out gltfNode)) return gltfObject;
                if (gltfNode!= null && gltfNode.camera>=0) gltfCamera = gltf.CamerasList[gltfNode.camera.GetValueOrDefault()];

                int samplerIndex = AddParameterSamplerAnimation(babylonCamera, gltfAnimation, exporter, gltf, animationExtensionInfo.startFrame, animationExtensionInfo.endFrame);
                if (samplerIndex >-1)
                {
                    GLTFExtensionAsoboPropertyAnimation gltfExtension = new GLTFExtensionAsoboPropertyAnimation();
                    List<GLTFAnimationPropertyChannel> animationPropertyChannels = new List<GLTFAnimationPropertyChannel>();

                    //TODO: for each animated property in the camera
                    //TODO: this should be generated based on the property itself
                    GLTFAnimationPropertyChannel propertyChannel = new GLTFAnimationPropertyChannel();
                    propertyChannel.sampler = samplerIndex;

                    propertyChannel.target = $"cameras/{gltfCamera.index}/perspective/yfov";

                    animationPropertyChannels.Add(propertyChannel);

                    gltfExtension.channels = animationPropertyChannels.ToArray();
                    return gltfExtension;
                }                
            }

            return null;            
        }

        public bool ExportBabylonExtension<T>(T babylonObject, ref BabylonScene babylonScene, BabylonExporter exporter)
        {
            var cameraNode = babylonObject as Autodesk.Max.IIGameCamera;
            bool isGLTFExported = exporter.exportParameters.outputFormat == "gltf";
            // todo: MaterialUtilities class should be splitted as the wrapper contains more idwrapper then the materials one

            if (isGLTFExported && FlightSimCameraUtilities.class_ID.Equals(new MaterialUtilities.ClassIDWrapper(cameraNode.CameraTarget.MaxNode.ClassID)))
            {
                var id = cameraNode.CameraTarget.MaxNode.GetGuid().ToString();
                // add a basic babylon material to the list to forward the max material reference
                var babylonCamera = new BabylonCamera();//(id)
                //{
                //    maxGameMaterial = materialNode,
                //    name = materialNode.MaterialName
                //};

                babylonCamera = ExportCameraAnimations(babylonCamera, cameraNode, exporter);
                babylonScene.CamerasList.Add(babylonCamera);
                return true;
            }

            return false;
        }

        private BabylonCamera ExportCameraAnimations(BabylonCamera babylonCamera, IIGameCamera camera, BabylonExporter exporter)
        {
            var animations = new List<BabylonAnimation>();

            int numProps = camera.IPropertyContainer.NumberOfProperties;
            for (int i = 0; i < numProps; ++i)
            {
                IIGameProperty property = camera.IPropertyContainer.GetProperty(i);

                if (property == null)
                    continue;

                IParamDef paramDef = property.MaxParamBlock2?.GetParamDef(property.ParamID);
                string propertyName = property.Name.ToUpperInvariant();

                switch (propertyName)
                {
                    default:
                        break;
                }
            }

            babylonCamera.animations = animations.ToArray();
            return babylonCamera;
        }


        private int AddParameterSamplerAnimation(BabylonCamera babylonCamera, GLTFAnimation gltfAnimation, GLTFExporter exporter, GLTF gltf, int startFrame, int endFrame)
        {
            var samplerList = gltfAnimation.SamplerList;
            // Combine babylon animations from .babylon file and cached ones
            var babylonAnimations = new List<BabylonAnimation>();
            if (babylonCamera.animations != null)
            {
                IEnumerable<BabylonAnimation> extendedAnimations = babylonCamera.animations.Where(anim => anim.name == "fov animation");
                babylonAnimations.AddRange(extendedAnimations);
            }

            if (babylonAnimations.Count > 0)
            {
                if (babylonAnimations.Count > 0)
                {
                    exporter.logger.RaiseMessage("GLTFExporter.Animation | Export animations of node named: " + babylonCamera.name, 2);
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
                        exporter.logger.RaiseWarning(String.Format("GLTFExporter.Animation | No frames to export in material animation \"{1}\" of node named \"{0}\". This will cause an error in the output gltf.", babylonCamera.name, babylonAnimation.name));
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
            else
            {
                return -1;    
            }

            return samplerList.Count - 1;
        }
        
    }
}
