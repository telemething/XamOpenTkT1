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
        public delegate void TestDelegate(string message);
        public TestDelegate handler;

        public string Hi = "Hello";
        public bool bTest = false;

        public ControlSurface()
        { }
    }
}
#endif //___XAM_FORMS___