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

namespace Max2Babylon.FlightSimExtension
{
    [DataContract]
    class GLTFExtensionAsoboCollider : GLTFProperty
    {
        [DataMember(EmitDefaultValue = false,Name = "type")] public string Type { get; set; }
        [DataMember(EmitDefaultValue = false, Name = "translation")] public object Translation;
        [DataMember(Name = "params")] public object Params { get; set; }
    }

    [DataContract]
    class GLTFExtensionAsoboBoxParams : GLTFProperty
    {
        
        [DataMember(EmitDefaultValue = false)] public float? width;
        [DataMember(EmitDefaultValue = false)] public float? height;
        [DataMember(EmitDefaultValue = false)] public float? length;
    }

    [DataContract]
    class GLTFExtensionAsoboSphereParams : GLTFProperty
    {
        [DataMember(EmitDefaultValue = false)] public float? radius;
    }

    [DataContract]
    class GLTFExtensionAsoboCylinderParams : GLTFProperty
    {
        [DataMember(EmitDefaultValue = false)] public float? radius;
        [DataMember(EmitDefaultValue = false)] public float? height;
    }


    class FlightSimColliderExtension : IBabylonExtensionExporter
    {
        readonly ClassIDWrapper BoxColliderClassID = new ClassIDWrapper(0x231f3b1a, 0x5a974704);
        readonly ClassIDWrapper CylinderColliderClassID = new ClassIDWrapper(0x7c242166, 0x5dbf7d08);
        readonly ClassIDWrapper SphereColliderClassID = new ClassIDWrapper(0x736e21e7, 0x45da3199);

        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return "ASOBO_collision_object";
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
                List<GLTFExtensionAsoboCollider> collisions = new List<GLTFExtensionAsoboCollider>();
                Guid guid = Guid.Empty;
                Guid.TryParse(babylonMesh.id, out guid);
                IINode maxNode = Tools.GetINodeByGuid(guid);
                foreach (IINode node in maxNode.NodeTree())
                {
                    IObject obj = node.ObjectRef;
                    if (new ClassIDWrapper(obj.ClassID).Equals(BoxColliderClassID))
                    {
                        GLTFExtensionAsoboCollider collider = new GLTFExtensionAsoboCollider();
                        GLTFExtensionAsoboBoxParams boxParams = new GLTFExtensionAsoboBoxParams();
                        float height = GetGizmoParameter(node, "BoxGizmo", "height");
                        float width = GetGizmoParameter(node, "BoxGizmo","width");
                        float length = GetGizmoParameter(node,"BoxGizmo", "length");
                        collider.Translation = GetTranslation(node);
                        boxParams.height = height;
                        boxParams.length = length;
                        boxParams.width = width;
                        collider.Params = boxParams;
                        collider.Type = "box";
                        collisions.Add(collider);
                    }
                    else if (new ClassIDWrapper(obj.ClassID).Equals(CylinderColliderClassID))
                    {
                        GLTFExtensionAsoboCollider collider = new GLTFExtensionAsoboCollider();
                        GLTFExtensionAsoboCylinderParams cylinderParams = new GLTFExtensionAsoboCylinderParams();
                        float radius = GetGizmoParameter(node,"CylGizmo", "radius");
                        float height = GetGizmoParameter(node,"CylGizmo", "height");
                        collider.Translation = GetTranslation(node);
                        cylinderParams.height = height;
                        cylinderParams.radius = radius;
                        collider.Params = cylinderParams;
                        collider.Type = "cylinder";
                        collisions.Add(collider);
                    }
                    else if (new ClassIDWrapper(obj.ClassID).Equals(SphereColliderClassID))
                    {
                        GLTFExtensionAsoboCollider collider = new GLTFExtensionAsoboCollider();
                        GLTFExtensionAsoboSphereParams sphereParams = new GLTFExtensionAsoboSphereParams();
                        float radius = GetGizmoParameter(node,"SphereGizmo", "radius");
                        collider.Translation = GetTranslation(node);
                        sphereParams.radius = radius;
                        collider.Type = "sphere";
                        collider.Params = sphereParams;
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

        private float GetGizmoParameter(IINode node, string gizmoClass, string paramName)
        {
            string mxs = $"(maxOps.getNodeByHandle {node.Handle}).{gizmoClass}.{paramName}";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            var r=  mxsRetVal.F;
            return r;
        }

        private float[] GetTranslation(IINode node)
        {
            float[] res = new float[3];
            string mxs = $"(maxOps.getNodeByHandle {node.Handle}).transform.pos * inverse (maxOps.getNodeByHandle {node.Handle}).parent.transform";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            var r=  mxsRetVal.P;
            res[0] = r.X;
            res[1] = r.Y;
            res[2] = r.Z;
            return res;
        }
    }
}
