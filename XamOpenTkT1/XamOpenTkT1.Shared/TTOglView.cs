using Button = Xamarin.Forms.Button;
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
    /// Base OpenGL View class. Adds a few convenience control methods
    /// </summary>
    ///
    /// ***********************************************************************

    public class TTOglView
    {
        private readonly double _widthInPixels = 0;
        private readonly double _heightInPixels = 0;
        private readonly Frame _gestureOverlayFrame;
#if WINDOWS_UWP
        private Windows.UI.Xaml.Controls.SwapChainPanel _swapChainPanel;
#else
        private readonly OpenGLView _oGlv;
#endif
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
        //private bool _firstMove = true;
        //private Vector2 _lastPos;

        public float CameraPitch
        {
            get { return _camera.Pitch; }
        }

        public float CameraYaw
        {
            get { return _camera.Yaw; }
        }

        public double WidthInPixels => _widthInPixels;
        public double HeightInPixels => _heightInPixels;

#if WINDOWS_UWP
        public Windows.UI.Xaml.Controls.SwapChainPanel View => _swapChainPanel;
#else
        public OpenGLView View => _oGlv;
#endif

        /*public Action<Xamarin.Forms.Rectangle> OnDisplayHandler
        {
            set => _oGlv.OnDisplay = value;
            get => _oGlv.OnDisplay;
        }*/

        public enum CameraMoveType
        {
            X, Y, Z, Roll, Pitch, Yaw
        }

        public void MoveCamera(CameraMoveType moveType, double amount)
        {
            switch (moveType)
            {
                case CameraMoveType.X:
                    _camera.Position += Vector3.UnitX * (float)amount;
                    break;
                case CameraMoveType.Y:
                    _camera.Position += Vector3.UnitY * (float)amount;
                    break;
                case CameraMoveType.Z:
                    _camera.Position += Vector3.UnitZ * (float)amount;
                    break;
                case CameraMoveType.Roll:
                    break;
                case CameraMoveType.Pitch:
                    _camera.Pitch = (float)amount;
                    break;
                case CameraMoveType.Yaw:
                    _camera.Yaw = (float)amount;
                    break;
            }
        }

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
            set { _scale = value; _scaleRequestHandler?.Invoke(value); }
        }

        public ScaleRequestHandlerDelegate OnScaleRequest
        {
            set => _scaleRequestHandler = value;
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

        public TTOglView(double heightRequest, double widthRequest,
            Frame gestureOverlayFrame)
        {
            _widthInPixels = widthRequest;
            _heightInPixels = heightRequest;
            _gestureOverlayFrame = gestureOverlayFrame;

#if WINDOWS_UWP
            _swapChainPanel = new SwapChainPanel(); //TODO: we need init values
#else
            _oGlv = new OpenGLView
            {
                HasRenderLoop = true,
                HeightRequest = heightRequest,
                WidthRequest = widthRequest,
                OnDisplay = OnDisplay
            };
#endif

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
#if WINDOWS_UWP
            //TODO: what do we do here?
#else
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
#endif

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
}

#endif // !___XAM_FORMS___