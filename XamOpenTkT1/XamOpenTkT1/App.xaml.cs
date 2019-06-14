using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamOpenTkT1.Services;
using XamOpenTkT1.Views;

namespace XamOpenTkT1
{
    public partial class App : Application
    {
        // the ControlSurface allows us to share data and events with the
        // shared code. We must declare this here in the App to make it
        // available for reference in all child objects
        public OpenGLDemo.ControlSurface controlSurface;

        public Page mainPage => MainPage;

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
        }

        //*********************************************************************
        //
        /// <summary>
        /// This constructor passes in and saves a reference to the
        /// ControlSurface.
        /// </summary>
        /// <param name="cs"></param>
        ///
        //*********************************************************************

        public App(OpenGLDemo.ControlSurface cs)
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();

            // save a reference to the ControlSurface
            controlSurface = cs;

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
