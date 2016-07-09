using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using LibBcore;

namespace GaganCameraController.Views
{
    public class ScanListFragment : Fragment
    {
        private const long ScanTimeoutLength = 10000;

        public static ScanListFragment NewInstance()
        {
            var fragment = new ScanListFragment();
            return fragment;
        }

        public event EventHandler<string> StartControl;

        private Button ScanButton { get; set; }

        private Button StartButton { get; set; }

        private ListView ListFoundBcore { get; set; }

        private ProgressBar ScanningProgressBar { get; set; }

        private BcoreScanner Scanner { get; set; }

        private Handler ScanTimeoutHandler { get; set; }

        private List<BcoreDeviceInfo> ListFoundBcoreInfo { get; } = new List<BcoreDeviceInfo>();

        private BcoreFoundListAdapter Adapter { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            Scanner = new BcoreScanner(Activity);
            Scanner.FoundBcore += OnFoundBcore;

            Adapter = new BcoreFoundListAdapter(Activity, ListFoundBcoreInfo);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.ScanList, container, false);

            ScanButton = view.FindViewById<Button>(Resource.Id.BtnScan);
            ScanButton.Click += (s, e) =>
            {
                if (Scanner.IsScanning) StopScan();
                else StartScan();
            };
            StartButton = view.FindViewById<Button>(Resource.Id.BtnStart);
            StartButton.Enabled = false;
            StartButton.Click += (s, e) =>
            {
                var info = Adapter[ListFoundBcore?.CheckedItemPosition ?? -1];

                if (info == null) return;

                StartControl?.Invoke(this, info.Addr);
            };
            ListFoundBcore = view.FindViewById<ListView>(Resource.Id.ListFoundBcore);
            ListFoundBcore.ItemsCanFocus = true;
            ListFoundBcore.ChoiceMode = ChoiceMode.Single;
            ListFoundBcore.Adapter = Adapter;
            ListFoundBcore.ItemClick += (s, e) =>
            {
                var item = Adapter[e.Position];

                StartButton.Enabled = item != null;
            };
            ScanningProgressBar = view.FindViewById<ProgressBar>(Resource.Id.ProgressScan);
            ScanningProgressBar.Visibility = ViewStates.Invisible;

            return view;
        }

        private void StartScan()
        {
            if (Scanner.IsScanning) return;
            
            Activity.RunOnUiThread(() =>
            {
                ListFoundBcoreInfo.Clear();
                Adapter.NotifyDataSetChanged();
                ScanButton.Text = GetText(Resource.String.BtnScanStop);
                ScanningProgressBar.Visibility = ViewStates.Visible;
                StartButton.Enabled = false;
            });
            Scanner.StartScan();

            ScanTimeoutHandler = new Handler();
            ScanTimeoutHandler.PostDelayed(OnTimeoutScan, ScanTimeoutLength);
        }

        private void StopScan()
        {
            if (!Scanner.IsScanning) return;

            if (ScanTimeoutHandler != null)
            {
                ScanTimeoutHandler.RemoveCallbacks(OnTimeoutScan);
                ScanTimeoutHandler = null;
            }

            Activity.RunOnUiThread(() =>
            {
                ScanButton.Text = GetText(Resource.String.BtnScanStart);
                ScanningProgressBar.Visibility = ViewStates.Invisible;
            });
            Scanner.StopScan();
        }

        private void OnTimeoutScan()
        {
            ScanTimeoutHandler = null;
            StopScan();
        }

        private void OnFoundBcore(object sender, BcoreFoundEventArgs e)
        {
            if (ListFoundBcoreInfo.Any(i => i.Equals(e.Info))) return;
            
            Activity.RunOnUiThread(() =>
            {
                ListFoundBcoreInfo.Add(e.Info);
                Adapter.NotifyDataSetChanged();
            });
        }
    }
}