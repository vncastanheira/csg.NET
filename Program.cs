using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics.ES20;

namespace csg_NET
{
    class Program
    {
        static GameWindow window;
        static MapFile map;

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


            window = new GameWindow(600, 600);
            window.Title = "MAP visualizer";
            window.Load += Window_Load;
            window.RenderFrame += Window_RenderFrame;
            window.Run(1 / 60);
        }

        private static void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            window.SwapBuffers();
        }

        private static void Window_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
        }
    }
}
