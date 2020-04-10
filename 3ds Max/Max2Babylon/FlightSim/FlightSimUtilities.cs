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

    public static class FlightSimMaterialUtilities
    {
        public static readonly MaterialUtilities.ClassIDWrapper class_ID = new MaterialUtilities.ClassIDWrapper(0x5ac74889, 0x27e705cd);

        public static bool IsFlightSimMaterial(IMtl mat)
        {
            return mat != null && class_ID.Equals(mat.ClassID);
        }

        public static bool HasFlightSimMaterials(IMtl mat)
        {
            if (mat.IsMultiMtl)
            {
                for (int i = 0; i < mat.NumSubMtls; i++)
                {
                    IMtl childMat = mat.GetSubMtl(i);
                    if (childMat!= null && class_ID.Equals(childMat.ClassID))
                    {
                        return true;
                    }
                }
            }
            else if (mat!= null && class_ID.Equals(mat.ClassID))
            {
                return true;
            }
            

            return false;
        }

        public static bool HasRuntimeAccess(IMtl mat)
        {
            if (mat.IsMultiMtl)
            {
                for (int i = 0; i < mat.NumSubMtls; i++)
                {
                    IMtl childMat = mat.GetSubMtl(i);
                    if (childMat!= null)
                    {
                        if (class_ID.Equals(childMat.ClassID))
                        {
                            int p =Tools.GetMaterialProperty(childMat, "uniqueInContainer");
                            if (Convert.ToBoolean(p))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else if (class_ID.Equals(mat.ClassID))
            { 
                int p =Tools.GetMaterialProperty(mat, "uniqueInContainer");
                if (Convert.ToBoolean(p))
                {
                    return true;
                }
            }
            return false;
        }
    }
}