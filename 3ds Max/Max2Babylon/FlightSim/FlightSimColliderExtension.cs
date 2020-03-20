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
using Utilities;

namespace Max2Babylon.FlightSimExtension
{
    enum AsoboTag
    {
        Collision,
        Road
    }

    [DataContract]
    class GLTFExtensionAsoboTags : GLTFProperty
    {
        public const string SerializedName = "ASOBO_tags";
        [DataMember]
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

        public BabylonExtendTypes GetExtendedType()
        {
            return new BabylonExtendTypes(typeof(GLTFMesh));
        }

        public bool ExportBabylonExtension<T>(T babylonObject, ExportParameters parameters, ref BabylonScene babylonScene, ILoggingProvider logger)
        {
            // just skip this extension is ment only for GLTF
            return false;
        }

        public object ExportGLTFExtension<T>(T babylonObject, ExportParameters parameters, ref GLTF gltf, ILoggingProvider logger)
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
                    GLTFExtensionGizmo gizmo = new GLTFExtensionGizmo();;
                    if (new ClassIDWrapper(obj.ClassID).Equals(BoxColliderClassID))
                    {
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

                        bool isRoad = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "BoxCollider", "IsRoad",IsSubClass:false);
                        bool isCollision = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "BoxCollider", "IsCollision",IsSubClass:false);
                        
                        if(isCollision) tags.Add(AsoboTag.Collision);
                        if(isRoad) tags.Add(AsoboTag.Road);

                    }
                    else if (new ClassIDWrapper(obj.ClassID).Equals(CylinderColliderClassID))
                    {
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

                        bool isRoad = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "CylCollider", "IsRoad",IsSubClass:false);
                        bool isCollision = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "CylCollider", "IsCollision",IsSubClass:false);
                        
                        if(isCollision) tags.Add(AsoboTag.Collision);
                        if(isRoad) tags.Add(AsoboTag.Road);
                    }
                    else if (new ClassIDWrapper(obj.ClassID).Equals(SphereColliderClassID))
                    {
                        GLTFExtensionAsoboSphereParams sphereParams = new GLTFExtensionAsoboSphereParams();
                        float radius = FlightSimExtensionUtility.GetGizmoParameterFloat(node,"SphereGizmo", "radius");
                        gizmo.Translation = FlightSimExtensionUtility.GetTranslation(node,maxNode);
                        sphereParams.radius = radius;
                        gizmo.Type = "sphere";
                        gizmo.Params = sphereParams;

                        bool isRoad = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "SphereCollider", "IsRoad",IsSubClass:false);
                        bool isCollision = FlightSimExtensionUtility.GetGizmoParameterBoolean(node, "SphereCollider", "IsCollision",IsSubClass:false);
                        
                        if(isCollision) tags.Add(AsoboTag.Collision);
                        if(isRoad) tags.Add(AsoboTag.Road);
                    }

                    GLTFExtensionAsoboTags asoboTagsExtension = new GLTFExtensionAsoboTags();
                    asoboTagsExtension.tags = tags.ConvertAll(x => x.ToString());
                    if (tags.Count > 0)
                    {
                        if(gizmo.extensions==null) gizmo.extensions = new GLTFExtensions();
                        gizmo.extensions.Add(GLTFExtensionAsoboTags.SerializedName,asoboTagsExtension);

                        if (gltf.extensionsUsed == null) gltf.extensionsUsed = new List<string>();
                        if (!gltf.extensionsUsed.Contains(GLTFExtensionAsoboTags.SerializedName))
                        {
                            gltf.extensionsUsed.Add(GLTFExtensionAsoboTags.SerializedName);
                        }
                    }

                    
                    collisions.Add(gizmo);
                    

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
