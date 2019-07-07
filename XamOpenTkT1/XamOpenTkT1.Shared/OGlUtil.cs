
using System.Runtime.InteropServices;
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

namespace TTOpenGl
{
    class OGlUtil
    {
        public static IntPtr GetIntPtr(float[] data, out GCHandle handle)
        {
            handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
        }

        public static IntPtr GetIntPtr<T>(T[] data, out GCHandle handle) 
        {
            handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
        }

        public static unsafe void* GetVoidPtr(float[] data, out GCHandle handle)
        {
            return GetVoidPtr(GetIntPtr(data, out handle));
        }

        public static unsafe void* GetVoidPtr<T>(T[] data, out GCHandle handle)
        {
            return GetVoidPtr(GetIntPtr<T>(data, out handle));
        }

        public static unsafe void* GetVoidPtr(IntPtr data)
        {
            return data.ToPointer();
        }

        //*********************************************************************
        ///
        /// <summary>
        /// Copy memory directly to a buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="bufferTarget">OGL BufferTarget type</param>
        /// <param name="targetBufferID">OGL Buffer ID</param>
        /// <param name="targetBufferLength">Target Buffer Length, this may be irrelevant</param>
        /// <param name="sourceData">Array of source data of type T</param>
        /// <param name="length">Length of type T objects to copy</param>
        ///
        //*********************************************************************

        public static void CopyToBuffer<T>(BufferTarget bufferTarget,
            int targetBufferID, uint targetBufferLength,
            T[] sourceData, uint length)
        {
            var pin = new GCHandle();

            try
            {
                if (0 == targetBufferLength)
                    throw new Exception("targetBufferLength = 0");

                // bind the given buffer
                GL.BindBuffer(bufferTarget, targetBufferID);

                TTOpenGl.OGlUtil.CheckOGLError();

                var typeSize = System.Runtime.CompilerServices.Unsafe.SizeOf<T>();

                // find the requested length in bytes
                uint uLength;
                if (0 == length)
                {
                    if (0 == sourceData.Length)
                        throw new Exception("sourceData.Length = 0");

                    uLength = (uint)(sourceData.Length * typeSize);
                }
                else
                {
                    if (sourceData.Length < length)
                        throw new Exception("sourceData.length < requested length");

                    uLength = (uint)(length * typeSize);
                }

                //TODO : for now we just clip, we should think about throwing or allocating more space
                uLength = Math.Min(uLength, (uint)(targetBufferLength * typeSize));

#if WINDOWS_UWP
            GL.BufferData(BufferTarget.ArrayBuffer, _cubeInstances.Length * sizeof(float), _cubeInstances, BufferUsageHint.StaticDraw);
#else
                //GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_cubeInstances.Length * sizeof(float)), _cubeInstances, BufferUsage.StaticDraw);
                GL.BufferData(bufferTarget, (IntPtr)uLength, IntPtr.Zero, BufferUsage.StaticDraw);
#endif
                TTOpenGl.OGlUtil.CheckOGLError();

                // get a pointer to the target buffer
                IntPtr bufferPointer = GL.MapBufferRange(bufferTarget, IntPtr.Zero,
                    (IntPtr)uLength, BufferAccessMask.MapWriteBit | BufferAccessMask.MapFlushExplicitBit);
                TTOpenGl.OGlUtil.CheckOGLError();

                unsafe
                {
                    System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(
                        TTOpenGl.OGlUtil.GetVoidPtr(bufferPointer),
                        TTOpenGl.OGlUtil.GetVoidPtr<T>(sourceData, out pin),
                        uLength);
                }

                // tell OGL that we are have copied the data
                GL.FlushMappedBufferRange(bufferTarget, IntPtr.Zero, (IntPtr)uLength);
                TTOpenGl.OGlUtil.CheckOGLError();

                // tell OGL that it can take control of the data
                if (!GL.UnmapBuffer(bufferTarget))
                    throw new Exception("GL.UnmapBuffer(), unable to map data");

                TTOpenGl.OGlUtil.CheckOGLError();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("CopyToBuffer(): " + e.Message);
            }
            finally
            {
                // return the array to control of the GC
                if (pin.IsAllocated)
                    pin.Free();

                // unbind the buffer so others don't accidentally operate on it
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        }

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