using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace csg_NET
{
    class Program
    {
        const float FOV = 95f;

        static GameWindow window;
        static MapFile map;

        static Vector2 position = new Vector2(-45, -FOV);
        static float speed = 0.3f;

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

            // Seach for MAP file
            string mapPath = Path.Combine(Directory.GetCurrentDirectory(), args[0]);
            if (File.Exists(mapPath))
            {
                // OK, parsing MAP...
                try
                {
                    StreamReader fStream = new StreamReader(mapPath);
                    string mapStr = fStream.ReadToEnd();
                    Tokenizer tokenizer = new Tokenizer(mapStr);

                    map = new MapFile();
                    map.Load(tokenizer);

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


            window = new GameWindow(600, 600, GraphicsMode.Default, "MAP Visualizer", GameWindowFlags.FixedWindow);
            window.Load += Window_Load;
            window.RenderFrame += Window_RenderFrame;
            window.Resize += Window_Resize;
            window.KeyPress += Window_KeyPress;
            window.Run(1 / 60);
        }

        private static void Window_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'w')
                position.Y += speed;

            if (e.KeyChar == 's')
                position.Y -= speed;

            if (e.KeyChar == 'a')
                position.X += speed;

            if (e.KeyChar == 'd')
                position.X -= speed;
        }

        private static void Window_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, window.Width, window.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 matrix = Matrix4.Perspective(FOV, window.Width / window.Height, 1.0f, 1000f);
            GL.LoadMatrix(ref matrix);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        private static void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.LoadIdentity();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Translate(position.X, 0.0, position.Y);

            //            DrawTutorialCube();
            DrawMap();

            GL.End();

            window.SwapBuffers();
        }

        private static void Window_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Enable(EnableCap.DepthTest);
        }

        static void DrawTutorialCube()
        {
            GL.Begin(PrimitiveType.Quads);

            GL.Color3(1.0, 1.0, 0.0); GL.Vertex3(-10.0, 10.0, 10.0); GL.Vertex3(-10.0, 10.0, -10.0); GL.Vertex3(-10.0, -10.0, -10.0); GL.Vertex3(-10.0, -10.0, 10.0); GL.Color3(1.0, 0.0, 1.0); GL.Vertex3(10.0, 10.0, 10.0); GL.Vertex3(10.0, 10.0, -10.0); GL.Vertex3(10.0, -10.0, -10.0); GL.Vertex3(10.0, -10.0, 10.0); GL.Color3(0.0, 1.0, 1.0); GL.Vertex3(10.0, -10.0, 10.0); GL.Vertex3(10.0, -10.0, -10.0); GL.Vertex3(-10.0, -10.0, -10.0); GL.Vertex3(-10.0, -10.0, 10.0); GL.Color3(1.0, 0.0, 0.0); GL.Vertex3(10.0, 10.0, 10.0); GL.Vertex3(10.0, 10.0, -10.0); GL.Vertex3(-10.0, 10.0, -10.0); GL.Vertex3(-10.0, 10.0, 10.0); GL.Color3(0.0, 1.0, 0.0); GL.Vertex3(10.0, 10.0, -10.0); GL.Vertex3(10.0, -10.0, -10.0); GL.Vertex3(-10.0, -10.0, -10.0); GL.Vertex3(-10.0, 10.0, -10.0); GL.Color3(0.0, 0.0, 1.0); GL.Vertex3(10.0, 10.0, 10.0); GL.Vertex3(10.0, -10.0, 10.0); GL.Vertex3(-10.0, -10.0, 10.0); GL.Vertex3(-10.0, 10.0, 10.0);
        }

        static bool first = true;
        static void DrawMap()
        {
            GL.Begin(PrimitiveType.Polygon);

            Entity entity = map.entityList;
            while (entity != null)
            {
                Poly poly = entity.GetPolys;
                while (poly != null)
                {
                    if (first)
                        Console.WriteLine("== Poly == ");

                    GL.Color4(poly.Color);
                    for (int v = 0; v < poly.GetNumberOfVertices; v++)
                    {
                        Vertex vert = poly.verts[v];
                        GL.Vertex3(vert.p.AsDouble());

                        if (first)
                            Console.WriteLine(vert.p);
                    }

                    if (first)
                        Console.WriteLine("========== ");

                    poly = poly.GetNext;
                }
                entity = entity.GetNext;
            }
            first = false;
        }
    }
}
