using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Camera.Simplemjpeg;
using GaganCameraController.Model;

namespace GaganCameraController.Views
{
    public class CamViewFragment : Fragment
    {
        public static CamViewFragment NewInstance()
        {
            var fragment = new CamViewFragment();
            return fragment;
        }

        private MjpegView CamView { get; set; }

        private Button StopButton { get; set; }

        private GaganPreference Pref { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.CameraView, container, false);

            CamView = view.FindViewById<MjpegView>(Resource.Id.CameraView);
            StopButton = view.FindViewById<Button>(Resource.Id.BtnStop);
            Pref = GaganPreference.Load(Activity);

            return view;
        }

        public override void OnHiddenChanged(bool hidden)
        {
            base.OnHiddenChanged(hidden);

            if (!hidden && !(CamView?.IsStreaming ?? true))
            {
                BeginStream();
            }
            else if (hidden && (CamView?.IsStreaming ?? false))
            {
                CamView.StopPlayback();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            CamView?.FreeCameraMemory();
        }

        private void BeginStream()
        {
            var url = Pref.CamUrl;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    return MjpegInputStream.Read(url);
                }
                catch (Exception)
                {
                    return null;
                }
            }).ContinueWith((t) =>
            {
                CamView.SetSource(t.Result);
                t.Result?.SetSkip(1);

                CamView.SetDisplayMode(MjpegView.SizeBestFit);
                CamView.ShowFps(false);
            });
        }
    }
}