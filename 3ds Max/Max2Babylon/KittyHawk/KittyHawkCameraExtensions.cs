using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GLTFExport.Entities;

namespace Max2Babylon.KittyHawkExtension
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
}
