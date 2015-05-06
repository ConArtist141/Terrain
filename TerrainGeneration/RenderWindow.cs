﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK.Input;

namespace TerrainGeneration
{
    /// <summary>
    /// A window class for displaying render results
    /// </summary>
    public class RenderWindow : GameWindow
    {
        public Renderer Renderer { get; protected set; }
        public Scene Scene { get; protected set; }
        public CameraController CameraController { get; protected set; }

        public RenderWindow() : base(800, 600)
        {
            Title = "Terrain Generation Project";
            Icon = Properties.Resources.ProgramIcon;
        }

        protected virtual Renderer CreateRenderer()
        {
            return new Renderer();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Debug.WriteLine("Initializing Renderer...");
            Renderer = CreateRenderer();
            Renderer.Initialize(this);

            Debug.WriteLine("Creating Scene...");
            CreateScene();
        }

        protected override void OnResize(EventArgs e)
        {
            Renderer.OnResize(ClientSize);

            base.OnResize(e);
        }

        protected virtual void CreateScene()
        {
            // Load our shader
            Debug.WriteLine("Loading Shaders...");
            ShaderProgram terrainShader = ResourceLoader.LoadProgramFromFile("Shaders\\Terrain.vert", "Shaders\\Terrain.frag");

            // Create our terrain entity
            Debug.WriteLine("Creating Mesh Data...");
            var terrainData = TerrainData.CreateGaussian(64, 64, Vector2.One * 4.0f, 40.0f, new Vector2(128f, 128f), 30f);
            var terrainMesh = terrainData.CreateMesh();
            var terrainEntity = new Entity()
            {
                EntityMesh = terrainMesh,
                Transform = Matrix4.Identity,
                EntityProgram = terrainShader
            };

            // Create our scene
            Scene = new Scene();
            Scene.Resources.Add(terrainMesh);
            Scene.Resources.Add(terrainShader);
            Scene.Entities.Add(terrainEntity); 
   
            // Position the camera correctly
            CameraController = new RotationCameraController(Renderer.Camera, Keyboard, Mouse)
            {
                CameraCenter = new Vector3(128f, 0f, 128f),
                Phi = (float)Math.PI / 4f,
                Radius = 128f
            };
            CameraController.UpdateCameraParams();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
                Close();

            if (CameraController != null)
                CameraController.UpdateCamera(e);

            Renderer.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Render the scene
            Renderer.Render(e, Scene);

            SwapBuffers();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (CameraController != null)
                CameraController.Dispose();

            // Dispose all remaining resources
            Debug.WriteLine("Disposing Resources...");
            Scene.Dispose();
            Renderer.Dispose();
            Renderer = null;

            base.OnClosed(e);
        }
    }
}
