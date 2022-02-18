using Autodesk.Max;
using Autodesk.Max.IQuadMenuContext;
using Autodesk.Max.Plugins;
using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;

namespace Max2Babylon
{
    class GlobalUtility : GUP
    {
        IIMenu menu;
        IIMenuItem menuItem;
        IIMenuItem menuItemBabylon;
        uint idActionTable;
        IActionTable actionTable;
        IActionCallback actionCallback;

        /// <summary>
        /// Store reference of exporter form to close it manually when exiting 3ds max
        /// </summary>
        BabylonExportActionItem babylonExportActionItem;

#if MAX2018 || MAX2019
        GlobalDelegates.Delegate5 m_SystemStartupDelegate;
#endif
        private static bool filePreOpenCallback = false;
        private GlobalDelegates.Delegate5 m_FilePreOpenDelegate;

        private static bool filePostOpenCallback = false;
        private GlobalDelegates.Delegate5 m_FilePostOpenDelegate;

        private static bool sceneCloseCallback = false;
        private GlobalDelegates.Delegate5 m_sceneCloseDelegate;

        private static bool postSceneResetCallback = false;
        private GlobalDelegates.Delegate5 m_PostSceneResetCallback;
        private GlobalDelegates.Delegate5 m_PostSceneResetCallback2;

        private static bool nodeAddedCallback = false;
        private GlobalDelegates.Delegate5 m_NodeAddedDelegate;

        private static bool nodeDeleteCallback = false;
        private GlobalDelegates.Delegate5 m_NodeDeleteDelegate;

        private static bool preSceneSave = false;
        private GlobalDelegates.Delegate5 m_PreSceneSave;

        private static bool postSceneSave = false;
        private GlobalDelegates.Delegate5 m_PostSceneSave;


        private void MenuSystemStartupHandler(IntPtr objPtr, INotifyInfo infoPtr)
        {
            InstallMenus();
            AddCallbacks();
        }

        private void InitializeBabylonGuids(IntPtr param0, IntPtr param1)
        {
            Tools.guids = new Dictionary<Guid, IAnimatable>();
        }

        private void InitializeBabylonGuids(IntPtr objPtr, INotifyInfo infoPtr)
        {
            Tools.guids = new Dictionary<Guid, IAnimatable>();
        }
#if MAX2015 || MAX2016
        private void FilePostOpen(IntPtr objPtr, IntPtr param1)
        {
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (var container in sceneContainers)
            {
                if (container.IsUnloaded == true && !container.IsUnique)
                {
                    bool n = container.LoadContainer;
                }
            }
        }
        private void MaxClose(IntPtr objPtr, IntPtr param1)
        {
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (var container in sceneContainers)
            {
                if (container.IsUnloaded == true && !container.IsUnique)
                {
                    bool n = container.LoadContainer;
                }
            }
        }

        private void ResetScene(IntPtr objPtr, IntPtr param1)
        {
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (var container in sceneContainers)
            {
                if (!container.IsUnique) 
                {
                    bool n = container.UnloadContainer;
                }
            }
        }
        private void SceneSave(IntPtr objPtr, IntPtr param1)
        {
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (var container in sceneContainers)
            {
                if (container.IsOpen == false && !container.IsUnique)
                {
                    bool n = container.UnloadContainer;
                }
            }
        }
        private void PostSceneSave(IntPtr objPtr, IntPtr infoPtr)
        {
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (var container in sceneContainers)
            {
                if (container.IsOpen == false && !container.IsUnique)
                {
                    bool n = container.LoadContainer;
                }
            }
        }
#endif


        private void FilePostOpen(IntPtr objPtr, INotifyInfo infoPtr)
        {
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (var container in sceneContainers)
            {
                if (container.IsUnloaded == true && !container.IsUnique)
                {
                    bool n = container.LoadContainer;
                }
            }
        }
        private void MaxClose(IntPtr objPtr, INotifyInfo infoPtr)
        {
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (var container in sceneContainers)
            {
                if (!container.IsUnique) 
                {
                    bool n = container.UnloadContainer;
                }
            }
        }

        private void ResetScene(IntPtr objPtr, INotifyInfo infoPtr)
        {
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (var container in sceneContainers)
            {
                if (!container.IsUnique) 
                {
                    bool n = container.UnloadContainer;
                }
            }
        }
        private void SceneSave(IntPtr objPtr, INotifyInfo infoPtr)
        {
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (var container in sceneContainers)
            {
                if (container.IsOpen == false && !container.IsUnique)
                {
                    bool n = container.UnloadContainer;
                }
            }
        }

        private void PostSceneSave(IntPtr objPtr, INotifyInfo infoPtr)
        {
            List<IIContainerObject> sceneContainers = Tools.GetAllContainers();
            foreach (var container in sceneContainers)
            {
                if (container.IsOpen == false && !container.IsUnique)
                {
                    bool n = container.LoadContainer;
                }
            }
        }

#if MAX2015 || MAX2016
        private void OnNodeAdded(IntPtr param0, IntPtr param1)
        {
            try
            {
                INotifyInfo obj = Loader.Global.NotifyInfo.Marshal(param1);

                IINode n = (IINode) obj.CallParam;
                //todo replace this with something like isXREFNODE
                //to have a distinction between added xref node and max node
                string guid = n.GetStringProperty("babylonjs_GUID", string.Empty);
                if (string.IsNullOrEmpty(guid))
                {
                    n.GetGuid(); // force to assigne a new guid if not exist yet for this node
                }
                n.DeleteProperty("flightsim_uniqueIDresolved");
                n.DeleteProperty("flightsim_uniqueID");

                IIContainerObject contaner = Loader.Global.ContainerManagerInterface.IsContainerNode(n);
                if (contaner != null)
                {
                    // a generic operation on a container is done (open/inherit)
                    contaner.ResolveContainer();
                }
            }
            catch
            {
                // Fails silently
            }
        }
#endif

#if MAX2015 || MAX2016
        private void OnNodeDeleted(IntPtr objPtr, IntPtr param1)
        {
            try
            {
                INotifyInfo obj = Loader.Global.NotifyInfo.Marshal(param1);

                IINode n = (IINode) obj.CallParam;
                Tools.guids.Remove(n.GetGuid());
            }
            catch
            {
                // Fails silently
            }
        }
#endif

        private void OnNodeAdded(IntPtr objPtr, INotifyInfo infoPtr)
        {
            try
            {
                IINode n = (IINode)infoPtr.CallParam;
                //todo replace this with something like isXREFNODE
                //to have a distinction between added xref node and max node
                string guid = n.GetStringProperty("babylonjs_GUID", string.Empty);
                if (string.IsNullOrEmpty(guid))
                {
                    n.GetGuid(); // force to assigne a new guid if not exist yet for this node
                }

                IIContainerObject container = Loader.Global.ContainerManagerInterface.IsContainerNode(n);
                if (container != null)
                {
                    // a generic operation on a container is done (open/inherit)
                    container.ResolveContainer();
                }
            }
            catch
            {
                // Fails silently
            }
        }

        private void OnNodeDeleted(IntPtr objPtr, INotifyInfo infoPtr)
        {
            try
            {
                IINode n = (IINode)infoPtr.CallParam;
                Tools.guids.Remove(n.GetGuid());
            }
            catch
            {
                // Fails silently
            }
        }

        public override void Stop()
        {
            try
            {
                // Close exporter form manually
                if (babylonExportActionItem != null)
                {
                    babylonExportActionItem.Close();
                }

                if (actionTable != null)
                {
                    Loader.Global.COREInterface.ActionManager.DeactivateActionTable(actionCallback, idActionTable);
                }

                // Clean up menu
                if (menu != null)
                {
                    Loader.Global.COREInterface.MenuManager.UnRegisterMenu(menu);
                    Loader.Global.ReleaseIMenu(menu);
                    Loader.Global.ReleaseIMenuItem(menuItemBabylon);
                    Loader.Global.ReleaseIMenuItem(menuItem);

                    menu = null;
                    menuItem = null;
                }
            }
            catch
            {
                // Fails silently
            }
        }

        public override uint Start
        {
            get
            {
                IIActionManager actionManager = Loader.Core.ActionManager;

                // Set up global actions
                idActionTable = (uint)actionManager.NumActionTables;

                string actionTableName = "Babylon Actions";
#if MAX2016 || MAX2017 || MAX2018 || MAX2019 || MAX2020 || MAX2021
                actionTable = Loader.Global.ActionTable.Create(idActionTable, 0, ref actionTableName);
#else
                actionTable = Loader.Global.ActionTable.Create(idActionTable, 0, actionTableName);
#endif
                babylonExportActionItem = new BabylonExportActionItem();
                actionTable.AppendOperation(babylonExportActionItem);
                actionTable.AppendOperation(new BabylonResolveUniqueIDActionItem());
                actionTable.AppendOperation(new BabylonPropertiesActionItem()); // Babylon Properties forms are modals => no need to store reference
                actionTable.AppendOperation(new BabylonAnimationActionItem());
                actionTable.AppendOperation(new BabylonSaveAnimations());
                actionTable.AppendOperation(new BabylonLoadAnimations());
                actionTable.AppendOperation(new BabylonSkipFlattenToggle());

                actionCallback = new BabylonActionCallback();

                actionManager.RegisterActionTable(actionTable);
                actionManager.ActivateActionTable(actionCallback as ActionCallback, idActionTable);



                // Set up menus
#if MAX2018 || MAX2019
                var global = GlobalInterface.Instance;
                m_SystemStartupDelegate = new GlobalDelegates.Delegate5(MenuSystemStartupHandler);
                global.RegisterNotification(m_SystemStartupDelegate, null, SystemNotificationCode.SystemStartup);


#else
                InstallMenus();
                AddCallbacks();
#endif
                RegisterFilePreOpen();
                RegisterFilePostOpen();
                RegisterSceneClose();
                RegisterPostSceneReset();
                RegisterOnPostSaveScene();
                RegisterNodeAddedCallback();
                RegisterNodeDeletedCallback();
                RegisterOnSaveScene();
                return 0;
            }
        }

        public void RegisterFilePreOpen()
        {
            if (!filePreOpenCallback)
            {
                m_FilePreOpenDelegate = new GlobalDelegates.Delegate5(this.InitializeBabylonGuids);
#if MAX2015
                GlobalInterface.Instance.RegisterNotification(this.m_FilePreOpenDelegate, null, SystemNotificationCode.NodeLinked);
#else
                GlobalInterface.Instance.RegisterNotification(this.m_FilePreOpenDelegate, null, SystemNotificationCode.FilePreOpen);
#endif

                filePreOpenCallback = true;
            }
        }

        public void RegisterFilePostOpen()
        {
            if (!filePostOpenCallback)
            {
                m_FilePostOpenDelegate = new GlobalDelegates.Delegate5(this.FilePostOpen);
#if MAX2015
                GlobalInterface.Instance.RegisterNotification(this.m_FilePostOpenDelegate, null, SystemNotificationCode.NodeLinked);
#else
                GlobalInterface.Instance.RegisterNotification(this.m_FilePostOpenDelegate, null, SystemNotificationCode.FilePostOpen);
#endif
                filePostOpenCallback = true;
            }
        }

        public void RegisterSceneClose()
        {
            if (!sceneCloseCallback)
            {
                m_sceneCloseDelegate = new GlobalDelegates.Delegate5(this.MaxClose);
#if MAX2015
                GlobalInterface.Instance.RegisterNotification(this.m_sceneCloseDelegate, null, SystemNotificationCode.NodeLinked);
#else
                GlobalInterface.Instance.RegisterNotification(this.m_sceneCloseDelegate, null, SystemNotificationCode.SystemShutdown);
#endif

                sceneCloseCallback = true;
            }
        }

        public void RegisterPostSceneReset()
        {
            if (!postSceneResetCallback)
            {
                m_PostSceneResetCallback = new GlobalDelegates.Delegate5(this.InitializeBabylonGuids);
                m_PostSceneResetCallback2 = new GlobalDelegates.Delegate5(this.ResetScene);
#if MAX2015
                GlobalInterface.Instance.RegisterNotification(this.m_PostSceneResetCallback, null, SystemNotificationCode.NodeLinked);
                GlobalInterface.Instance.RegisterNotification(this.m_PostSceneResetCallback2, null, SystemNotificationCode.NodeLinked);
#else
                GlobalInterface.Instance.RegisterNotification(this.m_PostSceneResetCallback, null, SystemNotificationCode.PostSceneReset);
                GlobalInterface.Instance.RegisterNotification(this.m_PostSceneResetCallback2, null, SystemNotificationCode.PostSceneReset);
#endif

                postSceneResetCallback = true;
            }
        }

        public void RegisterOnSaveScene()
        {
            if (!preSceneSave)
            {
                m_PreSceneSave = new GlobalDelegates.Delegate5(this.SceneSave);
#if MAX2015
                GlobalInterface.Instance.RegisterNotification(this.m_PreSceneSave, null, SystemNotificationCode.NodeLinked);
#else
                GlobalInterface.Instance.RegisterNotification(this.m_PreSceneSave, null, SystemNotificationCode.FilePreSave);
#endif

                preSceneSave = true;
            }
        }

        public void RegisterOnPostSaveScene()
        {
            if (!postSceneSave)
            {
                m_PostSceneSave = new GlobalDelegates.Delegate5(this.PostSceneSave);
#if MAX2015
                GlobalInterface.Instance.RegisterNotification(this.m_PostSceneSave, null, SystemNotificationCode.NodeLinked);
#else
                GlobalInterface.Instance.RegisterNotification(this.m_PostSceneSave, null, SystemNotificationCode.FilePostSave);
#endif

                postSceneSave = true;
            }
        }


        public void RegisterNodeAddedCallback()
        {
            if (!nodeAddedCallback)
            {
                m_NodeAddedDelegate = new GlobalDelegates.Delegate5(this.OnNodeAdded);
#if MAX2015
                //bug on Autodesk API  SystemNotificationCode.SceneAddedNode doesn't work for max 2015-2016
                GlobalInterface.Instance.RegisterNotification(this.m_NodeAddedDelegate, null, SystemNotificationCode.NodeLinked );
#else
                GlobalInterface.Instance.RegisterNotification(this.m_NodeAddedDelegate, null, SystemNotificationCode.SceneAddedNode);
#endif
                nodeAddedCallback = true;
            }
        }

        public void RegisterNodeDeletedCallback()
        {
            if (!nodeDeleteCallback)
            {
                m_NodeDeleteDelegate = new GlobalDelegates.Delegate5(this.OnNodeDeleted);
                GlobalInterface.Instance.RegisterNotification(this.m_NodeDeleteDelegate, null, SystemNotificationCode.ScenePreDeletedNode);
                nodeDeleteCallback = true;
            }
        }

        private void InstallMenus()
        {
            IIMenuManager menuManager = Loader.Core.MenuManager;

            // Set up menu
            menu = menuManager.FindMenu("Babylon");

            if (menu != null)
            {
                menuManager.UnRegisterMenu(menu);
                Loader.Global.ReleaseIMenu(menu);
                menu = null;
            }

            // Main menu
            menu = Loader.Global.IMenu;
            menu.Title = "Babylon";
            menuManager.RegisterMenu(menu, 0);

            // Launch option
            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "&File Exporter";
            menuItemBabylon.ActionItem = actionTable[0];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "&Resolve UniqueID";
            menuItemBabylon.ActionItem = actionTable[1];
            menu.AddItem(menuItemBabylon, -1);

            menuItem = Loader.Global.IMenuItem;
            menuItem.SubMenu = menu;

            menuManager.MainMenuBar.AddItem(menuItem, -1);

            // Quad
            var rootQuadMenu = menuManager.GetViewportRightClickMenu(RightClickContext.NonePressed);
            var quadMenu = rootQuadMenu.GetMenu(0);

            menu = menuManager.FindMenu("Babylon...");

            if (menu != null)
            {
                menuManager.UnRegisterMenu(menu);
                Loader.Global.ReleaseIMenu(menu);
                menu = null;
            }

            menu = Loader.Global.IMenu;
            menu.Title = "Babylon...";
            menuManager.RegisterMenu(menu, 0);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Properties";
            menuItemBabylon.ActionItem = actionTable[1];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Animation Groups";
            menuItemBabylon.ActionItem = actionTable[2];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Save Animation To Containers";
            menuItemBabylon.ActionItem = actionTable[3];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Load Animation From Containers";
            menuItemBabylon.ActionItem = actionTable[4];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Toggle Skip Flatten Status";
            menuItemBabylon.ActionItem = actionTable[5];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Actions Builder";
            menuItemBabylon.ActionItem = actionTable[6];
            menu.AddItem(menuItemBabylon, -1);

            menuItem = Loader.Global.IMenuItem;
            menuItem.SubMenu = menu;

            quadMenu.AddItem(menuItem, -1);

            Loader.Global.COREInterface.MenuManager.UpdateMenuBar();
        }

        private void AddCallbacks()
        {
            // Retreive the material just created
            string cmd = "maxMaterial = callbacks.notificationParam();";

            // Easy syntax for a switch/case expression
            cmd += "\r\n" + "if classof maxMaterial == StandardMaterial then";
            cmd += "\r\n" + "(";
            cmd += "\r\n" + BabylonExporter.GetStandardBabylonAttributesDataCA();
            cmd += "\r\n" + "custAttributes.add maxMaterial babylonAttributesDataCA;";
            cmd += "\r\n" + ")";
            cmd += "\r\n" + "else if classof maxMaterial == PhysicalMaterial then";
            cmd += "\r\n" + "(";
            cmd += "\r\n" + BabylonExporter.GetPhysicalBabylonAttributesDataCA();
            cmd += "\r\n" + "custAttributes.add maxMaterial babylonAttributesDataCA;";
            cmd += "\r\n" + ")";
            cmd += "\r\n" + "else if classof maxMaterial == ai_standard_surface then";
            cmd += "\r\n" + "(";
            cmd += "\r\n" + BabylonExporter.GetAiStandardSurfaceBabylonAttributesDataCA();
            cmd += "\r\n" + "custAttributes.add maxMaterial babylonAttributesDataCA;";
            cmd += "\r\n" + ")";

            // Escape cmd
            cmd = cmd.Replace("\"", "\\\"");

            // Create cmd as string
            ScriptsUtilities.ExecuteMaxScriptCommand("cmd = \"" + cmd + "\"");

            // Remove any definition of this callback
            ScriptsUtilities.ExecuteMaxScriptCommand("callbacks.removeScripts id:#BabylonAttributesMaterial;");

            // Add a callback triggered when a new material is created
            // Note:
            // The callback is NOT persistent (default value is false).
            // This means that it is not linked to a specific file.
            // Rather, the callback is active for the current run of 3ds Max.
            // See Autodesk documentation for more info: http://help.autodesk.com/view/3DSMAX/2015/ENU/?guid=__files_GUID_C1F6495F_5831_4FC8_A00C_667C5F2EAE36_htm
            ScriptsUtilities.ExecuteMaxScriptCommand("callbacks.addScript #mtlRefAdded cmd id:#BabylonAttributesMaterial;");
        }
    }
}
