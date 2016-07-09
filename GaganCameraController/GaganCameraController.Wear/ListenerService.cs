using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Wearable;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GaganCameraController.Wear.Models;
using GaganCameraMessage;

namespace GaganCameraController.Wear
{
    [Service]
    [IntentFilter(new [] { "com.google.android.gms.wearable.BIND_LISTENER" })]
    class MsgListenerService : WearableListenerService
    {
        private const string PathConnectBcore = "GaganCamera.ConnectBcore";

        public override void OnMessageReceived(IMessageEvent messageEvent)
        {
//            base.OnMessageReceived(p0);
            if (messageEvent.Path == MessageValue.PathStart)
            {
                var bcore = Encoding.UTF8.GetString(messageEvent.GetData());

                Android.Util.Log.Debug("MsgListenerService", "Recv MsgAPI:" + bcore);

                var intent = new Intent(this, typeof(ControllerService));
                intent.PutExtra(ControllerService.ExtraKeyBcore, bcore);
                StartService(intent);
            }
            else if (messageEvent.Path == MessageValue.PathStop)
            {
                var intent = new Intent(ControllerService.ActionKeyStop);
                SendBroadcast(intent);
            }
        }

        public override void OnDataChanged(DataEventBuffer dataEvents)
        {
            var setting = Settings.LoadSettings();

            foreach (var dataEvent in dataEvents.ToEnumerable<IDataEvent>().Where(d => d.Type == DataEvent.TypeChanged))
            {
                var dataMapItem = DataMapItem.FromDataItem(dataEvent.DataItem);
                var map = dataMapItem.DataMap;

                switch (dataMapItem.Uri.Path)
                {
                    case DataItemInfo.PathMode:
                        setting.Mode = (Settings.EMode) map.GetInt("0");
                        break;
                    case DataItemInfo.PathLeftCamera:
                        setting.LeftCamValue = map.GetInt("128");
                        break;
                    case DataItemInfo.PathRightCamera:
                        setting.RightCamValue = map.GetInt("128");
                        break;
                }
            }
        }
    }
}