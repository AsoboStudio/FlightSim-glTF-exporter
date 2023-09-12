using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using ExplorerFramework;
using ManagedServices;
using MaxCustomControls.SceneExplorerControls;
using SceneExplorer;
namespace Max2Babylon
{
    static class LayerUtilities
    {
        public static List<IILayer> GetSelectedLayers(SceneExplorerDialog sceneExplorer)
        {
            List<IILayer> selectedLayer = new List<IILayer>();
            TraversalNode[] traversalNodes = sceneExplorer.ExplorerControl.GetSelectedNodes(false, false, false);
            foreach (TraversalNode traversalNode in traversalNodes)
            {
                if (INodeUtilities.IsMaxLayerTraversalNode(traversalNode))
                {
                    ITraversalILayer l = traversalNode as ITraversalILayer;
                    IILayer layer = Loader.Core.LayerManager.GetLayer(l.SceneName);
                    selectedLayer.Add(layer);
                }
            }
            return selectedLayer;
        }

        public static bool HaveNode(this List<IILayer> layers, IINode node)
        {
            foreach (IILayer iLayer in layers)
            {
                if (iLayer.HasNode(node)) return true;
            }
            return false;
        }

        public static bool HasNode(this IILayer layer,IINode node,bool checkInChild = true)
        {
#if MAX2020 || MAX2021 || MAX2022 || MAX2023
            ITab<IINode> nodes = Loader.Global.INodeTab.Create();
#else
            ITab<IINode> nodes = Loader.Global.INodeTabNS.Create();
#endif
            IILayerProperties layerProperties = Loader.IIFPLayerManager.GetLayer(layer.Name);
            layerProperties.Nodes(nodes);

            foreach (IINode n in nodes.ToIEnumerable())
            {
                if (node.Handle == n.Handle) return true;
            }

            for (int i = 0; i < layer.NumOfChildLayers; i++)
            {
                IILayer child = layer.GetChildLayer(i);
                if (child.HasNode(node, checkInChild))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainLayer(this List<IILayer> layers, IILayer layer)
        {
            foreach (IILayer iLayer in layers)
            {
                if (iLayer.Name == layer.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<IILayer> RootLayers()
        {
            List<IILayer> rootILayers = new List<IILayer>();
            int layerCount = Loader.IIFPLayerManager.Count;
            for (int i = 0; i < layerCount; i++)
            {
                IILayerProperties l = Loader.IIFPLayerManager.GetLayer(i);
                if (l.ParentLayerProperties == null)
                {
                    rootILayers.Add(Loader.Core.LayerManager.GetLayer(i));
                }
            }

            return rootILayers;
        }

        public static IEnumerable<IILayer> LayerTree(this IILayer layer)
        {
            for (int i = 0; i < layer.NumOfChildLayers; i++)
            {
                yield return layer.GetChildLayer(i);
                foreach (var y in layer.GetChildLayer(i).LayerTree())
                    yield return y;
            }
        }

        public static IEnumerable<IINode> LayerNodes(this IILayer layer)
        {
            IILayerProperties layerProp = Loader.IIFPLayerManager.GetLayer(layer.Name);
#if MAX2020 || MAX2021 || MAX2022 || MAX2023
            ITab<IINode> nodes = Loader.Global.INodeTab.Create();
#else
            ITab<IINode> nodes = Loader.Global.INodeTabNS.Create();
#endif
            layerProp.Nodes(nodes);
            return nodes.ToIEnumerable();
        }

        public static IILayer GetNodeLayer(this IINode node)
        {
            int num = node.NumRefs;
            for (int i = 0; i < num; i++)
            {
                IILayer r = node.GetReference(i) as IILayer;
                if (r != null) return r;
            }
            return null;
        }

        public static List<IILayer> GetContainerLayers(this IIContainerObject container)
        {
            List<IILayer> result = new List<IILayer>();
            List<IINode> containerNodes = container.ContainerNode.ContainerNodeTree(true);
            foreach (IINode node in containerNodes)
            {
                IILayer nodeLayer = node.GetNodeLayer();
                if(nodeLayer!=null)result.Add(node.GetNodeLayer());
            }
            return result;
        }

        public static IEnumerable<IILayer> ParentsLayers(this IILayer layer)
        {
            var p = layer;
            while (p.ParentLayer != null) 
            {
                p = p.ParentLayer;
                yield return p;
            }
        }
        public static void SelectLayersChildren(IList<IILayer> exportedLayers)
        {
            if (exportedLayers == null) return;

            IINodeTab selection = Tools.CreateNodeTab();

            foreach (IILayer layer in exportedLayers)
            {
                foreach (IINode layerNode in layer.LayerNodes())
                {
                    selection.AppendNode(layerNode, false, 0);
                }
            }
            Loader.Core.SelectNodeTab(selection, true, false);
        }


        public static void ShowExportItemLayers(IList<IILayer> exportedLayers)
        {
            if (exportedLayers == null) return;

            foreach (IILayer rootLayer in LayerUtilities.RootLayers())
            {
                rootLayer.Hide(true, true);
                foreach (IILayer lay in rootLayer.LayerTree())
                {
                    lay.Hide(true, true);
                }
            }

            foreach (IILayer layer in exportedLayers)
            {
                layer.Hide(false, false);
                foreach (IINode layerNode in layer.LayerNodes())
                {
                    layerNode.Hide(false);
                }
            }
        }

    }
}
