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
    class GLTFExtensionAsoboCylinderCollider : GLTFProperty
    {
        public const string SerializedName = "ASOBO_cylinder_collider";
        [DataMember(EmitDefaultValue = false)] public float? radius;
        [DataMember(EmitDefaultValue = false)] public float? height;
    }


    class FlightSimCylinderColliderExtension : IBabylonExtensionExporter
    {
        readonly ClassIDWrapper CylinderColliderClassID = new ClassIDWrapper(0x7c242166, 0x5dbf7d08);

        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return GLTFExtensionAsoboCylinderCollider.SerializedName;
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
                List<GLTFExtensionAsoboCylinderCollider> collisions = new List<GLTFExtensionAsoboCylinderCollider>();
                Guid guid = Guid.Empty;
                Guid.TryParse(babylonMesh.id, out guid);
                IINode maxNode = Tools.GetINodeByGuid(guid);
                foreach (IINode node in maxNode.NodeTree())
                {
                    IObject obj = node.ObjectRef;
                    if (new ClassIDWrapper(obj.ClassID).Equals(CylinderColliderClassID))
                    {
                        GLTFExtensionAsoboCylinderCollider collider = new GLTFExtensionAsoboCylinderCollider();
                        float radius = GetGizmoParameter(node, "radius");
                        float height = GetGizmoParameter(node, "height");
                        collider.radius = radius;
                        collider.height = height;
                        collisions.Add(collider);
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
            string mxs = $"(maxOps.getNodeByHandle {node.Handle}).CylinderGizmo.{paramName}";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            var r=  mxsRetVal.F;
            return r;
        }
    }
}
