using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.Annotation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.Gms.Extensions;
using Android.Gms.Wearable;
using Android.Media;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using GaganCameraController.Model;
using GaganCameraController.Views;
using GaganCameraMessage;
using Javax.Crypto.Interfaces;
using LibBcore;
using Encoding = System.Text.Encoding;

namespace GaganCameraController
{
    [Activity(Label = "GaganCameraController", MainLauncher = true, 
        Icon = "@drawable/ic_launcher", Theme = "@style/AppTheme.NoActionBar", ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden)]
    public class MainActivity : AppCompatActivity
    {
        private const long TimeoutLength = 10000;

        private List<BcoreDeviceInfo> BcoreInfos { get; set; }

        private BcoreScanner Scanner { get; set; }

        private ProgressBar ProgressScanning { get; set; }

        private Button ButtonScan { get; set; }

        private BcoreFoundListAdapter Adapter { get; set; }

        private Handler HandlerScanTimeout { get; set; }

        private ListView ListViewBcores { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.main_toolbar);
            SetSupportActionBar(toolbar);

            BcoreInfos = new List<BcoreDeviceInfo>();

            Adapter = new BcoreFoundListAdapter(this, BcoreInfos);
            Scanner = new BcoreScanner(this);
            Scanner.FoundBcore += OnFoundBcore;

            ProgressScanning = FindViewById<ProgressBar>(Resource.Id.ProgressScanning);

            ButtonScan = FindViewById<Button>(Resource.Id.ButtonScan);
            ButtonScan.Click += (s, e) =>
            {
                if (Scanner.IsScanning) StopScan();
                else StartScan();
            };

            ListViewBcores = FindViewById<ListView>(Resource.Id.ListFoundBcore);
            ListViewBcores.Adapter = Adapter;
            ListViewBcores.ItemClick += OnClickedListItem;
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (Build.VERSION.SdkInt < BuildVersionCodes.M) return;

            if (CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                RequestPermissions(new string[] { Manifest.Permission.AccessCoarseLocation }, 100);
            }
        }

        [TargetApi(Value = (int)BuildVersionCodes.M)]
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == 100)
            {
                if (grantResults[0]!= Permission.Granted) Finish();
            }
        }

        private void StartScan()
        {
            if (Scanner.IsScanning) return;

            BcoreInfos.Clear();
            Adapter.NotifyDataSetChanged();

            Scanner.StartScan();
            ProgressScanning.Visibility = ViewStates.Visible;
            ButtonScan.SetText(Resource.String.BtnScanStop);

            HandlerScanTimeout = new Handler();
            HandlerScanTimeout.PostDelayed(OnTimeoutScan, TimeoutLength);
        }

        private void StopScan()
        {
            if (!Scanner.IsScanning) return;

            HandlerScanTimeout?.RemoveCallbacks(OnTimeoutScan);

            Scanner.StopScan();
            ProgressScanning.Visibility = ViewStates.Invisible;
            ButtonScan.SetText(Resource.String.BtnScanStop);
        }

        private void OnTimeoutScan()
        {
            HandlerScanTimeout = null;

            StopScan();
        }

        private void OnFoundBcore(object sender, BcoreFoundEventArgs e)
        {
            if (BcoreInfos.Any(i => i.Equals(e.Info))) return;

            RunOnUiThread(() =>
            {
                BcoreInfos.Add(e.Info);
                Adapter.NotifyDataSetChanged();
            });
        }

        private void OnClickedListItem(object sender, AdapterView.ItemClickEventArgs e)
        {
            var item = Adapter[e.Position];

            if (item == null) return;

            var manager = new WifiCamManager();
            manager.ConnectCam();

            Android.Util.Log.Debug("MainActivity", "Clicked:{0}/{1}", item.Name, item.Addr);
            var intent = new Intent(this, typeof(ControllerActivity));
            intent.PutExtra(ControllerActivity.ExtraKeyAddr, item.Addr);
            StartActivity(intent);

        }
    }
}

