using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Max;
using Babylon2GLTF;
using BabylonExport.Entities;
using GLTFExport.Entities;

namespace Max2Babylon.KittyHawkExtension
{
    [DataContract]
    class GLTFExtensionAsoboBoxCollider : GLTFProperty
    {
        public const string SerializedName = "ASOBO_box_collider";
        [DataMember(EmitDefaultValue = false)] public float? width;
        [DataMember(EmitDefaultValue = false)] public float? height;
        [DataMember(EmitDefaultValue = false)] public float? length;
    }


    class FlightSimBoxColliderExtension : IBabylonExtensionExporter
    {
        readonly ClassIDWrapper BoxColliderClassID = new ClassIDWrapper(0x231f3b1a, 0x5a974704);

        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return GLTFExtensionAsoboBoxCollider.SerializedName;
        }

        public Type GetGLTFExtendedType()
        {
            return typeof(GLTFMesh);
        }

        public object ExportBabylonExtension<T>(T babylonObject)
        {
            var babylonMesh = babylonObject as BabylonMesh;
            if (babylonMesh != null)
            {
                List<GLTFExtensionAsoboBoxCollider> collisions = new List<GLTFExtensionAsoboBoxCollider>();
                Guid guid = Guid.Empty;
                Guid.TryParse(babylonMesh.id, out guid);
                IINode maxNode = Tools.GetINodeByGuid(guid);
                foreach (IINode node in maxNode.NodeTree())
                {
                    IObject obj = node.ObjectRef;
                    if (new ClassIDWrapper(obj.ClassID).Equals(BoxColliderClassID))
                    {
                        GLTFExtensionAsoboBoxCollider boxCollider = new GLTFExtensionAsoboBoxCollider();
                        float height = GetGizmoParameter(node, "height");
                        float width = GetGizmoParameter(node, "width");
                        float length = GetGizmoParameter(node, "length");
                        boxCollider.height = height;
                        boxCollider.length = length;
                        boxCollider.width = width;
                        collisions.Add(boxCollider);
                    }
                }
                if(collisions.Count>0)
                {
                    return collisions;
                }
            }
            return null;
        }
        #endregion

        private float GetGizmoParameter(IINode node, string paramName)
        {
            string mxs = $"(maxOps.getNodeByHandle {node.Handle}).BoxGizmo.{paramName}";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            var r=  mxsRetVal.F;
            return r;
        }
    }
}
