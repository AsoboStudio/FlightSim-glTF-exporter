using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Max;
using Max2Babylon.FlightSim;

namespace Max2Babylon
{
    public partial class MaxMaterialView : ListView
    {
        private Dictionary<Guid,ListViewItem> previousMaterialMap= new Dictionary<Guid,ListViewItem>();
        private Dictionary<Guid,ListViewItem> currentMaterialMap= new Dictionary<Guid,ListViewItem>();

        private ListViewItem FillMaterialItem(IMtl material,Guid guid)
        {
            if(previousMaterialMap.ContainsKey(guid)) return null;
            ListViewItem item = new ListViewItem();
            item.Text = material.Name;
            Items.Add(item);
            return item;
        }

        public void AddMaterial(IMtl material)
        {
            if(material==null) return;
            if(!FlightSimMaterialUtilities.IsFlightSimMaterial(material)) return;
            Guid matGuid = material.GetGuid();
            ListViewItem item = FillMaterialItem(material,matGuid);
            if (item!=null) previousMaterialMap.Add(matGuid,item);
        }

        public void AddMaterialFromSelection(IMtl material)
        {
            if(material==null) return;
            if(!MaterialUtilities.IsMaterialAssignedInScene(material)) return;
            if(!FlightSimMaterialUtilities.IsFlightSimMaterial(material)) return;
            Guid matGuid = material.GetGuid();
            ListViewItem item = FillMaterialItem(material,matGuid);
            if (item != null)
            {
                currentMaterialMap.Add(matGuid,item);
                item.BackColor = Color.Green;
            }
        }

        public void RemoveMaterialFromSelection(IMtl material)
        {
            if(material==null) return;
            if(!FlightSimMaterialUtilities.IsFlightSimMaterial(material)) return;
            Guid matGuid = material.GetGuid();
            ListViewItem itemToRemove;
            if (previousMaterialMap.TryGetValue(matGuid, out itemToRemove))
            {
                itemToRemove.BackColor = Color.Red;
                currentMaterialMap.Remove(matGuid);
            }
        }

        public IList<Guid> GetMaterialGUIDs()
        {
            IList<Guid> materialGUIDs = new List<Guid>();
            if (previousMaterialMap.Count > 0)
            {
                materialGUIDs = previousMaterialMap.Keys.ToList();
            }
            return materialGUIDs;
        }

        public void FillMaterialsView(IList<Guid> infoMaterialGuids)
        {
            Items.Clear();
            foreach (var dicKeyValue in Tools.guids)
            {
                if (infoMaterialGuids.Contains(dicKeyValue.Key))
                {
                    IMtl animMat = dicKeyValue.Value as IMtl;
                    AddMaterial(animMat);
                }
            }

            currentMaterialMap = previousMaterialMap.ToDictionary(entry => entry.Key, entry => (ListViewItem) entry.Value.Clone());;
        }

        public bool ApplyMaterialsChanges(out IList<Guid> materialsGUIDchanged)
        {
            bool isChanged = false;
            materialsGUIDchanged = new List<Guid>();

            foreach (KeyValuePair<Guid, ListViewItem> keyValuePair in currentMaterialMap)
            {
                if (!previousMaterialMap.ContainsKey(keyValuePair.Key)) isChanged = true;
            }
            foreach (KeyValuePair<Guid, ListViewItem> keyValuePair in previousMaterialMap)
            {
                if (!currentMaterialMap.ContainsKey(keyValuePair.Key)) isChanged = true;
            }

            if (isChanged)
            {
                materialsGUIDchanged = currentMaterialMap.Keys.ToList();
                foreach (ListViewItem item in currentMaterialMap.Values)
                {
                    item.BackColor = Color.White;
                }
                return true;
            }
            return false;
        }

        public void Reset()
        {
            base.Clear();
            previousMaterialMap = new Dictionary<Guid, ListViewItem>();
            currentMaterialMap = new Dictionary<Guid, ListViewItem>();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MaxMaterialView
            // 
            this.View = System.Windows.Forms.View.List;
            this.ResumeLayout(false);

        }
    }
}
