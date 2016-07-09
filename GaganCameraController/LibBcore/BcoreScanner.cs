using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Xml.XPath;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Systems;
using Android.Text;
using ScanMode = Android.Bluetooth.LE.ScanMode;

namespace LibBcore
{
    public class BcoreFoundEventArgs : EventArgs
    {
        public BcoreDeviceInfo Info { get; }

        public BcoreFoundEventArgs(string name, string addr)
        {
            Info = new BcoreDeviceInfo(name, addr);
        }
    }

    public class BcoreScanner
    {
        #region inner class

        public class LeScanCallback : Java.Lang.Object, BluetoothAdapter.ILeScanCallback
        {
            private BcoreScanner Parent { get; }

            public LeScanCallback(BcoreScanner parent)
            {
                Parent = parent;
            }

            public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
            {
                if (CheckDeviceIsBcore(scanRecord))
                    Parent?.OnFoundBcore(device);
            }

            private bool CheckDeviceIsBcore(byte[] scanRecord)
            {
                var pos = 0;

                while (pos < scanRecord.Length - 2)
                {
                    var len = scanRecord[pos++];
                    var type = scanRecord[pos];

                    if ((type == 6 || type == 7) && len == 17)
                    {
                        var uuidStr = string.Empty;

                        for (var i = 1; i <= 16; i++)
                        {
                            uuidStr += scanRecord[pos + i].ToString("x2");
                            if (i == 4 || i == 6 || i == 8 || i == 10)
                            {
                                uuidStr += "-";
                            }
                        }

                        if (string.Compare(uuidStr, BcoreUuid.BcoreServiceRev.ToString(), StringComparison.OrdinalIgnoreCase) == 0) return true;
                    }

                    pos += len;
                }

                return false;
            }
        }

        public class ScanCallback : Android.Bluetooth.LE.ScanCallback
        {
            private BcoreScanner Parent { get; }

            public ScanCallback(BcoreScanner parent)
            {
                Parent = parent;
            }

            public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);

                Parent?.OnFoundBcore(result.Device);
            }
        }

        #endregion

        #region property

        public bool IsScanning { get; set; }

        private static bool IsAfterLollipop => Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop;

        private BluetoothManager BluetoothManager { get; }
        private BluetoothAdapter BluetoothAdapter => BluetoothManager?.Adapter;

        private LeScanCallback LeCallback { get; set; }

        private BluetoothLeScanner Scanner => BluetoothAdapter?.BluetoothLeScanner;
        private ScanCallback Callback { get; set; }
        private IList<ScanFilter> Filters { get; set; }
        private ScanSettings ScanSettings { get; set; }

        #endregion

        #region event

        public event EventHandler<BcoreFoundEventArgs> FoundBcore; 

        #endregion

        #region constructor

        public BcoreScanner(Context context)
        {
            BluetoothManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
        }

        #endregion

        public void StartScan()
        {
            if (IsScanning) return;

            InitScanner();

            if (IsAfterLollipop)
            {
                Scanner.StartScan(null, ScanSettings, Callback);
            }
            else
            {
                BluetoothAdapter.StartLeScan(LeCallback);
            }

            IsScanning = true;
        }

        public void StopScan()
        {
            if (!IsScanning) return;

            if (IsAfterLollipop)
            {
                Scanner.StopScan(Callback);
            }
            else
            {
                BluetoothAdapter.StopLeScan(LeCallback);
            }

            IsScanning = false;
        }

        private void InitScanner()
        {
            if (IsAfterLollipop && !(Filters?.Any() ?? false))
            {
                if (Filters == null)
                {
                    Filters = new List<ScanFilter>();
                }
                else
                {
                    Filters.Clear();
                }
                Filters.Add(new ScanFilter.Builder().SetServiceUuid(BcoreUuid.BcoreScanUuid).Build());
            }

            if (IsAfterLollipop && ScanSettings == null)
            {
                ScanSettings = new ScanSettings.Builder().SetScanMode(ScanMode.Balanced).Build();
            }

            if (IsAfterLollipop && Callback == null)
            {
                Callback = new ScanCallback(this);
            }

            if (!IsAfterLollipop && LeCallback == null)
            {
                LeCallback = new LeScanCallback(this);
            }
        }

        private void OnFoundBcore(BluetoothDevice device)
        {
            FoundBcore?.Invoke(this, new BcoreFoundEventArgs(device.Name, device.Address));
        }
    }
}
