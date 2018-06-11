﻿using Autodesk.Max;
using BabylonExport.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace Max2Babylon
{
    internal partial class BabylonExporter
    {
        public event Action<int> OnImportProgressChanged;
        public event Action<string, int> OnWarning;
        public event Action<string, Color, int, bool> OnMessage;
        public event Action<string, int> OnError;

        public Form callerForm;

        public ExportParameters exportParameters;
        public bool IsCancelled { get; set; }

        public string MaxSceneFileName { get; set; }

        public bool ExportQuaternionsInsteadOfEulers { get; set; }

        private bool isBabylonExported;
        private bool isGLTFExported;

        private string exporterVersion = "1.2.5";

        void ReportProgressChanged(int progress)
        {
            if (OnImportProgressChanged != null)
            {
                OnImportProgressChanged(progress);
            }
        }

        void RaiseError(string error, int rank = 0)
        {
            if (OnError != null)
            {
                OnError(error, rank);
            }
        }

        void RaiseWarning(string warning, int rank = 0)
        {
            if (OnWarning != null)
            {
                OnWarning(warning, rank);
            }
        }

        void RaiseMessage(string message, int rank = 0, bool emphasis = false)
        {
            RaiseMessage(message, Color.Black, rank, emphasis);
        }

        void RaiseMessage(string message, Color color, int rank = 0, bool emphasis = false)
        {
            if (OnMessage != null)
            {
                OnMessage(message, color, rank, emphasis);
            }
        }

        // For debug purpose
        void RaiseVerbose(string message, int rank = 0, bool emphasis = false)
        {
            //RaiseMessage(message, Color.DarkGray, rank, emphasis);
        }

        void CheckCancelled()
        {
            Application.DoEvents();
            if (IsCancelled)
            {
                throw new OperationCanceledException();
            }
        }
        
        public void Export(ExportParameters exportParameters)
        {
            // Check input text is valid
            var scaleFactorFloat = 1.0f;
            string scaleFactor = exportParameters.scaleFactor;
            try
            {
                scaleFactor = scaleFactor.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                scaleFactor = scaleFactor.Replace(",", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                scaleFactorFloat = float.Parse(scaleFactor);
            }
            catch
            {
                RaiseError("Scale factor is not a valid number.");
                return;
            }

            this.exportParameters = exportParameters;

            var gameConversionManger = Loader.Global.ConversionManager;
            gameConversionManger.CoordSystem = Autodesk.Max.IGameConversionManager.CoordSystem.D3d;

            var gameScene = Loader.Global.IGameInterface;
            gameScene.InitialiseIGame(false);
            gameScene.SetStaticFrame(0);

            MaxSceneFileName = gameScene.SceneFileName;

            IsCancelled = false;
            RaiseMessage("Exportation started", Color.Blue);
            ReportProgressChanged(0);

            string outputDirectory = Path.GetDirectoryName(exportParameters.outputPath);
            string outputFileName = Path.GetFileName(exportParameters.outputPath);

            // Check directory exists
            if (!Directory.Exists(outputDirectory))
            {
                RaiseError("Exportation stopped: Output folder does not exist");
                ReportProgressChanged(100);
                return;
            }

            var outputBabylonDirectory = outputDirectory;

            // Force output file extension to be babylon
            outputFileName = Path.ChangeExtension(outputFileName, "babylon");

            var babylonScene = new BabylonScene(outputBabylonDirectory);

            var rawScene = Loader.Core.RootNode;

            var watch = new Stopwatch();
            watch.Start();

            string outputFormat = exportParameters.outputFormat;
            isBabylonExported = outputFormat == "babylon" || outputFormat == "binary babylon";
            isGLTFExported = outputFormat == "gltf" || outputFormat == "glb";

            // Save scene
            if (exportParameters.autoSave3dsMaxFile)
            {
                RaiseMessage("Saving 3ds max file");
                var forceSave = Loader.Core.FileSave;

                callerForm?.BringToFront();
            }

            // Producer
            babylonScene.producer = new BabylonProducer
            {
                name = "3dsmax",
#if MAX2018
                version = "2018",
#elif MAX2017
                version = "2017",
#else
                version = Loader.Core.ProductVersion.ToString(),
#endif
                exporter_version = exporterVersion,
                file = outputFileName
            };

            // Global
            babylonScene.autoClear = true;
            babylonScene.clearColor = Loader.Core.GetBackGround(0, Tools.Forever).ToArray();
            babylonScene.ambientColor = Loader.Core.GetAmbient(0, Tools.Forever).ToArray();

            babylonScene.gravity = rawScene.GetVector3Property("babylonjs_gravity");
            ExportQuaternionsInsteadOfEulers = rawScene.GetBoolProperty("babylonjs_exportquaternions", 1);
            
            if (Loader.Core.UseEnvironmentMap && Loader.Core.EnvironmentMap != null)
            {
                // Environment texture
                var environmentMap = Loader.Core.EnvironmentMap;
                // Copy image file to output if necessary
                var babylonTexture = ExportEnvironmnentTexture(environmentMap, babylonScene);
                if (babylonTexture != null)
                {
                    babylonScene.environmentTexture = babylonTexture.name;

                    // Skybox
                    babylonScene.createDefaultSkybox = rawScene.GetBoolProperty("babylonjs_createDefaultSkybox");
                    babylonScene.skyboxBlurLevel = rawScene.GetFloatProperty("babylonjs_skyboxBlurLevel");
                }
            }
            
            // Instantiate custom material exporters
            materialExporters = new Dictionary<ClassIDWrapper, IMaterialExporter>();
            foreach (Type type in Tools.GetAllLoadableTypes())
            {
                if (type.IsAbstract || type.IsInterface || !typeof(IMaterialExporter).IsAssignableFrom(type))
                    continue;

                IMaterialExporter exporter = Activator.CreateInstance(type) as IMaterialExporter;

                if (exporter == null)
                    RaiseWarning("Creating exporter instance failed: " + type.Name, 1);

                materialExporters.Add(exporter.MaterialClassID, exporter);
            }

            // Sounds
            var soundName = rawScene.GetStringProperty("babylonjs_sound_filename", "");

            if (!string.IsNullOrEmpty(soundName))
            {
                var filename = Path.GetFileName(soundName);

                var globalSound = new BabylonSound
                {
                    autoplay = rawScene.GetBoolProperty("babylonjs_sound_autoplay", 1),
                    loop = rawScene.GetBoolProperty("babylonjs_sound_loop", 1),
                    name = filename
                };

                babylonScene.SoundsList.Add(globalSound);

                if (isBabylonExported)
                {
                    try
                    {
                        File.Copy(soundName, Path.Combine(babylonScene.OutputPath, filename), true);
                    }
                    catch
                    {
                    }
                }
            }

            // Root nodes
            RaiseMessage("Exporting nodes");
            HashSet<IIGameNode> maxRootNodes = getRootNodes(gameScene);
            var progressionStep = 80.0f / maxRootNodes.Count;
            var progression = 10.0f;
            ReportProgressChanged((int)progression);
            referencedMaterials.Clear();
            // Reseting is optionnal. It makes each morph target manager export starts from id = 0.
            BabylonMorphTargetManager.Reset();
            foreach (var maxRootNode in maxRootNodes)
            {
                exportNodeRec(maxRootNode, babylonScene, gameScene);
                progression += progressionStep;
                ReportProgressChanged((int)progression);
                CheckCancelled();
            };
            RaiseMessage(string.Format("Total meshes: {0}", babylonScene.MeshesList.Count), Color.Gray, 1);

            // Main camera
            BabylonCamera babylonMainCamera = null;
            ICameraObject maxMainCameraObject = null;
            if (babylonMainCamera == null && babylonScene.CamerasList.Count > 0)
            {
                // Set first camera as main one
                babylonMainCamera = babylonScene.CamerasList[0];
                babylonScene.activeCameraID = babylonMainCamera.id;
                RaiseMessage("Active camera set to " + babylonMainCamera.name, Color.Green, 1, true);

                // Retreive camera node with same GUID
                var maxCameraNodesAsTab = gameScene.GetIGameNodeByType(Autodesk.Max.IGameObject.ObjectTypes.Camera);
                var maxCameraNodes = TabToList(maxCameraNodesAsTab);
                var maxMainCameraNode = maxCameraNodes.Find(_camera => _camera.MaxNode.GetGuid().ToString() == babylonMainCamera.id);
                maxMainCameraObject = (maxMainCameraNode.MaxNode.ObjectRef as ICameraObject);
            }

            if (babylonMainCamera == null)
            {
                RaiseWarning("No camera defined", 1);
            }
            else
            {
                RaiseMessage(string.Format("Total cameras: {0}", babylonScene.CamerasList.Count), Color.Gray, 1);
            }

            // Default light
            if (babylonScene.LightsList.Count == 0)
            {
                RaiseWarning("No light defined", 1);
                RaiseWarning("A default hemispheric light was added for your convenience", 1);
                ExportDefaultLight(babylonScene);
            }
            else
            {
                RaiseMessage(string.Format("Total lights: {0}", babylonScene.LightsList.Count), Color.Gray, 1);
            }

            if (scaleFactorFloat != 1.0f)
            {
                RaiseMessage("A root node is added for scaling", 1);

                // Create root node for scaling
                BabylonMesh rootNode = new BabylonMesh { name = "root", id = Guid.NewGuid().ToString() };
                rootNode.isDummy = true;
                float rootNodeScale = 1.0f / scaleFactorFloat;
                rootNode.scaling = new float[3] { rootNodeScale, rootNodeScale, rootNodeScale };

                // Update all top nodes
                var babylonNodes = new List<BabylonNode>();
                babylonNodes.AddRange(babylonScene.MeshesList);
                babylonNodes.AddRange(babylonScene.CamerasList);
                babylonNodes.AddRange(babylonScene.LightsList);
                foreach (BabylonNode babylonNode in babylonNodes)
                {
                    if (babylonNode.parentId == null)
                    {
                        babylonNode.parentId = rootNode.id;
                    }
                }

                // Store root node
                babylonScene.MeshesList.Add(rootNode);
            }

            // Materials
            RaiseMessage("Exporting materials");

            var matsToExport = referencedMaterials.ToArray(); // Snapshot because multimaterials can export new materials
            foreach (var mat in matsToExport)
            {
                ExportMaterial(mat, babylonScene);
                CheckCancelled();
            }
            RaiseMessage(string.Format("Total: {0}", babylonScene.MaterialsList.Count + babylonScene.MultiMaterialsList.Count), Color.Gray, 1);

            // Fog
            for (var index = 0; index < Loader.Core.NumAtmospheric; index++)
            {
                var atmospheric = Loader.Core.GetAtmospheric(index);

                if (atmospheric.Active(0) && atmospheric.ClassName == "Fog")
                {
                    var fog = atmospheric as IStdFog;

                    RaiseMessage("Exporting fog");

                    if (fog != null)
                    {
                        babylonScene.fogColor = fog.GetColor(0).ToArray();
                        babylonScene.fogMode = 3;
                    }
                    if (babylonMainCamera != null)
                    {
                        babylonScene.fogStart = maxMainCameraObject.GetEnvRange(0, 0, Tools.Forever);
                        babylonScene.fogEnd = maxMainCameraObject.GetEnvRange(0, 1, Tools.Forever);
                    }
                }
            }

            // Skeletons
            if (skins.Count > 0)
            {
                RaiseMessage("Exporting skeletons");
                foreach (var skin in skins)
                {
                    ExportSkin(skin, babylonScene);
                }
            }

            // Output
            babylonScene.Prepare(false, false);
            if (isBabylonExported)
            {
                RaiseMessage("Saving to output file");
                
                var outputFile = Path.Combine(outputBabylonDirectory, outputFileName);

                var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings());
                var sb = new StringBuilder();
                var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
                
                using (var jsonWriter = new JsonTextWriterOptimized(sw))
                {
                    jsonWriter.Formatting = Formatting.None;
                    jsonSerializer.Serialize(jsonWriter, babylonScene);
                }
                File.WriteAllText(outputFile, sb.ToString());

                if (exportParameters.generateManifest)
                {
                    File.WriteAllText(outputFile + ".manifest",
                        "{\r\n\"version\" : 1,\r\n\"enableSceneOffline\" : true,\r\n\"enableTexturesOffline\" : true\r\n}");
                }

                // Binary
                if (outputFormat == "binary babylon")
                {
                    RaiseMessage("Generating binary files");
                    BabylonFileConverter.BinaryConverter.Convert(outputFile, outputBabylonDirectory + "\\Binary",
                        message => RaiseMessage(message, 1),
                        error => RaiseError(error, 1));
                }
            }

            ReportProgressChanged(100);

            // Export glTF
            if (isGLTFExported)
            {
                bool generateBinary = outputFormat == "glb";
                ExportGltf(babylonScene, outputDirectory, outputFileName, generateBinary);
            }

            watch.Stop();
            RaiseMessage(string.Format("Exportation done in {0:0.00}s", watch.ElapsedMilliseconds / 1000.0), Color.Blue);
        }

        private void exportNodeRec(IIGameNode maxGameNode, BabylonScene babylonScene, IIGameScene maxGameScene)
        {
            BabylonNode babylonNode = null;
            switch (maxGameNode.IGameObject.IGameType)
            {
                case Autodesk.Max.IGameObject.ObjectTypes.Mesh:
                    babylonNode = ExportMesh(maxGameScene, maxGameNode, babylonScene);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Camera:
                    babylonNode = ExportCamera(maxGameScene, maxGameNode, babylonScene);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Light:
                    babylonNode = ExportLight(maxGameScene, maxGameNode, babylonScene);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Unknown:
                    // Create a dummy (empty mesh) when type is unknown
                    // An example of unknown type object is the target of target light or camera
                    babylonNode = ExportDummy(maxGameScene, maxGameNode, babylonScene);
                    break;
                default:
                    // The type of node is not exportable (helper, spline, xref...)
                    break;
            }
            CheckCancelled();

            // If node is not exported successfully but is significant
            if (babylonNode == null &&
                isNodeRelevantToExport(maxGameNode))
            {
                // Create a dummy (empty mesh)
                babylonNode = ExportDummy(maxGameScene, maxGameNode, babylonScene);
            };
            
            if (babylonNode != null)
            {
                // Export its children
                for (int i = 0; i < maxGameNode.ChildCount; i++)
                {
                    var descendant = maxGameNode.GetNodeChild(i);
                    exportNodeRec(descendant, babylonScene, maxGameScene);
                }
            }


        }

        /// <summary>
        /// Return true if node descendant hierarchy has any exportable Mesh, Camera or Light
        /// </summary>
        private bool isNodeRelevantToExport(IIGameNode maxGameNode)
        {
            bool isRelevantToExport;
            switch (maxGameNode.IGameObject.IGameType)
            {
                case Autodesk.Max.IGameObject.ObjectTypes.Mesh:
                    isRelevantToExport = IsMeshExportable(maxGameNode);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Camera:
                    isRelevantToExport = IsCameraExportable(maxGameNode);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Light:
                    isRelevantToExport = IsLightExportable(maxGameNode);
                    break;
                case Autodesk.Max.IGameObject.ObjectTypes.Helper:
                    isRelevantToExport = IsNodeExportable(maxGameNode);
                    break;
                default:
                    isRelevantToExport = false;
                    break;
            }

            if (isRelevantToExport)
            {
                return true;
            }

            // Descandant recursivity
            List<IIGameNode> maxDescendants = getDescendants(maxGameNode);
            int indexDescendant = 0;
            while (indexDescendant < maxDescendants.Count) // while instead of for to stop as soon as a relevant node has been found
            {
                if (isNodeRelevantToExport(maxDescendants[indexDescendant]))
                {
                    return true;
                }
                indexDescendant++;
            }

            // No relevant node found in hierarchy
            return false;
        }

        private List<IIGameNode> getDescendants(IIGameNode maxGameNode)
        {
            var maxDescendants = new List<IIGameNode>();
            for (int i = 0; i < maxGameNode.ChildCount; i++)
            {
                maxDescendants.Add(maxGameNode.GetNodeChild(i));
            }
            return maxDescendants;
        }

        private HashSet<IIGameNode> getRootNodes(IIGameScene maxGameScene)
        {
            HashSet<IIGameNode> maxGameNodes = new HashSet<IIGameNode>();

            Func<IIGameNode, IIGameNode> getMaxRootNode = delegate (IIGameNode maxGameNode)
            {
                while (maxGameNode.NodeParent != null)
                {
                    maxGameNode = maxGameNode.NodeParent;
                }
                return maxGameNode;
            };

            Action<Autodesk.Max.IGameObject.ObjectTypes> addMaxRootNodes = delegate (Autodesk.Max.IGameObject.ObjectTypes type)
            {
                ITab<IIGameNode> maxGameNodesOfType = maxGameScene.GetIGameNodeByType(type);
                if (maxGameNodesOfType != null)
                {
                    TabToList(maxGameNodesOfType).ForEach(maxGameNode =>
                    {
                        var maxRootNode = getMaxRootNode(maxGameNode);
                        maxGameNodes.Add(maxRootNode);
                    });
                }
            };

            addMaxRootNodes(Autodesk.Max.IGameObject.ObjectTypes.Mesh);
            addMaxRootNodes(Autodesk.Max.IGameObject.ObjectTypes.Light);
            addMaxRootNodes(Autodesk.Max.IGameObject.ObjectTypes.Camera);
            addMaxRootNodes(Autodesk.Max.IGameObject.ObjectTypes.Helper);

            return maxGameNodes;
        }

        private static List<T> TabToList<T>(ITab<T> tab)
        {
            if (tab == null)
            {
                return null;
            }
            else
            {
                List<T> list = new List<T>();
                for (int i = 0; i < tab.Count; i++)
                {
#if MAX2017 || MAX2018
                    var item = tab[i];
#else
                    var item = tab[new IntPtr(i)];
#endif
                    list.Add(item);
                }
                return list;
            }
        }

        private bool IsNodeExportable(IIGameNode gameNode)
        {
            if (gameNode.MaxNode.GetBoolProperty("babylonjs_noexport"))
            {
                return false;
            }

            if (exportParameters.exportOnlySelected && !gameNode.MaxNode.Selected)
            {
                return false;
            }

            if (!exportParameters.exportHiddenObjects && gameNode.MaxNode.IsHidden(NodeHideFlags.None, false))
            {
                return false;
            }

            return true;
        }

        private IMatrix3 GetInvertWorldTM(IIGameNode gameNode, int key)
        {
            var worldMatrix = gameNode.GetWorldTM(key);
            var invertedWorldMatrix = worldMatrix.ExtractMatrix3();
            invertedWorldMatrix.Invert();
            return invertedWorldMatrix;
        }
    }
}
