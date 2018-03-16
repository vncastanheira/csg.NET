using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace csg_NET
{
    public class Camera
    {
        public OpenTK.Vector3 Position = new OpenTK.Vector3();
        public OpenTK.Vector3 Orientation = new OpenTK.Vector3((float)Math.PI, 0f, 0f);
        public float MoveSpeed = 0.2f;
        public float MouseSensitivity = 0.01f;

        public Camera()
        {

        }

        public Camera(float x, float y, float z)
        {
            Position = new OpenTK.Vector3(x, y, y);
        }

        public Matrix4 GetViewMatrix()
        {
            OpenTK.Vector3 lookat = new OpenTK.Vector3();

            lookat.X = (float)(Math.Sin((float)Orientation.X) * Math.Cos((float)Orientation.Y));
            lookat.Y = (float)Math.Sin((float)Orientation.Y);
            lookat.Z = (float)(Math.Cos((float)Orientation.X) * Math.Cos((float)Orientation.Y));

            return Matrix4.LookAt(Position, Position + lookat, OpenTK.Vector3.UnitY);
        }

        public void Move(float x, float y, float z)
        {
            OpenTK.Vector3 offset = new OpenTK.Vector3();

            OpenTK.Vector3 forward = new OpenTK.Vector3((float)Math.Sin((float)Orientation.X), 0, (float)Math.Cos((float)Orientation.X));
            OpenTK.Vector3 right = new OpenTK.Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = OpenTK.Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        public void AddRotation(float x, float y)
        {
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
        }
    }
}
