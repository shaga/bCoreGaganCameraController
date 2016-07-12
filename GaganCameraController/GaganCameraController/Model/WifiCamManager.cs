using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace GaganCameraController.Model
{
    class WifiCamManager
    {
        #region const

        private const string CamSsid = "gagancam";

        #endregion

        #region property

        private static Context Context => Application.Context;

        private WifiManager WifiManager { get; }

        public bool IsConnectedCam
        {
            get
            {
                var info = WifiManager.ConnectionInfo;

                if (info == null) return false;

                return info.SSID.Contains(CamSsid);
            }
        }

        #endregion

        #region constructor

        public WifiCamManager()
        {
            WifiManager = Context.GetSystemService(Context.WifiService) as WifiManager;
        }

        #endregion

        #region method

        public bool ConnectCam()
        {
            if (IsConnectedCam) return true;

            WifiManager.StartScan();

            var result = WifiManager?.ScanResults?.FirstOrDefault(r => r.Ssid.Contains(CamSsid));

            if (result == null) return false;

            var config = WifiManager.ConfiguredNetworks.FirstOrDefault(c => c.Ssid.Contains(CamSsid));

            if (config == null) return false;

            return WifiManager.EnableNetwork(config.NetworkId, true);
        }

        #endregion
    }
}