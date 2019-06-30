using System;
using GLUWP.ES20;
using GLUWP.ES20Enums;

namespace SampleApp
{
    public class SimpleRenderer2
    {
        private int _program;
        private int _windowWidth = 0;
        private int _windowHeight = 0;

        private int _vertexBufferObject;
        private int _vertexArrayObject;

        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
            0.5f, -0.5f, 0.0f, // Bottom-right vertex
            0.0f,  0.5f, 0.0f  // Top vertex
        };

        string _vertexShaderSource = @"
                attribute vec3 aPosition;
                void main(void)
                {
                    gl_Position = vec4(aPosition, 1.0);
                }";

        string _fragmentShaderSource = @"
                precision mediump float;
                void main()
                {
                    gl_FragColor = vec4(1.0, 1.0, 0.0, 1.0);
                }";

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);

            string[] sourceArray = new string[] { source };
            GL.ShaderSource(shader, sourceArray);
            
            GL.CompileShader(shader);

            int compileResult = GL.GetShaderiv(shader, ShaderParameter.CompileStatus);


            if (compileResult == 0)
            {
                int infoLogLength = GL.GetShaderiv(shader, ShaderParameter.InfoLogLength);

                GL.GetShaderInfoLog(shader, infoLogLength, out int length, out var infoLog);

                var errorMessage = $"Shader compilation failed: {infoLog}";

                throw new ApplicationException(errorMessage);
            }

            return shader;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vsSource"></param>
        /// <param name="fsSource"></param>
        /// <returns></returns>
        ///
        //*********************************************************************

        int CompileProgram(string vsSource, string fsSource)
        {
            int program = GL.CreateProgram();

            if (program == 0)
            {
                throw new ApplicationException("Program creation failed");
            }

            int vs = CompileShader(ShaderType.VertexShader, vsSource);
            int fs = CompileShader(ShaderType.FragmentShader, fsSource);

            if (vs == 0 || fs == 0)
            {
                GL.DeleteShader(fs);
                GL.DeleteShader(vs);
                GL.DeleteProgram(program);
                return 0;
            }

            GL.AttachShader(program, vs);
            GL.DeleteShader(vs);

            GL.AttachShader(program, fs);
            GL.DeleteShader(fs);

            GL.LinkProgram(program);

            int linkStatus = GL.GetProgramiv(program, GetProgramParameterName.LinkStatus);

            if (linkStatus == 0)
            {
                var infoLogLength = GL.GetProgramiv(program, GetProgramParameterName.InfoLogLength);

                GL.GetProgramInfoLog(program, infoLogLength, out int length, out var infoLog);

                var errorMessage = $"Program link failed: {infoLog}";

                throw new ApplicationException(errorMessage);
            }

            return program;
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        public SimpleRenderer2()
        {
            // Set up the shader and its uniform/attribute locations.
            _program = CompileProgram(_vertexShaderSource, _fragmentShaderSource);

            if (_program == 0)
                return;

            GL.UseProgram(_program);

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // vertex buffer
            _vertexBufferObject = GL.GenBuffers(1)[0];
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices, BufferUsageHint.StaticDraw);

            // vertex array
            _vertexArrayObject = GL.GenVertexArrays(1)[0];
            GL.BindVertexArray(_vertexArrayObject);

            // position attribute
            var positionAttribLocation = GL.GetAttribLocation(_program, "aPosition");
            GL.VertexAttribPointer(positionAttribLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(positionAttribLocation);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        ~SimpleRenderer2()
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteVertexArray(_vertexArrayObject);

            if (_program != 0)
            {
                GL.DeleteProgram(_program);
                _program = 0;
            }

            if (_vertexBufferObject != 0)
            {
                GL.DeleteBuffers(1, new int[]{ _vertexBufferObject });
                _vertexBufferObject = 0;
            }

            if (_vertexArrayObject != 0)
            {
                GL.DeleteVertexArray(_vertexArrayObject);
                _vertexArrayObject = 0;
            }
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        ///
        //*********************************************************************

        public void Draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(BeginMode.Triangles, 0, 3);
        }

        //*********************************************************************
        ///
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        ///
        //*********************************************************************

        public void UpdateWindowSize(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            _windowWidth = width;
            _windowHeight = height;
        }
    }
}
