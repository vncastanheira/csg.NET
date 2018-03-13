using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.ES20;

namespace csg_NET
{
    class Program
    {
        static GameWindow window;

        static void Main(string[] args)
        {
            // make sure parameters are valid
            if (args.Length != 3)
            {
                Console.WriteLine("csg [in] [out]");
                Console.WriteLine("in: MAP file");
                Console.WriteLine("out: CMF file");
                Console.ReadLine();
                return;
            }

            // Parse MAP file

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
