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
    class GLTFExtensionCameraYFOV : GLTFProperty
    {
        public const string SerializedName = "ASOBO_animation_camera_yfov";
        [DataMember(EmitDefaultValue = false)] public List<GLTFCameraChannel> channels;

        public static void Parse(GLTFExtensionCameraYFOV camera)
        {
            foreach (GLTFCameraChannel gltfCameraChannel in camera.channels)
            {
                if (gltfCameraChannel.target != null && gltfCameraChannel.target.path == "fov")
                {
                    gltfCameraChannel.target.path = "yfov";
                }
            }
        }
    }

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

            var maxCamera =  CameraUtilities.GetGenCameraFromNode(camNode);

            if (babylonCamera != null && gltfAnimation != null && FlightSimCameraUtilities.class_ID.Equals(new MaterialUtilities.ClassIDWrapper(maxCamera.ClassID)))
            {               

                GLTFNode gltfNode = null;  
                GLTFCamera gltfCamera =null;
                if (!exporter.nodeToGltfNodeMap.TryGetValue(babylonCamera, out gltfNode)) return gltfObject;
                if (gltfNode!= null && gltfNode.camera>=0) gltfCamera = gltf.CamerasList[gltfNode.camera.GetValueOrDefault()];

                int samlerIndex = AddParameterSamplerAnimation(babylonCamera, gltfAnimation, exporter, gltf, animationExtensionInfo.startFrame, animationExtensionInfo.endFrame);
                GLTFExtensionAsoboPropertyAnimation gltfExtension = new GLTFExtensionAsoboPropertyAnimation();
                List<GLTFAnimationPropertyChannel> animationPropertyChannels = new List<GLTFAnimationPropertyChannel>();

                //TODO: for each animated property in the camera
                //TODO: this should be generated based on the property itself
                GLTFAnimationPropertyChannel propertyChannel = new GLTFAnimationPropertyChannel();
                propertyChannel.sampler = samlerIndex;

                propertyChannel.target = $"cameras/{gltfCamera.index}/perspective/yfov";

                animationPropertyChannels.Add(propertyChannel);

                gltfExtension.channels = animationPropertyChannels.ToArray();
                return gltfExtension;
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
                    case "WIPERANIMSTATE1":
                        {
                            exporter.ExportFloatAnimation(property.Name, animations,
                                 key => new[] { camera.IPropertyContainer.GetFloatProperty(property.Name, key, 0) });
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
                babylonAnimations.AddRange(babylonCamera.animations);
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

            return samplerList.Count - 1;
        }

        //private void ExportCameraAnimations(GLTFAnimation gltfAnimation, int startFrame, int endFrame, GLTF gltf, BabylonNode babylonNode, GLTFNode gltfNode, BabylonScene babylonScene)
        //{
        //    var channelList = gltfAnimation.ChannelList;
        //    var samplerList = gltfAnimation.SamplerList;

        //    bool exportNonAnimated = exportParameters.animgroupExportNonAnimated;

        //    // Combine babylon animations from .babylon file and cached ones
        //    List<BabylonAnimation> babylonAnimations = new List<BabylonAnimation>();
        //    List<BabylonAnimation> babylonAnimationsExtension = new List<BabylonAnimation>();

        //    if (babylonNode.animations != null)
        //    {
        //        babylonAnimations.AddRange(babylonNode.animations);
        //    }
        //    if (babylonNode.extraAnimations != null)
        //    {
        //        babylonAnimations.AddRange(babylonNode.extraAnimations);
        //    }

        //    // Filter animations to keep extended Animations 
        //    babylonAnimationsExtension = babylonAnimations.FindAll(babylonAnimation => _getExtensionTargetPath(babylonAnimation.property) != null);
        //    // Filter animations to only keep TRS ones
        //    babylonAnimations = babylonAnimations.FindAll(babylonAnimation => _getTargetPath(babylonAnimation.property) != null);

        //    if (babylonAnimations.Count > 0 || exportNonAnimated)
        //    {
        //        if (babylonAnimations.Count > 0)
        //        {
        //            logger?.RaiseMessage("GLTFExporter.Animation | Export animations of node named: " + babylonNode.name, 2);
        //        }
        //        else if (exportNonAnimated)
        //        {
        //            logger?.RaiseMessage("GLTFExporter.Animation | Export dummy animation for node named: " + babylonNode.name, 2);
        //            // Export a dummy animation
        //            babylonAnimations.Add(GetDummyAnimation(gltfNode, startFrame, endFrame, babylonScene));
        //        }

        //        foreach (BabylonAnimation babylonAnimation in babylonAnimations)
        //        {
        //            // Target
        //            var gltfTarget = new GLTFChannelTarget
        //            {
        //                node = gltfNode.index
        //            };
        //            gltfTarget.path = _getTargetPath(babylonAnimation.property);

        //            // --- Input ---
        //            var accessorInput = _createAndPopulateInput(gltf, babylonAnimation, startFrame, endFrame);
        //            if (accessorInput == null)
        //                continue;

        //            // --- Output ---
        //            GLTFAccessor accessorOutput = _createAccessorOfPath(gltfTarget.path, gltf);

        //            // Populate accessor
        //            int numKeys = 0;
        //            foreach (var babylonAnimationKey in babylonAnimation.keys)
        //            {
        //                if (babylonAnimationKey.frame < startFrame)
        //                    continue;

        //                if (babylonAnimationKey.frame > endFrame)
        //                    continue;

        //                numKeys++;

        //                // copy data before changing it in case animation groups overlap
        //                float[] outputValues = new float[babylonAnimationKey.values.Length];
        //                babylonAnimationKey.values.CopyTo(outputValues, 0);

        //                // Switch coordinate system at object level
        //                if (babylonAnimation.property == "position")
        //                {
        //                    outputValues[2] *= -1;
        //                    outputValues[0] *= exportParameters.scaleFactor;
        //                    outputValues[1] *= exportParameters.scaleFactor;
        //                    outputValues[2] *= exportParameters.scaleFactor;
        //                }
        //                else if (babylonAnimation.property == "rotationQuaternion")
        //                {
        //                    outputValues[0] *= -1;
        //                    outputValues[1] *= -1;
        //                }

        //                // Store values as bytes
        //                foreach (var outputValue in outputValues)
        //                {
        //                    accessorOutput.bytesList.AddRange(BitConverter.GetBytes(outputValue));
        //                }
        //            };
        //            accessorOutput.count = numKeys;

        //            // bail out if no keyframes to export (?)
        //            // todo [KeyInterpolation]: bail out only when there are no keyframes at all (?) and otherwise add the appropriate (interpolated) keyframes
        //            if (numKeys == 0)
        //                continue;

        //            // Animation sampler
        //            var gltfAnimationSampler = new GLTFAnimationSampler
        //            {
        //                input = accessorInput.index,
        //                output = accessorOutput.index
        //            };
        //            gltfAnimationSampler.index = samplerList.Count;
        //            samplerList.Add(gltfAnimationSampler);

        //            // Channel
        //            var gltfChannel = new GLTFChannel
        //            {
        //                sampler = gltfAnimationSampler.index,
        //                target = gltfTarget
        //            };
        //            channelList.Add(gltfChannel);
        //        }
        //    }

        //    //export all extensions
        //    var channelListExtension = new List<GLTFCameraChannel>();

        //    if (babylonAnimationsExtension.Count > 0)
        //    {
        //        logger?.RaiseMessage("GLTFExporter.Animation | Export extension animations of node named: " + babylonNode.name, 2);

        //        GLTFExtensions animationExtensions = new GLTFExtensions();

        //        foreach (BabylonAnimation babylonAnimation in babylonAnimationsExtension)
        //        {
        //            // Target
        //            var gltfTarget = new GLTFChannelCameraTarget
        //            {
        //                camera = gltfNode.camera
        //            };
        //            gltfTarget.path = _getExtensionTargetPath(babylonAnimation.property);

        //            // --- Input ---
        //            var accessorInput = _createAndPopulateInput(gltf, babylonAnimation, startFrame, endFrame);
        //            if (accessorInput == null)
        //                continue;

        //            // --- Output ---
        //            GLTFAccessor accessorOutput = _createAccessorOfPath(gltfTarget.path, gltf);

        //            // Populate accessor
        //            int numKeys = 0;
        //            foreach (var babylonAnimationKey in babylonAnimation.keys)
        //            {
        //                if (babylonAnimationKey.frame < startFrame)
        //                    continue;

        //                if (babylonAnimationKey.frame > endFrame)
        //                    continue;

        //                numKeys++;

        //                // copy data before changing it in case animation groups overlap
        //                float[] outputValues = new float[babylonAnimationKey.values.Length];
        //                babylonAnimationKey.values.CopyTo(outputValues, 0);

        //                // Store values as bytes
        //                foreach (var outputValue in outputValues)
        //                {
        //                    accessorOutput.bytesList.AddRange(BitConverter.GetBytes(outputValue));
        //                }
        //            };
        //            accessorOutput.count = numKeys;

        //            // bail out if no keyframes to export (?)
        //            // todo [KeyInterpolation]: bail out only when there are no keyframes at all (?) and otherwise add the appropriate (interpolated) keyframes
        //            if (numKeys == 0)
        //                continue;

        //            // Animation sampler
        //            var gltfAnimationSampler = new GLTFAnimationSampler
        //            {
        //                input = accessorInput.index,
        //                output = accessorOutput.index
        //            };
        //            gltfAnimationSampler.index = samplerList.Count;
        //            samplerList.Add(gltfAnimationSampler);

        //            // Channel
        //            var gltfChannel = new GLTFCameraChannel()
        //            {
        //                sampler = gltfAnimationSampler.index,
        //                target = gltfTarget
        //            };
        //            channelListExtension.Add(gltfChannel);



        //            GLTFExtensionCameraYFOV cameraYfov = new GLTFExtensionCameraYFOV();
        //            cameraYfov.channels = channelListExtension;
        //            GLTFExtensionCameraYFOV.Parse(cameraYfov);

        //            animationExtensions.Add(GLTFExtensionCameraYFOV.SerializedName, cameraYfov);

        //            if (animationExtensions.Count > 0)
        //            {
        //                gltfAnimation.extensions = animationExtensions;

        //                // set all extensions as used but not required
        //                foreach (var pair in gltfAnimation.extensions)
        //                {
        //                    if (!gltf.extensionsUsed.Contains(pair.Key))
        //                        gltf.extensionsUsed.Add(pair.Key);
        //                }
        //            }
        //        }
        //    }


        //    if (babylonNode.GetType() == typeof(BabylonMesh))
        //    {
        //        var babylonMesh = babylonNode as BabylonMesh;

        //        // Morph targets
        //        var babylonMorphTargetManager = GetBabylonMorphTargetManager(babylonScene, babylonMesh);
        //        if (babylonMorphTargetManager != null)
        //        {
        //            ExportMorphTargetWeightAnimation(babylonMorphTargetManager, gltf, gltfNode, channelList, samplerList, startFrame, endFrame, babylonScene);
        //        }
        //    }
        //}
    }
}
