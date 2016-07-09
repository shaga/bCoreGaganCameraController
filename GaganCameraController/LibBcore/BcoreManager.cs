using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace LibBcore
{
    public class BcoreConnectionChangedEventArgs : EventArgs
    {
        public bool IsConnected { get; }

        public BcoreConnectionChangedEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }

    public class ReadBatteryVoltageEventArgs : EventArgs
    {
        public int BatteryVoltage { get; }

        public ReadBatteryVoltageEventArgs(byte[] value)
        {
            if (value == null || value.Length != 2) return;
            BatteryVoltage = value[0] | (value[1] << 8);
        }
    }

    public class ReadFunctionsEventArgs : EventArgs
    {
        private const int FunctionCount = 4;
        private const int IdxIndex = 0;
        private const int IdxServoPortout = 1;
        private const int BitMotor = 0;
        private const int BitServo = 0;
        private const int BitPortOut = 4;

        public bool[] IsEnableMotor { get; } = new bool[FunctionCount];
        public bool[] IsEnableServo { get; } = new bool[FunctionCount];
        public bool[] IsEnablePortOut { get; } = new bool[FunctionCount];

        public ReadFunctionsEventArgs(byte[] value)
        {
            if (value == null || value.Length != 2) return;

            for (var i = 0; i < FunctionCount; i++)
            {
                IsEnableMotor[i] = ((value[IdxIndex] >> (BitMotor + i)) & 0x01) == 0x01;
                IsEnableServo[i] = ((value[IdxServoPortout]) >> (BitServo + i) & 0x01) == 0x01;
                IsEnablePortOut[i] = ((value[IdxServoPortout] >> (BitPortOut + i)) & 0x01) == 0x01;
            }
        }
    }

    public class BcoreManager : BluetoothGattCallback
    {

        #region property

        public bool IsConnected { get; private set; }

        public bool IsInitialized { get; private set; }

        private BluetoothManager BluetoothManager { get;}
        private BluetoothAdapter BluetoothAdapter => BluetoothManager.Adapter;

        private Context Context { get; }
        private BluetoothDevice Device { get; set; }
        private BluetoothGatt DeviceGatt { get; set; }
        private BluetoothGattService BcoreService { get; set; }
        private IDictionary<string, BluetoothGattCharacteristic> BcoreCharacteristics { get; } = new Dictionary<string, BluetoothGattCharacteristic>();

        private SemaphoreSlim ReadSemaphore { get; } = new SemaphoreSlim(1, 1);
        private SemaphoreSlim WriteSemaphore { get; } = new SemaphoreSlim(1, 1);

        #endregion

        #region event

        public event EventHandler<BcoreConnectionChangedEventArgs> BcoreConnectionChanged;
        public event EventHandler BcoreServiceDiscovered;
        public event EventHandler<ReadBatteryVoltageEventArgs> ReadBatteryResult;
        public event EventHandler<ReadFunctionsEventArgs> ReadFunctionResult;

        #endregion

        #region constructor & destrutor

        public BcoreManager(Context context)
        {
            Context = context;
            BluetoothManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            
        }

        ~BcoreManager()
        {
            if(!IsConnected) return;

            DeviceGatt?.Disconnect();

            DeviceGatt?.Close();
        }

        #endregion

        #region override BluetoothGattCallback

        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);

            switch (newState)
            {
                case ProfileState.Connected:
                    IsConnected = true;
                    gatt.DiscoverServices();
                    BcoreConnectionChanged?.Invoke(this, new BcoreConnectionChangedEventArgs(true));
                    break;
                case ProfileState.Connecting:
                    break;
                case ProfileState.Disconnecting:
                    IsInitialized = false;
                    IsConnected = false;
                    break;
                case ProfileState.Disconnected:
                    BcoreConnectionChanged?.Invoke(this, new BcoreConnectionChangedEventArgs(false));
                    IsConnected = false;
                    IsInitialized = false;
                    break;
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);

            var service = gatt.Services.FirstOrDefault(s => s.Uuid.ToString().ToLower() == BcoreUuid.BcoreService.ToString().ToLower());

            if (service == null) return;

            BcoreService = service;

            BcoreCharacteristics.Clear();

            foreach (var characteristic in BcoreService.Characteristics)
            {
                BcoreCharacteristics.Add(characteristic.Uuid.ToString().ToLower(), characteristic);
            }

            IsInitialized = true;
            BcoreServiceDiscovered?.Invoke(this, EventArgs.Empty);
        }
    
        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);

            ReadSemaphore.Release();

            if (characteristic.Uuid == BcoreUuid.BatteryVol)
            {
                ReadBatteryResult?.Invoke(this, new ReadBatteryVoltageEventArgs(characteristic.GetValue()));
            }
            else if (characteristic.Uuid == BcoreUuid.GetFunctions)
            {
                ReadFunctionResult?.Invoke(this, new ReadFunctionsEventArgs(characteristic.GetValue()));
            }
        }

        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            base.OnCharacteristicWrite(gatt, characteristic, status);

            WriteSemaphore.Release();
        }

        #endregion

        #region public method

        public void Connect(string addr)
        {
            Device = BluetoothAdapter.GetRemoteDevice(addr);

            if (Device == null) return;

            DeviceGatt = Device.ConnectGatt(Context, false, this);
        }

        public void Disconnect()
        {
            DeviceGatt?.Disconnect();
            DeviceGatt?.Close();
        }

        public async void SetMotorPwm(int idx, int speed)
        {
            if (idx < 0 || 2 <= idx) return;

            if (speed < 0) speed = 0;
            else if (speed > 255) speed = 255;

            var value = new [] {(byte) idx, (byte) speed};

            await WriteCharacteristic(BcoreUuid.MotorPwm, value);
        }

        public async void SetServoPos(int idx, int pos)
        {
            if (idx < 0 || 4 <= idx) return;

            if (pos < 0) pos = 0;
            else if (pos > 255) pos = 255;

            var value = new[] {(byte) idx, (byte) pos};

            await WriteCharacteristic(BcoreUuid.ServoPos, value);
        }

        public async void SetPortout(bool isOn)
        {
            var value = new[] {(byte) (isOn ? 1 : 0)};

            await WriteCharacteristic(BcoreUuid.PortOut, value);
        }

        public async void GetBatteryVoltage()
        {
            await ReadCharacteristic(BcoreUuid.BatteryVol);
        }

        public async void GetFunctions()
        {
            await ReadCharacteristic(BcoreUuid.GetFunctions);
        }

        #endregion

        #region private method

        private async Task ReadCharacteristic(UUID uuid)
        {
            if (!BcoreCharacteristics.ContainsKey(uuid.ToString().ToLower())) return;

            var characteristic = BcoreCharacteristics[uuid.ToString().ToLower()];

            if (!characteristic.Properties.HasFlag(GattProperty.Read)) return;

            await ReadSemaphore.WaitAsync();

            DeviceGatt.ReadCharacteristic(characteristic);
        }

        private async Task WriteCharacteristic(UUID uuid, byte[] value)
        {
            if (!BcoreCharacteristics.ContainsKey(uuid.ToString().ToLower())) return;

            var characteristic = BcoreCharacteristics[uuid.ToString().ToLower()];

            if (!characteristic.Properties.HasFlag(GattProperty.WriteNoResponse)) return;

            await WriteSemaphore.WaitAsync();

            characteristic.SetValue(value);

            DeviceGatt.WriteCharacteristic(characteristic);
        }

        #endregion
    }
}