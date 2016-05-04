using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using LibBcore;

namespace GaganCameraController.Model
{
    public class GaganController
    {
        #region const

        private const long TimerIntervalRun = 500;
        private const long TimerIntervalStop = 50;

        private enum EState
        {
            None,
            Right,
            RStop,
            Left,
            LStop,
        }

        #endregion


        #region property

        public bool IsConnected => BcoreManager?.IsConnected ?? false;

        private Context Context { get; }
        private BcoreManager BcoreManager { get; }
        private GaganPreference Pref { get; }
        private bool IsTimerRunning { get; set; }
        private Handler TimerHandler { get; set; }
        private EState State { get; set; }

        #endregion

        #region event

        public event EventHandler<bool> BcoreConnectionChanged; 

        #endregion

        #region constructor

        public GaganController(Context context)
        {
            Context = context;
            Pref = GaganPreference.Load(context);
            BcoreManager = new BcoreManager(Context);
            BcoreManager.BcoreConnectionChanged += (s, e) =>
            {
                if (e.IsConnected)
                {
                    IsTimerRunning = false;
                    TimerHandler = new Handler(Looper.MainLooper);
                }
                else
                {
                    IsTimerRunning = false;
                    TimerHandler.RemoveCallbacks(OnTimer);
                }

                BcoreConnectionChanged?.Invoke(this, e.IsConnected);
            };
        }

        #endregion

        #region public method

        public void ConnectBcore(string addr)
        {
            BcoreManager.Connect(addr);
        }

        public void DisconnectBcore()
        {
            BcoreManager.Disconnect();
        }

        public void Stop(bool isStreight = false)
        {
            if (!isStreight)
                CancelTimer();

            if (State == EState.Left) State = EState.LStop;
            else if(State == EState.Right) State = EState.RStop;

            BcoreManager.SetMotorPwm(0, Bcore.StopMotorPwm);
        }

        public void Left(bool isStreight = false)
        {
            if (!isStreight) CancelTimer();
            State = EState.Left;
            BcoreManager.SetMotorPwm(0, Bcore.MinMotorPwm);
            BcoreManager.SetServoPos(0, Pref.ServoPosLeft);
        }

        public void Right(bool isStreight = false)
        {
            if (!isStreight) CancelTimer();
            State = EState.Right;
            BcoreManager.SetMotorPwm(0, Bcore.MaxMotorPwm);
            BcoreManager.SetServoPos(0, Pref.ServoPosRight);
        }

        public void Streight()
        {
            IsTimerRunning = true;
            OnTimer();
        }

        public void SetCamLeftPos(int value)
        {
            Pref.ServoPosLeft = value;
            Pref.Save();
            BcoreManager.SetServoPos(0, value);
        }

        public void SetCamRightPos(int value)
        {
            Pref.ServoPosRight = value;
            Pref.Save();
            BcoreManager.SetServoPos(0, value);
        }

        #endregion

        #region private method

        private void OnTimer()
        {
            if (!IsTimerRunning) return;

            var interval = TimerIntervalRun;

            switch (State)
            {
                case EState.Left:
                case EState.Right:
                    Stop(true);
                    interval = TimerIntervalStop;
                    break;
                case EState.LStop:
                    Right(true);
                    break;
                default:
                    Left(true);
                    break;
            }

            TimerHandler.PostDelayed(OnTimer, interval);
        }

        private void CancelTimer()
        {
            IsTimerRunning = false;
            TimerHandler.RemoveCallbacks(OnTimer);
        }

        #endregion
    }
}