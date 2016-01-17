using System;
using System.Collections.Generic;
using Microsoft.Advertising.Mobile.UI;
using System.Windows.Controls;

namespace DoIt
{
    internal static class AdControlHelper
    {
        /// <summary>
        /// Erstellt einen neuen Werbeblock
        /// </summary>
        internal static AdControl CreateAdControlSmall(string adUnitId)
        {
            AdControl adControl = new AdControl();
            adControl.Width = 300;
            adControl.Height = 50;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                adControl.ApplicationId = "test_client";
                adControl.AdUnitId = "Image300_50";
            }
            else
            {
                adControl.ApplicationId = "15808e38-d8c7-42b3-b1d8-790dbe8a640a";
                adControl.AdUnitId = adUnitId;
            }

            return adControl;
        }

        /// <summary>
        /// Erstellt einen neuen Werbeblock
        /// </summary>
        internal static AdControl CreateAdControl(string adUnitId)
        {
            AdControl adControl = new AdControl();
            adControl.Height = 80; adControl.Width = 480;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                adControl.ApplicationId = "test_client";
                adControl.AdUnitId = "Image480_80";
            }
            else
            {
                adControl.ApplicationId = "15808e38-d8c7-42b3-b1d8-790dbe8a640a";
                adControl.AdUnitId = adUnitId;
            }

            return adControl;
        }
    }
}
