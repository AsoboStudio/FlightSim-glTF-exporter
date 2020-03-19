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
using Max2Babylon.FlightSim;

namespace Max2Babylon.FlightSimExtension
{
    [DataContract]
    class GLTFExtensionAsoboCollider : GLTFProperty
    {
        [DataMember(Name = "collision_objects")] 
        public List<GLTFExtensionCollider> colliders { get; set; }
    }

    [DataContract]
    class GLTFExtensionCollider : GLTFProperty
    {
        [DataMember(EmitDefaultValue = false,Name = "type")] public string Type { get; set; }
        [DataMember(EmitDefaultValue = false, Name = "translation")] public object Translation;
        [DataMember(EmitDefaultValue = false, Name = "rotation")] public object Rotation;
        [DataMember(Name = "params")] public object Params { get; set; }
    }

    [DataContract]
    class GLTFExtensionAsoboBoxParams : GLTFProperty
    {
        
        [DataMember(EmitDefaultValue = false)] public float? width;
        [DataMember(EmitDefaultValue = false)] public float? height;
        [DataMember(EmitDefaultValue = false)] public float? length;
        [DataMember(EmitDefaultValue = false)] public bool? isRoad;
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
                GLTFExtensionAsoboCollider gltfExtensionAsoboCollider = new GLTFExtensionAsoboCollider();
                List<GLTFExtensionCollider> collisions = new List<GLTFExtensionCollider>();
                gltfExtensionAsoboCollider.colliders = collisions;

                Guid guid = Guid.Empty;
                Guid.TryParse(babylonMesh.id, out guid);
                IINode maxNode = Tools.GetINodeByGuid(guid);
                foreach (IINode node in maxNode.DirectChildren())
                {
                    IObject obj = node.ObjectRef;
                    if (new ClassIDWrapper(obj.ClassID).Equals(BoxColliderClassID))
                    {
                        GLTFExtensionCollider collider = new GLTFExtensionCollider();
                        GLTFExtensionAsoboBoxParams boxParams = new GLTFExtensionAsoboBoxParams();
                        float height = FlightSimExtensionUtility.GetGizmoParameterFloat(node, "BoxGizmo", "height");
                        float width = FlightSimExtensionUtility.GetGizmoParameterFloat(node, "BoxGizmo","width");
                        float length = FlightSimExtensionUtility.GetGizmoParameterFloat(node,"BoxGizmo", "length");
                        bool isRoad = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "BoxCollider", "IsRoad",IsSubClass:false);
                        collider.Translation = FlightSimExtensionUtility.GetTranslation(node,maxNode);
                        float[] rotation = FlightSimExtensionUtility.GetRotation(node, maxNode);
                        if (!FlightSimExtensionUtility.IsDefaultRotation(rotation))
                        {
                            collider.Rotation = rotation;
                        }
                        
                        boxParams.width = width;
                        boxParams.height = height;
                        boxParams.length = length;
                        if (isRoad) boxParams.isRoad = true;

                        collider.Params = boxParams;
                        collider.Type = "box";
                        collisions.Add(collider);
                    }
                    else if (new ClassIDWrapper(obj.ClassID).Equals(CylinderColliderClassID))
                    {
                        GLTFExtensionCollider collider = new GLTFExtensionCollider();
                        GLTFExtensionAsoboCylinderParams cylinderParams = new GLTFExtensionAsoboCylinderParams();
                        float radius = FlightSimExtensionUtility.GetGizmoParameterFloat(node,"CylGizmo", "radius");
                        float height = FlightSimExtensionUtility.GetGizmoParameterFloat(node,"CylGizmo", "height");
                        collider.Translation = FlightSimExtensionUtility.GetTranslation(node,maxNode);
                        float[] rotation = FlightSimExtensionUtility.GetRotation(node, maxNode);
                        if (!FlightSimExtensionUtility.IsDefaultRotation(rotation))
                        {
                            collider.Rotation = rotation;
                        }
                        cylinderParams.height = height;
                        cylinderParams.radius = radius;
                        collider.Params = cylinderParams;
                        collider.Type = "cylinder";
                        collisions.Add(collider);
                    }
                    else if (new ClassIDWrapper(obj.ClassID).Equals(SphereColliderClassID))
                    {
                        GLTFExtensionCollider collider = new GLTFExtensionCollider();
                        GLTFExtensionAsoboSphereParams sphereParams = new GLTFExtensionAsoboSphereParams();
                        float radius = FlightSimExtensionUtility.GetGizmoParameterFloat(node,"SphereGizmo", "radius");
                        collider.Translation = FlightSimExtensionUtility.GetTranslation(node,maxNode);
                        sphereParams.radius = radius;
                        collider.Type = "sphere";
                        collider.Params = sphereParams;
                        collisions.Add(collider);
                    }
                }
                if(collisions.Count>0)
                {
                    return gltfExtensionAsoboCollider;
                }
            }
            return null;
        }
        #endregion

        
    }
}
