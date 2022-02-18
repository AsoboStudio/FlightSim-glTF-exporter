using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using Autodesk.Max;
using Autodesk.Max.Plugins;
using ManagedServices;
using Max2Babylon.FlightSim;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;

namespace Max2Babylon
{

    public struct SerializableAnimationGroup
    {
        public string name;
        public int ticksStart;
        public int ticksEnd;

        public bool keepEmpty;
        public bool keepNonAnimated;

        public List<Guid> nodes;
        public List<Guid> materials;

        public SerializableAnimationGroup(string serializedData)
        {
            JObject o = JObject.Parse(serializedData);

            //Default Values
            name = "";
            ticksStart = 0;
            ticksEnd = 16000;
            keepEmpty = false;
            keepNonAnimated = true;
            nodes = null;
            materials = null;

            //Set result from the parsing
            if (o.ContainsKey("name"))
            {
                name = o["name"].Value<string>();
            }
            if (o.ContainsKey("ticksStart"))
            {
                ticksStart = o["ticksStart"].Value<int>();
            }
            if (o.ContainsKey("ticksEnd"))
            {
                ticksEnd = o["ticksEnd"].Value<int>();
            }
            if (o.ContainsKey("keepEmpty"))
            {
                keepEmpty = o["keepEmpty"].Value<bool>();
            }
            if (o.ContainsKey("keepNonAnimated"))
            {
                keepNonAnimated = o["keepNonAnimated"].Value<bool>();
            }
            if (o.ContainsKey("nodes"))
            {
                List<string> parsed = o["nodes"].Values<string>()?.ToList();
                if (parsed != null) nodes = parsed.ConvertAll(x => new Guid(x));
            }
            if (o.ContainsKey("materials"))
            {
                List<string> parsed = o["materials"].Values<string>()?.ToList();
                if (parsed != null) materials = parsed.ConvertAll(x => new Guid(x));
            }
            
        }

        /// <summary>
        /// Apply the settings stored in the SerializableAnimationGroup to the input AnimationGroup
        /// </summary>
        /// <param name="animGroup">This animation group will receive the values from the Serializable</param>
        public void FillSerializedData(AnimationGroup animGroup)
        {
            animGroup.Name = name;
            animGroup.TicksStart = ticksStart;
            animGroup.TicksEnd = ticksEnd;
            animGroup.KeepStaticAnimation = keepEmpty;
            animGroup.NodeGuids = nodes;
            animGroup.MaterialGuids = materials;
            animGroup.KeepNonAnimated = keepNonAnimated;
        }
        
        /// <summary>
        /// Gather the settings stored in the AnimationGroup and use them to set the values of the SerializableAnimationGroup
        /// </summary>
        /// <param name="animGroup"></param>
        public void GetDataToSerialize(AnimationGroup animGroup)
        {
            name = animGroup.Name;
            ticksStart = animGroup.TicksStart;
            ticksEnd = animGroup.TicksEnd;
            keepEmpty = animGroup.KeepStaticAnimation;
            nodes = animGroup.NodeGuids.ToList();
            materials = animGroup.MaterialGuids.ToList();
            keepNonAnimated = animGroup.KeepNonAnimated;
        }
    }

    [DataContract]
    public class AnimationGroupNode
    {
        [DataMember]
        public Guid Guid { get; set; } 
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ParentName { get; set; }

        public AnimationGroupNode(Guid _guid, string _name, string _parentName)
        {
            Guid = _guid;
            Name = _name;
            ParentName = _parentName;
        }

    }

    [DataContract]
    public class AnimationGroupMaterial
    {
        [DataMember]
        public Guid Guid { get; set; } 
        [DataMember]
        public string Name { get; set; }

        public AnimationGroupMaterial(Guid _guid, string _name)
        {
            Guid = _guid;
            Name = _name;
        }

    }

    [DataContract]
    public class AnimationGroup
    {
        public bool IsDirty { get; private set; } = true;
        public bool IsOldType { get; private set; } = false;

        [DataMember]
        public Guid SerializedId
        {
            get { return serializedId; }
            set
            {
                if (value.Equals(SerializedId))
                    return;
                IsDirty = true;
                serializedId = value;
            }
        }
        [DataMember]
        public string Name
        {
            get { return name; }
            set
            {
                if (value.Equals(name))
                    return;
                IsDirty = true;
                name = value;
            }
        }
        public int FrameStart
        {
            get { return MathUtilities.RoundToInt(ticksStart / (float)Loader.Global.TicksPerFrame); }
            set
            {
                if (value.Equals(FrameStart)) // property getter
                    return;
                IsDirty = true;
                ticksStart = value * Loader.Global.TicksPerFrame;
            }
        }
        public int FrameEnd
        {
            get { return MathUtilities.RoundToInt(TicksEnd / (float)Loader.Global.TicksPerFrame); }
            set
            {
                if (value.Equals(FrameEnd)) // property getter
                    return;
                IsDirty = true;
                TicksEnd = value * Loader.Global.TicksPerFrame;
            }
        }

        [DataMember]
        public List<AnimationGroupNode> AnimationGroupNodes {get; set;}

        [DataMember]
        public List<AnimationGroupMaterial> AnimationGroupMaterials {get; set;}

        public IList<Guid> NodeGuids
        {
            get { return nodeGuids.AsReadOnly(); }
            set
            {
                // if the lists are equal, return early so isdirty is not touched
                if (nodeGuids.Count == value.Count)
                {
                    bool equal = true;
                    int i = 0;
                    foreach (Guid newNodeGuid in value)
                    {
                        if (!newNodeGuid.Equals(nodeGuids[i]))
                        {
                            equal = false;
                            break;
                        }
                        ++i;
                    }
                    if (equal)
                        return;
                }

                IsDirty = true;
                nodeGuids.Clear();
                nodeGuids.AddRange(value);
            }
        }

        public IList<Guid> MaterialGuids
        {
            get { return materialsGuids.AsReadOnly(); }
            set
            {
                // if the lists are equal, return early so isdirty is not touched
                if (materialsGuids.Count == value.Count)
                {
                    bool equal = true;
                    int i = 0;
                    foreach (Guid newMaterialGuid in value)
                    {
                        if (!newMaterialGuid.Equals(materialsGuids[i]))
                        {
                            equal = false;
                            break;
                        }
                        ++i;
                    }
                    if (equal)
                        return;
                }

                IsDirty = true;
                materialsGuids.Clear();
                materialsGuids.AddRange(value);
            }
        }

        private int ticksStart = Loader.Core.AnimRange.Start;
        private int ticksEnd = Loader.Core.AnimRange.End;

        [DataMember]
        public int TicksStart
        {
            get { return ticksStart; }
            set { ticksStart = value; }
        }

        [DataMember]
        public int TicksEnd
        {
            get { return ticksEnd; }
            set { ticksEnd = value; }
        }

        private bool keepStaticAnimation = false;

        [DataMember]
        public bool KeepStaticAnimation
        {
            get { return keepStaticAnimation; }
            set { keepStaticAnimation = value; }
        }

        private bool keepNonAnimated = true;


        [DataMember]
        public bool KeepNonAnimated
        {
            get { return keepNonAnimated; }
            set { keepNonAnimated = value; }
        }

        public const string s_DisplayNameFormat = "{0} ({1:d}, {2:d})";
        public const char s_PropertySeparator = ';';
        public const char s_GUIDTypeSeparator = '~';
        public const string s_PropertyFormat = "{0};{1};{2};{3}";

        private Guid serializedId = Guid.NewGuid();
        private string name = "Animation";
        // use current timeline frame range by default

        private List<Guid> nodeGuids = new List<Guid>();
        private List<Guid> materialsGuids = new List<Guid>();

        public AnimationGroup() { }
        public AnimationGroup(AnimationGroup other)
        {
            DeepCopyFrom(other);
        }
        public void DeepCopyFrom(AnimationGroup other)
        {
            serializedId = other.serializedId;
            name = other.name;
            KeepStaticAnimation = other.KeepStaticAnimation;
            KeepNonAnimated = other.KeepNonAnimated;
            TicksStart = other.TicksStart;
            TicksEnd = other.TicksEnd;
            nodeGuids.Clear();
            nodeGuids.AddRange(other.nodeGuids);
            materialsGuids.Clear();
            materialsGuids.AddRange(other.materialsGuids);
            IsDirty = true;
        }

        public void MergeFrom(AnimationGroup other)
        {
            nodeGuids.AddRange(other.nodeGuids);
            materialsGuids.AddRange(other.materialsGuids);
            IsDirty = true;
        }

        public override string ToString()
        {
            return string.Format(s_DisplayNameFormat, name, FrameStart, FrameEnd);
        }

        #region Serialization

        public string GetPropertyName() { return serializedId.ToString(); }

        public void LoadFromData(string propertyName,IINode dataNode,Dictionary<string, string> rootNodePropDictionary = null)
        {
            if (!Guid.TryParse(propertyName, out serializedId))
                throw new Exception("Invalid ID, can't deserialize.");


            string propertiesString = string.Empty;

            if (rootNodePropDictionary == null)
            {
                if (!dataNode.GetUserPropString(propertyName, ref propertiesString))
                    return;
            }
            else
            {
                if (!rootNodePropDictionary.TryGetValue(propertyName, out propertiesString))
                    return;
            }


            int numFailed = 0;

            try // Try using the new way, if it's not working use the old way.
            {
                SerializableAnimationGroup serialAnimGroup = new SerializableAnimationGroup(propertiesString);
                serialAnimGroup.FillSerializedData(this);
            }
            catch
            {
                int indexOfguidPart = propertiesString
                    .Select((c, i) => new {c, i})
                    .Where(x => x.c == s_PropertySeparator)
                    .Skip(2)
                    .FirstOrDefault().i;
                string[] baseProperties = propertiesString.Substring(0, indexOfguidPart)?.Split(s_PropertySeparator);
                string guidPart = propertiesString.Substring(indexOfguidPart+1);

                if (baseProperties.Length != 3)
                    throw new Exception("Invalid number of properties, can't deserialize.");

                // set dirty explicitly just before we start loading, set to false when loading is done
                // if any exception is thrown, it will have a correct value
                IsDirty = true;

                name = baseProperties[0];
                if (!int.TryParse(baseProperties[1], out ticksStart))
                    throw new Exception("Failed to parse FrameStart property.");
                if (!int.TryParse(baseProperties[2], out ticksEnd))
                    throw new Exception("Failed to parse FrameEnd property.");

                if (string.IsNullOrEmpty(guidPart) || guidPart==s_GUIDTypeSeparator.ToString()) return;


                if (!guidPart.Contains(s_GUIDTypeSeparator))
                {
                    // to grant retro-compatiblity
                   numFailed = ParseOldProperties(guidPart);
                }
                else
                {
                    //new format with nodes and node materials guid
                    numFailed = ParseNewProperties(guidPart);
                }
            }           
            

            AnimationGroupNodes = new List<AnimationGroupNode>();
            foreach (Guid nodeGuid in nodeGuids)
            {
                IINode node = Tools.GetINodeByGuid(nodeGuid);

                if (node != null)
                {
                    string name = node.Name;
                    string parentName = node.ParentNode.Name;
                    AnimationGroupNode nodeData = new AnimationGroupNode(nodeGuid, name, parentName);
                    AnimationGroupNodes.Add(nodeData);
                }
            }

            AnimationGroupMaterials = new List<AnimationGroupMaterial>();
            foreach (Guid materialGUID in materialsGuids)
            {
                IMtl mat = Tools.GetIMtlByGuid(materialGUID);

                if (mat != null)
                {
                    string name = mat.Name;
                    AnimationGroupMaterial matData = new AnimationGroupMaterial(materialGUID, name);
                    AnimationGroupMaterials.Add(matData);
                }
            }

            if (numFailed > 0)
                throw new Exception(string.Format("Failed to parse {0} node ids.", numFailed));
            
            IsDirty = false;
        }

        private int ParseOldProperties(string guidPropPart)
        {
            IsOldType = true; //force it to be saved in the new format
            string[] properties = guidPropPart.Split(s_PropertySeparator);

            int numFailed = 0;

            int numNodeIDs = properties.Length;
            if (nodeGuids.Capacity < numNodeIDs) nodeGuids.Capacity = numNodeIDs;


            for (int i = 0; i < numNodeIDs; ++i)
            {
                Guid guid;
                if (!Guid.TryParse(properties[ i], out guid))
                {
                    uint id;
                    if (!uint.TryParse(properties[i], out id))
                    {
                        ++numFailed;
                        continue;
                    }
                    //node is serialized in the old way ,force the reassignation of a new Guid on
                    IINode node = Loader.Core.GetINodeByHandle(id);
                    if (node != null)
                    {
                        guid= node.GetGuid();
                    }
                }
                nodeGuids.Add(guid);
                
            }

            return numFailed;
        }

        private int ParseNewProperties(string guidPropPart)
        {
            int numFailed = 0;
            string[] guidProperties = guidPropPart.Split(s_GUIDTypeSeparator);
            string[] nodes = new string[0];
            string[] materials = new string[0];
            if (!string.IsNullOrWhiteSpace(guidProperties[0]))
            {
                nodes = guidProperties[0].Split(s_PropertySeparator);
            }

            if (!string.IsNullOrWhiteSpace(guidProperties[1]))
            {
                materials = guidProperties[1].Split(s_PropertySeparator);
            }
            
            int numNodeGUIDs = nodes.Length;
            int numMatGUIDs = materials.Length;


            for (int i = 0; i < numNodeGUIDs; ++i)
            {
                Guid guid;
                if (!Guid.TryParse(nodes[i], out guid))
                {
                    uint id;
                    if (!uint.TryParse(nodes[i], out id))
                    {
                        ++numFailed;
                        continue;
                    }
                    //node is serialized in the old way ,force the reassignation of a new Guid on
                    IINode node = Loader.Core.GetINodeByHandle(id);
                    if (node != null)
                    {
                            guid= node.GetGuid();
                    }
                }
                nodeGuids.Add(guid);
            }

            for (int i = 0; i < numMatGUIDs; ++i)
            {
                Guid guid;
                if (Guid.TryParse(materials[i], out guid))
                {
                    materialsGuids.Add(guid);
                }
            }
            
            return numFailed;
        }

        public void SaveToData(IINode dataNode = null)
        {
            dataNode = dataNode ?? Loader.Core.RootNode;
            // ' ' and '=' are not allowed by max, ';' is our data separator
            if (name.Contains(' ') || name.Contains('=') || name.Contains(s_PropertySeparator))
                throw new FormatException("Invalid character(s) in animation Name: " + name + ". Spaces, equal signs and the separator '" + s_PropertySeparator + "' are not allowed.");

            SerializableAnimationGroup serializableAnimation = new SerializableAnimationGroup();

            serializableAnimation.GetDataToSerialize(this);

            string serializedInfo = JsonConvert.SerializeObject(serializableAnimation);

            dataNode.SetStringProperty(GetPropertyName(), serializedInfo);

            IsDirty = false;
        }



        public void DeleteFromData(IINode dataNode = null)
        {
            dataNode = dataNode ?? Loader.Core.RootNode;
            dataNode.DeleteProperty(GetPropertyName());
            IsDirty = true;
        }

        #endregion
    }
    
    public class AnimationGroupList : List<AnimationGroup>
    {
        const string s_AnimationListPropertyName = "babylonjs_AnimationList";

        public AnimationGroup GetAnimationGroupByName(string name)
        {
            return this.First(animationGroup => animationGroup.Name == name);
        }

        public void LoadFromData(IINode dataNode = null)
        {
            dataNode = dataNode ?? Loader.Core.RootNode;

            Dictionary<string, string> nodePropDictionary = dataNode.UserPropToDictionary();
            string animProp = string.Empty;
            nodePropDictionary.TryGetValue(s_AnimationListPropertyName,out animProp);
            if (!string.IsNullOrWhiteSpace(animProp))
            {
                string[] animationPropertyNames = animProp.Split(';') ;

                if (Capacity < animationPropertyNames.Length)
                    Capacity = animationPropertyNames.Length;

                foreach (string propertyNameStr in animationPropertyNames)
                {
                    AnimationGroup info = new AnimationGroup();
                        
                    info.LoadFromData(propertyNameStr,dataNode,nodePropDictionary);
                    Add(info);
                }
            }
        }

        public static AnimationGroupList InitAnimationGroups(ILoggingProvider logger)
        {
            AnimationGroupList animationList = new AnimationGroupList();
            animationList.LoadFromData(Loader.Core.RootNode);

            if (animationList.Count > 0)
            {
                int timelineStart = Loader.Core.AnimRange.Start / Loader.Global.TicksPerFrame;
                int timelineEnd = Loader.Core.AnimRange.End / Loader.Global.TicksPerFrame;

                List<string> warnings = new List<string>();
                foreach (AnimationGroup animGroup in animationList)
                {
                    // ensure min <= start <= end <= max
                    warnings.Clear();
                    if (animGroup.FrameStart < timelineStart || animGroup.FrameStart > timelineEnd)
                    {
                        warnings.Add("Start frame '" + animGroup.FrameStart + "' outside of timeline range [" + timelineStart + ", " + timelineEnd + "]. Set to timeline start time '" + timelineStart + "'");
                        animGroup.FrameStart = timelineStart;
                    }
                    if (animGroup.FrameEnd < timelineStart || animGroup.FrameEnd > timelineEnd)
                    {
                        warnings.Add("End frame '" + animGroup.FrameEnd + "' outside of timeline range [" + timelineStart + ", " + timelineEnd + "]. Set to timeline end time '" + timelineEnd + "'");
                        animGroup.FrameEnd = timelineEnd;
                    }
                    if (animGroup.FrameEnd <= animGroup.FrameStart)
                    {
                        if (animGroup.FrameEnd < animGroup.FrameStart)
                            // Strict
                            warnings.Add("End frame '" + animGroup.FrameEnd + "' lower than Start frame '" + animGroup.FrameStart + "'. Start frame set to timeline start time '" + timelineStart + "'. End frame set to timeline end time '" + timelineEnd + "'.");
                        else
                            // Equal
                            warnings.Add("End frame '" + animGroup.FrameEnd + "' equal to Start frame '" + animGroup.FrameStart + "'. Single frame animation are not allowed. Start frame set to timeline start time '" + timelineStart + "'. End frame set to timeline end time '" + timelineEnd + "'.");

                        animGroup.FrameStart = timelineStart;
                        animGroup.FrameEnd = timelineEnd;
                    }
                    foreach (Guid guid in animGroup.NodeGuids)
                    {
                        IINode node = Tools.GetINodeByGuid(guid);
                        if(node!= null)
                        {
                            if(!(node.TMController.IsKeyAtTime(animGroup.TicksStart, (1<<0)) || node.TMController.IsKeyAtTime(animGroup.TicksStart, (1<<1)) ||node.TMController.IsKeyAtTime(animGroup.TicksStart, (1<<2))))
                            {
                                int key = animGroup.TicksStart /160;
                                string msg = string.Format("Node {0} has no key on min frame: {1} of animation group {2}", node.NodeName,key,animGroup.Name);
                                warnings.Add(msg);
                            }
                              if(!(node.TMController.IsKeyAtTime(animGroup.TicksEnd, (1<<0)) || node.TMController.IsKeyAtTime(animGroup.TicksEnd, (1<<1)) ||node.TMController.IsKeyAtTime(animGroup.TicksEnd, (1<<2))))
                            {
                                int key = animGroup.TicksEnd /160;
                                string msg = string.Format("Node {0} has no key on max frame: {1} of animation group {2}", node.NodeName,key,animGroup.Name);
                                warnings.Add(msg);
                            }
                        }
                    }

                    // Print animation group warnings if any
                    // Nothing printed otherwise
                    if (warnings.Count > 0)
                    {
                        logger?.RaiseWarning(animGroup.Name, 1);
                        foreach (string warning in warnings)
                        {
                            logger?.RaiseWarning(warning, 2);
                        }
                    }
                }
            }

            return animationList;
        }

        public void SaveToData(IINode dataNode = null)
        {
            dataNode = dataNode ?? Loader.Core.RootNode;
            List<string> animationPropertyNameList = new List<string>();
            for(int i = 0; i < Count; ++i)
            {
                if(this[i].IsDirty )
                    this[i].SaveToData(dataNode);
                animationPropertyNameList.Add(this[i].GetPropertyName());
            }
            
            dataNode.SetStringArrayProperty(s_AnimationListPropertyName, animationPropertyNameList);
        }

        public void SaveToJson(string filePath, List<AnimationGroup> exportList)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(exportList));
        }

        public void LoadFromJson(string jsonContent, bool merge = false)
        {
            List<string> animationPropertyNameList = Loader.Core.RootNode.GetStringArrayProperty(s_AnimationListPropertyName).ToList();

            if (!merge)
            {
                animationPropertyNameList = new List<string>();
                Clear();
            }
            
            List<AnimationGroup> animationGroupsData = JsonConvert.DeserializeObject<List<AnimationGroup>>(jsonContent);

            List<Guid> nodeGuids = new List<Guid>();
            List<Guid> materialsGuids = new List<Guid>();
            foreach (AnimationGroup animData in animationGroupsData)
            {
                nodeGuids.Clear();
                
                if (animData.AnimationGroupNodes != null)
                {
                    string missingNodes= "";
                    string movedNodes = "";
                    foreach (AnimationGroupNode nodeData in animData.AnimationGroupNodes)
                    {
                        //check here if something changed between export\import
                        // a node handle is reassigned the moment the node is created
                        // it is no possible to have consistency at 100% sure between two file
                        // we need to prevent artists
                        IINode node = Loader.Core.GetINodeByName(nodeData.Name);
                        if (node == null)
                        {
                            //node is missing
                            missingNodes += nodeData.Name + "\n";
                            continue;
                        }

                        if (node.ParentNode.Name != nodeData.ParentName)
                        {
                            //node has been moved in hierarchy 
                            movedNodes += node.Name + "\n";
                            continue;
                        }

                        nodeGuids.Add(node.GetGuid());
                    }

                    if (!string.IsNullOrEmpty(movedNodes))
                    {
                        //skip restoration of evaluated animation group
                        nodeGuids = new List<Guid>();
                        MessageBox.Show(string.Format("{0} has been moved in hierarchy,{1} import skipped", movedNodes, animData.Name));
                    }

                    if (!string.IsNullOrEmpty(missingNodes))
                    {
                        //skip restoration of evaluated animation group
                        nodeGuids = new List<Guid>();
                        MessageBox.Show(string.Format("{0} does not exist,{1} import skipped", missingNodes, animData.Name));
                    }
                }

                if (animData.AnimationGroupMaterials != null)
                {
                    string missingMaterials= "";
                    foreach (AnimationGroupMaterial matData in animData.AnimationGroupMaterials)
                    {
                        //check here if something changed between export\import
                        // a material handle is reassigned the moment the node is created
                        // it is no possible to have consistency at 100% sure between two file
                        // we need to prevent artists
                        IMtl mtl = Tools.GetIMtlByGuid(matData.Guid);
                        if (mtl == null)
                        {
                            //material is missing
                            missingMaterials += matData.Name + "\n";
                            continue;
                        }
                        materialsGuids.Add(mtl.GetGuid());
                    }

                    if (!string.IsNullOrEmpty(missingMaterials))
                    {
                        //skip restoration of evaluated animation group
                        materialsGuids = new List<Guid>();
                        MessageBox.Show(string.Format("{0} does not exist,{1} import skipped", missingMaterials, animData.Name));
                    }
                }


                animData.NodeGuids = nodeGuids;
                animData.MaterialGuids = materialsGuids;
                string nodes = string.Join(AnimationGroup.s_PropertySeparator.ToString(), animData.NodeGuids);
                string materials = string.Join(AnimationGroup.s_PropertySeparator.ToString(), animData.MaterialGuids);
                string guids = string.Join(AnimationGroup.s_GUIDTypeSeparator.ToString(), nodes, materials);

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat(AnimationGroup.s_PropertyFormat, animData.Name, animData.TicksStart, animData.TicksEnd, guids);

                Loader.Core.RootNode.SetStringProperty(animData.SerializedId.ToString(), stringBuilder.ToString());

                string id = animData.SerializedId.ToString();

                if (merge)
                {
                    //if json are merged check if the same animgroup is already in list
                    //and skip in that case
                    if (!animationPropertyNameList.Contains(id))
                    {
                        animationPropertyNameList.Add(animData.SerializedId.ToString());
                    }
                }
                else
                {
                    animationPropertyNameList.Add(animData.SerializedId.ToString());
                }
            }

            Loader.Core.RootNode.SetStringArrayProperty(s_AnimationListPropertyName, animationPropertyNameList);

            LoadFromData(Loader.Core.RootNode);
            
        }

        public static void SaveDataToAnimationHelper()
        {
            AnimationGroupList animationGroupList = new AnimationGroupList();
            animationGroupList.LoadFromData(Loader.Core.RootNode);

            List<string> animationPropertyNameList = new List<string>();
            var helper = Tools.BabylonAnimationHelper();
            foreach (AnimationGroup animationGroup in animationGroupList)
            {
                string prop = Loader.Core.RootNode.GetStringProperty(animationGroup.GetPropertyName(),"");
                helper.SetStringProperty(animationGroup.GetPropertyName(), prop);
                animationPropertyNameList.Add(animationGroup.GetPropertyName());
            }

            if (animationPropertyNameList.Count > 0)
            {
                helper.SetStringArrayProperty(s_AnimationListPropertyName, animationPropertyNameList);
            }
        }

        public static void SaveDataToContainerHelper(IIContainerObject iContainerObject)
        {
            if (!iContainerObject.IsOpen)
            {
                MessageBox.Show("Animations of " + iContainerObject.ContainerNode.Name +" cannot be saved because Container is closed");
                return;
            }

            AnimationGroupList animationGroupList = new AnimationGroupList();
            animationGroupList.LoadFromData(Loader.Core.RootNode);

            RemoveDataOnContainer(iContainerObject); //cleanup for a new serialization
            List<string> animationPropertyNameList = new List<string>();
            foreach (AnimationGroup animationGroup in animationGroupList)
            {
                IIContainerObject containerObject = animationGroup.NodeGuids.InSameContainer();
                if (containerObject!=null && containerObject.ContainerNode.Handle == iContainerObject.ContainerNode.Handle)
                {
                    string prop = Loader.Core.RootNode.GetStringProperty(animationGroup.GetPropertyName(),"");
                    containerObject.BabylonContainerHelper().SetStringProperty(animationGroup.GetPropertyName(), prop);
                    animationPropertyNameList.Add(animationGroup.GetPropertyName());
                }
            }

            if (animationPropertyNameList.Count > 0)
            {
                iContainerObject.BabylonContainerHelper().SetStringArrayProperty(s_AnimationListPropertyName, animationPropertyNameList);
            }
        }

        public static void LoadDataFromAnimationHelpers()
        {
            AnimationGroupList sceneAnimationGroupList = new AnimationGroupList();
            sceneAnimationGroupList.LoadFromData();

            foreach (IINode node in Loader.Core.RootNode.DirectChildren())
            {
                if (node.IsBabylonAnimationHelper())
                {
                    AnimationGroupList helperAnimationGroupList = new AnimationGroupList();
                    helperAnimationGroupList.LoadFromData(node);

                    //merge
                    foreach (AnimationGroup animationGroup in helperAnimationGroupList)
                    {
                        AnimationGroup toMerge = sceneAnimationGroupList.Find(a => a.Name==animationGroup.Name);
                        if (toMerge != null)
                        {
                            toMerge.MergeFrom(animationGroup);
                        }
                        else
                        {
                            AnimationGroup newAnimationGroup = new AnimationGroup();
                            newAnimationGroup.DeepCopyFrom(animationGroup);
                            sceneAnimationGroupList.Add(newAnimationGroup);
                        }
                    }
                }
            }
            sceneAnimationGroupList.SaveToData();
            Loader.Global.SetSaveRequiredFlag(true, false);
        }

        public static void LoadDataFromAllContainers()
        {
            List<IIContainerObject> containers = Tools.GetAllContainers();
            if (containers.Count<=0) return;

            foreach (IIContainerObject iContainerObject in containers)
            {
                LoadDataFromContainerHelper(iContainerObject);
            }
        }

        public static void LoadDataFromContainerHelper(IIContainerObject iContainerObject)
        {
            if (!iContainerObject.IsOpen)
            {
                MessageBox.Show("Animations of " + iContainerObject.ContainerNode.Name +" cannot be loaded because Container is closed");
                return;
            }
            
            ResolveMultipleInheritedContainer(iContainerObject);
            

            //on container added in scene try retrieve info from containers
            string[] sceneAnimationPropertyNames = Loader.Core.RootNode.GetStringArrayProperty(s_AnimationListPropertyName);
            string[] containerAnimationPropertyNames = iContainerObject.BabylonContainerHelper().GetStringArrayProperty(s_AnimationListPropertyName);
            string[] mergedAnimationPropertyNames = sceneAnimationPropertyNames.Concat(containerAnimationPropertyNames).Distinct().ToArray();

            Loader.Core.RootNode.SetStringArrayProperty(s_AnimationListPropertyName, mergedAnimationPropertyNames);

            foreach (string propertyNameStr in containerAnimationPropertyNames)
            {
                //copy
                string prop = iContainerObject.BabylonContainerHelper().GetStringProperty(propertyNameStr,"");
                Loader.Core.RootNode.SetStringProperty(propertyNameStr, prop);
            }
        }
        
        private static void ResolveMultipleInheritedContainer(IIContainerObject container)
        {
            //resolve container name
            int b = 0;
            if (container.ContainerNode.GetUserPropBool("flightsim_resolved", ref b) && b!=0)
            {
                return;
            }

            List<IINode> containerHierarchy = new List<IINode>() { };
            containerHierarchy.AddRange(container.ContainerNode.ContainerNodeTree(false));

            string helperPropBuffer = string.Empty;
            container.BabylonContainerHelper().GetUserPropBuffer(ref helperPropBuffer);
            int containerID = 1;
            container.ContainerNode.GetUserPropInt("babylonjs_ContainerID", ref containerID);
            container.ContainerNode.Name = containerID == -1 ? $"{container.ContainerNode.Name}" : $"{container.ContainerNode.Name}_{containerID}";
            // resolve nodes , adding an id at the end
            {               
                //manage multiple containers inherited from the same source
                foreach (IINode n in containerHierarchy)
                {
                    if (n.IsBabylonContainerHelper()) continue;
                    //change the guid of the node
                    //replace the guid in the babylon helper
                    string oldGuid = n.GetStringProperty("babylonjs_GUID", Guid.NewGuid().ToString());
                    n.DeleteProperty("babylonjs_GUID");
                    Guid newGuid = n.GetGuid();
                    helperPropBuffer = helperPropBuffer.Replace(oldGuid, newGuid.ToString());

                    n.Name = containerID == -1 ? $"{n.Name}" : $"{n.Name}_{containerID}";
                    if (n.Mtl != null && FlightSimMaterialUtilities.HasFlightSimMaterials(n.Mtl) && FlightSimMaterialUtilities.HasRuntimeAccess(n.Mtl))
                    {
                        if (n.Mtl.IsMultiMtl)
                        {
                            throw new Exception($@"Material {n.Mtl.Name} has a property ""Unique In Container"" enabled, cannot be child of a multi material");
                        }
                        else
                        {
                            string cmd = $"mNode = maxOps.getNodeByHandle {n.Handle} \r\n" +
                                      $"newMat = copy mNode.material \r\n" +
                                      $"newMat.name = \"{n.Mtl.Name}_{containerID}\" \r\n" +
                                      $"mNode.material = newMat";

                            ScriptsUtilities.ExecuteMaxScriptCommand(cmd);
                        }
                    }
                }
            }



            //replace animationList guid to have distinct list of AnimationGroup for each container
            string animationListStr = string.Empty;
            IINode containerHelper = container.BabylonContainerHelper();
            

            containerHelper.GetUserPropString(s_AnimationListPropertyName, ref animationListStr);
            if (!string.IsNullOrEmpty(animationListStr))
            {
                string[] animationGroupGuid = animationListStr.Split(AnimationGroup.s_PropertySeparator);
                foreach (string guidStr in animationGroupGuid)
                {
                    Guid newAnimGroupGuid = Guid.NewGuid();
                    helperPropBuffer = helperPropBuffer.Replace(guidStr, newAnimGroupGuid.ToString());
                }
                
                containerHelper.SetUserPropBuffer(helperPropBuffer);

                //add ID of container to animationGroup name to identify animation in viewer
               containerHelper.GetUserPropString(s_AnimationListPropertyName, ref animationListStr);
                string[] newAnimationGroupGuid = animationListStr.Split(AnimationGroup.s_PropertySeparator);
                
                foreach (string guidStr in newAnimationGroupGuid)
                {
                    string propertiesString = string.Empty;
                    if (!containerHelper.GetUserPropString(guidStr, ref propertiesString))
                        return;

                    try // new serialization method
                    {
                        SerializableAnimationGroup serialAnimGroup = new SerializableAnimationGroup(propertiesString);
                        serialAnimGroup.name = containerID == -1 ? serialAnimGroup.name : serialAnimGroup.name + $"_{containerID}";
                        string serializedInfo = JsonConvert.SerializeObject(serialAnimGroup);
                        container.BabylonContainerHelper().SetUserPropString(guidStr, serializedInfo);
                    }
                    catch (Exception)
                    {
                        string[] properties = propertiesString.Split(AnimationGroup.s_PropertySeparator);
                        if (properties.Length < 4)
                            throw new Exception($"Invalid number of properties, can't de-serialize property of {containerHelper.Name} of {container.ContainerNode.Name}.");

                        string name = properties[0];
                        if (!string.IsNullOrEmpty(name))
                        {
                            propertiesString = propertiesString.Replace(name, name + "_" + containerID);
                            container.BabylonContainerHelper().SetUserPropString(guidStr, propertiesString);
                        }
                    }
                }
            }
            container.ContainerNode.SetUserPropBool("flightsim_resolved", true);
        }

        public static void RemoveDataOnContainer(IIContainerObject containerObject)
        {
            //remove all property related to animation group 
            string[] animationPropertyNames =containerObject.BabylonContainerHelper().GetStringArrayProperty(s_AnimationListPropertyName);

            foreach (string propertyNameStr in animationPropertyNames)
            {
                containerObject.BabylonContainerHelper().DeleteProperty(propertyNameStr);
            }

            containerObject.BabylonContainerHelper().DeleteProperty(s_AnimationListPropertyName);
        }
    }
}
