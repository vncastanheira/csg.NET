﻿using System;
using System.IO;
using csg_NET;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Map_Visualizer
{
    class Program
    {
        static float FOV = 1.3f;

        static GameWindow window;
        static Camera camera;
        static MapFile map;

        static int pgmID;
        static int vertID;
        static int fragID;

        static void Main(string[] args)
        {
            // make sure parameters are valid
            if (args.Length == 0)
            {
                Console.WriteLine("csg map_file");
                Console.WriteLine("no map was given to the program ='(");
                Console.ReadLine();
                return;
            }

            MapLoad(args[0]);

            window = new GameWindow(600, 600, GraphicsMode.Default, "MAP Visualizer", GameWindowFlags.FixedWindow);
            camera = new Camera(new OpenTK.Vector3(0, 0, 0), FOV, window.Width / (float)window.Height, 0.3f, 1000.0f);

            window.Load += Window_Load;
            window.RenderFrame += Window_RenderFrame;
            window.Resize += Window_Resize;
            window.KeyPress += Window_KeyPress;
            window.FocusedChanged += Window_FocusedChanged;
            window.Run(1 / 60);
        }

        #region Window 

        /// <summary> Window mouse focus </summary>
        private static void Window_FocusedChanged(object sender, EventArgs e)
        {
            if (window.Focused)
            {
                ResetCursor();
            }
        }

        /// <summary> On Key pressing </summary>
        private static void Window_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'w')
                camera.Move(0f, 0.1f, 0f);

            if (e.KeyChar == 's')
                camera.Move(0f, -0.1f, 0f);

            if (e.KeyChar == 'a')
                camera.Move(-0.1f, 0f, 0f);

            if (e.KeyChar == 'd')
                camera.Move(0.1f, 0f, 0f);

            if (e.KeyChar == 'q')
                camera.Move(0f, 0f, 0.1f);

            if (e.KeyChar == 'e')
                camera.Move(0f, 0f, -0.1f);

            if (e.KeyChar == 'r')
            {
                map.Reload();
                first = true;
            }

            if (e.KeyChar == '+')
                FOV += 0.1f;

            if (e.KeyChar == '-')
                FOV -= 0.1f;

            if (FOV > 3f)
                FOV = 3f;

            if (e.KeyChar == 'o' || e.KeyChar == 27)
                Environment.Exit(0);
        }

        /// <summary> On Window Resize </summary>
        private static void Window_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, window.Width, window.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 matrix = camera.GetViewMatrix() * camera.Perspective;
            GL.LoadMatrix(ref matrix);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.Translate(new OpenTK.Vector3(-24f, 26f, -25.5f));
        }

        /// <summary> On Window Load </summary>
        private static void Window_Load(object sender, EventArgs e)
        {
            window.CursorVisible = false;

            //pgmID = GL.CreateProgram();
            //LoadShader("vs.glsl", ShaderType.VertexShader, pgmID, out vertID);
            //LoadShader("fs.glsl", ShaderType.FragmentShader, pgmID, out fragID);
            //GL.LinkProgram(pgmID);
            //GL.ValidateProgram(pgmID);
            //GL.CompileShader(pgmID);
            //string info = "Shader: " + GL.GetProgramInfoLog(pgmID);
            //Console.WriteLine(string.IsNullOrEmpty(info) ? "no log" : info);

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Enable(EnableCap.DepthTest);

            Console.WriteLine("Error status: " + GL.GetError());

            //Matrix4 matrix = Matrix4.CreateTranslation(new OpenTK.Vector3(0, -90, -90));
            //GL.LoadMatrix(ref matrix);
        }

        /// <summary> On Window Update </summary>
        private static void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            window.Title = string.Format("MAP Viewer | FPS: {0:0.##}", (1f / e.Time));

            if (window.Focused)
            {
                Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);

                camera.AddRotation(delta.X, delta.Y);
                ResetCursor();
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 matrix = camera.GetViewMatrix();
            GL.LoadMatrix(ref matrix);

            //GL.UseProgram(pgmID);
            //DrawTutorialCube();
            DrawMap();

            GL.End();

            window.SwapBuffers();
        }

        #endregion

        static void DrawTutorialCube()
        {
            GL.Begin(PrimitiveType.Quads);

            GL.Color3(1.0, 1.0, 0.0); GL.Vertex3(-10.0, 10.0, 10.0); GL.Vertex3(-10.0, 10.0, -10.0); GL.Vertex3(-10.0, -10.0, -10.0); GL.Vertex3(-10.0, -10.0, 10.0); GL.Color3(1.0, 0.0, 1.0); GL.Vertex3(10.0, 10.0, 10.0); GL.Vertex3(10.0, 10.0, -10.0); GL.Vertex3(10.0, -10.0, -10.0); GL.Vertex3(10.0, -10.0, 10.0); GL.Color3(0.0, 1.0, 1.0); GL.Vertex3(10.0, -10.0, 10.0); GL.Vertex3(10.0, -10.0, -10.0); GL.Vertex3(-10.0, -10.0, -10.0); GL.Vertex3(-10.0, -10.0, 10.0); GL.Color3(1.0, 0.0, 0.0); GL.Vertex3(10.0, 10.0, 10.0); GL.Vertex3(10.0, 10.0, -10.0); GL.Vertex3(-10.0, 10.0, -10.0); GL.Vertex3(-10.0, 10.0, 10.0); GL.Color3(0.0, 1.0, 0.0); GL.Vertex3(10.0, 10.0, -10.0); GL.Vertex3(10.0, -10.0, -10.0); GL.Vertex3(-10.0, -10.0, -10.0); GL.Vertex3(-10.0, 10.0, -10.0); GL.Color3(0.0, 0.0, 1.0); GL.Vertex3(10.0, 10.0, 10.0); GL.Vertex3(10.0, -10.0, 10.0); GL.Vertex3(-10.0, -10.0, 10.0); GL.Vertex3(-10.0, 10.0, 10.0);
        }

        static bool first = true;
        static void DrawMap()
        {
            GL.Begin(PrimitiveType.Polygon);

            for (int e = 0; e < map.entityList.Count; e++)
            {
                Poly poly = map.entityList[e].Polys;
                while (poly != null)
                {
                    if (first)
                    {
                        poly.SetRandomColor();
                        Console.WriteLine("== Poly == ");
                    }

                    GL.Color4(poly.Color);
                    for (int v = 0; v < poly.NumberOfVertices; v++)
                    {
                        Vertex vert = poly.verts[v];
                        GL.Vertex3(vert.p.AsDouble());

                        if (first)
                            Console.WriteLine(vert.p);
                    }

                    if (first)
                        Console.WriteLine("========== ");

                    poly = poly.Next;
                }
            }
            first = false;
        }

        static void LoadShader(string filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            string path = Path.Combine(Directory.GetCurrentDirectory(), filename);
            using (StreamReader sr = new StreamReader(path))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);

            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        static Vector2 lastMousePos = new Vector2();
        static void ResetCursor()
        {
            OpenTK.Input.Mouse.SetPosition(window.Bounds.Left + window.Bounds.Width / 2, window.Bounds.Top + window.Bounds.Height / 2);
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }

        static void MapLoad(string mapName)
        {
            // Seach for MAP file
            string mapPath = Path.Combine(Directory.GetCurrentDirectory(), mapName);
            if (File.Exists(mapPath))
            {
                // OK, parsing MAP...
                try
                {
                    using (StreamReader fStream = new StreamReader(mapPath))
                    {
                        string mapStr = fStream.ReadToEnd();
                        Tokenizer tokenizer = new Tokenizer(mapStr);

                        map = new MapFile(mapPath);
                        map.Load(tokenizer);
                    }

                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message + "\n");
                    Console.Write(ex.StackTrace);
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                Console.WriteLine("The file path could not be found.");
                Console.ReadLine();
                return;
            }
        }
    }
}
