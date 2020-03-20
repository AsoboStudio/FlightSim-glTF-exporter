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
    enum AsoboTag
    {
        Collision,
        Road
    }

    [DataContract]
    class GLTFExtensionAsoboTag : GLTFProperty
    {
        public List<string> tags { get; set; }
    }

    [DataContract]
    class GLTFExtensionAsoboGizmo : GLTFProperty
    {
        [DataMember(Name = "gizmo_objects")] 
        public List<GLTFExtensionGizmo> gizmos { get; set; }
    }

    [DataContract]
    class GLTFExtensionGizmo : GLTFProperty
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
            return "ASOBO_gizmo_object";
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
                GLTFExtensionAsoboGizmo gltfExtensionAsoboGizmo = new GLTFExtensionAsoboGizmo();
                List<GLTFExtensionGizmo> collisions = new List<GLTFExtensionGizmo>();
                gltfExtensionAsoboGizmo.gizmos = collisions;

                Guid guid = Guid.Empty;
                Guid.TryParse(babylonMesh.id, out guid);
                IINode maxNode = Tools.GetINodeByGuid(guid);
                foreach (IINode node in maxNode.DirectChildren())
                {
                    IObject obj = node.ObjectRef;
                    List<AsoboTag> tags = new List<AsoboTag>();
                    if (new ClassIDWrapper(obj.ClassID).Equals(BoxColliderClassID))
                    {
                        GLTFExtensionGizmo gizmo = new GLTFExtensionGizmo();
                        GLTFExtensionAsoboBoxParams boxParams = new GLTFExtensionAsoboBoxParams();
                        float height = FlightSimExtensionUtility.GetGizmoParameterFloat(node, "BoxGizmo", "height");
                        float width = FlightSimExtensionUtility.GetGizmoParameterFloat(node, "BoxGizmo","width");
                        float length = FlightSimExtensionUtility.GetGizmoParameterFloat(node,"BoxGizmo", "length");
                        gizmo.Translation = FlightSimExtensionUtility.GetTranslation(node,maxNode);
                        float[] rotation = FlightSimExtensionUtility.GetRotation(node, maxNode);
                        if (!FlightSimExtensionUtility.IsDefaultRotation(rotation))
                        {
                            gizmo.Rotation = rotation;
                        }
                        
                        boxParams.width = width;
                        boxParams.height = height;
                        boxParams.length = length;

                        gizmo.Params = boxParams;
                        gizmo.Type = "box";
                        collisions.Add(gizmo);

                        bool isRoad = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "BoxCollider", "IsRoad",IsSubClass:false);
                        bool isCollision = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "BoxCollider", "IsCollision",IsSubClass:false);
                        
                        if(isCollision) tags.Add(AsoboTag.Collision);
                        if(isRoad) tags.Add(AsoboTag.Road);

                    }
                    else if (new ClassIDWrapper(obj.ClassID).Equals(CylinderColliderClassID))
                    {
                        GLTFExtensionGizmo gizmo = new GLTFExtensionGizmo();
                        GLTFExtensionAsoboCylinderParams cylinderParams = new GLTFExtensionAsoboCylinderParams();
                        float radius = FlightSimExtensionUtility.GetGizmoParameterFloat(node,"CylGizmo", "radius");
                        float height = FlightSimExtensionUtility.GetGizmoParameterFloat(node,"CylGizmo", "height");
                        gizmo.Translation = FlightSimExtensionUtility.GetTranslation(node,maxNode);
                        float[] rotation = FlightSimExtensionUtility.GetRotation(node, maxNode);
                        if (!FlightSimExtensionUtility.IsDefaultRotation(rotation))
                        {
                            gizmo.Rotation = rotation;
                        }
                        cylinderParams.height = height;
                        cylinderParams.radius = radius;
                        gizmo.Params = cylinderParams;
                        gizmo.Type = "cylinder";
                        collisions.Add(gizmo);

                        bool isRoad = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "CylCollider", "IsRoad",IsSubClass:false);
                        bool isCollision = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "CylCollider", "IsCollision",IsSubClass:false);
                        
                        if(isCollision) tags.Add(AsoboTag.Collision);
                        if(isRoad) tags.Add(AsoboTag.Road);
                    }
                    else if (new ClassIDWrapper(obj.ClassID).Equals(SphereColliderClassID))
                    {
                        GLTFExtensionGizmo gizmo = new GLTFExtensionGizmo();
                        GLTFExtensionAsoboSphereParams sphereParams = new GLTFExtensionAsoboSphereParams();
                        float radius = FlightSimExtensionUtility.GetGizmoParameterFloat(node,"SphereGizmo", "radius");
                        gizmo.Translation = FlightSimExtensionUtility.GetTranslation(node,maxNode);
                        sphereParams.radius = radius;
                        gizmo.Type = "sphere";
                        gizmo.Params = sphereParams;
                        collisions.Add(gizmo);

                        bool isRoad = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "SphereCollider", "IsRoad",IsSubClass:false);
                        bool isCollision = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "SphereCollider", "IsCollision",IsSubClass:false);
                        
                        if(isCollision) tags.Add(AsoboTag.Collision);
                        if(isRoad) tags.Add(AsoboTag.Road);
                    }

                    GLTFExtensionAsoboTag asoboTagExtension = new GLTFExtensionAsoboTag();
                    asoboTagExtension.tags = tags.ConvertAll(x => x.ToString());
                    if (tags.Count > 0)
                    {
                        gltfExtensionAsoboGizmo.extensions = new GLTFExtensions()
                        {
                            {"ASOBO_tags",asoboTagExtension}
                        };
                    }
                    

                }
                if(collisions.Count>0)
                {
                    return gltfExtensionAsoboGizmo;
                }
            }
            return null;
        }
        #endregion

        
    }
}
