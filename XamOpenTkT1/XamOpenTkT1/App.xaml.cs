using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamOpenTkT1.Services;
using XamOpenTkT1.Views;

namespace XamOpenTkT1
{
    public partial class App : Application
    {

        public Page mainPage => MainPage;

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
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
