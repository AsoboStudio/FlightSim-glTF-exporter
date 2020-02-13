using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Max;

namespace Max2Babylon.FlightSimExtension
{
    static class FlightSimUtilities
    {
        public static bool ExportItemHasClosedContainers(IINode itemRootNode)
        {
            if (itemRootNode == null)
            {
                itemRootNode = Loader.Core.RootNode;
            }

            List<IINode> nodesList = new List<IINode>();
            nodesList.Add(itemRootNode);

            nodesList.AddRange(itemRootNode.NodeTree().ToList());
            

            foreach (var node in nodesList)
            {
                IIContainerObject containerNode = Loader.Global.ContainerManagerInterface.IsContainerNode(node);

                if (containerNode != null && !containerNode.IsOpen)
                {
                    MessageBox.Show("You are tring to export a CLOSED Container\nUse the Container Manager");
                    return true;
                }
            }

            return false;
        }

    }
}