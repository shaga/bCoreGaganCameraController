using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Object = Java.Lang.Object;

namespace LibBcore
{
    public class BcoreDeviceInfo : Java.Lang.Object
    {
        public string Name { get; }
        public string Addr { get; }

        public BcoreDeviceInfo(string name, string addr)
        {
            Name = name;
            Addr = addr;
        }

        public override bool Equals(Object o)
        {
            var info = o as BcoreDeviceInfo;

            return info != null && info.Addr == Addr;
        }
    }

    public enum EBcoreStatus
    {
        Disconnected = 0,
        Connecting,
        Connected,
        DiscoveredService,
        Disconnecting,
    }

    public class BcoreFunctionInfo
    {
        public bool[] IsEnableMotor { get; }
        public bool[] IsEnableServo { get; }
        public bool[] IsEnablePortOut { get; }


    }

    public static class Bcore
    {
        public const int MaxFunctionCount = 4;

        public const int MinMotorPwm = 0;
        public const int MaxMotorPwm = 0xff;
        public const int StopMotorPwm = 0x80;

        public const int MinServoPos = 0;
        public const int MaxServoPos = 0xff;
        public const int CenterServoPos = 0x80;

        private const int IdxBatteryVoltageDataLow = 0;
        private const int IdxBatteryVoltageDataHigh = 1;
        private const int BatteryVoltageDataLength = 2;

        public static int GetBatteryVoltage(this BluetoothGattCharacteristic characteristic)
        {
            if (characteristic == null) return -1;
            if (characteristic.Uuid != BcoreUuid.BatteryVol) return -1;

            var value = characteristic.GetValue();

            if (value == null || value.Length != BatteryVoltageDataLength) return -1;

            return (value[IdxBatteryVoltageDataLow] & 0xff) | (value[IdxBatteryVoltageDataHigh] << 8);
        }

        //public static 
    }
}