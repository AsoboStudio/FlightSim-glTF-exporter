using System;
using System.IO;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFImage : GLTFIndexedChildRootProperty
    {
        [DataMember(EmitDefaultValue = false)]
        public string uri
        {
            get { return _uri; }
            set
            {
                _uri = value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public string mimeType { get; set; } // "image/jpeg" or "image/png"

        [DataMember(EmitDefaultValue = false)]
        public int? bufferView { get; set; }

        public string FileExtension;
        private string _uri;
    }
}
