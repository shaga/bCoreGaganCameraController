using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Wearable;
using Android.Media;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Camera.Simplemjpeg;
using GaganCameraController.Model;
using Java.Interop;
using Encoding = System.Text.Encoding;

namespace GaganCameraController.Views
{
    [Activity(Label = "ControllerActivity", Theme = "@style/AppTheme.NoActionBar", ScreenOrientation = ScreenOrientation.Landscape)]
    public class ControllerActivity : AppCompatActivity, ISensorEventListener
    {
        public const string ExtraKeyAddr = "GaganCameraController.Views.ControllerActivity.Addr";

        private MjpegView CamView { get; set; }
        private GaganPreference Pref { get; set; }

        private SettingFragment SettingFragment { get; set; }
        private ControllerFragment ControllerFragment { get; set; }
        private GaganController Controller { get; set; }
        private TextView TextMessage { get; set; }
        private string Addr { get; set; }

        private bool IsCamSuspended { get; set; }

        private bool IsShowSetting { get; set; }

        private SensorManager SensorManager => GetSystemService(SensorService) as SensorManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Addr = Intent.GetStringExtra(ExtraKeyAddr);
            if (string.IsNullOrEmpty(Addr))
            {
                Finish();
                return;
            }


            Pref = GaganPreference.Load(this);

            SetContentView(Resource.Layout.Control);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.controller_toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            CamView = FindViewById<MjpegView>(Resource.Id.CamView);
            CamView?.SetResolution(640, 480);

            BeginStream();

            //TextMessage = FindViewById<TextView>(Resource.Id.TextConnectMessage);

            //Controller = new GaganController(this);
            //Controller.BcoreConnectionChanged += OnConnectedBcore;
            //Controller.ConnectBcore(Addr);


            //SettingFragment = SettingFragment.newInstance();
            //SettingFragment.Controller = Controller;

            //ControllerFragment = ControllerFragment.NewInstance();
            //ControllerFragment.Controller = Controller;

            //var transaction = FragmentManager.BeginTransaction();
            //transaction.Add(Resource.Id.FrameFragment, ControllerFragment);
            //transaction.Hide(ControllerFragment);
            //transaction.Add(Resource.Id.FrameFragment, SettingFragment);
            //transaction.Hide(SettingFragment);
            //transaction.Commit();

        }

        protected override void OnResume()
        {
            base.OnResume();

            if (IsCamSuspended)
            {
                BeginStream();
            }

            InitSensor();
        }

        protected override void OnPause()
        {
            base.OnPause();

            if (!IsCamSuspended && (CamView?.IsStreaming ?? false))
            {
                CamView?.StopPlayback();
                IsCamSuspended = true;
            }

            FinishSensor();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            CamView?.FreeCameraMemory();
            //Controller.BcoreConnectionChanged -= OnConnectedBcore;
            //Controller.DisconnectBcore();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (!IsShowSetting)
            {
                MenuInflater.Inflate(Resource.Menu.menu_controller, menu);
                return true;
            }
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    //if (IsShowSetting)
                    //{
                    //    SwitchFragment(false);
                    //}
                    //else
                    //{
                        Finish();
                    //}
                    return false;
                case Resource.Id.ShowMenuSetting:
//                    SwitchFragment(true);
                    return false;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed()
        {
            if (IsShowSetting)
            {
//                SwitchFragment(false);
                return;
            }
            base.OnBackPressed();
        }

        private void InitSensor()
        {
            //var manager = GetSystemService(Context.SensorService) as SensorManager;

            //manager?.RegisterListener(this, manager.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Ui);
        }

        private void FinishSensor()
        {
            //var manager = GetSystemService(SensorService) as SensorManager;

            //manager?.UnregisterListener(this);
        }

        public void BeginStream()
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

        private void OnConnectedBcore(object sender, bool isConnected)
        {
            if (isConnected)
            {
                RunOnUiThread(() =>
                {
                    TextMessage.Visibility = ViewStates.Gone;
                    var transaction = FragmentManager.BeginTransaction();
                    transaction.Show(ControllerFragment);
                    transaction.Commit();
                });
            }
            else
            {
                RunOnUiThread(() => Toast.MakeText(this, Resource.String.Disconnectedmessage, ToastLength.Short).Show());
                
                Finish();
            }
        }

        private void SwitchFragment(bool showSetting)
        {
            var transaction = FragmentManager.BeginTransaction();
            if (showSetting)
            {
                transaction.Hide(ControllerFragment);
                transaction.Show(SettingFragment);
            }
            else
            {
                transaction.Hide(SettingFragment);
                transaction.Show(ControllerFragment);
            }

            transaction.Commit();
            FragmentManager.InvalidateOptionsMenu();
            IsShowSetting = showSetting;
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            
        }

        public void OnSensorChanged(SensorEvent e)
        {

        }
    }
}