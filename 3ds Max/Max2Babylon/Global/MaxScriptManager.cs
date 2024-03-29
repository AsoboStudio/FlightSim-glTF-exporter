using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BabylonExport.Entities;
using Utilities;
using Autodesk.Max;
using FlightSimExtension;

namespace Max2Babylon
{
    public class MaxScriptManager
    {
        //deprecated
        public static void Export( bool logInListener)
        {
            Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf("WARNING - This method is DEPRECATED use "+ "\n"+"Export(MaxExportParameters exportParameters, BabylonLogger _logger)" + "\n");
            string storedModelPath = Loader.Core.RootNode.GetStringProperty(MaxExportParameters.ModelFilePathProperty, string.Empty);
            string userRelativePath = Tools.ResolveRelativePath(storedModelPath);
            var logger = new MaxScriptLogger(logInListener);
            Export(InitParameters(userRelativePath),logger);
        }

        //deprecated
        public static void Export(string outputPath, bool logInListener)
        {
            Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf("WARNING - This method is DEPRECATED use " + "\n" + "Export(MaxExportParameters exportParameters, BabylonLogger _logger)" + "\n");
            var logger = new MaxScriptLogger(logInListener);
            Export(InitParameters(outputPath),logger);
        }

        //deprecated
        public static void Export(MaxExportParameters exportParameters, bool logInListener)
        {
            Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf("WARNING - This method is DEPRECATED use "+ "\n"+"Export(MaxExportParameters exportParameters, BabylonLogger _logger)" + "\n");
            var logger = new MaxScriptLogger(logInListener);
            Export(exportParameters, logger);
        }

        public static void Export(MaxExportParameters exportParameters, ILoggingProvider _logger)
        {
            if (Loader.Class_ID == null)
            {
                Loader.AssemblyMain();
            }
            // Check output format is valid
            List<string> validFormats = new List<string>(new string[] { "babylon", "binary babylon", "gltf", "glb" });
            if (!validFormats.Contains(exportParameters.outputFormat))
            {
                Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf("ERROR - Valid output formats are: "+ validFormats.ToArray().ToString(true) + "\n");
                return;
            }
           
            BabylonExporter exporter = new BabylonExporter(_logger);

            // Start export
            exporter.Export(exportParameters);
        }

        public static void InitializeGuidTable()
        {
            Tools.InitializeGuidsMap();
        }

        //leave the possibility to get the output path from the babylon exporter with all the settings previously-saved
        public static MaxExportParameters InitParameters(string outputPath)
        {
            long txtQuality = 100;
            float scaleFactor = 1f;

            MaxExportParameters exportParameters = new MaxExportParameters();
            exportParameters.outputPath = outputPath;
            exportParameters.outputFormat = Path.GetExtension(outputPath)?.Substring(1);
            exportParameters.textureFolder = Loader.Core.RootNode.GetStringProperty("textureFolderPathProperty", string.Empty);
            exportParameters.generateManifest = Loader.Core.RootNode.GetBoolProperty("babylonjs_generatemanifest");
            exportParameters.writeTextures = Loader.Core.RootNode.GetBoolProperty("babylonjs_writetextures");
            exportParameters.overwriteTextures = Loader.Core.RootNode.GetBoolProperty("babylonjs_overwritetextures");
            exportParameters.exportHiddenObjects = Loader.Core.RootNode.GetBoolProperty("babylonjs_exporthidden");
            exportParameters.exportAsSubmodel = Loader.Core.RootNode.GetBoolProperty("flightsim_exportAsSubmodel");
            exportParameters.autoSaveSceneFile = Loader.Core.RootNode.GetBoolProperty("babylonjs_autosave");
            exportParameters.exportOnlySelected = Loader.Core.RootNode.GetBoolProperty("babylonjs_onlySelected");
            exportParameters.exportTangents = Loader.Core.RootNode.GetBoolProperty("babylonjs_exporttangents");
            exportParameters.scaleFactor = float.TryParse(Loader.Core.RootNode.GetStringProperty("babylonjs_txtScaleFactor", "1"), out scaleFactor) ? scaleFactor : 1;
            exportParameters.txtQuality = long.TryParse(Loader.Core.RootNode.GetStringProperty("babylonjs_txtCompression", "100"), out txtQuality) ? txtQuality : 100;
            exportParameters.mergeAOwithMR = Loader.Core.RootNode.GetBoolProperty("babylonjs_mergeAOwithMR");
            exportParameters.dracoCompression = Loader.Core.RootNode.GetBoolProperty("babylonjs_dracoCompression");
            exportParameters.enableKHRLightsPunctual = Loader.Core.RootNode.GetBoolProperty("babylonjs_khrLightsPunctual");
            exportParameters.enableKHRTextureTransform = Loader.Core.RootNode.GetBoolProperty("babylonjs_khrTextureTransform");
            exportParameters.enableKHRMaterialsUnlit = Loader.Core.RootNode.GetBoolProperty("babylonjs_khr_materials_unlit");
            exportParameters.exportMaterials = Loader.Core.RootNode.GetBoolProperty("babylonjs_export_materials");

            exportParameters.exportMorphTangents = Loader.Core.RootNode.GetBoolProperty("babylonjs_export_Morph_Tangents");
            exportParameters.exportMorphNormals = Loader.Core.RootNode.GetBoolProperty("babylonjs_export_Morph_Normals");
            exportParameters.usePreExportProcess = Loader.Core.RootNode.GetBoolProperty("babylonjs_preproces");
            exportParameters.mergeContainersAndXRef = Loader.Core.RootNode.GetBoolProperty("babylonjs_mergecontainersandxref");
            exportParameters.flattenGroups = Loader.Core.RootNode.GetBoolProperty("flightsim_flattenGroups");
            exportParameters.applyPreprocessToScene = Loader.Core.RootNode.GetBoolProperty("babylonjs_applyPreprocess");

            exportParameters.pbrFull = Loader.Core.RootNode.GetBoolProperty(ExportParameters.PBRFullPropertyName);
            exportParameters.pbrNoLight = Loader.Core.RootNode.GetBoolProperty(ExportParameters.PBRNoLightPropertyName);
            exportParameters.pbrEnvironment = Loader.Core.RootNode.GetStringProperty(ExportParameters.PBREnvironmentPathPropertyName, string.Empty);
            exportParameters.exportNode = null;

            exportParameters.animationExportType =(AnimationExportType) Loader.Core.RootNode.GetFloatProperty("babylonjs_export_animations_type", 0);
            exportParameters.enableASBAnimationRetargeting =Loader.Core.RootNode.GetBoolProperty("babylonjs_asb_animation_retargeting",0);
            exportParameters.enableASBUniqueID = Loader.Core.RootNode.GetBoolProperty("flightsim_asb_unique_id",1);

            exportParameters.removeLodPrefix = Loader.Core.RootNode.GetBoolProperty("flightsim_removelodprefix");
            exportParameters.removeNamespaces = Loader.Core.RootNode.GetBoolProperty("flightsim_removenamespaces");
            exportParameters.tangentSpaceConvention =(TangentSpaceConvention)Loader.Core.RootNode.GetFloatProperty("flightsim_tangent_space_convention", 0);
            exportParameters.bakeAnimationType = (BakeAnimationType)Loader.Core.RootNode.GetFloatProperty("babylonjs_bakeAnimationsType", 0);
            exportParameters.keepInstances = Loader.Core.RootNode.GetBoolProperty("flightsim_keepInstances",1);

            exportParameters.logLevel = (LogLevel) Loader.Core.RootNode.GetFloatProperty("babylonjs_logLevel", 1);

            return exportParameters;
        }

        public static void DisableBabylonAutoSave()
        {
            Loader.Core.RootNode.SetUserPropBool("babylonjs_autosave",false);
        }

        public static void ImportAnimationGroups(string jsonPath)
        {
            AnimationGroupList animationGroups = new AnimationGroupList();
            var fileStream = File.Open(jsonPath, FileMode.Open);

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string jsonContent = reader.ReadToEnd();
                animationGroups.LoadFromJson(jsonContent);
            }
        }

        public static void MergeAnimationGroups(string jsonPath)
        {
            AnimationGroupList animationGroups = new AnimationGroupList();
            var fileStream = File.Open(jsonPath, FileMode.Open);

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string jsonContent = reader.ReadToEnd();
                animationGroups.LoadFromJson(jsonContent,true);
            }
        }

        public static void MergeAnimationGroups(string jsonPath, string old_root, string new_root)
        {
            AnimationGroupList animationGroups = new AnimationGroupList();
            var fileStream = File.Open(jsonPath, FileMode.Open);

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string jsonContent = reader.ReadToEnd();
                string textToFind = string.Format(@"\b{0}\b", old_root);
                string overridedJsonContent = Regex.Replace(jsonContent, textToFind, new_root);
                animationGroups.LoadFromJson(overridedJsonContent, true);
            }
        }

        public AnimationGroup GetAnimationGroupByName(string name)
        {
            AnimationGroupList animationGroupList = new AnimationGroupList();
            animationGroupList.LoadFromData();

            foreach (AnimationGroup animationGroup in animationGroupList)
            {
                if (animationGroup.Name == name)
                {
                    return animationGroup;
                }
            }

            return null;
        }

        public void AutoAssignLodInAnimationGroup()
        {
            AnimationGroupList animationGroupList = new AnimationGroupList();
            animationGroupList.LoadFromData();

            var nodes = Loader.Core.RootNode.NodeTree();

            List<IINode> nodeToAdd = new List<IINode>();
            foreach (AnimationGroup anim in animationGroupList)
            {
                nodeToAdd.Clear();
                foreach (Guid guid in anim.NodeGuids)
                {
                    IINode n = Tools.GetINodeByGuid(guid);
                    if (n == null) continue;    
                    if(!Regex.IsMatch(n.Name, "(?i)x[0-9]_")) continue;
                    string noLodName = n.Name.Substring(3);
                    foreach (IINode node in nodes)
                    {
                        if(Regex.IsMatch(node.Name,$"(?i)x[0-9]_{noLodName}$"))
                        {
                            nodeToAdd.Add(node);
                        }
                    }
                }

                foreach (IINode n in nodeToAdd)
                {
                    List<Guid> newGuids = anim.NodeGuids.ToList();
                    newGuids.Add(n.GetGuid());
                    anim.NodeGuids = newGuids;
                }
                anim.SaveToData();
            }
        }

        public AnimationGroup CreateAnimationGroup()
        {
            AnimationGroupList animationGroupList = new AnimationGroupList();
            animationGroupList.LoadFromData();

            AnimationGroup info = new AnimationGroup();

            // get a unique name and guid
            string baseName = info.Name;
            int i = 0;
            bool hasConflict = true;
            while (hasConflict)
            {
                hasConflict = false;
                foreach (AnimationGroup animationGroup in animationGroupList)
                {
                    if (info.Name.Equals(animationGroup.Name))
                    {
                        info.Name = baseName + i.ToString();
                        ++i;
                        hasConflict = true;
                        break;
                    }
                    if (info.SerializedId.Equals(animationGroup.SerializedId))
                    {
                        info.SerializedId = Guid.NewGuid();
                        hasConflict = true;
                        break;
                    }
                }
            }

            // save info and animation list entry
            animationGroupList.Add(info);
            animationGroupList.SaveToData();
            Loader.Global.SetSaveRequiredFlag(true, false);
            return info;
        }

        public string RenameAnimationGroup(AnimationGroup info,string name)
        {
            AnimationGroupList animationGroupList = new AnimationGroupList();
            animationGroupList.LoadFromData();

            AnimationGroup animGroupToRename = animationGroupList.GetAnimationGroupByName(info.Name);

            string baseName = name;
            int i = 0;
            bool hasConflict = true;
            while (hasConflict)
            {
                hasConflict = false;
                foreach (AnimationGroup animationGroup in animationGroupList)
                {
                    if (baseName.Equals(animationGroup.Name))
                    {
                        baseName = name + i.ToString();
                        ++i;
                        hasConflict = true;
                        break;
                    }
                }
            }

            animGroupToRename.Name = baseName;

            // save info and animation list entry
            animationGroupList.SaveToData();
            Loader.Global.SetSaveRequiredFlag(true, false);
            return baseName;
        }

        public void AddNodeInAnimationGroup(AnimationGroup info, uint nodeHandle)
        {
            if (info == null)
                return;

            IINode node = Loader.Core.GetINodeByHandle(nodeHandle);
            if (node == null)
            {
                return;
            }

            List<Guid> newGuids = info.NodeGuids.ToList();
            newGuids.Add(node.GetGuid());
            info.NodeGuids = newGuids;
            info.SaveToData();
        }

        public int GetTimeRange(AnimationGroup info)
        {
            return Tools.CalculateEndFrameFromAnimationGroupNodes(info);
        }

        public void SetAnimationGroupTimeRange(AnimationGroup info, int start,int end)
        {
            if (info == null)
                return;

            info.FrameStart = start;
            info.FrameEnd = end;
            info.SaveToData();
        }

        public void RemoveAllNodeFromAnimationGroup(AnimationGroup info)
        {
            if (info == null)
                return;

            info.NodeGuids = new List<Guid>();
            info.SaveToData();
        }

        public void RemoveNodeFromAnimationGroup(AnimationGroup info, uint nodeHandle)
        {
            if (info == null)
                return;

            IINode node = Loader.Core.GetINodeByHandle(nodeHandle);
            if (node == null)
            {
                return;
            }

            List<Guid> newGuids = info.NodeGuids.ToList();
            newGuids.Remove(node.GetGuid());
            info.NodeGuids = newGuids;
            info.SaveToData();
        }

        public void ExecuteLoadAnimationAction()
        {
            var loadAnimationAction = new BabylonLoadAnimations();
            loadAnimationAction.ExecuteAction();
        }

        public void InitGUID(IntPtr matHandle)
        {
            try
            {
                IAnimatable mat = Loader.Global.Animatable.GetAnimByHandle((UIntPtr)(int)matHandle);
                Guid uid = Guid.NewGuid();
                Tools.SetMaterialProperty((IMtl)mat, "guid", uid.ToString());
            }
            catch (System.Exception ex)
            {
            	Autodesk.Max.GlobalInterface.Instance.TheListener.EditStream.Printf(ex.Message);
            }
            
        }
    }
}
