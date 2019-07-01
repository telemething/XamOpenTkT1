using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace XamOpenTkT1.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            //LoadApplication(new XamOpenTkT1.App());
            //LoadApplication(new XamOpenTkT1.OpenTkT1App());

            var controlSurface = new OpenGLDemo.ControlSurface();
            var app = new XamOpenTkT1.App(controlSurface);
            LoadApplication(new XamOpenTkT1.OpenTkT1App(app.mainPage, "OGL", controlSurface));
        }
    }
}
