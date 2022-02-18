using System.Windows.Forms;
using System.Collections.Generic;
using System;
using System.Drawing;
using Autodesk.Max;
using Max2Babylon.FlightSimExtension;
using Max2Babylon.Forms;
using Max2Babylon.FlightSim;

namespace Max2Babylon
{
    public partial class AnimationGroupControl : UserControl
    {
        enum AnimationParseType
        {
            Nodes,
            Materials
        }
        public Color ChangedTextColor { get; set; } = Color.Red;

        AnimationGroup currentInfo = null;

        // Typically called when the user presses confirm, but can also happen when scene changes are detected.
        public event Action<AnimationGroup> InfoChanged;
        public event Action<AnimationGroup> ConfirmPressed;
        private AnimationParseType currentAnimationParseType = AnimationParseType.Nodes;


        public AnimationGroupControl()
        {
            InitializeComponent();
            MaxNodeTree.BackColor = Color.DodgerBlue;
        }

        public void SetAnimationGroupInfo(AnimationGroup info)
        {
            if (info == null)
                currentInfo = null;
            else
                currentInfo = info;
            
            SetFieldsFromInfo(currentInfo);
        }

        void SetFieldsFromInfo(AnimationGroup info)
        {
            maxMaterialView.Reset();

            if (info != null)
            {
                nameTextBox.Enabled = true;
                startTextBox.Enabled = true;
                endTextBox.Enabled = true;
                keepStaticAnimBox.Enabled = true;
                keepNonAnimatedBox.Enabled = true;
                nameTextBox.Text = info.Name.ToString();
                startTextBox.Text = info.FrameStart.ToString();
                endTextBox.Text = info.FrameEnd.ToString();
                keepStaticAnimBox.Checked = info.KeepStaticAnimation;
                keepNonAnimatedBox.Checked = info.KeepNonAnimated;

                // a color can still be red after setting the string:
                // possible if we change a name, don't confirm and switch to another item with the same name
                ResetChangedTextBoxColors();

                MaxNodeTree.BeginUpdate();
                //here we garanty retrocompatibility
                MaxNodeTree.QueueSetNodes(info.NodeGuids.ToHandles(), false);
                List<uint> handles;
                MaxNodeTree.ApplyQueuedChanges(out handles, false);

                MaxNodeTree.EndUpdate();
                
                maxMaterialView.FillMaterialsView(info.MaterialGuids);

                // if the nodes changed on max' side, even though the data has not changed, the list may be different (e.g. deleted nodes)
                // since we haven't loaded the list before, we can't compare it to the node tree
                // thus, we save it, and the property checks for actual differences (and set isdirty to true)
                info.NodeGuids = handles.ToGuids();
                info.MaterialGuids = maxMaterialView.GetMaterialGUIDs();

                if (info.IsDirty)
                {
                    //this is causing the event select changed to be triggered multiple times
                    //InfoChanged?.Invoke(info);
                }
            }
            else
            {
                nameTextBox.Enabled = false;
                startTextBox.Enabled = false;
                endTextBox.Enabled = false;
                keepStaticAnimBox.Enabled = false;
                keepNonAnimatedBox.Enabled = false;
                nameTextBox.Text = "";
                startTextBox.Text = "";
                endTextBox.Text = "";
                keepStaticAnimBox.Checked = false;

                MaxNodeTree.BeginUpdate();
                MaxNodeTree.QueueSetNodes(null, false);
                List<uint> handles;
                MaxNodeTree.ApplyQueuedChanges(out handles, false);
                MaxNodeTree.EndUpdate();
                
            }
        }
        
        void ResetChangedTextBoxColors()
        {
            nameTextBox.ForeColor = DefaultForeColor;
            startTextBox.ForeColor = DefaultForeColor;
            endTextBox.ForeColor = DefaultForeColor;
            keepStaticAnimBox.ForeColor = DefaultForeColor;
            keepNonAnimatedBox.ForeColor = DefaultForeColor;
        }


        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            bool changed = currentInfo != null && nameTextBox.Text != currentInfo.Name;
            nameTextBox.ForeColor = changed ? ChangedTextColor : DefaultForeColor;
        }

        private void startTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currentInfo == null)
            {
                startTextBox.ForeColor = DefaultForeColor;
                return;
            }

            int newFrameStart;

            if (!int.TryParse(startTextBox.Text, out newFrameStart))
                newFrameStart = currentInfo.FrameStart;

            bool changed = newFrameStart != currentInfo.FrameStart;
            startTextBox.ForeColor = changed ? ChangedTextColor : DefaultForeColor;
        }

        private void endTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currentInfo == null)
            {
                endTextBox.ForeColor = DefaultForeColor;
                return;
            }

            int newFrameEnd;

            if (!int.TryParse(endTextBox.Text, out newFrameEnd))
                newFrameEnd = currentInfo.FrameEnd;

            bool changed = newFrameEnd != currentInfo.FrameEnd;
            endTextBox.ForeColor = changed ? ChangedTextColor : DefaultForeColor;
        }
        

        private void confirmButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            AnimationGroup confirmedInfo = currentInfo;

            string newName = nameTextBox.Text;

            bool newKeepEmpty = keepStaticAnimBox.Checked;
            bool newKeepNonAnimated = keepNonAnimatedBox.Checked;

            int newFrameStart;

            if (!int.TryParse(startTextBox.Text, out newFrameStart))
                newFrameStart = confirmedInfo.FrameStart;
            int newFrameEnd;
            if (!int.TryParse(endTextBox.Text, out newFrameEnd))
                newFrameEnd = confirmedInfo.FrameEnd;

            List<uint> newHandles;
            bool nodesChanged = MaxNodeTree.ApplyQueuedChanges(out newHandles);
            IList<Guid> newMaterialGUIDs;
            bool materialsChanged = maxMaterialView.ApplyMaterialsChanges(out newMaterialGUIDs);

            bool changed = newKeepEmpty != confirmedInfo.KeepStaticAnimation 
                        || newName != confirmedInfo.Name 
                        || newFrameStart != confirmedInfo.FrameStart 
                        || newFrameEnd != confirmedInfo.FrameEnd 
                        || nodesChanged 
                        || materialsChanged
                        || newKeepNonAnimated != confirmedInfo.KeepNonAnimated;
            
            if (!changed)
                return;

            confirmedInfo.Name = newName;
            confirmedInfo.FrameStart = newFrameStart;
            confirmedInfo.FrameEnd = newFrameEnd;
            confirmedInfo.KeepStaticAnimation = newKeepEmpty;
            confirmedInfo.KeepNonAnimated = newKeepNonAnimated;

            if (nodesChanged)
            {
                confirmedInfo.NodeGuids = newHandles.ToGuids();
                if (confirmedInfo.AnimationGroupNodes == null)
                {
                    confirmedInfo.AnimationGroupNodes = new List<AnimationGroupNode>();
                }
                
                foreach (uint handle in newHandles)
                {
                    IINode node = Loader.Core.GetINodeByHandle(handle);
                    if (node != null)
                    {
                        string name = node.Name;
                        string parentName = node.ParentNode.Name;
                        AnimationGroupNode nodeData = new AnimationGroupNode(node.GetGuid(), name, parentName);
                        confirmedInfo.AnimationGroupNodes.Add(nodeData);
                    }
                }
            }

            if (materialsChanged)
            {
                confirmedInfo.MaterialGuids = newMaterialGUIDs;

                if (confirmedInfo.AnimationGroupMaterials == null)
                {
                    confirmedInfo.AnimationGroupMaterials = new List<AnimationGroupMaterial>();
                }
                
                foreach (Guid guid in newMaterialGUIDs)
                {
                    IMtl mat = Tools.GetIMtlByGuid(guid);
                    if (mat != null)
                    {
                        string name = mat.Name;
                        AnimationGroupMaterial matData = new AnimationGroupMaterial(guid, name);
                        confirmedInfo.AnimationGroupMaterials.Add(matData);
                    }
                }
            }

            ResetChangedTextBoxColors();
            MaxNodeTree.SelectedNode = null;

            InfoChanged?.Invoke(confirmedInfo);
            ConfirmPressed?.Invoke(confirmedInfo);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            SetFieldsFromInfo(currentInfo);
        }


        private void addSelectedButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            if (currentAnimationParseType == AnimationParseType.Nodes)
            {
                MaxNodeTree.BeginUpdate();
                for (int i = 0; i < Loader.Core.SelNodeCount; ++i)
                {
                    IINode node = Loader.Core.GetSelNode(i);

                    //added in flightsim to add lod node "x0_name" and all other lod relative
                    // x1_name,x2_name etc
                    //todo expost addnode to maxscript and call this outside
                    if (node.Name.StartsWith("x"))
                    {
                        string lod_name = node.Name.Substring(3);
                        string lod_prefix = node.Name.Replace(lod_name, "");
                        if (lod_prefix.ToCharArray()[0] == 'x' && lod_prefix.ToCharArray()[2] == '_')
                        {
                            for (int j = 0; j < 7; j++)
                            {
                                string relativeLodName = "x" + j + "_" + lod_name;
                                IINode relativeLodNode = Loader.Core.GetINodeByName(relativeLodName);
                                if (relativeLodNode != null)
                                {
                                    MaxNodeTree.QueueAddNode(relativeLodNode);
                                }
                            }
                        }
                        else
                        {
                            MaxNodeTree.QueueAddNode(node);
                        }
                    }
                    else
                    {
                        MaxNodeTree.QueueAddNode(node);
                    }


                }

                MaxNodeTree.EndUpdate();
            }


            if (currentAnimationParseType==AnimationParseType.Materials)
            {
                if(Loader.Core.SelNodeCount< 1)
                { 
                    MessageBox.Show("You need to select at least one Node");
                    return;
                }
                for (int i = 0; i < Loader.Core.SelNodeCount; ++i)
                {
                    IINode node = Loader.Core.GetSelNode(i);
                    IMtl material = node.GetAnimatableMaterial();
                    if(material!=null) maxMaterialView.AddMaterialFromSelection(material);
                }               
                
            }
        }

        private void removeNodeButton_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            if (currentAnimationParseType==AnimationParseType.Nodes)
            {
                MaxNodeTree.BeginUpdate();
                for (int i = 0; i < Loader.Core.SelNodeCount; ++i)
                {
                    IINode node = Loader.Core.GetSelNode(i);
                    MaxNodeTree.QueueRemoveNode(node);
                }
                MaxNodeTree.EndUpdate();
            }

            if (currentAnimationParseType==AnimationParseType.Materials)
            {
                if (Loader.Core.SelNodeCount < 1)
                {
                    MessageBox.Show("You need to select at least one Node");
                    return;
                }
                for (int i = 0; i < Loader.Core.SelNodeCount; ++i)
                {
                    IINode node = Loader.Core.GetSelNode(i);
                    IMtl material = node.GetAnimatableMaterial();
                    if(material!=null)maxMaterialView.RemoveMaterialFromSelection(material);
                }
                
            }
        }

		private void calculateTimeRangeBtn_Click(object sender, EventArgs e)
        {
            if (currentInfo == null)
                return;

            endTextBox.Text = Tools.CalculateEndFrameFromAnimationGroupNodes(currentInfo).ToString();
        }

        private void OnMaterialFocusEnter(object sender, EventArgs e)
        {
            currentAnimationParseType = AnimationParseType.Materials;
            maxMaterialView.BackColor = Color.DodgerBlue;
            MaxNodeTree.BackColor = Color.White;
        }

        private void OnNodesFocusEnter(object sender, EventArgs e)
        {
            currentAnimationParseType = AnimationParseType.Nodes;
            MaxNodeTree.BackColor = Color.DodgerBlue;
            maxMaterialView.BackColor = Color.White;
        }

        private void keepStaticAnimation_CheckedChanged(object sender, EventArgs e)
        {
            if (currentInfo == null)
            {
                keepStaticAnimBox.ForeColor = DefaultForeColor;
                return;
            }

            bool state = false;

            state = keepStaticAnimBox.Checked;

            bool changed = state != currentInfo.KeepStaticAnimation;
            keepStaticAnimBox.ForeColor = changed ? ChangedTextColor : DefaultForeColor;
        }

        private void keepNonAnimated_CheckedChanged(object sender, EventArgs e)
        {
            if (currentInfo == null)
            {
                keepNonAnimatedBox.ForeColor = DefaultForeColor;
                return;
            }

            bool state = false;

            state = keepNonAnimatedBox.Checked;

            bool changed = state != currentInfo.KeepNonAnimated;
            keepNonAnimatedBox.ForeColor = changed ? ChangedTextColor : DefaultForeColor;
        }
    }
}
