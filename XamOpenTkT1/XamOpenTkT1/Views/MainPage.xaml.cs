﻿using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamOpenTkT1.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : TabbedPage
    {
        private OpenGLDemo.ControlSurface controlSurface;

        public MainPage()
        {
            InitializeComponent();

            var bc = this.BindingContext;
        }
    }
}