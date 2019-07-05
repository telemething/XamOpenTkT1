using System;
using System.Collections.Generic;
using System.Text;

namespace XamOpenTkT1
{
    //*************************************************************************
    ///
    /// <summary>
    /// Interface for UpdateWindowSize and Draw events from Windows
    /// </summary>
    ///
    /// ***********************************************************************

    public interface ITTRender
    {
        void UpdateWindowSize(int width, int height);
        void Draw();
    }

}
