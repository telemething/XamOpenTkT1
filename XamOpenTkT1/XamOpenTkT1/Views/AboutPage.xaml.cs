using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamOpenTkT1.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class AboutPage : ContentPage
    {
        private XamOpenTkT1.App originalApp;

        // the control surface allows us to share data and events with the
        // shared code
        private OpenGLDemo.ControlSurface controlSurface;

        /// *******************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ///
        /// *******************************************************************

        public AboutPage()
        {
            InitializeComponent();
            controlSurface = FetchControlSurface();
        }

        /// *******************************************************************
        ///
        /// <summary>
        /// Fetch the ControlSurface from the parent Application. This
        /// ControlSurface is defined in the shared code, it allows us to share
        /// access to objects like data and delegates in two directions between
        /// shared code and this Xamarin.Forms lib.
        /// </summary>
        /// <returns>OpenGLDemo.ControlSurface</returns>
        ///
        /// *******************************************************************

        private OpenGLDemo.ControlSurface FetchControlSurface()
        {
            if (null == Application.Current)
                throw new Exception("null == Application.Current");

            originalApp = Application.Current as XamOpenTkT1.App;

            if(null == originalApp)
                throw new Exception(
                    "Application.Current is not of type 'XamOpenTkT1.App'");

            return originalApp.controlSurface;
        }

        /// *******************************************************************
        ///
        /// <summary>
        /// Demonstrate 1) reading a string updated in the shared code,
        ///     2) calling a delegate which is defined in the shared code.
        /// </summary>
        /// <returns></returns>
        ///
        /// *******************************************************************

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // read the value to prove that it wsa changed in the shared code
            var tt = originalApp.controlSurface.Hi;

            // call the delegate to prove that it gets invoked in the shared 
            // code
            Device.BeginInvokeOnMainThread(() => 
                originalApp.controlSurface.handler("callback works"));
        }
    }
}