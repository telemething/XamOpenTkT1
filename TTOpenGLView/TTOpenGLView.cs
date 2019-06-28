using System;
using System.ComponentModel;
using System.Collections.Generic;
//using Xamarin.Forms.Platform;
using Xamarin.Forms;

//
// custom renderers: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/custom-renderer/view
// built in renderer: https://github.com/xamarin/Xamarin.Forms/blob/bd31e1e9fc8b2f9ad94cc99e0c7ab058174821f3/Xamarin.Forms.Core/OpenGLView.cs
//

namespace Xamarin.Forms
{
    //[RenderWith(typeof(_TTOpenGLViewRenderer))]
    public class OpenGLViewTT : View, IOpenGlViewController, IElementConfiguration<OpenGLViewTT>
    {
        #region Statics

        public static readonly BindableProperty HasRenderLoopProperty = BindableProperty.Create("HasRenderLoop", typeof(bool), typeof(OpenGLViewTT), default(bool));

        readonly Lazy<TTPlatformConfigurationRegistry<OpenGLViewTT>> _platformConfigurationRegistry;

        #endregion

        public bool HasRenderLoop
        {
            get { return (bool)GetValue(HasRenderLoopProperty); }
            set { SetValue(HasRenderLoopProperty, value); }
        }

        public Action<Rectangle> OnDisplay { get; set; }

        public void Display()
            => DisplayRequested?.Invoke(this, EventArgs.Empty);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler DisplayRequested;

        public OpenGLViewTT()
        {
            _platformConfigurationRegistry = new Lazy<TTPlatformConfigurationRegistry<OpenGLViewTT>>(() => new TTPlatformConfigurationRegistry<OpenGLViewTT>(this));
        }

        public IPlatformElementConfiguration<T, OpenGLViewTT> On<T>() where T : IConfigPlatform
        {
            return _platformConfigurationRegistry.Value.On<T>();
        }
    }

    /// <summary>
    /// Helper that handles storing and lookup of platform specifics implementations
    /// https://github.com/xamarin/Xamarin.Forms/blob/master/Xamarin.Forms.Core/PlatformConfigurationRegistry.cs
    /// </summary>
    /// <typeparam name="TElement">The Element type</typeparam>
    internal class TTPlatformConfigurationRegistry<TElement> : IElementConfiguration<TElement>
        where TElement : Element
    {
        readonly TElement _element;
        readonly Dictionary<Type, object> _platformSpecifics = new Dictionary<Type, object>();

        internal TTPlatformConfigurationRegistry(TElement element)
        {
            _element = element;
        }

        public IPlatformElementConfiguration<T, TElement> On<T>() where T : IConfigPlatform
        {
            if (_platformSpecifics.ContainsKey(typeof(T)))
            {
                return (IPlatformElementConfiguration<T, TElement>)_platformSpecifics[typeof(T)];
            }

            var emptyConfig = Configuration<T, TElement>.Create(_element);

            _platformSpecifics.Add(typeof(T), emptyConfig);

            return emptyConfig;
        }
    }
}