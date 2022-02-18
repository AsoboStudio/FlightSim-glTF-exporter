using System;
using System.Collections.Generic;
using Autodesk.Max;
using BabylonExport.Entities;
using System.Linq;

namespace Max2Babylon
{
    public enum BakeAnimationType
    {
        DoNotBakeAnimation,
        BakeAllAnimations,
        BakeSelective
    }


    public class MaxExportParameters : ExportParameters
    {
        public Autodesk.Max.IINode exportNode;
        private List<Autodesk.Max.IILayer> _exportLayers;

        public List<Autodesk.Max.IILayer> exportLayers
        {
            get { return _exportLayers; }
            set 
            { 
                _exportLayers = value;
                LayerUtilities.ShowExportItemLayers(_exportLayers);
            }
        }
        public bool usePreExportProcess = false;
        public bool applyPreprocessToScene = false;
        public bool flattenGroups = false;
        public bool mergeContainersAndXRef = false;
        public bool flattenScene = false;
        public BakeAnimationType bakeAnimationType = BakeAnimationType.DoNotBakeAnimation;
        public LogLevel logLevel = LogLevel.WARNING;
        private List<IILayer> _dependentLayers = new List<IILayer>();

        public IINode GetNodeByHandle(uint handle) 
        {
            return Loader.Core.GetINodeByHandle(handle);
        }

        public List<IILayer> NameToIILayer(string[] layers )
        {
            List<IILayer> result = new List<IILayer>();
            foreach (var l in layers)
            {
                IILayer lay = Loader.Core.LayerManager.GetLayer(l);

                if ( lay != null) 
                {
                    _dependentLayers.AddRange(lay.ParentsLayers());
                    result.Add(lay);
                }
            }

            foreach (var PNode in _dependentLayers)
            {
                if (!result.Contains(PNode))
                {
                    result.Add(PNode);
                }
            }

            return result;
        }

    }
}
