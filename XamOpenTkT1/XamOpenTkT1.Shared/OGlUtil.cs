#if !___XAM_FORMS___
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Buffers;
#if __IOS__
using UIKit;
using OpenTK;
using Foundation;
using CoreGraphics;
using OpenTK.Graphics.ES30;
#elif WINDOWS_UWP
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
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

namespace OpenGLDemo
{
    class OGlUtil
    {
        public static void CheckOGLError()
        {
#if WINDOWS_UWP
            var ec = GL.GetError();
#else
            var ec = GL.GetErrorCode();
#endif
            if (ec != ErrorCode.NoError)
            {
                throw new Exception("OGL Exception : " + ec.ToString());
            }
        }

        public static void ClearOGLErrors()
        {
#if WINDOWS_UWP
            GL.GetError();
#else
            GL.GetErrorCode();
#endif
        }
    }
}
#endif //!___XAM_FORMS___