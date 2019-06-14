#if !___XAM_FORMS___
using OpenGLDemo;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
#if __IOS__
using UIKit;
using OpenTK;
using Foundation;
using CoreGraphics;
using OpenTK.Graphics.ES30;
#elif WINDOWS_UWP
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

// OpenGLView Class https://docs.microsoft.com/en-us/dotnet/api/Xamarin.Forms.OpenGLView?view=xamarin-forms

namespace XamOpenTkT1
{
    //*************************************************************************
    ///
    /// <summary>
    /// Base OpenGL View class. Adds a few convenience control methods
    /// </summary>
    ///
    /// ***********************************************************************
    
    public class TTOpenGLView
    {
        private OpenGLView oGLV;

        public OpenGLView View => oGLV;

        public Action<Rectangle> OnDisplayHandler
        {
            set => oGLV.OnDisplay = value;
            get => oGLV.OnDisplay;
        }

        public TTOpenGLView(double heightRequest, double widthRequest)
        {
            oGLV = new OpenGLView
            {
                HasRenderLoop = true,
                HeightRequest = heightRequest,
                WidthRequest = widthRequest,
            };
        }

        public TTOpenGLView(Action<Rectangle> onDisplay, double heightRequest, double widthRequest)
        {
            oGLV = new OpenGLView
            {
                HasRenderLoop = true,
                HeightRequest = heightRequest,
                WidthRequest = widthRequest,
                OnDisplay = onDisplay
            };

            
        }

        public enum RenderTypeEnum
        {
            run,
            stop,
            single
        };

        public void Render(RenderTypeEnum renderType)
        {
            switch (renderType)
            {
                case RenderTypeEnum.run:
                    oGLV.HasRenderLoop = true;
                    break;
                case RenderTypeEnum.stop:
                    oGLV.HasRenderLoop = false;
                    break;
                case RenderTypeEnum.single:
                    oGLV.Display();
                    break;
            }
        }
    }

    //*************************************************************************
    ///
    /// <summary>
    /// Xamarin Application in shared (not Xamarin.Forms) code
    /// </summary>
    ///
    /// ***********************************************************************

    public class OpenTkT1App : Xamarin.Forms.Application
    {
        private OpenGLDemo.ControlSurface controlSurface;

        public static void TestDelegateMethod(string message)
        {
            System.Console.WriteLine(message);
        }

        public OpenTkT1App()
        {
            MainPage = new OpenTkT1Page { };
        }

        //*********************************************************************
        ///
        /// <summary>
        /// The bait and switch constructor. This allows us to embed a page
        /// from shared code in a Xamarin.Forms.App. This is a trixy way to
        /// use code which does not exist as a .Net Standard and would
        /// otherwise clash with Xamarin.Forms, for example, OpenGL.
        ///
        /// 1) Accepts another page as an argument, substitute that page as
        /// this app's main page.
        /// 2) Accept a child page name, find the child page that matches that
        /// name, replaces it with this page.
        /// 3) Accept a ControlSurface as an argument. Set values in that
        /// ControlSurface to values from this class, to enable two way sharing
        /// of access to objects like data and delegates.
        /// 
        /// </summary>
        /// <param name="otherPage"></param>
        /// <param name="replacePageName"></param>
        /// <param name="cs"></param>
        ///
        /// *******************************************************************
        
        public OpenTkT1App(Page otherPage, string replacePageName, 
            OpenGLDemo.ControlSurface cs)
        {
            // use the given page as the main page for this app
            MainPage = otherPage;

            // set the control surface
            controlSurface = cs;

            // change a value in the control surface, read it elsewhere to
            // prove that it was changed
            cs.Hi = "Hello from OpenTkT1App";

            // Set a delegage in the control surface, call it from
            // elsewhere to prove that the callback works
            controlSurface.handler = TestDelegateMethod;

            var op = otherPage as TabbedPage;

            // find the child page that matches that name, replace it with
            // this page
            for (int index = 0; index < op.Children.Count; index++)
            {
                if (op.Children[index].Title.Equals(replacePageName))
                    op.Children[index] = new OpenTkT1Page { };
            }
        }
    }

    //*************************************************************************
    ///
    /// <summary>
    /// The OpenGL content page in (not Xamarin.Forms) code 
    /// </summary>
    ///
    /// ***********************************************************************

    class OpenTkT1Page : ContentPage
    {
        public OpenTkT1Page()
        {
            //var myView = new TTOpenGLView(OnDisplay, 300, 300);

            //var myView = new MyOpenGLView();
            var myView = new OpenTkTutorialView();

            Title = "OpenGL";

            var view = myView.View;

            var toggle = new Switch { IsToggled = true };
            var button = new Button { Text = "Display" };

            toggle.Toggled += (s, a) =>
            {
                var tt = this.BindingContext;

                if (toggle.IsToggled)
                    myView.Render(TTOpenGLView.RenderTypeEnum.run);
                else
                    myView.Render(TTOpenGLView.RenderTypeEnum.stop);
            };

            button.Clicked += (s, a) =>
            {
                var tt = this.BindingContext;

                myView.Render(TTOpenGLView.RenderTypeEnum.single);
            };

            var stack = new StackLayout
            {
                Padding = new Xamarin.Forms.Size(20, 20),
                Children = { view, toggle, button }
            };

            Content = stack;
        }
    }

    //*************************************************************************
    ///
    /// <summary>
    /// OpenGL immediate mode view class, draws a triangle
    /// </summary>
    ///
    /// ***********************************************************************

    public class MyOpenGLView : TTOpenGLView
    {
        float red, green, blue;

        public MyOpenGLView() : base(300, 300)
        {
            base.OnDisplayHandler = OnDisplay;
        }

        private void OnDisplay(Rectangle obj)
        {
            GL.ClearColor(red, green, blue, 1.0f);
            GL.Clear((ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            red += 0.01f;
            if (red >= 1.0f)
                red -= 1.0f;
            green += 0.02f;
            if (green >= 1.0f)
                green -= 1.0f;
            blue += 0.03f;
            if (blue >= 1.0f)
                blue -= 1.0f;
        }
    }

    //*************************************************************************
    ///
    /// <summary>
    /// OpenGL shader view class. Demonstrates shaders and buffers and such
    /// things
    /// </summary>
    ///
    /// ***********************************************************************

    public class OpenTkTutorialView : TTOpenGLView
    {
        // Create the vertices for our triangle. These are listed in normalized device coordinates (NDC)
        // In NDC, (0, 0) is the center of the screen.
        // Negative X coordinates move to the left, positive X move to the right.
        // Negative Y coordinates move to the bottom, positive Y move to the top.
        // OpenGL only supports rendering in 3D, so to create a flat triangle, the Z coordinate will be kept as 0.
        //private readonly float[] _vertices =
        //{
        //    -0.5f, -0.5f, 0.0f, // Bottom-left vertex
        //    0.5f, -0.5f, 0.0f, // Bottom-right vertex
        //    0.0f,  0.5f, 0.0f  // Top vertex
        //};

        //private readonly float[] _vertices =
        //{
        // positions         // colors
        //    0.5f,  -0.5f, 0.0f, 1.0f, 0.0f, 0.0f,  // bottom right
        //    -0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f,  // bottom left
        //    0.0f,  0.5f,  0.0f, 0.0f, 0.0f, 1.0f   // top 
        //};

        //private readonly float[] _vertices =
        //{
            // positions         // colors
         //   0.5f,  -0.5f, 0.0f, 16711680.0f, 0.0f, 0.0f,  // bottom right
         //   -0.5f, -0.5f, 0.0f, 65280.0f, 1.0f, 0.0f,  // bottom left
         //   0.0f,  0.5f,  0.0f, 255.0f, 0.0f, 1.0f   // top 
        //};

        private readonly float[] _vertices =
        {
            // positions         // colors
            0.5f,  -0.5f, 0.0f, 4278190080.0f, 0.0f, 0.0f,  // bottom right
            -0.5f, -0.5f, 0.0f, 16711680.0f, 1.0f, 0.0f,  // bottom left
            0.0f,  0.5f,  0.0f, 65280.0f, 0.0f, 0.0f   // top 
        };

        // These are the handles to OpenGL objects. A handle is an integer representing where the object lives on the
        // graphics card. Consider them sort of like a pointer; we can't do anything with them directly, but we can
        // send them to OpenGL functions that need them.

        // What these objects are will be explained in OnLoad.
        private int _vertexBufferObject;
        private int _vertexArrayObject;

        // This class is a wrapper around a shader, which helps us manage it.
        // The shader class's code is in the Common project.
        // What shaders are and what they're used for will be explained later in this tutorial.

        //*** changed ***
        private TTOpenGl.Shader _shader;

        private bool glInitialized = false;

        public OpenTkTutorialView() : base(300, 300)
        {
            OnLoad();
        }

        //*********************************************************************
        //*
        //* OnLoad
        //*
        //* Non openGL init stuff goes here
        //*
        //*********************************************************************

        private void OnLoad()
        {
            base.OnDisplayHandler = OnDisplay;
        }

        private float[] ExtractRgbFromPacky(float inColor)
        {
            uint ucolor = (uint)inColor;
            float[] color = new float[3];

            color[0] = (0xff & (ucolor >> 24)) / 255f;
            color[1] = (0xff & (ucolor >> 16)) / 255f;
            color[2] = (0xff & (ucolor >> 8)) / 255f;

            return color;
        }

        private int floor(double inVal)
        {
            return (int)inVal;
        }

        private float[] ExtractRgbFromPack(float f)
        {
            float[] color = new float[3];

            color[2] = floor(f / 256.0 / 256.0 / 256.0 );
            color[1] = floor((f - color[2] * 256.0 * 256.0 * 256.0) / 256.0 / 256);
            color[0] = floor((f - color[2] * 256.0 * 256.0 * 256.0 - color[1] * 256.0 * 256.0) / 256);
            // now we have a vec3 with the 3 components in range [0..255]. Let's normalize it!
            //return color / 255.0;
            return color;
        }

        //*********************************************************************
        //*
        //* InitGL
        //*
        //* Call this method from within the 'OnDisplay()' method.
        //* https://stackoverflow.com/questions/17399087/glcreateshader-and-glcreateprogram-fail-on-android
        //* Shader calls should be within a GL thread that is onSurfaceChanged(),
        //* onSurfaceCreated() or onDrawFrame()
        //*
        //*********************************************************************

        private void InitGl()
        {
            

            //var color = ExtractRgbFromPack(_vertices[3]);
            //color = ExtractRgbFromPack(_vertices[9]);
            //color = ExtractRgbFromPack(_vertices[15]);

            // This will be the color of the background after we clear it, in normalized colors.
            // Normalized colors are mapped on a range of 0.0 to 1.0, with 0.0 representing black, and 1.0 representing
            // the largest possible value for that channel.
            // This is a deep green.
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // We need to send our vertices over to the graphics card so OpenGL can use them.
            // To do this, we need to create what's called a Vertex Buffer Object (VBO).
            // These allow you to upload a bunch of data to a buffer, and send the buffer to the graphics card.
            // This effectively sends all the vertices at the same time.

            // First, we need to create a buffer. This function returns a handle to it, but as of right now, it's empty.

            //*** changed ***
            //_vertexBufferObject = GL.GenBuffer();
            GL.GenBuffers(1, out _vertexBufferObject);

            // Now, bind the buffer. OpenGL uses one global state, so after calling this,
            // all future calls that modify the VBO will be applied to this buffer until another buffer is bound instead.
            // The first argument is an enum, specifying what type of buffer we're binding. A VBO is an ArrayBuffer.
            // There are multiple types of buffers, but for now, only the VBO is necessary.
            // The second argument is the handle to our buffer.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Finally, upload the vertices to the buffer.
            // Arguments:
            //   Which buffer the data should be sent to.
            //   How much data is being sent, in bytes. You can generally set this to the length of your array, multiplied by sizeof(array type).
            //   The vertices themselves.
            //   How the buffer will be used, so that OpenGL can write the data to the proper memory space on the GPU.
            //   There are three different BufferUsageHints for drawing:
            //     StaticDraw: This buffer will rarely, if ever, update after being initially uploaded.
            //     DynamicDraw: This buffer will change frequently after being initially uploaded.
            //     StreamDraw: This buffer will change on every frame.
            //   Writing to the proper memory space is important! Generally, you'll only want StaticDraw,
            //   but be sure to use the right one for your use case.

            //*** changed ****
            //GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsage.StaticDraw);
#if WINDOWS_UWP
             GL.BufferData( BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
#else
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(_vertices.Length * sizeof(float)), _vertices, BufferUsage.StaticDraw);
#endif

            // We've got the vertices done, but how exactly should this be converted to pixels for the final image?
            // Modern OpenGL makes this pipeline very free, giving us a lot of freedom on how vertices are turned to pixels.
            // The drawback is that we actually need two more programs for this! These are called "shaders".
            // Shaders are tiny programs that live on the GPU. OpenGL uses them to handle the vertex-to-pixel pipeline.
            // Check out the Shader class in Common to see how we create our shaders, as well as a more in-depth explanation of how shaders work.
            // shader.vert and shader.frag contain the actual shader code.

            //*** changed ***
            try
            {
                _shader = new TTOpenGl.Shader("shader.vert", "shader.frag");
                //_shader = new TTOpenGl.Shader("Vertex", "Fragment");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            // Now, enable the shader.
            // Just like the VBO, this is global, so every function that uses a shader will modify this one until a new one is bound instead.
            _shader.Use();


            // Ignore this for now, it will be explained later.

            //*** changed ***
            //_vertexArrayObject = GL.GenVertexArray();
            int[] vertexArrayObjects = new int[1];
            GL.GenVertexArrays(1, vertexArrayObjects);
            _vertexArrayObject = vertexArrayObjects[0];

            GL.BindVertexArray(_vertexArrayObject);


            // Now, we need to setup how the vertex shader will interpret the VBO data; you can send almost any C datatype (and a few non-C ones too) to it.
            // While this makes them incredibly flexible, it means we have to specify how that data will be mapped to the shader's input variables.

            // To do this, we use the GL.VertexAttribPointer function
            // Arguments:
            //   Location of the input variable in the shader. the layout(location = 0) line in the vertex shader explicitly sets it to 0.
            //   How many elements will be sent to the variable. In this case, 3 floats for every vertex.
            //   The data type of the elements set, in this case float.
            //   Whether or not the data should be converted to normalized device coordinates. In this case, false, because that's already done.
            //   The stride; this is how many bytes are between the last element of one vertex and the first element of the next. 3 * sizeof(float) in this case.
            //   The offset; this is how many bytes it should skip to find the first element of the first vertex. 0 as of right now.
            // Stride and Offset are just sort of glossed over for now, but when we get into texture coordinates they'll be shown in better detail.

            //*** changed ***
            //GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            int aPositionLocation = _shader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(aPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            // Enable variable 0 in the shader.
            GL.EnableVertexAttribArray(aPositionLocation);


            int aColorLocation = _shader.GetAttribLocation("aColor");
            //GL.VertexAttribPointer(aColorLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribPointer(aColorLocation, 1, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            // Enable variable 0 in the shader.
            GL.EnableVertexAttribArray(aColorLocation);


            // For a simple project, this would probably be enough. However, if you have a bunch of objects with their own shaders being drawn, it would be incredibly
            // tedious to do this over and over again every time you need to switch what object is being drawn. Because of this, OpenGL now *requires* that you create
            // what is known as a Vertex Array Object (VAO). This stores the layout you create with VertexAttribPointer/EnableVertexAttribArray so that it can be
            // recreated with one simple function call.
            // By creating the VertexArrayObject above, it has automatically saved this layout, so you can simply bind the VAO again to get everything back how it should be.

            // Finally, we bind the VBO again so that the VAO will bind that as well.
            // This means that, when you bind the VAO, it will automatically bind the VBO as well.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Mark GL as  initialized
            glInitialized = true;

            // Setup is now complete! Now we move to the OnRenderFrame function to finally draw the triangle.
        }

        //*********************************************************************
        //*
        //* OnDisplay
        //*
        //* invoked from the OpenGl subsystem when it is ready to draw, once
        //* per frame.
        //*
        //*********************************************************************

        private void OnDisplay(Rectangle obj)
        {
            // GL Initialization must occur on this thread. Make sure it is called only once
            if (!glInitialized)
                InitGl();

            // This clears the image, using what you set as GL.ClearColor earlier.
            // OpenGL provides several different types of data that can be rendered.
            // You can clear multiple buffers by using multiple bit flags.
            // However, we only modify the color, so ColorBufferBit is all we need to clear.
            GL.Clear(ClearBufferMask.ColorBufferBit);


            // To draw an object in OpenGL, it's typically as simple as binding your shader,
            // setting shader uniforms (not done here, will be shown in a future tutorial)
            // binding the VAO,
            // and then calling an OpenGL function to render.

            // Bind the shader
            _shader.Use();

            // Bind the VAO
            GL.BindVertexArray(_vertexArrayObject);

            // And then call our drawing function.
            // For this tutorial, we'll use GL.DrawArrays, which is a very simple rendering function.
            // Arguments:
            //   Primitive type; What sort of geometric primitive the vertices represent.
            //     OpenGL used to support many different primitive types, but almost all of the ones still supported
            //     is some variant of a triangle. Since we just want a single triangle, we use Triangles.
            //   Starting index; this is just the start of the data you want to draw. 0 here.
            //   How many vertices you want to draw. 3 for a triangle.

            //*** changed ***
            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
#if WINDOWS_UWP
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
#else
            GL.DrawArrays(BeginMode.Triangles, 0, 3);
#endif
        }
    }


}
#endif // !___XAM_FORMS___