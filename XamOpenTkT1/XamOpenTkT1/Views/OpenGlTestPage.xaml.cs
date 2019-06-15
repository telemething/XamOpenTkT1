using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamOpenTkT1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OpenGlTestPage : ContentPage
    {
        // the control surface allows us to share data and events with the
        // shared code
        private OpenGLDemo.ControlSurface controlSurface;

        /// *******************************************************************
        ///
        /// <summary>
        /// Fetch the ControlSurface from the parent Application in the
        /// constructor, it will change to a different App later and you don't
        /// want to use that one
        /// </summary>
        ///
        /// *******************************************************************

        public OpenGlTestPage()
        {
            InitializeComponent();

            // Fetch the ControlSurface from the parent Application
            controlSurface = OpenGLDemo.ControlSurface.FetchControlSurface();
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

        async void OnTestNowButtonClicked(object sender, EventArgs args)
        {
            // read the value to prove that it was changed in the shared code
            var tt = controlSurface.Hi;

            // call the delegate to prove that it gets invoked in the shared 
            // code
            Device.BeginInvokeOnMainThread(() =>
                controlSurface.handler("callback works"));
        }

    }
}