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
using LibBcore;

namespace GaganCameraController.Model
{
    class GaganPreference
    {
        private const int DefaultServoPos = Bcore.CenterServoPos;
        private const string DefaultCamUrl = "http://192.168.2.215/?action=stream";

        private const string PrefFileName = "GaganPref";
        private const string PrefIdServoPosLeft = "servo_pos_left";
        private const string PrefIdServoPosRight = "servo_pos_right";
        private const string PrefIdCamUrl = "cam_url";

        public int ServoPosLeft { get; set; }
        public int ServoPosRight { get; set; }
        public string CamUrl { get; set; }

        private ISharedPreferences Pref { get; }

        private static GaganPreference _pref;

        public static GaganPreference Load(Context context)
        {
            if (_pref == null)
            {
                _pref = new GaganPreference(context);
            }

            return _pref;
        }

        private GaganPreference(Context context)
        {
            Pref = context.GetSharedPreferences(PrefFileName, FileCreationMode.Private);

            ServoPosLeft = Pref.GetInt(PrefIdServoPosLeft, DefaultServoPos);
            ServoPosRight = Pref.GetInt(PrefIdServoPosRight, DefaultServoPos);
            CamUrl = Pref.GetString(PrefIdCamUrl, DefaultCamUrl);
        }

        public void Save()
        {
            var editor = Pref.Edit();
            editor.PutInt(PrefIdServoPosLeft, ServoPosLeft);
            editor.PutInt(PrefIdServoPosRight, ServoPosRight);
            editor.PutString(PrefIdCamUrl, CamUrl);
            editor.Commit();
        }
    }
}