using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GLTFExport.Entities;
using GLTFExport.Tools;

namespace Max2Babylon.FlightSim
{
    static class FlightSimAsoboPropertyAnimationExtension
    {
        public const string SerializedName = "ASOBO_property_animation";

        public static GLTFAccessor _createAccessorOfProperty(string path, GLTF gltf)
        {
            var buffer = GLTFBufferService.Instance.GetBuffer(gltf);
            GLTFAccessor accessorOutput = null;
            switch (path)
            {
                case "wiperAnimState1":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                        "accessorAnimationWiperAnimState1",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.SCALAR
                    );
                    break;
                case "fov":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                        "accessorAnimationFOV",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.SCALAR
                    );
                    break;
            }
            
            return accessorOutput;
        }
    }

    [DataContract]
    class GLTFExtensionAsoboPropertyAnimation : GLTFProperty
    {        
        /// <summary>
        /// An array of channels, each of which targets an animation's sampler of a generic property.
        /// Different channels of the same animation can't have equal targets.
        /// </summary>
        [DataMember(IsRequired = true)]
        public GLTFAnimationPropertyChannel[] channels { get; set; }
    }

    [DataContract]
    public class GLTFAnimationPropertyChannel : GLTFProperty
    {
        /// <summary>
        /// The index of a sampler in this animation used to compute the value for the target,
        /// </summary>
        [DataMember(IsRequired = true)]
        public int sampler { get; set; }

        /// <summary>
        /// The path to the animated property
        /// "/materials/1/pbrMetallicRoughness/roughnessFactor"
        /// "/cameras/1/perspective/yfov"
        /// </summary>
        [DataMember(IsRequired = true)]
        public string target { get; set; }
    }

    
}