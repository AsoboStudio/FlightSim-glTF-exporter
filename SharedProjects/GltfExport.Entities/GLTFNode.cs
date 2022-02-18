using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFNode : GLTFIndexedChildRootProperty
    {
        [DataMember(EmitDefaultValue = false)]
        public int? camera { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int[] children { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? skin { get; set; }

        // Either matrix or Translation+Rotation+Scale
        //[DataMember]
        //public float[] matrix { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? mesh { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float[] translation { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float[] rotation { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float[] scale { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float[] weights { get; set; }

        public List<int> ChildrenList { get; private set; }

        // Used to compute transform world matrix
        public GLTFNode parent;

        public GLTFNode()
        {
            ChildrenList = new List<int>();
        }

        public void Prepare()
        {
            // Do not export empty arrays
            if (ChildrenList.Count > 0)
            {
                children = ChildrenList.ToArray();
            }
            if (IsTranslationDefault(translation)) translation = null;
            if (IsRotationDefault(rotation)) rotation = null;
            if (IsScaleDefault(scale)) scale = null;
        }

        private bool IsTranslationDefault(float[] translation)
        {
            if (translation.Length != 3) return false;
            if (translation[0] != 0) return false;
            if (translation[1] != 0) return false;
            if (translation[2] != 0) return false;
            return true;
        }

        private bool IsRotationDefault(float[] rotation) 
        {
            if (rotation.Length != 4) return false;
            if (rotation[0] != 0) return false;
            if (rotation[1] != 0) return false;
            if (rotation[2] != 0) return false;
            if (rotation[3] != 1) return false;
            return true;
        }

        private bool IsScaleDefault(float[] scale)
        {
            if (scale.Length != 3) return false;
            if (scale[0] != 1) return false;
            if (scale[1] != 1) return false;
            if (scale[2] != 1) return false;
            return true;
        }
    }
}
