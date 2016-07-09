using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LibBcore;

namespace GaganCameraController.Wear
{
    [Service]
    class ControllerService : Service, ISensorEventListener
    {
        public const string ActionKeyStart = "GaganCameraController.Wear.ControllerService.ActionStart";
        public const string ActionKeyStop = "GaganCameraController.Wear.ControllerService.ActionStop";
        public const string ExtraKeyBcore = "GaganCameraController.Wear.ControllerService.ExtraBcore";

        private enum EState
        {
            Stop,
            Right,
            Left,
        }

        private class ServiceReceiver : BroadcastReceiver
        {
            private ControllerService Parent { get; }

            public  ServiceReceiver(ControllerService parent)
            {
                Parent = parent;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                switch (intent.Action)
                {
                    case ActionKeyStart:
                        var bcore = intent.Extras.GetString(ExtraKeyBcore);
                        Parent?.OnActionStart(bcore);
                        break;
                    case ActionKeyStop:
                        Parent?.OnActionStop();
                        break;
                }
            }
        }

        private SensorManager SensorManager => GetSystemService(Context.SensorService) as SensorManager;

        private ServiceReceiver Receiver { get; set; }

        private bool IsRegisteredSensor { get; set; }

        private BcoreManager BcoreManager { get; set; }

        private EState State { get; set; }

        public override void OnCreate()
        {
            base.OnCreate();

            Receiver = new ServiceReceiver(this);
            var filter = new IntentFilter(ActionKeyStart);

            RegisterReceiver(Receiver, filter);
            IsRegisteredSensor = false;

            BcoreManager = new BcoreManager(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            UnregisterReceiver(Receiver);

            StopMeasSensor();

            if (BcoreManager.IsConnected) BcoreManager.Disconnect();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var bcore = intent.GetStringExtra(ExtraKeyBcore);

            if (string.IsNullOrEmpty(bcore)) return StartCommandResult.NotSticky;

            Android.Util.Log.Debug("ControllerService", "Conntect:" + bcore);

            if (BcoreManager?.IsConnected ?? false) BcoreManager.Disconnect();

            if (BcoreManager == null) BcoreManager = new BcoreManager(this);

            BcoreManager.BcoreConnectionChanged += (s, e) =>
            {
                if (e.IsConnected) return;

                State = EState.Stop;
                StopMeasSensor();
                StopSelf();
            };

            BcoreManager.Connect(bcore);

            StartMeasSensor();

            return base.OnStartCommand(intent, flags, startId);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (!BcoreManager.IsInitialized) return;

            var accz = e.Values[2];

            //Android.Util.Log.Debug("ControllerService", "acc:{0}", accz);

            var state = EState.Stop;

            if (accz > 6.0) state = EState.Left;
            else if (accz < -6.0) state = EState.Right;

            if (state == State) return;

            var speed = 0x80;

            switch (state)
            {
                case EState.Left:
                    speed = 255;
                    break;
                case EState.Right:
                    speed = 0;
                    break;
            }

            BcoreManager.SetMotorPwm(0, speed);
            State = state;
        }

        private void OnActionStart(string bcore)
        {
            StartMeasSensor();

            BcoreManager.Connect(bcore);
        }

        private void OnActionStop()
        {
            StopMeasSensor();

            if (BcoreManager?.IsConnected ?? false) BcoreManager.Disconnect();
        }

        private void StartMeasSensor()
        {
            if (IsRegisteredSensor) return;

            var manager = SensorManager;
            manager.RegisterListener(this, manager.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Ui);
            IsRegisteredSensor = true;
        }

        private void StopMeasSensor()
        {
            if (!IsRegisteredSensor) return;

            SensorManager.UnregisterListener(this);
            IsRegisteredSensor = false;
        }

    }
}