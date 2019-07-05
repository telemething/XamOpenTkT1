
#if !___XAM_FORMS___
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Buffers;
#if __IOS__
using UIKit;
using OpenTK;
using Foundation;
using CoreGraphics;
using OpenTK.Graphics.ES30;
#elif WINDOWS_UWP
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
#elif __ANDROID__
using Android.Icu.Text;
using Android.Animation;
using OpenTK;
using Android.Util;
using Android.App;
using Android.Opengl;
using Android.Graphics;
using Android;
using OpenTK.Graphics.ES30;
#endif

namespace OpenGLDemo
{
    public class Camera
    {
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        private float _pitch;
        private float _yaw = -MathHelper.PiOver2; 
        private float _fov = MathHelper.PiOver2;

        private float _zNear = .01f;
        private float _zFar = 100.0f;

        public Vector3 Position { get; set; }
        public float AspectRatio { private get; set; }

        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                // Clamp to prevent gimbal lock
                var angle = Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // Vertical FOV
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = Clamp(value, 1f, 45f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="aspectRatio"></param>
        ///
        //*********************************************************************

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
            UpdateVectors();
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="fov"></param>
        /// <param name="zNear"></param>
        /// <param name="zFar"></param>
        ///
        //*********************************************************************

        public Camera(Vector3 position, float aspectRatio, float fov, float zNear, float zFar)
        {
            Position = position;
            AspectRatio = aspectRatio;

            Fov = fov;
            _zNear = zNear;
            _zFar = zFar;

            UpdateVectors();
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ///
        //*********************************************************************

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ///
        //*********************************************************************

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, _zNear, _zFar);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private void UpdateVectors()
        {
            _front.X = (float)Math.Cos(_pitch) * (float)Math.Cos(_yaw);
            _front.Y = (float)Math.Sin(_pitch);
            _front.Z = (float)Math.Cos(_pitch) * (float)Math.Sin(_yaw);

            _front = Vector3.Normalize(_front);

            // Calculate both the right and the up vector using cross product
            // Note that we are calculating the right from the global up, this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        private static float Clamp(float n, float min, float max)
        {
            return Math.Max(Math.Min(n, max), min);
        }
    }
}

#endif //!___XAM_FORMS___


