
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if !___XAM_FORMS___
using System;
using Xamarin.Forms;
#if __IOS__
using OpenTK;
using OpenTK.Graphics.ES30;
#elif WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenTK;
#elif __ANDROID__
using OpenTK;
using OpenTK.Graphics.ES30;
using TTOpenGl;

#endif

// OpenGLView Class https://docs.microsoft.com/en-us/dotnet/api/Xamarin.Forms.OpenGLView?view=xamarin-forms

namespace XamOpenTkT1
{
    //*************************************************************************
    ///
    /// <summary>
    /// Draws a point cloud
    /// </summary>
    ///
    /// ***********************************************************************

    public class DrawPointCloud : TTOglView, ITTRender
    {
        private OpenGLDemo.ControlSurface controlSurface;
        private RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 pointCloudData;
        private TTOpenGl.Shader _shader;

        private const Single defaultCubeEdgeLength = 0.1f;
        private const int defaultCubeInstanceCount = 3;
        private const Single defaultCubeInstanceOffset = 0.2f;

        private float[] _cubeVertices;
        private float[] _cubeInstances;
        private uint[] _cubeIndices =
        {
            0, 1, 2, 3, 7, 1, 5, 4, 7, 6, 2, 4, 0, 1
        };

        private int _cubeInstanceCount;

        private double _time;

        // Buffer handles
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _elementBufferObject;
        private int _instanceBufferObject;

        private bool glInitialized = false;
        private bool shadersBuilt = false;

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="gestureOverlayFrame"></param>
        ///
        //*********************************************************************

        public DrawPointCloud(OpenGLDemo.ControlSurface cs, 
            Xamarin.Forms.Frame gestureOverlayFrame) :
            base(300, 300, gestureOverlayFrame)
        {
            controlSurface = cs;
            _cubeVertices = CreateCubeVertices(defaultCubeEdgeLength);

            // Test
            _cubeInstanceCount = defaultCubeInstanceCount;
            _cubeInstances = CreateTestCubeInstances(
                _cubeInstanceCount, defaultCubeInstanceOffset);
            // Test

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

            color[2] = floor(f / 256.0 / 256.0 / 256.0);
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
        /// 
        /// </summary>
        /// <param name="edgeLength"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        private float[] CreateCubeVertices(float edgeLength)
        {
            var height = edgeLength;
            var width = edgeLength;
            var depth = edgeLength;

            return new float[]
            {
                -width, -height, depth,// 0
                width, -height, depth, // 1
                -width, height, depth, // 2
                width, height, depth, // 3
                -width, -height, -depth, // 4
                width, -height, -depth, // 5
                -width, height, -depth, // 6
                width, height, -depth // 7
            };
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        private float[] CreateTestCubeInstances(int count, float offset)
        {
            Single colorBlue = 4278190080.0f;
            Single colorGreen = 16711680.0f;
            Single colorRed = 65280.0f;

            Single[] colors = {colorBlue, colorGreen, colorRed};
            float[] instances = new float[count * 4];
            float current = 0.0f;

            for (int index = 0; index < count * 4; index++)
            {
                instances[index++] = current;
                instances[index++] = current;
                instances[index++] = current;
                instances[index] = colors[index % 3];
                current += offset;
            }

            return instances;
        }

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

                TTOpenGl.OGlUtil.CopyToBuffer<float>(
                    BufferTarget.ArrayBuffer, _instanceBufferObject,
                    (uint)_cubeInstances.Length, _cubeInstances, 0);

                /*TTOpenGl.OGlUtil.CopyToBuffer<byte>(
                    BufferTarget.ArrayBuffer, _instanceBufferObject,
                    (uint)1000, pointCloudData.data, 48);*/

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
                //MapCopyBuffers();
            }

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            // instance buffer
            GL.GenBuffers(1, out _instanceBufferObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceBufferObject);
#if WINDOWS_UWP
            GL.BufferData(BufferTarget.ArrayBuffer, _cubeInstances.Length * sizeof(float), _cubeInstances, BufferUsageHint.StaticDraw);
#else
            //GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_cubeInstances.Length * sizeof(float)), _cubeInstances, BufferUsage.StaticDraw);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(
                _cubeInstances.Length * sizeof(float)), IntPtr.Zero, BufferUsage.DynamicDraw);

            TTOpenGl.OGlUtil.CopyToBuffer<float>(
                BufferTarget.ArrayBuffer, _instanceBufferObject,
                (uint)_cubeInstances.Length, _cubeInstances, 0);

#endif
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // vertex buffer
            GL.GenBuffers(1, out _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
#if WINDOWS_UWP
            GL.BufferData(BufferTarget.ArrayBuffer, _cubeVertices.Length * sizeof(float), _cubeVertices, BufferUsageHint.StaticDraw);
#else
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(
                _cubeVertices.Length * sizeof(float)), _cubeVertices, BufferUsage.StaticDraw);
#endif
            // element buffer
            GL.GenBuffers(1, out _elementBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
#if WINDOWS_UWP
            GL.BufferData( BufferTarget.ElementArrayBuffer, _cubeIndices.Length * sizeof(float), _cubeIndices, BufferUsageHint.StaticDraw);
#else
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(
                _cubeIndices.Length * sizeof(uint)), _cubeIndices, BufferUsage.StaticDraw);
#endif

            // Enable the shader, this is global, so every function that uses a shader will modify this one until a new one is bound 
            _shader.Use();

            //*** TODO * Problem: If we map the buffer, then the GL.BufferSubData() call appears to have no effect
            //MapCopyBuffers();

            //VAO stores the layout subsequently created with VertexAttribPointer and EnableVertexAttribArray
            GL.GenVertexArrays(1, out _vertexArrayObject);
            GL.BindVertexArray(_vertexArrayObject);

            //*** position ***
            int aPositionLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(aPositionLocation);
            // location, element count, type, normalized?, stride bytes, offset bytes
            GL.VertexAttribPointer(aPositionLocation, 3, VertexAttribPointerType.Float, 
                false, 3 * sizeof(float), 0);

            // Bind the VBO and EBO again so that the VAO will bind them as well.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            //*** instance offset ***
            int aOffsetLocation = _shader.GetAttribLocation("aOffset");
            GL.EnableVertexAttribArray(aOffsetLocation);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceBufferObject);
            // location, element count, type, normalized?, stride bytes, offset bytes
            GL.VertexAttribPointer(aOffsetLocation, 3, VertexAttribPointerType.Float, 
                false, 4 * sizeof(float), IntPtr.Zero);
            GL.VertexAttribDivisor(aOffsetLocation, 1);

            //*** instance color ***
            int aColorLocation = _shader.GetAttribLocation("aColor");
            GL.EnableVertexAttribArray(aColorLocation);
            // location, element count, type, normalized?, stride bytes, offset bytes
            GL.VertexAttribPointer(aColorLocation, 1, VertexAttribPointerType.Float, 
                false, 4 * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribDivisor(aColorLocation, 1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

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
            try
            {
                // GL Initialization must occur on this thread. Make sure it is called only once
                if (!glInitialized)
                    InitGl();

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                DoUpdateVertexData();

                // Bind the shader
                _shader.Use();

                TTOpenGl.OGlUtil.CheckOGLError();

                // Bind the VAO
                GL.BindVertexArray(_vertexArrayObject);

                TTOpenGl.OGlUtil.CheckOGLError();

                _time = _stopwatch.ElapsedMilliseconds / 40;
                var model = Matrix4.Identity * Matrix4.CreateRotationX(
                                (float) MathHelper.DegreesToRadians(_time));
                _shader.SetMatrix4("model", model);
                _shader.SetMatrix4("view", base._camera.GetViewMatrix());
                _shader.SetMatrix4("projection", base._camera.GetProjectionMatrix());

                TTOpenGl.OGlUtil.CheckOGLError();

#if WINDOWS_UWP
                GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
#else
                GL.DrawElementsInstanced(PrimitiveType.TriangleStrip, 
                    _cubeIndices.Length, DrawElementsType.UnsignedInt,
                    IntPtr.Zero, _cubeInstanceCount); // with indices
#endif
                TTOpenGl.OGlUtil.CheckOGLError();
            }
            catch (Exception ex)
            {
                var ff = ex.Message;
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        ///
        //*********************************************************************

        void ITTRender.UpdateWindowSize(int width, int height)
        {
            System.Diagnostics.Debug.WriteLine("ITTRender.UpdateWindowSize()");
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        void ITTRender.Draw()
        {
            System.Diagnostics.Debug.WriteLine("ITTRender.Draw()");
        }
    }


}
#endif // !___XAM_FORMS___