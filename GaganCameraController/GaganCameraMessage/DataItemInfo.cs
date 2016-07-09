using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace GaganCameraMessage
{
    public class DataItemInfo
    {
        private const string PathPrefix = "/GaganCamera/";

        public const string PathMode = PathPrefix + "Mode";
        public const string PathLeftCamera = PathPrefix + "Camera/Left";
        public const string PathRightCamera = PathPrefix + "Camera/Right";
    }
}