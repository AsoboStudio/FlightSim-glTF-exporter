using Autodesk.Max;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Max2Babylon.PreExport
{
     public class PreExportProcess
     {
        private ExportParameters exportParameters;
        public ILoggingProvider logger;

        public PreExportProcess(ExportParameters _exportParameters)
        {
            exportParameters = _exportParameters;
        }

        private bool IsMeshFlattenable(IINode node, AnimationGroupList animationGroupList, ref List<IINode> flattenableNodes)
        {
            //a node can't be flatten if:
            //- is marked as not flattenable
            //- is hidden
            //- is not selected when exportOnlyselected is checked
            //- is part of animation group
            //- is skinned
            //- is linked to animated node

            if (node.IsMarkedAsNotFlattenable()) return false;

            if (node.IsRootNode)
            {
                for (int i = 0; i < node.NumChildren; i++)
                {
                    IINode n = node.GetChildNode(i);
                    return IsMeshFlattenable(n, animationGroupList, ref flattenableNodes);
                }
                return false;
            }

            if (!exportParameters.exportHiddenObjects && node.IsNodeHidden(false)) return false;

            if (exportParameters.exportOnlySelected && !node.IsNodeSelected()) return false;

            if (node.IsSkinned())
            {
                string message = $"{node.Name} can't be flatten, because is skinned";
                logger?.RaiseMessage(message, 0);
                for (int i = 0; i < node.NumChildren; i++)
                {
                    IINode n = node.GetChildNode(i);
                    return IsMeshFlattenable(n, animationGroupList, ref flattenableNodes);
                }
                return false;

            }

            if (node.IsNodeTreeAnimated())
            {
                string message = $"{node.Name} can't be flatten, his hierarchy contains animated node";
                logger?.RaiseMessage(message, 0);
                for (int i = 0; i < node.NumChildren; i++)
                {
                    IINode n = node.GetChildNode(i);
                    return IsMeshFlattenable(n, animationGroupList, ref flattenableNodes);
                }
                return false;
            }

            if (node.IsInAnimationGroups(animationGroupList))
            {
                string message = $"{node.Name} can't be flatten, because is part of an AnimationGroup";
                logger?.RaiseMessage(message, 0);
                for (int i = 0; i < node.NumChildren; i++)
                {
                    IINode n = node.GetChildNode(i);
                    return IsMeshFlattenable(n, animationGroupList, ref flattenableNodes);
                }
                return false;
            }

            flattenableNodes.Add(node);
            return true;
        }

        public void FlattenItem(ref IINode itemNode)
        {
            AnimationGroupList animationGroupList = new AnimationGroupList();
            animationGroupList.LoadFromData(Loader.Core.RootNode);

            if (itemNode == null)
            {
                string message = "Flattening nodes of scene not supported...";
                logger?.RaiseMessage(message, 0);
            }
            else
            {
                string message = $"Flattening child nodes of {itemNode.Name}...";
                logger?.RaiseMessage(message, 0);
                List<IINode> flattenableNodes = new List<IINode>();
                if (IsMeshFlattenable(itemNode, animationGroupList, ref flattenableNodes))
                {
                    itemNode = itemNode.FlattenHierarchy();
                }


            }
        }

        public void BakeAnimationsFrame(IINode node, BakeAnimationType bakeAnimationType)
        {
            if (bakeAnimationType == BakeAnimationType.DoNotBakeAnimation) return;

            IINode hierachyRoot = (node != null) ? node : Loader.Core.RootNode;

#if MAX2020 || MAX2021
            var tobake = Loader.Global.INodeTab.Create();
#else
            var tobake = Loader.Global.NodeTab.Create();
#endif
            if (bakeAnimationType == BakeAnimationType.BakeSelective)
            {
                foreach (IINode iNode in hierachyRoot.NodeTree())
                {
                    if (iNode.IsMarkedAsObjectToBakeAnimation())
                    {
                        tobake.AppendNode(iNode, false, 0);
                    }
                }
            }


            if (!hierachyRoot.IsRootNode) tobake.AppendNode(hierachyRoot, false, 0);

            Loader.Core.SelectNodeTab(tobake, true, false);

            if (bakeAnimationType == BakeAnimationType.BakeAllAnimations)
            {
                foreach (IINode n in Tools.ITabToIEnumerable(tobake))
                {
                    n.SetUserPropBool("babylonjs_BakeAnimation", true);
                }
            }

            ScriptsUtilities.ExecuteMaxScriptCommand(@"
                for obj in selection do 
                (
                    tag = getUserProp obj ""babylonjs_BakeAnimation""
                    if tag!=true then continue

                    tmp = Point()
                    --store anim to a point
                    for t = animationRange.start to animationRange.end do (
                       with animate on at time t tmp.transform = obj.transform
                       )

                    --remove constraint on original object
                    obj.transform.controller = Link_Constraint ()
                    obj.transform.controller = prs ()
                    obj.transform = tmp.transform

                    --copy back anim from point
                    for t = animationRange.start to animationRange.end do (
                       with animate on at time t obj.transform = tmp.transform
                       )
                    delete tmp
                )
             ");

        }

        public void ExportClosedContainers()
        {
            List<IILayer> containerLayers = new List<IILayer>();
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (IIContainerObject containerObject in sceneContainers)
            {
                if (!containerObject.IsInherited) continue;
                containerLayers.Clear();
                ScriptsUtilities.ExecuteMaxScriptCommand($@"(getNodeByName(""{containerObject.ContainerNode.Name}"")).LoadContainer()");
                ScriptsUtilities.ExecuteMaxScriptCommand($@"(getNodeByName(""{containerObject.ContainerNode.Name}"")).UpdateContainer()");
                bool makeUnique = containerObject.MakeUnique;
                containerLayers = containerObject.GetContainerLayers();
                foreach (var layer in containerLayers)
                {
                    layer.Hide(false, false);
                }
                foreach (IINode node in containerObject.ContainerNode.NodeTree())
                {
                    node.Hide(false);
                }

                logger?.Print($"Update and merge container {containerObject.ContainerNode.Name}...", Color.Green);

            }
            AnimationGroupList.LoadDataFromAllContainers();
        }

        public void MergeAllXrefRecords()
        {
            if (Loader.IIObjXRefManager.RecordCount <= 0) return;
            while (Loader.IIObjXRefManager.RecordCount > 0)
            {
                var record = Loader.IIObjXRefManager.GetRecord(0);
                logger?.Print($"Merge XRef record {record.SrcFile.FileName}...", Color.Black);
                Loader.IIObjXRefManager.MergeRecordIntoScene(record);
                //todo: load data from animation helper of xref scene merged
                //to prevent to load animations from helper created without intention
            }
            AnimationGroupList.LoadDataFromAnimationHelpers();
        }

        public void ApplyPreExport()
        {
            var watch = new Stopwatch();
            watch.Start();
            IINode exportNode = null;
            if (exportParameters is MaxExportParameters)
            {
                MaxExportParameters maxExporterParameters = (exportParameters as MaxExportParameters);
                exportNode = maxExporterParameters.exportNode;

                if (maxExporterParameters.usePreExportProcess)
                {
                    if (maxExporterParameters.mergeContainersAndXRef)
                    {
                        string message = "Merging containers and Xref...";
                        logger?.Print(message, Color.Black, 0);
                        ExportClosedContainers();
                        MergeAllXrefRecords();
#if DEBUG
                        var containersXrefMergeTime = watch.ElapsedMilliseconds / 1000.0;
                        logger?.Print(string.Format("Containers and Xref  merged in {0:0.00}s", containersXrefMergeTime), Color.Blue);
#endif
                    }
                    BakeAnimationsFrame(exportNode, maxExporterParameters.bakeAnimationType);
                }
            }
            watch.Stop();
        }

        public void RevertScene()
        {
            if (exportParameters is MaxExportParameters)
            {
                MaxExportParameters maxExporterParameters = (exportParameters as MaxExportParameters);
                //flattening bake should not be reserved as it is part of a pre-process
                //if (maxExporterParameters.flattenScene)
                //{
                //    Tools.RemoveFlattenModification();
                //}

                if (!maxExporterParameters.applyPreprocessToScene && maxExporterParameters.usePreExportProcess)
                {
                    Loader.Core.SetQuietMode(true);
                    Loader.Core.LoadFromFile(Loader.Core.CurFilePath, true);
                    Loader.Core.SetQuietMode(false);
                }
            }
        }
    }
}
