﻿using System;
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
            ITab<IINode> nodes = Loader.Global.INodeTabNS.Create();
            IILayerProperties layerProperties = Loader.IIFPLayerManager.GetLayer(layer.Name);
            layerProperties.Nodes(nodes);

            foreach (IINode n in ITabToIEnumerable(nodes))
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

        /// <summary>
        /// Converts the ITab to a more convenient IEnumerable.
        /// </summary>
        public static IEnumerable<T> ITabToIEnumerable<T>(ITab<T> tab)
        {
#if MAX2015
            for (int i = 0; i < tab.Count; i++)
            {
                yield return tab[(IntPtr)i];
            }
#else
            for (int i = 0; i < tab.Count; i++)
            {
                yield return tab[i];
            }
#endif
                
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
    }
}
