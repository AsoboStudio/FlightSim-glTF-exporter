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
            switch (path.ToUpperInvariant())
            {
                case "WIPERANIMSTATE1":
                case "WIPERANIMSTATE2":
                case "WIPERANIMSTATE3":
                case "WIPERANIMSTATE4":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                        $"accessorAnimation{path}",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.SCALAR
                    );
                    break;

                case "BASECOLOR":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatVec4(gltf, buffer),
                        "accessorAnimationBaseColor",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.VEC4
                    );
                    break;

                case "EMISSIVE":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatVec3(gltf, buffer),
                        "accessorAnimationEmissiveColor",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.VEC3
                    );
                    break;

                case "ROUGHNESS":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                        "accessorAnimationRoughnessFactor",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.SCALAR
                    );
                    break;

                case "METALLIC":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                        "accessorAnimationMetallicFactor",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.SCALAR
                    );
                    break;

                case "FOV":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                        "accessorAnimationFOV",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.SCALAR
                    );
                    break;

                case "UVOFFSETU":
                case "UVOFFSETV":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                        "accessorAnimationUVOffset",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.SCALAR
                    );
                    break;

                case "UVTILINGU":
                case "UVTILINGV":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                        "accessorAnimationUVTiling",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.SCALAR
                    );
                    break;
                case "UVROTATION":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                        "accessorAnimationUVRotation",
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

        public bool isValid()
        {
            if (this.sampler <0 ) return false;
            if( string.IsNullOrWhiteSpace(this.target)) return false;

            return true;
        }
    }

    
}