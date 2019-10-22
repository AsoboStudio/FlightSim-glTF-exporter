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
                foreach (IINode node in maxNode.NodeTree())
                {
                    IObject obj = node.ObjectRef;
                    if (new ClassIDWrapper(obj.ClassID).Equals(BoxColliderClassID))
                    {
                        GLTFExtensionCollider collider = new GLTFExtensionCollider();
                        GLTFExtensionAsoboBoxParams boxParams = new GLTFExtensionAsoboBoxParams();
                        float height = GetGizmoParameter(node, "BoxGizmo", "height");
                        float width = GetGizmoParameter(node, "BoxGizmo","width");
                        float length = GetGizmoParameter(node,"BoxGizmo", "length");
                        collider.Translation = GetTranslation(node,maxNode);
                        float[] rotation = GetRotation(node, maxNode);
                        if (!IsDefaultRotation(rotation))
                        {
                            collider.Rotation = rotation;
                        }
                        boxParams.height = height;
                        boxParams.length = length;
                        boxParams.width = width;
                        collider.Params = boxParams;
                        collider.Type = "box";
                        collisions.Add(collider);
                    }
                    else if (new ClassIDWrapper(obj.ClassID).Equals(CylinderColliderClassID))
                    {
                        GLTFExtensionCollider collider = new GLTFExtensionCollider();
                        GLTFExtensionAsoboCylinderParams cylinderParams = new GLTFExtensionAsoboCylinderParams();
                        float radius = GetGizmoParameter(node,"CylGizmo", "radius");
                        float height = GetGizmoParameter(node,"CylGizmo", "height");
                        collider.Translation = GetTranslation(node,maxNode);
                        float[] rotation = GetRotation(node, maxNode);
                        if (!IsDefaultRotation(rotation))
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
                        float radius = GetGizmoParameter(node,"SphereGizmo", "radius");
                        collider.Translation = GetTranslation(node,maxNode);
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

        private float GetGizmoParameter(IINode node, string gizmoClass, string paramName)
        {
            string mxs = $"(maxOps.getNodeByHandle {node.Handle}).{gizmoClass}.{paramName}";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            var r=  mxsRetVal.F;
            return r;
        }

        private float[] GetTranslation(IINode node,IINode renderedNode)
        {
            float[] res = new float[3];
            string mxs = $"((maxOps.getNodeByHandle {node.Handle}).center * inverse (maxOps.getNodeByHandle {renderedNode.Handle}).transform) as string";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);
            if (!string.IsNullOrEmpty(mxsRetVal.S))
            {
                float[] r = PointStringToVector3(mxsRetVal.S);
                res[0] = -r[0];
                res[1] = r[2];
                res[2] = r[1];
            }
            
            return res;
        }

        private float[] GetRotation(IINode node,IINode renderedNode)
        {
            float[] res = new float[4];
            string mxs = $"((maxOps.getNodeByHandle {node.Handle}).rotation * inverse (maxOps.getNodeByHandle {renderedNode.Handle}).transform) as string";
            IFPValue mxsRetVal = Loader.Global.FPValue.Create();
            Loader.Global.ExecuteMAXScriptScript(mxs, true, mxsRetVal, true);

            if (!string.IsNullOrEmpty(mxsRetVal.S))
            {
                float[] r = QuaternionStringToVector4(mxsRetVal.S);
                //max to babylon
                BabylonQuaternion qFix = new BabylonQuaternion((float) Math.Sin(Math.PI / 4), 0, 0, (float) Math.Cos(Math.PI / 4));
                BabylonQuaternion quaternion = new BabylonQuaternion(r[0], r[1], r[2], r[3]);
                BabylonQuaternion rotationQuaternion = quaternion.MultiplyWith(qFix);

                //babylon to GLTF
                res[0] = -rotationQuaternion.X;
                res[1] = -rotationQuaternion.Z;
                res[2] = rotationQuaternion.Y;
                res[3] = rotationQuaternion.W;
            }

            return res;
        }

        private bool IsDefaultRotation(float[] rotation)
        {
            if (rotation[0] == 0 && rotation[1] == 0 && rotation[2] == 0 && rotation[3] == 1) return true;
            return false;
        }

        private float[] PointStringToVector3(string pointString)
        {
            string[] mxsRes = pointString.Substring(1, pointString.Length - 2).Split(',');
            float[] result = new float[mxsRes.Length];
            for (int i = 0; i < mxsRes.Length; i++)
            {
                float r = 0;
                float.TryParse(mxsRes[i], out r);
                result[i] = r;
            }

            return result;
        }

        private float[] QuaternionStringToVector4(string quaternionString)
        {
            quaternionString = quaternionString.Replace("quat ", "");
            quaternionString = quaternionString.Substring(1, quaternionString.Length - 2);
            string[] mxsRes = quaternionString.Split(' ');
            float[] result = new float[mxsRes.Length];
            for (int i = 0; i < mxsRes.Length; i++)
            {
                float r = 0;
                float.TryParse(mxsRes[i], out r);
                result[i] = r;
            }

            return result;
        }
    }
}
