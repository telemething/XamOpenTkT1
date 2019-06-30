﻿#if !___XAM_FORMS___
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
#elif WINDOWS_UWP
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
    /// Base OpenGL View class. Adds a few convenience control methods
    /// </summary>
    ///
    /// ***********************************************************************

    public class TTOpenGLView
    {
        private readonly double _widthInPixels = 0;
        private readonly double _heightInPixels = 0;
        private readonly OpenGLView _oGlv;
        private readonly Frame _gestureOverlayFrame;

        private double _scale;
        public Vector3 _position = new Vector3(0.0f, 0.0f, 0.8f);
        public Matrix4 _model;

        private readonly TapGestureRecognizer _tapGestureRecognizer = 
            new TapGestureRecognizer();
        private readonly PinchGestureRecognizer _pinchGesture = 
            new PinchGestureRecognizer();
        private readonly PanGestureRecognizer _panGesture = 
            new PanGestureRecognizer();
        private readonly SwipeGestureRecognizer _swipeGesture =
            new SwipeGestureRecognizer();

        private ScaleRequestHandlerDelegate _scaleRequestHandler;

        private Action<Xamarin.Forms.Rectangle> _onDisplayHandler;

        public delegate void ScaleRequestHandlerDelegate(double scale);


        protected Stopwatch _stopwatch = new Stopwatch();

        // camera
        protected OpenGLDemo.Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;

        public double WidthInPixels => _widthInPixels;
        public double HeightInPixels => _heightInPixels;

        public OpenGLView View => _oGlv;

        /*public Action<Xamarin.Forms.Rectangle> OnDisplayHandler
        {
            set => _oGlv.OnDisplay = value;
            get => _oGlv.OnDisplay;
        }*/

        public Action<Xamarin.Forms.Rectangle> OnDisplayHandler
        {
            set => _onDisplayHandler = value;
            get => _onDisplayHandler;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// Control setters
        /// </summary>
        ///
        //*********************************************************************

        public double Scale
        {
            set { _scale = value; _scaleRequestHandler?.Invoke(value);  }
        }

        public ScaleRequestHandlerDelegate OnScaleRequest
        {
            set => _scaleRequestHandler  = value;
        }

        //*********************************************************************
        ////
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        ///
        //*********************************************************************

        private void OnDisplay(Xamarin.Forms.Rectangle obj)
        {
            _onDisplayHandler?.Invoke(obj);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// Gesture handler setters
        /// </summary>
        ///
        //*********************************************************************

        public EventHandler OnTapHandler
        {
            set
            {
                _tapGestureRecognizer.Tapped += value;
                _gestureOverlayFrame.GestureRecognizers.Add(_tapGestureRecognizer);
            }
        }

        public EventHandler<PinchGestureUpdatedEventArgs> OnPinchHandler
        {
            set
            {
                _pinchGesture.PinchUpdated += value;
                _gestureOverlayFrame.GestureRecognizers.Add(_pinchGesture);
            }
        }

        public EventHandler<PanUpdatedEventArgs> OnPanHandler
        {
            set
            {
                _panGesture.PanUpdated += value;
                _gestureOverlayFrame.GestureRecognizers.Add(_panGesture);
            }
        }

        public EventHandler<SwipedEventArgs> OnSwipeHandler
        {
            set
            {
                _swipeGesture.Swiped += value;
                _gestureOverlayFrame.GestureRecognizers.Add(_swipeGesture);
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="heightRequest"></param>
        /// <param name="widthRequest"></param>
        /// <param name="gestureOverlayFrame"></param>
        ///
        //*********************************************************************

        public TTOpenGLView(double heightRequest, double widthRequest, 
            Frame gestureOverlayFrame)
        {
            _widthInPixels = widthRequest;
            _heightInPixels = heightRequest;
            _gestureOverlayFrame = gestureOverlayFrame;

            _oGlv = new OpenGLView
            {
                HasRenderLoop = true,
                HeightRequest = heightRequest,
                WidthRequest = widthRequest,
                OnDisplay = OnDisplay
            };

            init();
        }

        //*********************************************************************
        //*
        //* Init
        //*
        //* Non openGL init stuff goes here
        //*
        //*********************************************************************

        private void init()
        {
            _stopwatch.Start();

            // We initialize the camera so that it is 3 units back from where the rectangle is
            // and give it the proper aspect ratio
            _camera = new OpenGLDemo.Camera(Vector3.UnitZ * (float)1.0, (float)(_widthInPixels / _heightInPixels));
            //_camera = new OpenGLDemo.Camera(Vector3.UnitZ * -30, (float)(WidthInPixels / HeightInPixels));

            OnTapHandler = OnTap;
            OnPinchHandler = OnPinch;
            OnPanHandler = OnPan;
            OnSwipeHandler = OnSwipe;
        }

        //*********************************************************************
        //
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        public enum RenderTypeEnum
        {
            run,
            stop,
            single
        };

        //*********************************************************************
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderType"></param>
        ///
        //*********************************************************************

        public void Render(RenderTypeEnum renderType)
        {
            switch (renderType)
            {
                case RenderTypeEnum.run:
                    _oGlv.HasRenderLoop = true;
                    break;
                case RenderTypeEnum.stop:
                    _oGlv.HasRenderLoop = false;
                    break;
                case RenderTypeEnum.single:
                    _oGlv.Display();
                    break;
            }
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
            double cameraSpeed = 0.001;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    break;
                case GestureStatus.Running:

                    // If pan
                    _camera.Position += _camera.Right * (float)(e.TotalX * cameraSpeed);
                    //_camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up

                    // If rotate
                    //_camera.Yaw += deltaX * sensitivity;
                    //_camera.Pitch -= deltaY * sensitivity; // reversed since y-coordinates range from bottom to top

                    break;
                case GestureStatus.Canceled:
                    break;
                case GestureStatus.Completed:
                    break;
            }

            // positive x: right, y: down
            // negative x: left, y: up

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

            // If zoom
            //_camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward 

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
    }

    //*************************************************************************
    ///
    /// <summary>
    /// Xamarin Application in shared (not Xamarin.Forms) code
    /// </summary>
    ///
    /// ***********************************************************************

    public class OpenTkT1App : Xamarin.Forms.Application
    {
        private OpenGLDemo.ControlSurface controlSurface;
        private OpenTkT1Page openTkT1Page;

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        ///
        //*********************************************************************

        public void TestDelegateMethod(string message)
        {
            openTkT1Page.openTkTutorialView.SetUpdateVertexData();
            System.Console.WriteLine(message);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pc"></param>
        ///
        //*********************************************************************

        public void GotNewPointcloudData(RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 pc)
        {
            openTkT1Page.openTkTutorialView.SetUpdateVertexData(pc);
            System.Console.WriteLine("Got new pointcloud data");
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        public OpenTkT1App()
        {
            MainPage = new OpenTkT1Page(controlSurface);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// The bait and switch constructor. This allows us to embed a page
        /// from shared code in a Xamarin.Forms.App. This is a trixy way to
        /// use code which does not exist as a .Net Standard and would
        /// otherwise clash with Xamarin.Forms, for example, OpenGL.
        ///
        /// 1) Accepts another page as an argument, substitute that page as
        /// this app's main page.
        /// 2) Accept a child page name, find the child page that matches that
        /// name, replaces it with this page.
        /// 3) Accept a ControlSurface as an argument. Set values in that
        /// ControlSurface to values from this class, to enable two way sharing
        /// of access to objects like data and delegates.
        /// 
        /// </summary>
        /// <param name="otherPage"></param>
        /// <param name="replacePageName"></param>
        /// <param name="cs"></param>
        ///
        /// *******************************************************************
        
        public OpenTkT1App(Page otherPage, string replacePageName, 
            OpenGLDemo.ControlSurface cs)
        {
            // use the given page as the main page for this app
            MainPage = otherPage;

            // set the control surface
            controlSurface = cs;

            // change a value in the control surface, read it elsewhere to
            // prove that it was changed
            cs.Hi = "Hello from OpenTkT1App";

            // Set a delegate in the control surface, call it from
            // elsewhere to prove that the callback works
            controlSurface.handler = TestDelegateMethod;

            controlSurface.GotNewPointcloudData = GotNewPointcloudData;

            var op = otherPage as TabbedPage;
            openTkT1Page = new OpenTkT1Page(controlSurface);

            // find the child page that matches that name, replace it with
            // this page
            for (int index = 0; index < op.Children.Count; index++)
            {
                if (op.Children[index].Title.Equals(replacePageName))
                    op.Children[index] = openTkT1Page;
            }
        }
    }

    //*************************************************************************
    ///
    /// <summary>
    /// The OpenGL content page in (not Xamarin.Forms) code 
    /// </summary>
    ///
    /// ***********************************************************************

    class OpenTkT1Page : ContentPage
    {
        private OpenTkTutorialView _openTkTutorialView;
        private Frame _gestureOverlayFrame;

        public OpenTkTutorialView openTkTutorialView
        {
            get => _openTkTutorialView;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlSurface"></param>
        ///
        //*********************************************************************

        public OpenTkT1Page(OpenGLDemo.ControlSurface controlSurface)
        {
            Title = "OpenGL";

            // This frame overlays the openGlView object, it receives gesture
            // events from the user. We need to do it this way because the
            // iOS OpenGlView does not forward any user input.
            _gestureOverlayFrame = new Frame
            {
                BorderColor = Xamarin.Forms.Color.Accent,
                BackgroundColor = Xamarin.Forms.Color.Transparent,
                Padding = new Thickness(5, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };

            // create the openGlView
            _openTkTutorialView = new OpenTkTutorialView(controlSurface, _gestureOverlayFrame);
            var openGlView = _openTkTutorialView.View;
            //_openTkTutorialView.gestureOverlayFrame = _gestureOverlayFrame;

            // We use the grid to overlay the frame and openGlView
            var grid = new Grid();

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            grid.Children.Add(openGlView, 0, 0);
            grid.Children.Add(_gestureOverlayFrame, 0, 0);

            // switch and button
            var toggle = new Xamarin.Forms.Switch { IsToggled = true };
            var button = new Button { Text = "Display" };
            var zoomSlider = new Slider();

            toggle.Toggled += (s, a) =>
            {
                if (toggle.IsToggled)
                    _openTkTutorialView.Render(TTOpenGLView.RenderTypeEnum.run);
                else
                    _openTkTutorialView.Render(TTOpenGLView.RenderTypeEnum.stop);
            };

            button.Clicked += (s, a) =>
            {
                _openTkTutorialView.Render(TTOpenGLView.RenderTypeEnum.single);
            };

            zoomSlider.ValueChanged += (sender, args) =>
            {
                _openTkTutorialView.Scale = args.NewValue;
            };

            // create the stack
            var stack = new StackLayout
            {
                Padding = new Xamarin.Forms.Size(20, 20),
                Children = { grid, toggle, button, zoomSlider }
            };

            Content = stack;
        }
    }

    //*************************************************************************
    ///
    /// <summary>
    /// OpenGL immediate mode view class, draws a triangle
    /// </summary>
    ///
    /// ***********************************************************************

    public class MyOpenGLView : TTOpenGLView
    {
        float red, green, blue;

        public MyOpenGLView() : base(300, 300, null)
        {
            base.OnDisplayHandler = OnDisplay;
        }

        private void OnDisplay(Xamarin.Forms.Rectangle obj)
        {
            GL.ClearColor(red, green, blue, 1.0f);
            GL.Clear((ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            red += 0.01f;
            if (red >= 1.0f)
                red -= 1.0f;
            green += 0.02f;
            if (green >= 1.0f)
                green -= 1.0f;
            blue += 0.03f;
            if (blue >= 1.0f)
                blue -= 1.0f;
        }

    }

    //*************************************************************************
    ///
    /// <summary>
    /// OpenGL shader view class. Demonstrates shaders and buffers and such
    /// things
    /// </summary>
    ///
    /// ***********************************************************************

    public class OpenTkTutorialView : TTOpenGLView
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
        };

        private float[] _instances =
        {
            0.0f, 0.0f, 0.0f, // 0
            0.3f, 0.3f, 0.3f, // 0
            0.6f, 0.6f, 0.6f // 0
        };*/

        private float[] _instances =
        {
            0.0f, 0.0f, 0.0f // 0
        };

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

        public OpenTkTutorialView(OpenGLDemo.ControlSurface cs, Frame gestureOverlayFrame) : 
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

        private void CreateTranslations()
        {

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
            /*int aOffsetLocation = _shader.GetAttribLocation("aOffset");
            GL.EnableVertexAttribArray(aOffsetLocation);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceBufferObject);
            // location, element count, type, normalized?, stride bytes, offset bytes
            GL.VertexAttribPointer(aOffsetLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 3 * sizeof(float));
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.VertexAttribDivisor(aOffsetLocation,1);*/

            GL.BindVertexArray(0);

            // We initialize the camera so that it is 3 units back from where the rectangle is
            // and give it the proper aspect ratio
            //_camera = new OpenGLDemo.Camera(Vector3.UnitZ * (float)-1.0, (float)(WidthInPixels / HeightInPixels));
            //_camera = new OpenGLDemo.Camera(Vector3.UnitZ * -30, (float)(WidthInPixels / HeightInPixels));

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
            //_shader.SetMatrix4("projection", base._camera.GetProjectionMatrix());

#if WINDOWS_UWP
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
#else
            GL.LineWidth((float)2.0);
            //GL.DrawArrays(BeginMode.Triangles, 0, 3); // Original
            //GL.DrawElements(BeginMode.Triangles, _indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero); // with indices
            //GL.DrawElements(BeginMode.TriangleStrip, _indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero); // strip with indices
            GL.DrawElementsInstanced(PrimitiveType.TriangleStrip, _indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero, 1); // with indices
            //GL.DrawElements(BeginMode.Points, _indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero); // with indices
            //GL.DrawArrays(BeginMode.Points, 0, 4);

            //GL.DrawElementsInstanced();
#endif
        }
    }


}
#endif // !___XAM_FORMS___