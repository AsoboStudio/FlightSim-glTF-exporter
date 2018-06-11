﻿using BabylonExport.Entities;
using GLTFExport.Entities;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        private GLTFCamera ExportCamera(ref GLTFNode gltfNode, BabylonCamera babylonCamera, GLTF gltf, GLTFNode gltfParentNode)
        {
            RaiseMessage("GLTFExporter.Camera | Export camera named: " + babylonCamera.name, 2);

            // --- prints ---
            #region prints

            RaiseVerbose("GLTFExporter.Camera | babylonCamera data", 3);
            RaiseVerbose("GLTFExporter.Camera | babylonCamera.type=" + babylonCamera.type, 4);
            RaiseVerbose("GLTFExporter.Camera | babylonCamera.fov=" + babylonCamera.fov, 4);
            RaiseVerbose("GLTFExporter.Camera | babylonCamera.maxZ=" + babylonCamera.maxZ, 4);
            RaiseVerbose("GLTFExporter.Camera | babylonCamera.minZ=" + babylonCamera.minZ, 4);
            #endregion


            // --------------------------
            // ------- gltfCamera -------
            // --------------------------

            RaiseMessage("GLTFExporter.Camera | create gltfCamera", 3);

            // Camera
            var gltfCamera = new GLTFCamera { name = babylonCamera.name };
            gltfCamera.index = gltf.CamerasList.Count;
            gltf.CamerasList.Add(gltfCamera);
            gltfNode.camera = gltfCamera.index;
            gltfCamera.gltfNode = gltfNode;

            // Camera type
            switch (babylonCamera.mode)
            {
                case (BabylonCamera.CameraMode.ORTHOGRAPHIC_CAMERA):
                    var gltfCameraOrthographic = new GLTFCameraOrthographic();
                    gltfCameraOrthographic.xmag = 1; // Do not bother about it - still mandatory
                    gltfCameraOrthographic.ymag = 1; // Do not bother about it - still mandatory
                    gltfCameraOrthographic.zfar = babylonCamera.maxZ;
                    gltfCameraOrthographic.znear = babylonCamera.minZ;

                    gltfCamera.type = GLTFCamera.CameraType.orthographic.ToString();
                    gltfCamera.orthographic = gltfCameraOrthographic;
                    break;
                case (BabylonCamera.CameraMode.PERSPECTIVE_CAMERA):
                    var gltfCameraPerspective = new GLTFCameraPerspective();
                    gltfCameraPerspective.aspectRatio = null; // Do not bother about it - use default glTF value
                    gltfCameraPerspective.yfov = babylonCamera.fov; // Babylon camera fov mode is assumed to be vertical (FOVMODE_VERTICAL_FIXED)
                    gltfCameraPerspective.zfar = babylonCamera.maxZ;
                    gltfCameraPerspective.znear = babylonCamera.minZ;

                    gltfCamera.type = GLTFCamera.CameraType.perspective.ToString();
                    gltfCamera.perspective = gltfCameraPerspective;
                    break;
                default:
                    RaiseError("GLTFExporter.Camera | camera mode not found");
                    break;
            }
            
            return gltfCamera;
        }
    }
}
