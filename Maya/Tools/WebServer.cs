﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Threading.Tasks;

namespace Maya2Babylon
{
    public static class WebServer
    {
        
        private static readonly HttpListener listener;
        private static Task runningTask;

        const string HtmlResponseText = @"
<!doctype html>
<html>

<head>
    <title>Babylon.js</title>
    <script type='text/javascript' src='https://preview.babylonjs.com/oimo.js'></script>
    <script type='text/javascript' src='https://preview.babylonjs.com/cannon.js'></script>
    <script type='text/javascript' src='https://preview.babylonjs.com/babylon.js'></script>
    <script type='text/javascript' src='https://preview.babylonjs.com/loaders/babylon.glTFFileLoader.js'></script>
    <script type='text/javascript' src='https://preview.babylonjs.com/inspector/babylon.inspector.bundle.js'></script>
    <style type='text/css'>
        html, body, canvas {
            width: 100%;
            height: 100%;
            padding: 0;
            margin: 0;
            overflow: hidden;
        }

        #debugLayerButton {
            position: absolute;
            border: white solid 1px;
            background: rgba(128, 128, 128, 0.3);
            color: white;
            left: 50%;
            width: 100px;
            margin-left:-50px;
            bottom: 10px;
        }
    </style>
</head>

<body>
    <canvas id='canvas'></canvas>
    <button id='debugLayerButton'>Debug layer</button>
    <script type='text/javascript'>
        var canvas = document.getElementById('canvas');
        var engine = new BABYLON.Engine(canvas, true);
       
        BABYLON.SceneLoader.Load('', '###SCENE###', engine, function (newScene) {

            // Attach camera to canvas inputs
            if (!newScene.activeCamera || newScene.lights.length === 0) {
                newScene.createDefaultCameraOrLight(true);
                // Enable camera's behaviors
                newScene.activeCamera.useFramingBehavior = true;

                var framingBehavior = newScene.activeCamera.getBehaviorByName('Framing');
                framingBehavior.framingTime = 0;
                framingBehavior.elevationReturnTime = -1;

                if (newScene.meshes.length) {
                    var worldExtends = newScene.getWorldExtends();
                    newScene.activeCamera.lowerRadiusLimit = null;
                    framingBehavior.zoomOnBoundingInfo(worldExtends.min, worldExtends.max);
                }

                newScene.activeCamera.pinchPrecision = 200 / newScene.activeCamera.radius;
                newScene.activeCamera.upperRadiusLimit = 5 * newScene.activeCamera.radius;

                newScene.activeCamera.wheelDeltaPercentage = 0.01;
                newScene.activeCamera.pinchDeltaPercentage = 0.01;
            }

            newScene.activeCamera.attachControl(canvas);

            var keyboard = newScene.activeCamera.inputs.attached.keyboard;
            keyboard.keysUp.push(87);
            keyboard.keysDown.push(83);
            keyboard.keysLeft.push(65);
            keyboard.keysRight.push(68);

            engine.runRenderLoop(function() {
                newScene.render();
            });

            window.addEventListener('resize', function () {
                engine.resize();
            });

            document.getElementById('debugLayerButton').addEventListener('click', function () {
                if (newScene.debugLayer.isVisible()) {
                    newScene.debugLayer.hide();
                } else {
                    newScene.debugLayer.show();
                }
            });
        });
    </script>
</body>
</html>";

        public const int Port = 45478;

        public static bool IsSupported { get; private set; }

        static WebServer()
        {
            try
            {
                listener = new HttpListener();

                if (!HttpListener.IsSupported)
                {
                    IsSupported = false;
                    return;
                }

                listener.Prefixes.Add("http://localhost:" + Port + "/");
                listener.Start();


                runningTask = Task.Run(() => Listen());

                IsSupported = true;
            }
            catch
            {
                IsSupported = false;
            }
        }

        public static string SceneFilename { get; set; }
        public static string SceneFolder { get; set; }
        static Random r = new Random();
        static void Listen()
        {
            try
            {
                while (listener.IsListening)
                {
                    var context = listener.GetContext();
                    var request = context.Request;
                    var url = request.Url;

                    context.Response.AddHeader("Cache-Control", "no-cache");
                    if (string.IsNullOrEmpty(url.LocalPath) || url.LocalPath == "/")
                    {

                        var responseText = HtmlResponseText.Replace("###SCENE###", SceneFilename + "?once=" + r.Next());
                        WriteResponse(context, responseText);
                    }
                    else
                    {
                        try
                        {
                            var path = Path.Combine(SceneFolder, HttpUtility.UrlDecode(url.PathAndQuery.Substring(1)));
                            var questionMarkIndex = path.IndexOf("?");
                            if (questionMarkIndex != -1)
                            {
                                path = path.Substring(0, questionMarkIndex);
                            }
                            var hashIndex = path.IndexOf("#");
                            if (hashIndex != -1)
                            {
                                path = path.Substring(0, hashIndex);
                            }
                            var buffer = File.ReadAllBytes(path);
                            WriteResponse(context, buffer);
                        }
                        catch
                        {
                            context.Response.StatusCode = 404;
                            context.Response.Close();
                        }
                    }

                }
            }
            catch
            {
            }
        }

        static void WriteResponse(HttpListenerContext context, string s)
        {
            WriteResponse(context.Response, s);
        }

        static void WriteResponse(HttpListenerContext context, byte[] buffer)
        {
            WriteResponse(context.Response, buffer);
        }

        static void WriteResponse(HttpListenerResponse response, string s)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(s);
            WriteResponse(response, buffer);
        }

        static void WriteResponse(HttpListenerResponse response, byte[] buffer)
        {
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }
}
