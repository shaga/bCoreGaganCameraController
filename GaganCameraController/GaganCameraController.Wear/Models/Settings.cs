using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database.Sqlite;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LibBcore;

namespace GaganCameraController.Wear.Models
{
    class Settings
    {
        private static Settings AppSettings { get; set; }

        public static Settings LoadSettings()
        {
            if (AppSettings == null) AppSettings = new Settings();

            return AppSettings;
        }

        public enum EMode
        {
            Left,
            Right,
        }

        public int LeftCamValue { get; set; } = Bcore.CenterServoPos;
        public int RightCamValue { get; set; } = Bcore.CenterServoPos;

        public EMode Mode { get; set; } = EMode.Left;

        private Settings()
        {
            
        }

    }
}