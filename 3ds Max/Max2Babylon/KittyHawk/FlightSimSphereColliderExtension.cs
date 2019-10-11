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
    class GLTFExtensionAsoboSphereCollider : GLTFProperty
    {
        public const string SerializedName = "ASOBO_sphere_collider";
        [DataMember(EmitDefaultValue = false)] public float? radius;
    }


    class FlightSimSphereColliderExtension : IBabylonExtensionExporter
    {
        readonly ClassIDWrapper SphereColliderClassID = new ClassIDWrapper(0x736e21e7, 0x45da3199);

        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return GLTFExtensionAsoboSphereCollider.SerializedName;
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
                List<GLTFExtensionAsoboSphereCollider> collisions = new List<GLTFExtensionAsoboSphereCollider>();
                Guid guid = Guid.Empty;
                Guid.TryParse(babylonMesh.id, out guid);
                IINode maxNode = Tools.GetINodeByGuid(guid);
                foreach (IINode node in maxNode.NodeTree())
                {
                    IObject obj = node.ObjectRef;
                    if (new ClassIDWrapper(obj.ClassID).Equals(SphereColliderClassID))
                    {
                        GLTFExtensionAsoboSphereCollider collider = new GLTFExtensionAsoboSphereCollider();
                        float radius = GetGizmoParameter(node, "radius");
                        collider.radius = radius;
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
            string mxs = $"(maxOps.getNodeByHandle {node.Handle}).BoxGizmo.{paramName}";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            var r=  mxsRetVal.F;
            return r;
        }
    }
}
