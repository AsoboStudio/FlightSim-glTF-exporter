using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ActionItem = Autodesk.Max.Plugins.ActionItem;
using System.Linq;

namespace Max2Babylon
{
    public class BabylonResolveUniqueIDActionItem : ActionItem
    {
        NativeWindow parentWindow;
        MessageBox dialog;
        public override bool ExecuteAction()
        {
            Dictionary<string,IINode> uniqueIDs = new Dictionary<string,IINode>();
            foreach (IINode iNode in Loader.Core.RootNode.NodeTree())
            {
                if (iNode.GetBoolProperty("flightsim_uniqueIDresolved")) continue;
                string id = iNode.GetUniqueID();
                KeyValuePair<string,IINode> element = uniqueIDs.FirstOrDefault(x => x.Key.ToUpper() == id.ToUpper());
                if (!element.Equals(default(KeyValuePair<string, IINode>)))
                {
                    var rand = new Random();
                    id = iNode.Name + $"_{rand.Next(1000000)}{iNode.Handle}";
                    iNode.SetStringProperty("flightsim_uniqueID", id);
                    iNode.SetUserPropBool("flightsim_uniqueIDresolved", true);
                }
                else 
                {
                    iNode.SetUserPropBool("flightsim_uniqueIDresolved", true);
                    iNode.SetStringProperty("flightsim_uniqueID", id);
                }
                uniqueIDs.Add(id,iNode);
            }

            MessageBox.Show("UniqueID has been resolved...please save the scene to apply those modifications");

            return true;
        }

        public void Close()
        {
        }

        public override int Id_
        {
            get { return 1; }
        }

        public override string ButtonText
        {
            get { return "Babylon Resolve UniqueIDs"; }
        }

        public override string MenuText
        {
            get { return "&Babylon Resolve UniqueIDs..."; }
        }

        public override string DescriptionText
        {
            get { return "Resolve UniqueIDs in scene"; }
        }

        public override string CategoryText
        {
            get { return "Babylon"; }
        }

        public override bool IsChecked_
        {
            get { return false; }
        }

        public override bool IsItemVisible
        {
            get { return true; }
        }

        public override bool IsEnabled_
        {
            get { return true; }
        }
    }
}
