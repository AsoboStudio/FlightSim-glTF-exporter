﻿using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Max2Babylon
{
    public class ExportItem
    {
        public bool IsDirty { get; private set; } = true;

        public uint NodeHandle
        {
            get { return exportNodeHandle; }
            set
            {
                // if the handle is equal, return early so isdirty is not touched
                if (exportNodeHandle == value)
                    return;

                if (value == Loader.Core.RootNode.Handle)
                {
                    exportNode = Loader.Core.RootNode;
                    NodeName = "<SceneRoot>";
                }
                else
                {
                    exportNode = Loader.Core.RootNode.FindChildNode(value);
                    if (exportNode != null) NodeName = exportNode.NodeName;
                }

                IsDirty = true;
                exportNodeHandle = value;
            }
        }
        
        public string ExportFilePathRelative
        {
            get { return exportPathRelative; }
        }
        
        public string ExportFilePathAbsolute
        {
            get { return exportPathAbsolute; }
        }

        public string NodeName { get; private set; } = "<Invalid>";

        public IINode Node
        {
            get
            {
                return exportNode;
            }
        }

		public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected == value) return;
                selected = value;
                IsDirty = true;
            }
        }

        const string s_DisplayNameFormat = "{0} | {1} | \"{2}\"";
        const char s_PropertySeparator = ';';
        const string s_PropertyFormat = "{0};{1}";
        const string s_PropertyNamePrefix = "babylonjs_ExportItem";

        private string outputFileExt;
        private IINode exportNode;
        private uint exportNodeHandle = uint.MaxValue; // 0 is the scene root node
        private bool selected = true;
        private string exportPathRelative = "";
        private string exportPathAbsolute = "";

        public ExportItem(string outputFileExt) { this.outputFileExt = outputFileExt; }

        public ExportItem(string outputFileExt, uint nodeHandle)
        {
            this.outputFileExt = outputFileExt;
            NodeHandle = nodeHandle;
            
            if (nodeHandle == Loader.Core.RootNode.Handle)
            {
                string fileNameNoExt = Path.GetFileNameWithoutExtension(Loader.Core.CurFileName);
                SetExportFilePath(string.IsNullOrEmpty(fileNameNoExt) ? "Untitled" : fileNameNoExt);
            }
            else SetExportFilePath(NodeName);
        }

        public ExportItem(string outputFileExt, string propertyName)
        {
            this.outputFileExt = outputFileExt;
            LoadFromData(propertyName);
        }

        public ExportItem(ExportItem other)
        {
            DeepCopyFrom(other);
        }

        public void DeepCopyFrom(ExportItem other)
        {
            outputFileExt = other.outputFileExt;
            exportPathAbsolute = other.exportPathAbsolute;
            exportPathRelative = other.exportPathRelative;
            exportNodeHandle = other.exportNodeHandle;
            NodeName = other.NodeName;
            selected = other.selected;
            IsDirty = true;
        }

        public override string ToString() { return string.Format(s_DisplayNameFormat, selected, NodeName, exportPathRelative); }

        public void SetExportFilePath(string filePath)
        {
            string dirName = Loader.Core.CurFilePath;
            if (string.IsNullOrEmpty(Loader.Core.CurFilePath))
                dirName = Loader.Core.GetDir((int)MaxDirectory.ProjectFolder);
            else
            {
                dirName = Path.GetDirectoryName(dirName);
            }
            if (dirName.Last() != Path.AltDirectorySeparatorChar || dirName.Last() != Path.DirectorySeparatorChar)
                dirName += Path.DirectorySeparatorChar;

            string absolutePath;
            string relativePath;

            filePath = Path.ChangeExtension(filePath, outputFileExt);

            if (string.IsNullOrWhiteSpace(filePath)) // empty path
            {
                absolutePath = string.Empty;
                relativePath = string.Empty;
            }
            else if (Path.IsPathRooted(filePath)) // absolute path
            {
                absolutePath = Path.GetFullPath(filePath);
                relativePath = Tools.GetRelativePath(dirName, filePath);
            }
            else // relative path
            {
                absolutePath = Path.GetFullPath(Path.Combine(dirName, filePath));
                relativePath = Tools.GetRelativePath(dirName, absolutePath);

                exportPathRelative = relativePath;
            }

            // set absolute path (it may be different even if the relative path is equal, if the root dir changes for some reason)
            exportPathAbsolute = absolutePath;
            if (exportPathRelative.Equals(relativePath))
                return;
            exportPathRelative = relativePath;
            
            IsDirty = true;
        }

        #region Serialization

        public string GetPropertyName() { return s_PropertyNamePrefix + exportNodeHandle.ToString(); }

        public void LoadFromData(string propertyName)
        {
            if (!propertyName.StartsWith(s_PropertyNamePrefix))
                throw new Exception("Invalid property name, can't deserialize.");
            string uintStr = propertyName.Remove(0, s_PropertyNamePrefix.Length);

            uint handle;
            if (!uint.TryParse(uintStr, out handle))
                throw new Exception("Invalid ID, can't deserialize.");

            // set dirty explicitly just before we start loading, set to false when loading is done
            // if any exception is thrown, it will have a correct value
            IsDirty = true;

            NodeHandle = handle;
            IINode node = Node;
            if (node==null) return; // node was deleted

            string propertiesString = string.Empty;
            if (!node.GetUserPropString(propertyName, ref propertiesString))
                return; // node has no properties yet

            string[] properties = propertiesString.Split(s_PropertySeparator);
            if (properties.Length < 2)
                throw new Exception("Invalid number of properties, can't deserialize.");


            if (!bool.TryParse(properties[0], out selected))
                throw new Exception(string.Format("Failed to parse selected property from string {0}", properties[0]));

            SetExportFilePath(properties[1]);

            IsDirty = false;
        }

        public bool SaveToData()
        {
            // ' ' and '=' are not allowed by max, ';' is our data separator
            if (exportPathRelative.Contains(' ') || exportPathRelative.Contains('='))
                throw new FormatException("Invalid character(s) in export path: " + exportPathRelative + ". Spaces and equal signs are not allowed.");

            IINode node = Node;
            if (node == null) return false;

            node.SetStringProperty(GetPropertyName(), string.Format(s_PropertyFormat, selected.ToString(), exportPathRelative));

            IsDirty = false;
            return true;
        }

        public void DeleteFromData()
        {
            IINode node = Node;
            if (node == null) return;
            node.DeleteProperty(GetPropertyName());
            IsDirty = true;
        }

        #endregion
    }
    public class ExportItemList : List<ExportItem>
    {
        const string s_ExportItemListPropertyName = "babylonjs_ExportItemList";

        public string OutputFileExtension { get; private set; }

        public ExportItemList(string outputFileExt)
        {
            this.OutputFileExtension = outputFileExt;
        }

        public void LoadFromData()
        {
            string[] exportItemPropertyNames = Loader.Core.RootNode.GetStringArrayProperty(s_ExportItemListPropertyName);

            if (Capacity < exportItemPropertyNames.Length)
                Capacity = exportItemPropertyNames.Length;

            foreach (string propertyNameStr in exportItemPropertyNames)
            {
                ExportItem info = new ExportItem(OutputFileExtension, propertyNameStr);
                if(info.Node != null)
					Add(info);
            }
        }

        public void SaveToData()
        {
            List<string> exportItemPropertyNameList = new List<string>();
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].IsDirty)
                {
                    if (!this[i].SaveToData())
                    {
                        RemoveAt(i);
                        --i;
                    }
					else exportItemPropertyNameList.Add(this[i].GetPropertyName());
                }
                else exportItemPropertyNameList.Add(this[i].GetPropertyName());
            }

            Loader.Core.RootNode.SetStringArrayProperty(s_ExportItemListPropertyName, exportItemPropertyNameList);
        }

        public void DeleteFromData()
        {
            Loader.Core.RootNode.DeleteProperty(s_ExportItemListPropertyName);
        }
    }
}
