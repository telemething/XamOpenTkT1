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
    /// Xamarin Application in shared (not Xamarin.Forms) code
    /// </summary>
    ///
    /// ***********************************************************************

    public class TTOglApp : Xamarin.Forms.Application
    {
        private OpenGLDemo.ControlSurface controlSurface;
        private TTOglPage _ttOglPage;

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
            _ttOglPage.Renderer.SetUpdateVertexData();
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
            _ttOglPage.Renderer.SetUpdateVertexData(pc);
            System.Console.WriteLine("Got new pointcloud data");
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        public TTOglApp()
        {
            MainPage = new TTOglPage(controlSurface);
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

        public TTOglApp(Page otherPage, string replacePageName,
            OpenGLDemo.ControlSurface cs)
        {
            // use the given page as the main page for this app
            MainPage = otherPage;

            // set the control surface
            controlSurface = cs;

            // change a value in the control surface, read it elsewhere to
            // prove that it was changed
            cs.Hi = "Hello from TTOglApp";

            // Set a delegate in the control surface, call it from
            // elsewhere to prove that the callback works
            controlSurface.handler = TestDelegateMethod;

            controlSurface.GotNewPointcloudData = GotNewPointcloudData;

            var op = otherPage as TabbedPage;
            _ttOglPage = new TTOglPage(controlSurface);

            // find the child page that matches that name, replace it with
            // this page
            for (int index = 0; index < op.Children.Count; index++)
            {
                if (op.Children[index].Title.Equals(replacePageName))
                    op.Children[index] = _ttOglPage;
            }
        }
    }
}
#endif // !___XAM_FORMS___
