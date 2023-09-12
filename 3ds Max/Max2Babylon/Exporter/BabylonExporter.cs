using Autodesk.Max;
using BabylonExport.Entities;
using Babylon2GLTF;
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
using Autodesk.Max.Plugins;
using GLTFExport.Entities;
using Color = System.Drawing.Color;
using Utilities;
using System.Configuration;

namespace Max2Babylon
{
    public partial class BabylonExporter
    {
        public ILoggingProvider logger;
        public BabylonExporter(ILoggingProvider _logger) 
        {
            logger = _logger;
        }
        public Form callerForm;

        public ExportParameters exportParameters;
        
        public bool IsCancelled { get; set; }

        public string MaxSceneFileName { get; set; }

        public bool ExportQuaternionsInsteadOfEulers { get; set; }

        private bool isBabylonExported, isGltfExported;
        private bool optimizeAnimations;
        private bool exportNonAnimated;

        public float scaleFactor = 1.0f;

        public const int MaxSceneTicksPerSecond = 4800; //https://knowledge.autodesk.com/search-result/caas/CloudHelp/cloudhelp/2016/ENU/MAXScript-Help/files/GUID-141213A1-B5A8-457B-8838-E602022C8798-htm.html

        public static string exporterVersion = GetConfigurationValue();
        

        public static string GetConfigurationValue()
        {
            string title = "1.0.0.0";
            string dllPath = Assembly.GetExecutingAssembly().Location;
            string dllFolder = Path.GetDirectoryName(dllPath);
            string configPath = Path.Combine(dllFolder,"Max2Babylon.dll.config");
            if(File.Exists(configPath))
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = configPath;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                var key = config.AppSettings.Settings["Version"];
                if(key!=null)
                {
                    title = key.Value;    
                }
            }

            return title;
        }

        public void Export(ExportParameters exportParameters)
        {
            
            var watch = new Stopwatch();
            watch.Start();

            this.exportParameters = exportParameters;
            IINode exportNode = null;
            double flattenTime = 0;
            if (exportParameters is MaxExportParameters)
            {
                MaxExportParameters maxExporterParameters = (exportParameters as MaxExportParameters);
                if (logger!=null) logger.LoggerLevel = maxExporterParameters.logLevel;
                exportNode = maxExporterParameters.exportNode;
            }

            Tools.InitializeGuidsMap();
            if (exportParameters.enableASBUniqueID) 
            {
                var nodeToResolve = Tools.VerifyUniqueIds();
                if (nodeToResolve.Count > 0)
                {
                    string criticalNodes ="";
                    foreach (IINode node in nodeToResolve)
                    {
                        criticalNodes += node.Name + "\n";
                        logger?.RaiseError($"Error: {node.Name} uniqueID has a conflict");
                    }
                    logger?.RaiseError($"Export interrupted due to UniqueID conflicts,Fix the issue through Babylon -> Resolve UniqueID button");
                    return;
                }
                
            }
            
            string fileExportString = exportNode != null? $"{exportNode.NodeName} | {exportParameters.outputPath}": exportParameters.outputPath;
            logger?.Print($"Exportation started: {fileExportString}", Color.Blue);


            this.scaleFactor = Tools.GetScaleFactorToMeters();

            long quality = exportParameters.txtQuality;
            try
            {
                if (quality < 0 || quality > 100)
                {
                    throw new Exception();
                }
            }
            catch
            {
                logger?.RaiseError("Quality is not a valid number. It should be an integer between 0 and 100.");
                logger?.RaiseError("This parameter sets the quality of jpg compression.");
                return;
            }

            var gameConversionManger = Loader.Global.ConversionManager;
            gameConversionManger.CoordSystem = Autodesk.Max.IGameConversionManager.CoordSystem.D3d;

            var gameScene = Loader.Global.IGameInterface;

            if (exportNode == null || exportNode.IsRootNode)
            {
                MaxExportParameters maxExportParameters = (exportParameters as MaxExportParameters);
                if (exportParameters.exportAsSubmodel)
                {
                    if (maxExportParameters.exportLayers != null)
                    {
                        //clear the current selection
                    IINodeTab selection = Tools.CreateNodeTab();
                        Loader.Core.SelectNodeTab(selection, true, false);
                        LayerUtilities.SelectLayersChildren(maxExportParameters.exportLayers);
                    }
                    exportParameters.exportOnlySelected = true;
                    gameScene.InitialiseIGame(false);
                }
                else 
                {
                    gameScene.InitialiseIGame(false);
                }
            }
            else if(exportNode!=null)
            {
                gameScene.InitialiseIGame(exportNode, true);
            }
            else
            {
                logger?.RaiseError("Impossible to initialize the scene");
            }

            gameScene.SetStaticFrame(0);

            MaxSceneFileName = gameScene.SceneFileName;

            IsCancelled = false;

            
            logger?.ReportProgressChanged(0);

            string tempOutputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string outputDirectory = Path.GetDirectoryName(exportParameters.outputPath);
            string folderOuputDirectory = exportParameters.textureFolder;
            string outputFileName = Path.GetFileName(exportParameters.outputPath);

            // Check directory exists
            if (!Directory.Exists(outputDirectory))
            {
                logger?.RaiseCriticalError($"Exportation stopped: Output folder ({folderOuputDirectory}\\{outputFileName}) does not exist");
                logger?.ReportProgressChanged(100);
                return;
            }
            Directory.CreateDirectory(tempOutputDirectory);
            
            var outputBabylonDirectory = tempOutputDirectory;

            // Force output file extension to be babylon
            outputFileName = Path.ChangeExtension(outputFileName, "babylon");

            var babylonScene = new BabylonScene(outputBabylonDirectory);

            var rawScene = Loader.Core.RootNode;

           

            string outputFormat = exportParameters.outputFormat;
            isBabylonExported = outputFormat == "babylon" || outputFormat == "binary babylon";
            isGltfExported = outputFormat == "gltf" || outputFormat == "glb";

            // Get scene parameters
            optimizeAnimations = true;
            exportNonAnimated = Loader.Core.RootNode.GetBoolProperty("babylonjs_animgroup_exportnonanimated");

            // Producer
            babylonScene.producer = new BabylonProducer
            {
                name = "3dsmax",
#if MAX2023
                version = "2023",
#elif MAX2022
                version = "2022",
#elif MAX2021
                version = "2021",
#elif MAX2020
                version = "2020",
#elif MAX2019
                version = "2019",
#elif MAX2018
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

            babylonScene.TimelineStartFrame = Loader.Core.AnimRange.Start / Loader.Global.TicksPerFrame;
            babylonScene.TimelineEndFrame = Loader.Core.AnimRange.End / Loader.Global.TicksPerFrame;
            babylonScene.TimelineFramesPerSecond = MaxSceneTicksPerSecond / Loader.Global.TicksPerFrame;

            babylonScene.gravity = rawScene.GetVector3Property("babylonjs_gravity");
            ExportQuaternionsInsteadOfEulers = rawScene.GetBoolProperty("babylonjs_exportquaternions", 1);
            if (string.IsNullOrEmpty(exportParameters.pbrEnvironment) && Loader.Core.UseEnvironmentMap && Loader.Core.EnvironmentMap != null)
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
            else if (!string.IsNullOrEmpty(exportParameters.pbrEnvironment))
            {
                babylonScene.createDefaultSkybox = rawScene.GetBoolProperty("babylonjs_createDefaultSkybox");
                babylonScene.skyboxBlurLevel = rawScene.GetFloatProperty("babylonjs_skyboxBlurLevel");
            }

            
            // Instantiate custom material exporters
            //materialExporters = new Dictionary<ClassIDWrapper, IBabylonMaterialExtensionExporter>();
            foreach (Type type in Tools.GetAllLoadableTypes())
            {
                if (type.IsAbstract || type.IsInterface )
                    continue;

                if (typeof(IBabylonExtensionExporter).IsAssignableFrom(type))
                {
                    IBabylonExtensionExporter exporter = Activator.CreateInstance(type) as IBabylonExtensionExporter;

                    if (exporter == null)
                        logger?.RaiseWarning("Creating exporter instance failed: " + type.Name, 1);

                    ExtendedTypes t = exporter.GetExtendedType();
                    babylonScene.BabylonToGLTFExtensions.Add(exporter,t);
                }
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
            logger?.Print("Exporting nodes",Color.Black);
            if (exportParameters.exportAsSubmodel) logger?.RaiseWarning("Exported As SubModel");
            HashSet<IIGameNode> maxRootNodes = (exportParameters.exportAsSubmodel == false) ? getRootNodes(gameScene) : getSubModelsRootNodes(gameScene);
            
            var progressionStep = 80.0f / maxRootNodes.Count;
            var progression = 10.0f;
            logger?.ReportProgressChanged((int)progression);
            referencedMaterials.Clear();

            // Reseting is optional. It makes each morph target manager export starts from id = 0.
            BabylonMorphTargetManager.Reset();

            List<BabylonNode> rootNodes = new List<BabylonNode>();

            if(exportParameters.exportAsSubmodel)
            {
                rootNodes = setRootNodesForSubmodel(gameScene, outputBabylonDirectory, babylonScene, exportParameters);
                }
            else
                {
                rootNodes = setRootNodesForBaseModel(gameScene, exportNode, babylonScene, exportParameters);
                    }

            if(rootNodes.Count() > 0)
            {
                babylonScene.RootNodes.AddRange(rootNodes);
                }
                

            logger?.RaiseMessage(string.Format("Total meshes: {0}", babylonScene.MeshesList.Count), Color.Gray, 1);


            // In 3DS Max the default camera look down (in the -z direction for the 3DS Max reference (+y for babylon))
            // In Babylon the default camera look to the horizon (in the +z direction for the babylon reference)
            // In glTF the default camera look to the horizon (in the +Z direction for glTF reference)
            logger?.RaiseMessage("Update camera rotation and position", 1);
            for (int index = 0; index < babylonScene.CamerasList.Count; index++)
            {
                BabylonCamera camera = babylonScene.CamerasList[index];
                FixCamera(ref camera, ref babylonScene);
            }

            // Light for glTF
            if (isGltfExported)
            {
                logger?.RaiseMessage("Update light rotation for glTF export", 1);
                for (int index = 0; index < babylonScene.LightsList.Count; index++)
                {
                    BabylonNode light = babylonScene.LightsList[index];
                    FixNodeRotation(ref light, ref babylonScene, -Math.PI / 2);
                }

            }

            // Main camera
            BabylonCamera babylonMainCamera = null;
            ICameraObject maxMainCameraObject = null;
            if (babylonMainCamera == null && babylonScene.CamerasList.Count > 0)
            {
                // Set first camera as main one
                babylonMainCamera = babylonScene.CamerasList[0];
                babylonScene.activeCameraID = babylonMainCamera.id;
                logger?.Print("Active camera set to " + babylonMainCamera.name, Color.Green, 1, true);

                // Retrieve camera node with same GUID
                var maxCameraNodesAsTab = gameScene.GetIGameNodeByType(Autodesk.Max.IGameObject.ObjectTypes.Camera);
                var maxCameraNodes = TabToList(maxCameraNodesAsTab);
                var maxMainCameraNode = maxCameraNodes.Find(_camera => _camera.MaxNode.GetGuid().ToString() == babylonMainCamera.id);
                maxMainCameraObject = (maxMainCameraNode.MaxNode.ObjectRef as ICameraObject);
            }

            if (babylonMainCamera == null)
            {
                logger?.RaiseWarning("No camera defined", 1);
            }
            else
            {
                logger?.RaiseMessage(string.Format("Total cameras: {0}", babylonScene.CamerasList.Count), Color.Gray, 1);
            }

            // Default light
            bool addDefaultLight = false; //rawScene.GetBoolProperty("babylonjs_addDefaultLight", 0); //on FLIGHTSIM we do not use default light at all
            if (!exportParameters.pbrNoLight && addDefaultLight && babylonScene.LightsList.Count == 0)
            {
                logger?.RaiseWarning("No light defined", 1);
                logger?.RaiseWarning("A default hemispheric light was added for your convenience", 1);
                ExportDefaultLight(babylonScene);
            }
            else
            {
                logger?.RaiseMessage(string.Format("Total lights: {0}", babylonScene.LightsList.Count), Color.Gray, 1);
            }

            if (exportParameters.scaleFactor != 1.0f)
            {
                logger?.RaiseMessage(String.Format("A root node is added to globally scale the scene by {0}", exportParameters.scaleFactor), 1);

                // Create root node for scaling
                BabylonMesh rootNode = new BabylonMesh { name = "root", id = Guid.NewGuid().ToString() };
                rootNode.isDummy = true;
                float rootNodeScale = exportParameters.scaleFactor;
                rootNode.scaling = new float[3] { rootNodeScale, rootNodeScale, rootNodeScale };

                if (ExportQuaternionsInsteadOfEulers)
                {
                    rootNode.rotationQuaternion = new float[] { 0, 0, 0, 1 };
                }
                else
                {
                    rootNode.rotation = new float[] { 0, 0, 0 };
                }

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
                if (!babylonScene.MeshesList.Any(x => x.id == rootNode.id)) babylonScene.MeshesList.Add(rootNode);
            }

#if DEBUG
            var nodesExportTime = watch.ElapsedMilliseconds / 1000.0 - flattenTime;
            logger?.Print($"Nodes exported in {nodesExportTime:0.00}s", Color.Blue);
#endif

            // Materials
            if (exportParameters.exportMaterials)
            {
                logger?.Print("Exporting materials",Color.Black);
                var matsToExport =
                    referencedMaterials.ToArray(); // Snapshot because multimaterials can export new materials
                foreach (var mat in matsToExport)
                {
                    ExportMaterial(mat, babylonScene);
                    logger?.CheckCancelled(this);
                }

                logger?.RaiseMessage(string.Format("Total: {0}",babylonScene.MaterialsList.Count + babylonScene.MultiMaterialsList.Count), Color.Gray, 1);
            }
            else
            {
                logger?.Print("Skipping material export.",Color.Black);
            }
#if DEBUG
            var materialsExportTime = watch.ElapsedMilliseconds / 1000.0 - nodesExportTime;
            logger?.Print($"Materials exported in {materialsExportTime:0.00}s", Color.Blue);
#endif


            // Fog
            for (var index = 0; index < Loader.Core.NumAtmospheric; index++)
            {
                var atmospheric = Loader.Core.GetAtmospheric(index);
                if (atmospheric == null) continue;

#if MAX2016 || MAX2017 || MAX2018 || MAX2019 || MAX2020 || MAX2021
                string atmosphericClassName = atmospheric.ClassName;
#else
                string atmosphericClassName = "";
                atmospheric.GetClassName(ref atmosphericClassName);
#endif
                if (atmospheric.Active(0) && atmosphericClassName == "Fog")
                {
                    var fog = atmospheric as IStdFog;

                   logger?.RaiseMessage("Exporting fog");

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
                logger?.Print("Exporting skeletons",Color.Black);
                foreach (var skin in skins)
                {
                    ExportSkin(skin, babylonScene);
                }
            }
#if DEBUG
            var skeletonsExportTime = watch.ElapsedMilliseconds / 1000.0 - materialsExportTime;
            logger?.Print($"Skeletons exported in {skeletonsExportTime:0.00}s", Color.Blue);
#endif


            // ----------------------------
            // ----- Animation groups -----
            // ----------------------------

            //Remove useless animations
            
            logger?.Print("Export animation groups",Color.Black);
            // add animation groups to the scene

            //if (optimizeAnimations)
            //{
             //   RemoveStaticAnimations(ref babylonScene);
            //}

            babylonScene.animationGroups = ExportAnimationGroups(babylonScene);
#if DEBUG
            var animationGroupExportTime = watch.ElapsedMilliseconds / 1000.0 -nodesExportTime;
            logger?.Print(string.Format("Animation groups exported in {0:0.00}s", animationGroupExportTime), Color.Blue);
#endif


            if (isBabylonExported)
            {
                // if we are exporting to .Babylon then remove then remove animations from nodes if there are animation groups.
                if (babylonScene.animationGroups?.Count > 0)
                {
                    foreach (BabylonNode node in babylonScene.MeshesList)
                    {
                        node.animations = null;
                    }
                    foreach (BabylonNode node in babylonScene.LightsList)
                    {
                        node.animations = null;
                    }
                    foreach (BabylonNode node in babylonScene.CamerasList)
                    {
                        node.animations = null;
                    }
                    foreach (BabylonSkeleton skel in babylonScene.SkeletonsList)
                    {
                        foreach (BabylonBone bone in skel.bones)
                        {
                            bone.animation = null;
                        }
                    }
                }

                // setup a default skybox for the scene for .Babylon export.
                var sourcePath = exportParameters.pbrEnvironment;
                if (!string.IsNullOrEmpty(sourcePath)) {
                    var fileName = Path.GetFileName(sourcePath);

                    // Allow only dds file format
                    if (!fileName.EndsWith(".dds"))
                    {
                        logger?.RaiseWarning("Failed to export default environment texture: only .dds format is supported.");
                    }
                    else
                    {
                        logger?.RaiseMessage($"texture id = Max_Babylon_Default_Environment");
                        babylonScene.environmentTexture = fileName;

                        if (exportParameters.writeTextures)
                        {
                            try
                            {
                                var destPath = Path.Combine(babylonScene.OutputPath, fileName);
                                if (File.Exists(sourcePath) && sourcePath != destPath)
                                {
                                    File.Copy(sourcePath, destPath, true);
                                }
                            }
                            catch
                            {
                                // silently fails
                                logger?.RaiseMessage($"Fail to export the default env texture", 3);
                            }
                        }
                    }
                }
            }

            // Output
            babylonScene.Prepare(false, false);
            if (isBabylonExported)
            {
                logger?.Print("Saving to output file",Color.Black);

                var outputFile = Path.Combine(outputBabylonDirectory, outputFileName);

                var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings());
                var sb = new StringBuilder();
                var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
                using (var jsonWriter = new JsonTextWriterOptimized(sw))
                {
                    jsonWriter.Formatting = Newtonsoft.Json.Formatting.None;
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
                    logger?.RaiseMessage("Generating binary files");
                    BabylonFileConverter.BinaryConverter.Convert(outputFile, outputBabylonDirectory + "\\Binary",
                        message => logger?.RaiseMessage(message, 1),
                        error => logger?.RaiseError(error, 1));
                }
            }

            logger?.ReportProgressChanged(100);

            // Export glTF
            if (isGltfExported)
            {
                bool generateBinary = outputFormat == "glb";

                GLTFExporter gltfExporter = new GLTFExporter();
                //exportParameters.customGLTFMaterialExporter = new MaxGLTFMaterialExporter(exportParameters, gltfExporter, this);
                gltfExporter.ExportGltf(this.exportParameters, babylonScene, tempOutputDirectory, outputFileName, generateBinary, logger);
            }
            // Move files to output directory
            var filePaths = Directory.GetFiles(tempOutputDirectory);
            if (outputFormat == "binary babylon")
            {
                var tempBinaryOutputDirectory = Path.Combine(tempOutputDirectory, "Binary");
                var binaryFilePaths = Directory.GetFiles(tempBinaryOutputDirectory);
                foreach(var filePath in binaryFilePaths)
                {
                    if (filePath.EndsWith(".binary.babylon"))
                    {
                        var file = Path.GetFileName(filePath);
                        var tempFilePath = Path.Combine(tempBinaryOutputDirectory, file);
                        var outputFile = Path.Combine(outputDirectory, file);

                        IUTF8Str maxNotification = GlobalInterface.Instance.UTF8Str.Create(outputFile);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PreExport, maxNotification);
                        moveFileToOutputDirectory(tempFilePath, outputFile, exportParameters);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PostExport, maxNotification);
                    }
                    else if (filePath.EndsWith(".babylonbinarymeshdata"))
                    {
                        var file = Path.GetFileName(filePath);
                        var tempFilePath = Path.Combine(tempBinaryOutputDirectory, file);
                        var outputFile = Path.Combine(outputDirectory, file);

                        IUTF8Str maxNotification = GlobalInterface.Instance.UTF8Str.Create(outputFile);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PreExport, maxNotification);
                        moveFileToOutputDirectory(tempFilePath, outputFile, exportParameters);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PostExport, maxNotification);
                    }
                }
            }
            if (outputFormat == "glb")
            {
                foreach (var file_path in filePaths)
                {
                    if (Path.GetExtension(file_path) == ".glb")
                    {
                        var file = Path.GetFileName(file_path);
                        var tempFilePath = Path.Combine(tempOutputDirectory, file);
                        var outputFile = Path.Combine(outputDirectory, file);


                        IUTF8Str maxNotification = GlobalInterface.Instance.UTF8Str.Create(outputFile);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PreExport, maxNotification);
                        moveFileToOutputDirectory(tempFilePath, outputFile, exportParameters);
                        Loader.Global.BroadcastNotification(SystemNotificationCode.PostExport, maxNotification);

                        break;
                    }   
                }
            }
            else
            { 
                foreach (var filePath in filePaths)
                {
                    var file = Path.GetFileName(filePath);
                    string ext = Path.GetExtension(file);
                    var tempFilePath = Path.Combine(tempOutputDirectory, file);
                    var outputPath = Path.Combine(outputDirectory, file);
                    if (!string.IsNullOrWhiteSpace(exportParameters.textureFolder) && TextureUtilities.ExtensionIsValidGLTFTexture(ext))
                    {
                        outputPath = Path.Combine(exportParameters.textureFolder, file);
                    }

                    IUTF8Str maxNotification = GlobalInterface.Instance.UTF8Str.Create(outputPath);
                    Loader.Global.BroadcastNotification(SystemNotificationCode.PreExport, maxNotification);
                    moveFileToOutputDirectory(tempFilePath, outputPath, exportParameters);
                    Loader.Global.BroadcastNotification(SystemNotificationCode.PostExport, maxNotification);
                }
            }

            if(Directory.Exists(tempOutputDirectory))
            {
                try
                {
            Directory.Delete(tempOutputDirectory, true);
                }
                catch
                {
                    logger?.RaiseError(tempOutputDirectory + " is ReadOnly");
                }
            }
            
            
            watch.Stop();

            logger?.Print(string.Format("Exportation done in {0:0.00}s: {1}", watch.ElapsedMilliseconds / 1000.0, fileExportString), Color.Blue);
            IUTF8Str max_notification = Autodesk.Max.GlobalInterface.Instance.UTF8Str.Create("BabylonExportComplete");
            Loader.Global.BroadcastNotification(SystemNotificationCode.PostExport, max_notification);
            }

        private bool ExportBabylonExtension<T1>(T1 sceneObject,Type babylonType, ref BabylonScene babylonScene)
        {
            foreach (var extensionExporter in babylonScene.BabylonToGLTFExtensions)
            {
                if (extensionExporter.Value.babylonType == babylonType)
                {
                   if(extensionExporter.Key.ExportBabylonExtension(sceneObject,ref babylonScene,this)) return true;
                }
            }

            return false;
        }

        private void moveFileToOutputDirectory(string sourceFilePath, string targetFilePath, ExportParameters exportParameters)
        {
            var texDir = Path.GetDirectoryName(targetFilePath);
            if (!Directory.Exists(texDir)) 
            {
                logger.RaiseCriticalError($"Impossible to find the following directory: {texDir}");
            }
            var fileExtension = Path.GetExtension(sourceFilePath).Substring(1).ToLower();
            if (validFormats.Contains(fileExtension))
            {
                
                if (exportParameters.writeTextures)
                {
                    if (File.Exists(targetFilePath))
                    {
                        FileInfo fileInfo = new FileInfo(targetFilePath);
                        fileInfo.IsReadOnly = false;
                        if (exportParameters.overwriteTextures)
                        {
                            File.Delete(targetFilePath);
                            File.Move(sourceFilePath, targetFilePath);
                            logger?.Print(sourceFilePath + " -> " + targetFilePath,Color.Green);
                        }
                    }
                    else
                    {
                        File.Move(sourceFilePath, targetFilePath);
                        logger?.Print(sourceFilePath + " -> " + targetFilePath,Color.Green);
                    }
                }
            }
            else
            {
                if (File.Exists(targetFilePath))
                {
                    FileInfo fileInfo = new FileInfo(targetFilePath);
                    fileInfo.IsReadOnly = false;
                    File.Delete(targetFilePath);
                }
                File.Move(sourceFilePath, targetFilePath);
                logger?.Print(sourceFilePath + " -> " + targetFilePath, Color.Green);
            }
        }

        
        private BabylonNode exportNodeRec(IIGameNode maxGameNode, BabylonScene babylonScene, IIGameScene maxGameScene)
        {
            BabylonNode babylonNode = null;
            try
            {
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
            }
            catch (Exception e)
            {
                this.logger?.RaiseWarning(String.Format("Exception raised during export. Node will be exported as dummy node. \r\nMessage: \r\n{0} \r\n{1}", e.Message, e.InnerException), 2);
            }

            logger?.CheckCancelled(this);

            // If node is not exported successfully but is significant
            if (babylonNode == null &&
                isNodeRelevantToExport(maxGameNode))
            {
                // Create a dummy (empty mesh)
                babylonNode = ExportDummy(maxGameScene, maxGameNode, babylonScene);
            };
            
            if (babylonNode != null)
            {
                string tag = maxGameNode.MaxNode.GetStringProperty("babylonjs_tag", "");
                if (tag != "")
                {
                    babylonNode.tags = tag;
                }

                babylonNode.UniqueID = maxGameNode.MaxNode.GetUniqueID();
                babylonNode.AnimationTargetId = babylonNode.UniqueID;

                // Export its children
                for (int i = 0; i < maxGameNode.ChildCount; i++)
                {
                    var descendant = maxGameNode.GetNodeChild(i);
                    exportNodeRec(descendant, babylonScene, maxGameScene);
                }
                babylonScene.NodeMap[babylonNode.id] = babylonNode;
            }

            return babylonNode;
        }

        private void calculateSkeletonList(IIGameNode maxGameNode, BabylonScene babylonScene, IIGameScene maxGameScene )
        {
            if (maxGameNode.IGameObject.IGameType is Autodesk.Max.IGameObject.ObjectTypes.Mesh)
            {
                var gameMesh = maxGameNode.IGameObject.AsGameMesh();
                // Skin
                var isSkinned = gameMesh.IsObjectSkinned;
                var skin = gameMesh.IGameSkin;
                IGMatrix skinInitPoseMatrix = Loader.Global.GMatrix.Create(Loader.Global.Matrix3.Create(true));

                if (isSkinned && GetSkinnedBones(skin, maxGameNode).Count > 0)  // if the mesh has a skin with at least one bone
                {
                    var skinAlreadyStored = skins.Find(_skin => IsSkinEqualTo(_skin, skin));
                    if (skinAlreadyStored == null)
                    {
                        skins.Add(skin);
                        skinNodeMap.Add(skin,maxGameNode);
                    }

                    skin.GetInitSkinTM(skinInitPoseMatrix);
                }
            }
            else
            {
                BabylonNode babylonNode = ExportDummy(maxGameScene, maxGameNode, babylonScene);   
                babylonScene.NodeMap[babylonNode.id] = babylonNode;
            }

            for (int i = 0; i < maxGameNode.ChildCount; i++)
            {
                var descendant = maxGameNode.GetNodeChild(i);
                calculateSkeletonList(descendant,babylonScene,maxGameScene);
            }

            
        }

        private List<IIGameNode> CalculateSubModelSkinsDependencies(IIGameNode maxGameNode, BabylonScene babylonScene, IIGameScene maxGameScene)
        {
            List<IIGameNode> subModelDeps = new List<IIGameNode>();
            if (maxGameNode.IGameObject.IGameType is Autodesk.Max.IGameObject.ObjectTypes.Mesh)
            {
                var gameMesh = maxGameNode.IGameObject.AsGameMesh();
                // Skin
                var isSkinned = gameMesh.IsObjectSkinned;
                var skin = gameMesh.IGameSkin;

                if (isSkinned) subModelDeps = GetSubModelSkinHierarchy(skin, maxGameNode); // if the mesh has a skin with at least one bone
            }


            for (int i = 0; i < maxGameNode.ChildCount; i++)
            {
                var descendant = maxGameNode.GetNodeChild(i);
                subModelDeps.AddRange(CalculateSubModelSkinsDependencies(descendant, babylonScene, maxGameScene));
            }

            return subModelDeps;
        }

        private BabylonNode CalculateSubModelBonesDependencies(IIGameNode maxGameNode, BabylonScene babylonScene, IIGameScene maxGameScene) 
        {
            BabylonNode subModelRoot = null;
            List<IIGameNode> subModelDeps = CalculateSubModelSkinsDependencies(maxGameNode, babylonScene, maxGameScene);

            foreach (IIGameNode subModelDep in subModelDeps)
            {
                if (!subModelDep.MaxNode.Selected)
                {
                    BabylonNode babylonNode = ExportSubModelExtraNode(maxGameScene, subModelDep, babylonScene);
                    if (!subModelDeps.Contains(subModelDep.NodeParent)) subModelRoot = babylonNode;
                    babylonScene.NodeMap[babylonNode.id] = babylonNode;
                }

            }
            return subModelRoot;
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

        private HashSet<IIGameNode> getSubModelsRootNodes(IIGameScene maxGameScene)
        {
            HashSet<IIGameNode> maxGameNodes = new HashSet<IIGameNode>();

            Func<IIGameNode, IIGameNode> getMaxRootNode = delegate (IIGameNode maxGameNode)
            {
                while (maxGameNode.NodeParent != null && maxGameNode.NodeParent.MaxNode.Selected == true)
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
                        if (maxGameNode.MaxNode.Selected) 
                        {
                            var maxRootNode = getMaxRootNode(maxGameNode);
                            maxGameNodes.Add(maxRootNode);
                        }
                        
                    });
                }
            };

            addMaxRootNodes(Autodesk.Max.IGameObject.ObjectTypes.Mesh);
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
#if MAX2017 || MAX2018 || MAX2019 || MAX2020 || MAX2021 || MAX2022 || MAX2023
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
            if (gameNode.MaxNode.GetBoolProperty("babylonjs_flatteningTemp"))
            {
                return true;
            }

            if (exportParameters is MaxExportParameters)
            {
                MaxExportParameters maxExporterParameters = (exportParameters as MaxExportParameters);
                if (maxExporterParameters.exportLayers!=null && maxExporterParameters.exportLayers.Count>0)
                {
                    if (!maxExporterParameters.exportLayers.HaveNode(gameNode.MaxNode))
                    {
                        return false;
                    }
                }
            }

            if (gameNode.MaxNode.GetBoolProperty("babylonjs_flattened"))
            {
                return false;
            }


            if (gameNode.MaxNode.GetBoolProperty("babylonjs_noexport"))
            {
                return false;
            }

            if (gameNode.MaxNode.IsBabylonContainerHelper() || gameNode.MaxNode.IsBabylonAnimationHelper())
            {
                return false;
            }

            if ((exportParameters.exportOnlySelected|| exportParameters.exportAsSubmodel) && !gameNode.MaxNode.Selected)
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

        private IMatrix3 GetOffsetTM(IIGameNode gameNode, int key)
        {
            IPoint3 objOffsetPos = gameNode.MaxNode.ObjOffsetPos;
            IQuat objOffsetQuat = gameNode.MaxNode.ObjOffsetRot;
            IPoint3 objOffsetScale = gameNode.MaxNode.ObjOffsetScale.S;

            // conversion: LH vs RH coordinate system (swap Y and Z)
            var tmpSwap = objOffsetPos.Y;
            objOffsetPos.Y = objOffsetPos.Z;
            objOffsetPos.Z = tmpSwap;

            tmpSwap = objOffsetQuat.Y;
            objOffsetQuat.Y = objOffsetQuat.Z;
            objOffsetQuat.Z = tmpSwap;
            var objOffsetRotMat = Tools.Identity;
            objOffsetQuat.MakeMatrix(objOffsetRotMat, true);

            tmpSwap = objOffsetScale.Y;
            objOffsetScale.Y = objOffsetScale.Z;
            objOffsetScale.Z = tmpSwap;

            // build the offset transform; equivalent in maxscript: 
            // offsetTM = (scaleMatrix $.objectOffsetScale) * ($.objectOffsetRot as matrix3) * (transMatrix $.objectOffsetPos)
            IMatrix3 offsetTM = Tools.Identity;
            offsetTM.Scale(objOffsetScale, false);
            offsetTM.MultiplyBy(objOffsetRotMat);
            offsetTM.Translate(objOffsetPos); 

            return offsetTM;
        }

        /// <summary>
        /// In 3DS Max default element can look in different direction than the same default element in Babylon or in glTF.
        /// This function correct the node rotation.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="babylonScene"></param>
        /// <param name="angle"></param>
        private void FixNodeRotation(ref BabylonNode node, ref BabylonScene babylonScene, double angle)
        {
            string id = node.id;
            IList<BabylonMesh> meshes = babylonScene.MeshesList.FindAll(mesh => mesh.parentId == null ? false : mesh.parentId.Equals(id));

            logger?.RaiseMessage($"{node.name}", 2);

            // fix the vue
            // Rotation around the axis X of PI / 2 in the indirect direction for camera
            // double angle = Math.PI / 2; // for camera
            // double angle = -Math.PI / 2; // for light

            if (node.rotation != null)
            {
                node.rotation[0] += (float)angle;
            }
            if (node.rotationQuaternion != null)
            {
                BabylonQuaternion rotationQuaternion = FixCameraQuaternion(node, angle);

                node.rotationQuaternion = rotationQuaternion.ToArray();
                node.rotation = rotationQuaternion.toEulerAngles().ToArray();
            }

            // animation
            List<BabylonAnimation> animations = new List<BabylonAnimation>(node.animations);
            BabylonAnimation animationRotationQuaternion = animations.Find(animation => animation.property.Equals("rotationQuaternion"));
            if (animationRotationQuaternion != null)
            {
                foreach (BabylonAnimationKey key in animationRotationQuaternion.keys)
                {
                    key.values = FixCameraQuaternion(key.values, angle);
                }
            }
            else   // if the camera has a lockedTargetId, it is the extraAnimations that stores the rotation animation
            {
                if (node.extraAnimations != null)
                {
                    List<BabylonAnimation> extraAnimations = new List<BabylonAnimation>(node.extraAnimations);
                    animationRotationQuaternion = extraAnimations.Find(animation => animation.property.Equals("rotationQuaternion"));
                    if (animationRotationQuaternion != null)
                    {
                        foreach (BabylonAnimationKey key in animationRotationQuaternion.keys)
                        {
                            key.values = FixCameraQuaternion(key.values, angle);
                        }
                    }
                }
            }

            // fix direct children
            // Rotation around the axis X of -PI / 2 in the direct direction for camera children
            angle = -angle;
            foreach (var mesh in meshes)
            {
                logger?.RaiseVerbose($"{mesh.name}", 3);
                mesh.position = new float[] { mesh.position[0], mesh.position[2], -mesh.position[1] };

                // Add a rotation of PI/2 axis X in direct direction
                if (mesh.rotationQuaternion != null)
                {
                    // Rotation around the axis X of -PI / 2 in the direct direction
                    BabylonQuaternion quaternion = FixChildQuaternion(mesh, angle);

                    mesh.rotationQuaternion = quaternion.ToArray();
                }
                if (mesh.rotation != null)
                {
                    mesh.rotation[0] += (float)angle;
                }


                // Animations
                animations = new List<BabylonAnimation>(mesh.animations);
                // Position
                BabylonAnimation animationPosition = animations.Find(animation => animation.property.Equals("position"));
                if (animationPosition != null)
                {
                    foreach (BabylonAnimationKey key in animationPosition.keys)
                    {
                        key.values = new float[] { key.values[0], key.values[2], -key.values[1] };
                    }
                }

                // Rotation
                animationRotationQuaternion = animations.Find(animation => animation.property.Equals("rotationQuaternion"));
                if (animationRotationQuaternion != null)
                {
                    foreach (BabylonAnimationKey key in animationRotationQuaternion.keys)
                    {
                        key.values = FixChildQuaternion(key.values, angle);
                    }
                }
            }

        }

        private void SetNodePosition(ref BabylonNode node, ref BabylonScene babylonScene, float[] newPosition)
        {
            float[] offset = new float[] { newPosition[0] - node.position[0], newPosition[1] - node.position[1], newPosition[2] - node.position[2] };
            node.position = newPosition;

            if (node.animations == null) return;

            List<BabylonAnimation> animations = new List<BabylonAnimation>(node.animations);
            BabylonAnimation animationPosition = animations.Find(animation => animation.property.Equals("position"));
            if (animationPosition != null)
            {
                foreach (BabylonAnimationKey key in animationPosition.keys)
                {
                    key.values = new float[] {
                        key.values[0] + offset[0],
                        key.values[1] + offset[1],
                        key.values[2] + offset[2] };
                }
            }
        }

        private List<BabylonNode> setRootNodesForBaseModel(IIGameScene gameScene, IINode exportNode, BabylonScene babylonScene, ExportParameters exportParameters)
        {
            HashSet<IIGameNode> maxRootNodes = getRootNodes(gameScene);
            List<BabylonNode> result = new List<BabylonNode>();

            foreach (var maxRootNode in maxRootNodes)
            {
                if(isGltfExported && exportParameters.animationExportType == AnimationExportType.ExportOnly)
                {
                    calculateSkeletonList(maxRootNode, babylonScene, gameScene);
                }
                else
                {
                    BabylonNode node = exportNodeRec(maxRootNode, babylonScene, gameScene);

                    if (node != null) result.Add(node);

                    // if we're exporting from a specific node, reset the pivot to {0,0,0}
                    if (node != null && exportNode != null && !exportNode.IsRootNode)
                        SetNodePosition(ref node, ref babylonScene, new float[] { 0, 0, 0 });
                }
            }

            return result;
        }

        private List<BabylonNode> setRootNodesForSubmodel(IIGameScene gameScene, string outputBabylonDirectory, BabylonScene babylonScene, ExportParameters exportParameters)
        {
            HashSet<IIGameNode> maxRootNodes = getRootNodes(gameScene);
            List<BabylonNode> result = new List<BabylonNode>();

            BabylonScene sceneTemp = new BabylonScene(outputBabylonDirectory);

            //Init scene nodemap from max game scene
            foreach (var maxRootNode in maxRootNodes)
            {
                BabylonNode node = exportNodeRec(maxRootNode, sceneTemp, gameScene);
            }

            //Init root nodes
            List<BabylonNode> copyRootNodes = new List<BabylonNode>();

            foreach (var maxRootNode in maxRootNodes)
            {
                if (isGltfExported && exportParameters.animationExportType == AnimationExportType.ExportOnly)
                {
                    calculateSkeletonList(maxRootNode, babylonScene, gameScene);
                }

                // in case the skin has one or more bones that are not part of the submodel hierarchy
                BabylonNode subModelRoot = CalculateSubModelBonesDependencies(maxRootNode, babylonScene, gameScene);
                BabylonNode node = exportNodeRec(maxRootNode, babylonScene, gameScene);

                if (node != null)
                {
                    if (subModelRoot == null)
                    {
                        // in case the skin has all the skinned nodes in the submodel child hierarchy
                        // in case it is not a mesh 
                        // in case it is not skinned
                        subModelRoot = node;
                    }
                    else
                    {
                        if (!copyRootNodes.Any(x => x.id == subModelRoot.id)) copyRootNodes.Add(subModelRoot);
                    }

                    subModelRoot.subModelRoot = true;


                    if (subModelRoot != null && !result.Any(x => x.id == subModelRoot.id)) result.Add(subModelRoot);
                }
            }

            // If exported as submodel and we need to keep only the common ancesstor for the skin meshs
            if (copyRootNodes.Count > 0)
            {
                foreach (var node in copyRootNodes)
                {
                    if (result.Any(x => x.id == node.id))
                    {
                        //List<BabylonNode> childRootNodes = babylonScene.RootNodes.FindAll(x => x.parentId == node.id);
                        List<BabylonNode> childRootNodes = new List<BabylonNode>();

                        foreach (var root in result)
                        {
                            if (root.parentId != null)
                            {
                                BabylonNode parent = getRootParent(root, sceneTemp);

                                if (parent.id == node.id)
                                {
                                    childRootNodes.Add(root);
                                }
                            }

                        }

                        if (childRootNodes.Count > 0)
                        {
                            foreach (var child in childRootNodes)
                            {
                                result.Remove(child);
                            }
                        }
                    }

                }
            }


            return result;
        }

        BabylonNode getRootParent(BabylonNode node, BabylonScene scene)
        {
            if (node.parentId == null)
                return node;
            else return getRootParent(scene.NodeMap[node.parentId], scene);
        }
    }
}
