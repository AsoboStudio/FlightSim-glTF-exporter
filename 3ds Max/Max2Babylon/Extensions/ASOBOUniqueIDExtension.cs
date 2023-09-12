using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Babylon2GLTF;
using BabylonExport.Entities;
using GLTFExport.Entities;

namespace BabylonExport.Entities
{
    public partial class BabylonNode
    {
        private string uniqueId;
        public string UniqueID
        {
            get
            {
                if (string.IsNullOrWhiteSpace(uniqueId))
                {
                    return name;
                }

                return uniqueId;
            }
            set => uniqueId = value;
        }
    }

}

namespace Babylon2GLTF
{
    #region Serializable glTF Objects

    [DataContract]
    class GLTFExtensionASBUniqueID: GLTFProperty
    {
        [DataMember(EmitDefaultValue = false)]
        public string id { get; set; }
    }

    #endregion

    public partial class GLTFExporter
    {
        private const string AsoboUniqueID = "ASOBO_unique_id";

        public void ASOBOUniqueIDExtension(ref GLTF gltf, ref GLTFNode gltfNode,BabylonNode babylonNode )
        {
            GLTFExtensionASBUniqueID extensionObject = new GLTFExtensionASBUniqueID
            {
                id = babylonNode.UniqueID
            };

            if (gltfNode != null)
            {
                if (gltfNode.extensions == null)
                {
                    gltfNode.extensions = new GLTFExtensions();
                }
                gltfNode.extensions[AsoboUniqueID] = extensionObject;
            }

            if (gltf.extensionsUsed == null)
            {
                gltf.extensionsUsed = new List<string>();
            }
            if (!gltf.extensionsUsed.Contains(AsoboUniqueID))
            {
                gltf.extensionsUsed.Add(AsoboUniqueID);
            }
        }
    }
    
}


namespace Max2Babylon.FlightSimExtension
{


    class AsoboUniqueIDExtension : IBabylonExtensionExporter
    {
        #region Implementation of IBabylonExtensionExporter

        public string GetGLTFExtensionName()
        {
            return "ASOBO_unique_id";
        }

        public ExtendedTypes GetExtendedType()
        {
            return new ExtendedTypes(typeof(GLTFScene));
        }

        public bool ExportBabylonExtension<T>(T babylonObject, ref BabylonScene babylonScene, BabylonExporter exporter)
        {
            // just skip this extension is ment only for GLTF
            return false;
        }

        public object ExportGLTFExtension<T1, T2>(T1 babylonObject, ref T2 gltfObject, ref GLTF gltf, GLTFExporter exporter, ExtensionInfo extInfo)
        {
            if (!exporter.exportParameters.exportAsSubmodel) return null;

            var babylonScene = babylonObject as BabylonScene;
            var gltfScene = gltfObject as GLTFScene;


            if (gltf.scenes[0] != gltfScene) 
            {
                return null;
            } 
            else // execute this extension only one time
            {
                GLTFScene[] subScenes = new GLTFScene[0];
                GLTFExtensionASBUniqueID uniqueIDext = null;
                foreach (BabylonNode rootNode in babylonScene.RootNodes)
                {
                    GLTFNode subSceneGltfRoot = exporter.nodeToGltfNodeMap.First(n => n.Key.id == rootNode.id).Value;
                    int subSceneRootIndex = subSceneGltfRoot.index;

                    GLTFScene subScene = new GLTFScene();
                    subScene.nodes = new int[subSceneRootIndex];
                    subScene.NodesList.Add(subSceneRootIndex);

                    Guid subSceneRootGUID = new Guid(rootNode.id);
                    IINode maxNode = Tools.GetINodeByGuid(subSceneRootGUID);

                    if (!maxNode.ParentNode.IsRootNode) 
                    {
                        string subSceneParentId = maxNode.ParentNode.GetUniqueID();
                        uniqueIDext = new GLTFExtensionASBUniqueID();
                        uniqueIDext.id = subSceneParentId;
                        subScene.extensions.Add(GetGLTFExtensionName(), uniqueIDext);
                    };


                    Array.Resize(ref subScenes, subScenes.Length + 1);
                    subScenes[subScenes.Length - 1] = subScene;
                }

                return uniqueIDext;
            }
            
        }
        #endregion

    }
}
