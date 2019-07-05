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
    /// ***********************************************************************
    ///
    /// <summary>
    /// The OpenGL content page in (not Xamarin.Forms) code 
    /// </summary>
    ///
    /// ***********************************************************************

    class TTOglPage : ContentPage
    {
        private DrawMulticoloredCubes _renderer;
        private Frame _gestureOverlayFrame;

#if WINDOWS_UWP
        readonly Windows.UI.Xaml.Controls.SwapChainPanel _swapChainPanel;
        Xamarin.Forms.View _swapChainView;
        private GLUWP.OpenGLES mOpenGLES;
        private GLUWP.EGLSurface mRenderSurface; // This surface is associated with a swapChainPanel on the page
        private object mRenderSurfaceCriticalSection = new object();
        private Windows.Foundation.IAsyncAction mRenderLoopWorker;
#endif

        public DrawMulticoloredCubes Renderer
        {
            get => _renderer;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlSurface"></param>
        ///
        //*********************************************************************

        public TTOglPage(OpenGLDemo.ControlSurface controlSurface)
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
            _renderer = new DrawMulticoloredCubes(controlSurface, _gestureOverlayFrame);
            var openGlView = _renderer.View;
            //_renderer.gestureOverlayFrame = _gestureOverlayFrame;

            // We use the grid to overlay the frame and openGlView
            var gridV = new Grid();

            gridV.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            gridV.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

#if WINDOWS_UWP
            _swapChainPanel = DrawMulticoloredCubes.View;
            _swapChainView = openGlView.ToView();
            gridV.Children.Add(_swapChainView, 0, 0); //TODO: Does this work?

            mOpenGLES = new GLUWP.OpenGLES(); //TODO: can we create this here? example shows created from App.
            mRenderSurface = GLUWP.EGL.NO_SURFACE;

            Windows.UI.Core.CoreWindow window = Windows.UI.Xaml.Window.Current.CoreWindow;

            window.VisibilityChanged += 
                new Windows.Foundation.TypedEventHandler<
                    Windows.UI.Core.CoreWindow, 
                    Windows.UI.Core.VisibilityChangedEventArgs>((win, args) => OnVisibilityChanged(win, args));

            //Loaded += (sender, args) => OnPageLoaded(sender, args);
            Windows.UI.Xaml.Window.Current.Activated += (sender, args) => OnPageLoaded(sender, args); //TODO : does this work?
#else
            gridV.Children.Add(openGlView, 0, 0);
#endif
            gridV.Children.Add(_gestureOverlayFrame, 0, 0);

            // switch and button
            var toggle = new Xamarin.Forms.Switch { IsToggled = true };
            var button = new Button { Text = "Display" };

            var zoomSlider = new Slider();
            var rollSlider = new Slider();
            var pitchSlider = new Slider() { Minimum = -90.0, Maximum = 90, Value = _renderer.CameraPitch };
            var yawSlider = new Slider() { Minimum = -189.0, Maximum = 180, Value = _renderer.CameraYaw };

            var forwardButton = new Button() { Text = "F" };
            var backButton = new Button() { Text = "B" };
            var leftButton = new Button() { Text = "L" };
            var rightButton = new Button() { Text = "R" };
            var upButton = new Button() { Text = "U" };
            var downButton = new Button() { Text = "D" };

            forwardButton.Clicked += (sender, args) => { _renderer.MoveCamera(TTOglView.CameraMoveType.Z, -0.5); };
            backButton.Clicked += (sender, args) => { _renderer.MoveCamera(TTOglView.CameraMoveType.Z, 0.5); };
            leftButton.Clicked += (sender, args) => { _renderer.MoveCamera(TTOglView.CameraMoveType.X, -0.5); };
            rightButton.Clicked += (sender, args) => { _renderer.MoveCamera(TTOglView.CameraMoveType.X, 0.5); };
            upButton.Clicked += (sender, args) => { _renderer.MoveCamera(TTOglView.CameraMoveType.Y, 0.5); };
            downButton.Clicked += (sender, args) => { _renderer.MoveCamera(TTOglView.CameraMoveType.Y, -0.5); };
            pitchSlider.ValueChanged += (sender, args) => { _renderer.MoveCamera(TTOglView.CameraMoveType.Pitch, args.NewValue); };
            yawSlider.ValueChanged += (sender, args) => { _renderer.MoveCamera(TTOglView.CameraMoveType.Yaw, args.NewValue); };

            var stackC = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                Children = { leftButton, forwardButton, upButton, downButton, backButton, rightButton }
            };

            toggle.Toggled += (s, a) =>
            {
                if (toggle.IsToggled)
                    _renderer.Render(TTOglView.RenderTypeEnum.run);
                else
                    _renderer.Render(TTOglView.RenderTypeEnum.stop);
            };

            button.Clicked += (s, a) =>
            {
                _renderer.Render(TTOglView.RenderTypeEnum.single);
            };

            zoomSlider.ValueChanged += (sender, args) =>
            {
                _renderer.Scale = args.NewValue;
            };

            // create the stack
            var stackM = new StackLayout
            {
                Padding = new Xamarin.Forms.Size(20, 20),
                Children = { gridV, toggle, button, zoomSlider, rollSlider, pitchSlider, yawSlider, stackC }
            };

            Content = stackM;
        }

#if WINDOWS_UWP

        private void OnPageLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // The SwapChainPanel has been created and arranged in the page layout, so EGL can be initialized.
            CreateRenderSurface();
            StartRenderLoop();
        }

        private void OnPageLoaded(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            // The SwapChainPanel has been created and arranged in the page layout, so EGL can be initialized.
            CreateRenderSurface();
            StartRenderLoop();
        }

        private void OnVisibilityChanged(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.VisibilityChangedEventArgs args)
        {
            if (args.Visible && mRenderSurface != GLUWP.EGL.NO_SURFACE)
            {
                StartRenderLoop();
            }
            else
            {
                StopRenderLoop();
            }
        }

        private void CreateRenderSurface()
        {
            if (mOpenGLES != null && mRenderSurface == GLUWP.EGL.NO_SURFACE)
            {
                // The app can configure the the SwapChainPanel which may boost performance.
                // By default, this template uses the default configuration.
                mRenderSurface = mOpenGLES.CreateSurface(_swapChainPanel, null, null);

                // You can configure the SwapChainPanel to render at a lower resolution and be scaled up to
                // the swapchain panel size. This scaling is often free on mobile hardware.
                //
                // One way to configure the SwapChainPanel is to specify precisely which resolution it should render at.
                // Size customRenderSurfaceSize = Size(800, 600);
                // mRenderSurface = mOpenGLES->CreateSurface(swapChainPanel, &customRenderSurfaceSize, nullptr);
                //
                // Another way is to tell the SwapChainPanel to render at a certain scale factor compared to its size.
                // e.g. if the SwapChainPanel is 1920x1280 then setting a factor of 0.5f will make the app render at 960x640
                // float customResolutionScale = 0.5f;
                // mRenderSurface = mOpenGLES->CreateSurface(swapChainPanel, nullptr, &customResolutionScale);
                // 
            }
        }

        private void DestroyRenderSurface()
        {
            if (mOpenGLES != null)
            {
                mOpenGLES.DestroySurface(mRenderSurface);
            }

            mRenderSurface = GLUWP.EGL.NO_SURFACE;
        }

        void RecoverFromLostDevice()
        {
            // Stop the render loop, reset OpenGLES, recreate the render surface
            // and start the render loop again to recover from a lost device.

            StopRenderLoop();

            {
                lock (mRenderSurfaceCriticalSection)
                {
                    DestroyRenderSurface();
                    mOpenGLES.Reset();
                    CreateRenderSurface();
                }
            }

            StartRenderLoop();
        }

        void StartRenderLoop()
        {
            // If the render loop is already running then do not start another thread.
            if (mRenderLoopWorker != null && mRenderLoopWorker.Status == Windows.Foundation.AsyncStatus.Started)
            {
                return;
            }

            // Create a task for rendering that will be run on a background thread.
            var workItemHandler =
                new Windows.System.Threading.WorkItemHandler(action =>
                {
                    lock (mRenderSurfaceCriticalSection)
                    {
                        mOpenGLES.MakeCurrent(mRenderSurface);
                        
                        //SimpleRenderer renderer = new SimpleRenderer();
                        ITTRender renderer = _renderer;

                        while (action.Status == Windows.Foundation.AsyncStatus.Started)
                        {
                            int panelWidth = 0;
                            int panelHeight = 0;
                            mOpenGLES.GetSurfaceDimensions(mRenderSurface, ref panelWidth, ref panelHeight);

                            // Logic to update the scene could go here
                            renderer.UpdateWindowSize(panelWidth, panelHeight);
                            renderer.Draw();

                            // The call to eglSwapBuffers might not be successful (i.e. due to Device Lost)
                            // If the call fails, then we must reinitialize EGL and the GL resources.
                            if (mOpenGLES.SwapBuffers(mRenderSurface) != GLUWP.EGL.TRUE)
                            {
                                // XAML objects like the SwapChainPanel must only be manipulated on the UI thread.
                                _swapChainPanel.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                                    new Windows.UI.Core.DispatchedHandler(() =>
                                    {
                                        RecoverFromLostDevice();
                                    }));

                                return;
                            }
                        }
                    }
                });

            // Run task on a dedicated high priority background thread.
            mRenderLoopWorker = Windows.System.Threading.ThreadPool.RunAsync(workItemHandler,
                Windows.System.Threading.WorkItemPriority.High,
                Windows.System.Threading.WorkItemOptions.TimeSliced);
        }

        void StopRenderLoop()
        {
            if (mRenderLoopWorker != null)
            {
                mRenderLoopWorker.Cancel();
                mRenderLoopWorker = null;
            }
        }

#endif
    }
}

#endif // !___XAM_FORMS___
