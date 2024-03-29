﻿using Button = Xamarin.Forms.Button;
using ColumnDefinition = Xamarin.Forms.ColumnDefinition;
using Frame = Xamarin.Forms.Frame;
using Grid = Xamarin.Forms.Grid;
using Page = Xamarin.Forms.Page;
using RowDefinition = Xamarin.Forms.RowDefinition;
using Slider = Xamarin.Forms.Slider;
#if !___XAM_FORMS___
using OpenGLDemo;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#if __IOS__
using UIKit;
using OpenTK;
using Foundation;
using CoreGraphics;
using OpenTK.Graphics.ES30;
using Xamarin.Forms.PlatformConfiguration;
#elif WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenTK;
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

// OpenGLView Class https://docs.microsoft.com/en-us/dotnet/api/Xamarin.Forms.OpenGLView?view=xamarin-forms

namespace XamOpenTkT1
{
    //*************************************************************************
    ///
    /// <summary>
    /// Draws a multicolored cube with three instances. Some unused code left
    /// in place as examples of how to do some things.
    /// </summary>
    ///
    /// ***********************************************************************
    
    public class DrawMulticoloredCubes : TTOglView, ITTRender
    {
        private OpenGLDemo.ControlSurface controlSurface;
        private RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 pointCloudData;

        // Create the vertices for our triangle. These are listed in normalized device coordinates (NDC)
        // In NDC, (0, 0) is the center of the screen.
        // Negative X coordinates move to the left, positive X move to the right.
        // Negative Y coordinates move to the bottom, positive Y move to the top.

        const Single colorBlue= 4278190080.0f;
        const Single colorGreen = 16711680.0f;
        const Single colorRed = 65280.0f;

        /* draws a square, must use 'BeginMode.Triangles' in 'GL.DrawElements' below
        private float[] _vertices =
        {
            // positions         // colors
            0.5f,  0.5f, 0.0f, colorRed,  // top right
            0.5f,  -0.5f, 0.0f, colorRed,  // bottom right
            -0.5f, -0.5f, 0.0f, colorBlue,  // bottom left
            -0.5f,  0.5f,  0.0f, colorGreen   // top left
        };

        private uint[] _indices =
        {
            0, 1, 3, // The first triangle will be the bottom-right half of the triangle
            1, 2, 3  // Then the second will be the top-right half of the triangle
        };
        */

        /* draws a cube, must use 'BeginMode.TriangleStrip' in 'GL.DrawElements' below */
        private const Single height = 0.1f;
        private const Single width = 0.1f;
        private const Single depth = 0.1f;

        private float[] _vertices =
        {
            -width, -height, depth, colorRed,// 0
            width, -height, depth, colorBlue,// 1
            -width, height, depth, colorGreen,// 2
            width, height, depth, colorRed,// 3
            -width, -height, -depth, colorBlue,// 4
            width, -height, -depth, colorGreen,// 5
            -width, height, -depth, colorRed,// 6
            width, height, -depth, colorBlue // 7
        };

        private uint[] _indices =
        {
            0, 1, 2, 3, 7, 1, 5, 4, 7, 6, 2, 4, 0, 1
        };

        /*private float[] _instances =
        {
            -0.3f, -0.3f, -0.3f, colorRed,// 0
            0.0f, 0.0f, 0.0f, colorGreen,// 0
            0.3f, 0.3f, 0.3f, colorBlue // 0
        };*/

        private float[] _instances =
        {
            0.0f, 0.0f, 0.0f, // 0
            0.3f, 0.3f, 0.3f, // 0
            0.6f, 0.6f, 0.6f // 0
        };

        /*private float[] _instances =
        {
            0.0f, 0.0f, 0.0f // 0
        };*/

        // transform intitialized to do nothing
        //private Matrix4 _transform = Matrix4.Identity;

        private double _time;

        // Buffer handles
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _elementBufferObject;
        private int _instanceBufferObject;

        private TTOpenGl.Shader _shader;

        private bool glInitialized = false;
        private bool shadersBuilt = false;

        private double _scale = 0.5;

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="gestureOverlayFrame"></param>
        ///
        //*********************************************************************

        public DrawMulticoloredCubes(OpenGLDemo.ControlSurface cs, Frame gestureOverlayFrame) : 
            base(300, 300, gestureOverlayFrame)
        {
            controlSurface = cs;
            OnLoad();
        }

        //*********************************************************************
        //*
        //* OnLoad
        //*
        //* Non openGL init stuff goes here
        //*
        //*********************************************************************

        private void OnLoad()
        {
            base.OnDisplayHandler = OnDisplay;

            //OnTapHandler = OnTap;
            //OnPinchHandler = OnPinch;
            //OnPanHandler = OnPan;
            //OnSwipeHandler = OnSwipe;

            OnScaleRequest = OnScaleRequestHandler;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scale"></param>
        ///
        //*********************************************************************

        private void OnScaleRequestHandler(double scale)
        {
            System.Diagnostics.Debug.WriteLine($"> OnScaleRequest({scale})");

            _scale = scale + 0.5;

            //_transform = BuildTransform();
        }

        //*********************************************************************
        ///
        /// <summary>
        /// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/gestures/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        private void OnSwipe(object sender, SwipedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"> OnSwipe({e.Direction})");
        }

        //*********************************************************************
        ///
        /// <summary>
        /// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/gestures/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        private void OnPan(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    break;
            }

            System.Diagnostics.Debug.WriteLine(
                $"> OnPan({e.StatusType.ToString()}, x:{e.TotalX}, y:{e.TotalY})");
        }

        //*********************************************************************
        ///
        /// <summary>
        /// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/gestures/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        private void OnPinch(object sender, PinchGestureUpdatedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                $"> OnPinch({e.Status.ToString()}, x:{e.ScaleOrigin.X}, y:{e.ScaleOrigin.Y}, scale:{e.Scale})");
        }

        //*********************************************************************
        ///
        /// <summary>
        /// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/gestures/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        //*********************************************************************

        private void OnTap(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("> OnTap()");
        }


#region Shader Test Region
        private float[] ExtractRgbFromPacky(float inColor)
        {
            uint ucolor = (uint)inColor;
            float[] color = new float[3];

            color[0] = (0xff & (ucolor >> 24)) / 255f;
            color[1] = (0xff & (ucolor >> 16)) / 255f;
            color[2] = (0xff & (ucolor >> 8)) / 255f;

            return color;
        }

        private int floor(double inVal)
        {
            return (int)inVal;
        }

        private float[] ExtractRgbFromPack(float f)
        {
            float[] color = new float[3];

            color[2] = floor(f / 256.0 / 256.0 / 256.0 );
            color[1] = floor((f - color[2] * 256.0 * 256.0 * 256.0) / 256.0 / 256);
            color[0] = floor((f - color[2] * 256.0 * 256.0 * 256.0 - color[1] * 256.0 * 256.0) / 256);
            // now we have a vec3 with the 3 components in range [0..255]. Let's normalize it!
            //return color / 255.0;
            return color;
        }
#endregion

        private bool haveNewTestCubeVertexData = false;
        private bool haveNewVertexData = false;

        //*********************************************************************
        ///
        /// <summary>
        /// Updating buffer data can only take place in the OpenGL thread, so is a
        /// two step process. 1) we signal the update here, 2) we execute the update
        /// by calling DoUpdateVertexData() from OnDisplay()
        /// </summary>
        ///
        //*********************************************************************
        public void SetUpdateVertexData()
        {
            haveNewTestCubeVertexData = true;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pc"></param>
        ///
        //*********************************************************************

        public void SetUpdateVertexData(RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 pc)
        {
            pointCloudData = pc;
            haveNewVertexData = true;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// glBufferData : https://www.khronos.org/opengl/wiki/GLAPI/glBufferData
        /// Buffer Object Streaming : https://www.khronos.org/opengl/wiki/Buffer_Object_Streaming
        /// thread on streaming : https://community.khronos.org/t/vbos-strangely-slow/60109/32
        /// </summary>
        ///
        //*********************************************************************

        public void DoUpdateVertexData()
        {
            if (haveNewVertexData)
            {
                haveNewVertexData = false;

                // do we need a new BindBuffer operation?

                // do we need a new index array too?

                // will this work somehow?
                // GL.BufferSubData<float>(BufferTarget.ArrayBuffer, (IntPtr)(3 * floatSize), (IntPtr)floatSize, ref d1);

                //*** Set new buffer data ****
#if WINDOWS_UWP
                GL.BufferData( BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
#else
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_vertices.Length * sizeof(float)), _vertices,
                    BufferUsage.StaticDraw);

                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(_indices.Length * sizeof(uint)), _indices,
                    BufferUsage.StaticDraw);
#endif

                return;
            }

            if (haveNewTestCubeVertexData)
                DoUpdateTestCubeVertexData();
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        public void DoUpdateTestCubeVertexData()
        {
            haveNewTestCubeVertexData = false;

            var floatSize = sizeof(float);
            float d1 = 65280.0f;

             GL.BufferSubData<float>(BufferTarget.ArrayBuffer, (IntPtr)(3 * floatSize), (IntPtr)floatSize, ref d1);
             GL.BufferSubData<float>(BufferTarget.ArrayBuffer, (IntPtr)(7 * floatSize), (IntPtr)floatSize, ref d1);
             GL.BufferSubData<float>(BufferTarget.ArrayBuffer, (IntPtr)(11 * floatSize), (IntPtr)floatSize, ref d1);
        }

        //*********************************************************************
        ///
        /// <summary>
        ///
        /// allows fast transfer in GPU with:
        /// GL.CopyBufferSubData(BufferTarget.CopyWriteBuffer, BufferTarget.ArrayBuffer,
        ///     IntPtr.Zero, IntPtr.Zero, (IntPtr)32);
        /// </summary>
        ///
        /// GLAPI / glMapBufferRange : https://www.khronos.org/opengl/wiki/GLAPI/glMapBufferRange
        /// Buffer Object : https://www.khronos.org/opengl/wiki/Buffer_Object#Data_Specification
        /// https://www.khronos.org/registry/OpenGL/extensions/ARB/ARB_copy_buffer.txt
        /// https://learnopengl.com/Advanced-OpenGL/Advanced-Data
        ///
        /// MapReadBit = 1,
        /// MapWriteBit = 2,
        /// MapInvalidateRangeBit = 4,
        /// MapInvalidateBufferBit = 8,
        /// MapFlushExplicitBit = 16,
        /// MapUnsynchronizedBit = 32
        ///
        //***********************************************************************************

        private void MapCopyBuffersy()
        {
            //IntPtr bufferPointer = GL.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)64, BufferAccessMask.MapWriteBit);
            //IntPtr elementPointer = GL.MapBufferRange(BufferTarget.ElementArrayBuffer, IntPtr.Zero, (IntPtr)64, BufferAccessMask.MapWriteBit);

            TTOpenGl.OGlUtil.ClearOGLErrors();

            float[] testData1 =
            {
                // positions         // colors
                0.5f,  0.5f, 0.0f, 4278190080.0f,  // bottom right
                0.5f,  -0.5f, 0.0f, 4278190080.0f,  // bottom right
                -0.5f, -0.5f, 0.0f, 16711680.0f,  // bottom left
                -0.5f,  0.5f,  0.0f, 65280.0f   // top 
            };

            uint[] testData2 =
            {
                0, 1, 3, // The first triangle will be the bottom-right half of the triangle
                1, 2, 3  // Then the second will be the top-right half of the triangle
            };


            int _copyReadBuffer;
            int _copyWriteBuffer;

            // ReadBuffer
            GL.GenBuffers(1, out _copyReadBuffer);

            TTOpenGl.OGlUtil.CheckOGLError();

            GL.BindBuffer(BufferTarget.CopyReadBuffer, _copyReadBuffer);

            TTOpenGl.OGlUtil.CheckOGLError();

#if WINDOWS_UWP
            GL.BufferData(BufferTarget.CopyReadBuffer, (IntPtr)32,  (IntPtr)null, BufferUsageHint.StaticDraw);
#else
            GL.BufferData(BufferTarget.CopyReadBuffer, (IntPtr)(testData1.Length * sizeof(float)), testData1, BufferUsage.StaticDraw);
#endif
            TTOpenGl.OGlUtil.CheckOGLError();

            controlSurface.CopyReadBufferPointer = GL.MapBufferRange(BufferTarget.CopyReadBuffer,
                IntPtr.Zero, (IntPtr)(testData1.Length * sizeof(float)), BufferAccessMask.MapUnsynchronizedBit);

            TTOpenGl.OGlUtil.CheckOGLError();

            //WriteBuffer
            GL.GenBuffers(1, out _copyWriteBuffer);
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, _copyWriteBuffer);
#if WINDOWS_UWP
            GL.BufferData(BufferTarget.CopyWriteBuffer, (IntPtr)32, (IntPtr)null, BufferUsageHint.StaticDraw);
#else
            GL.BufferData(BufferTarget.CopyWriteBuffer, (IntPtr)(testData2.Length * sizeof(uint)), testData2, BufferUsage.StaticDraw);
#endif
            controlSurface.CopyWriteBufferPointer = GL.MapBufferRange(BufferTarget.CopyWriteBuffer,
                IntPtr.Zero, (IntPtr)(testData2.Length * sizeof(uint)), BufferAccessMask.MapUnsynchronizedBit);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private void MapCopyBuffers()
        {
            //*** TODO * Look into this
            // https://www.khronos.org/opengl/wiki/Pixel_Buffer_Object
            //GL.MapBufferRange(BufferTarget.PixelPackBuffer, IntPtr.Zero, (IntPtr) (_vertices.Length * sizeof(float)),
            //    BufferAccessMask.MapWriteBit);

            TTOpenGl.OGlUtil.ClearOGLErrors();
            controlSurface.ArrayBufferPointer = GL.MapBufferRange(BufferTarget.ArrayBuffer, 
                IntPtr.Zero, (IntPtr)(_vertices.Length * sizeof(float)), BufferAccessMask.MapWriteBit);
            TTOpenGl.OGlUtil.CheckOGLError();
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private void BuildShaders()
        {
            try
            {
                _shader = new TTOpenGl.Shader("shader.vert", "shader.frag");
                shadersBuilt = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        private Matrix4 BuildTransformy()
        {
            //var transform = Matrix4.Identity;

            TTOpenGl.OGlUtil.ClearOGLErrors();

            var rotation1 = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(20.0f));
            var rotation2 = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(20.0f));
            var rotation = rotation1 * rotation2;

#if WINDOWS_UWP
            var scale = Matrix4.CreateScale(0.5f, 0.5f, 0.5f);
#else
            //var scale = Matrix4.Scale(0.5f, 0.5f, 0.5f);
            var scale = Matrix4.Scale((float)_scale, (float)_scale, (float)_scale);
#endif

            var translation = Matrix4.CreateTranslation(0.1f, 0.1f, 0.0f);
            Matrix4 transform = rotation * scale * translation;

            TTOpenGl.OGlUtil.CheckOGLError();

            return transform;
        }

        private void SetupProjection()
        {
            TTOpenGl.OGlUtil.ClearOGLErrors();

            float aspect = (float)(WidthInPixels / HeightInPixels);
            if (aspect > 1)
            {
#if WINDOWS_UWP
                Matrix4 scale = Matrix4.CreateScale(aspect);
#else
                Matrix4 scale = Matrix4.Scale(aspect);
#endif
                base._model = Matrix4.Mult(base._model, scale);
            }
        }

        ///*********************************************************************
        ///
        /// <summary>
        /// Call this method from within the 'OnDisplay()' method.
        /// https://stackoverflow.com/questions/17399087/glcreateshader-and-glcreateprogram-fail-on-android
        /// Shader calls should be within a GL thread that is onSurfaceChanged(),
        /// onSurfaceCreated() or onDrawFrame()
        /// Vertex Specification : https://www.khronos.org/opengl/wiki/Vertex_Specification
        /// </summary>
        ///
        ///*********************************************************************

        private void InitGl()
        {
            if (!shadersBuilt)
            {
                BuildShaders();
                SetupProjection();
                //_transform = BuildTransform();
                //MapCopyBuffers();
            }

            //controlSurface

            //var color = ExtractRgbFromPack(_vertices[3]);
            //color = ExtractRgbFromPack(_vertices[9]);
            //color = ExtractRgbFromPack(_vertices[15]);

            // https://github.com/xamarin/xamarin-macios/blob/master/src/OpenGL/OpenTK/Graphics/OpenGL/GLEnums.cs
            //ProgramPointSize = ((int)0x8642)
            //GL.Enable((EnableCap)((int)0x8642));
            //GL.Enable(EnableCap.ProgramPointSize);
            //GL.ProgramParameter(1,ProgramParameterName., 0);

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            // instance buffer
            GL.GenBuffers(1, out _instanceBufferObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceBufferObject);
#if WINDOWS_UWP
            GL.BufferData(BufferTarget.ArrayBuffer, _instances.Length * sizeof(float), _instances, BufferUsageHint.StaticDraw);
#else
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_instances.Length * sizeof(float)), _instances, BufferUsage.StaticDraw);
#endif
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // vertex buffer
            GL.GenBuffers(1, out _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
#if WINDOWS_UWP
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
#else
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_vertices.Length * sizeof(float)), _vertices, BufferUsage.StaticDraw);
#endif
            // element buffer
            GL.GenBuffers(1, out _elementBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
#if WINDOWS_UWP
            GL.BufferData( BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(float), _indices, BufferUsageHint.StaticDraw);
#else
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr) (_indices.Length * sizeof(uint)), _indices, BufferUsage.StaticDraw);
#endif

            // Enable the shader, this is global, so every function that uses a shader will modify this one until a new one is bound 
            _shader.Use();

            //*** TODO * Problem: If we map the buffer, then the GL.BufferSubData() call appears to have no effect
            MapCopyBuffers();

            //VAO stores the layout subsequently created with VertexAttribPointer and EnableVertexAttribArray
            GL.GenVertexArrays(1, out _vertexArrayObject);
            GL.BindVertexArray(_vertexArrayObject);

            //*** position ***
            int aPositionLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(aPositionLocation);
            // location, element count, type, normalized?, stride bytes, offset bytes
            GL.VertexAttribPointer(aPositionLocation, 3, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            //*** color ***
            int aColorLocation = _shader.GetAttribLocation("aColor");
            GL.EnableVertexAttribArray(aColorLocation);
            // location, element count, type, normalized?, stride bytes, offset bytes
            GL.VertexAttribPointer(aColorLocation, 1, VertexAttribPointerType.Float, false, 4 * sizeof(float), 3 * sizeof(float));

            // Bind the VBO and EBO again so that the VAO will bind them as well.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            //*** instance offset ***
            int aOffsetLocation = _shader.GetAttribLocation("aOffset");
            GL.EnableVertexAttribArray(aOffsetLocation);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceBufferObject);
            // location, element count, type, normalized?, stride bytes, offset bytes
            GL.VertexAttribPointer(aOffsetLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 3 * sizeof(float));
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.VertexAttribDivisor(aOffsetLocation,1);

            GL.BindVertexArray(0);

            // Mark GL as  initialized
            glInitialized = true;
        }

        //*********************************************************************
        //*
        //* OnDisplay
        //*
        //* invoked from the OpenGl subsystem when it is ready to draw, once
        //* per frame.
        //*
        //*********************************************************************

        private void OnDisplay(Xamarin.Forms.Rectangle obj)
        {
            // GL Initialization must occur on this thread. Make sure it is called only once
            if (!glInitialized)
                InitGl();

            DoUpdateVertexData();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Bind the shader
            _shader.Use();

            // Bind the VAO
            GL.BindVertexArray(_vertexArrayObject);

            // Transform
            //_shader.SetMatrix4("transform", _transform);

            _time = _stopwatch.ElapsedMilliseconds / 40;
            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", base._camera.GetViewMatrix());
            _shader.SetMatrix4("projection", base._camera.GetProjectionMatrix());

#if WINDOWS_UWP
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
#else
            GL.LineWidth((float)2.0);
            //GL.DrawArrays(BeginMode.Triangles, 0, 3); // Original
            //GL.DrawElements(BeginMode.Triangles, _indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero); // with indices
            //GL.DrawElements(BeginMode.TriangleStrip, _indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero); // strip with indices
            GL.DrawElementsInstanced(PrimitiveType.TriangleStrip, _indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero, 3); // with indices
            //GL.DrawElements(BeginMode.Points, _indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero); // with indices
            //GL.DrawArrays(BeginMode.Points, 0, 4);

            //GL.DrawElementsInstanced();
#endif
        }

        void ITTRender.UpdateWindowSize(int width, int height)
        {
            System.Diagnostics.Debug.WriteLine("ITTRender.UpdateWindowSize()");
        }

        void ITTRender.Draw()
        {
            System.Diagnostics.Debug.WriteLine("ITTRender.Draw()");
        }
    }


}
#endif // !___XAM_FORMS___