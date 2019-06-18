using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;

namespace XamOpenTkT1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OpenGlTestPage : ContentPage
    {
        // the control surface allows us to share data and events with the
        // shared code
        private OpenGLDemo.ControlSurface controlSurface;
        private enum FileSourceEnum { Filesystem, AssemblyResource }

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

        async void OnTestReadAndCallbackButtonClicked(object sender, EventArgs args)
        {
            // read the value to prove that it was changed in the shared code
            var tt = controlSurface.Hi;

            // call the delegate to prove that it gets invoked in the shared 
            // code
            Device.BeginInvokeOnMainThread(() =>
                controlSurface.handler("callback works"));
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        ///
        //*********************************************************************

        async void OnReadAndDisplayPcFileClicked(object sender, EventArgs args)
        {
            var pc = ReadPointCloudFile("pointcloudfile.bin", 
                FileSourceEnum.AssemblyResource);

            Device.BeginInvokeOnMainThread(() =>
                controlSurface.GotNewPointcloudData(pc));
        }

        //*********************************************************************
        ///
        /// <summary>
        /// Save a Pointcloud2 file
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="fileName"></param>
        ///
        //*********************************************************************

        private void SavePointCloudFile(
            RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 pc, string fileName)
        {
            var mainDir = Xamarin.Essentials.FileSystem.AppDataDirectory;
            var filePath = mainDir + "\\" + fileName;

            TTPointCloudLib.TTPointCloud.SavePointCloudFile(pc, filePath);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// Read a Pointcloud2 file
        /// </summary>
        /// <param name="fileName"></param>
        ///
        //*********************************************************************

        private RosSharp.RosBridgeClient.Messages.Sensor.PointCloud2 
            ReadPointCloudFile(string fileName, FileSourceEnum fileSource)
        {
            switch (fileSource)
            {
                case FileSourceEnum.AssemblyResource:
                    var assembly = IntrospectionExtensions.GetTypeInfo(typeof(XamOpenTkT1.App)).Assembly;

                    return TTPointCloudLib.TTPointCloud.ReadPointCloudFile(assembly,
                        "DataFiles.pointcloudfile.bin");
                    break;
                case FileSourceEnum.Filesystem:

                    var mainDir = Xamarin.Essentials.FileSystem.AppDataDirectory;
                    var filePath = mainDir + "\\" + fileName;

                    return TTPointCloudLib.TTPointCloud.ReadPointCloudFile(filePath);
                    break;
                default:
                    throw new ArgumentException("Invalid fileSource requested");
            }
        }
    }
}