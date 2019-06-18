#if ___XAM_FORMS___
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenGLDemo
{
    /// *******************************************************************
    ///
    /// <summary>
    /// The ControlSurface allows us to share access to objects like data
    /// and delegates in two directions between this shared code and a
    /// Xamarin.Forms lib.
    /// </summary>
    ///
    /// *******************************************************************

    public class ControlSurface
    {
        private static XamOpenTkT1.App originalApp;

        public delegate void TestDelegate(string message);
        public TestDelegate handler;

        public string Hi = "Hello";
        public bool bTest = false;

        public IntPtr CopyReadBufferPointer;
        public IntPtr CopyWriteBufferPointer;
        public IntPtr ArrayBufferPointer;

        public ControlSurface()
        { }

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

        public static OpenGLDemo.ControlSurface FetchControlSurface()
        {
            if (null == Xamarin.Forms.Application.Current)
                throw new Exception("null == Application.Current");

            originalApp = Xamarin.Forms.Application.Current as XamOpenTkT1.App;

            if (null == originalApp)
                throw new Exception(
                    "Application.Current is not of type 'XamOpenTkT1.App'");

            return originalApp.controlSurface;
        }
    }
}
#endif //___XAM_FORMS___