
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Utilities;

namespace Max2Babylon 
{
    static class GroupsUtilities 
    {
        public static List<IINode> GetAllGroups(IINode topNode = null)
        {
            IINode startNode = (topNode == null) ? Loader.Core.RootNode : topNode;
            List <IINode> objectList = new List<IINode>();
            foreach (IINode node in startNode.NodeTree())
            {
                if (node.IsHidden(NodeHideFlags.None, false)) continue;
                if (node.IsGroupHead) objectList.Add(node);

            }
            return objectList;
        }

    }
}
