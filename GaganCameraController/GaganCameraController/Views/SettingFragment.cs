using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using GaganCameraController.Model;

namespace GaganCameraController.Views
{
    public class SettingFragment : Fragment
    {
        public static SettingFragment newInstance()
        {
            var fragment = new SettingFragment();
            var args = new Bundle();
            fragment.Arguments = args;
            return fragment;
        }

        private GaganPreference Preference { get; set; }
        public GaganController Controller { get; set; }

        private Button BtnSetCamUrl { get; set; }
        private Button BtnSetServoLeft { get; set; }
        private Button BtnSetServoRight { get; set; }

        private TextView TextCamUrl { get; set; }
        private SeekBar SeekServoLeft { get; set; }
        private SeekBar SeekServoRight { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            Preference = GaganPreference.Load(Activity);

            var view = inflater.Inflate(Resource.Layout.Setting, container, false);

            TextCamUrl = view.FindViewById<TextView>(Resource.Id.EditCamUrl);
            TextCamUrl.Text = Preference.CamUrl;
            BtnSetCamUrl = view.FindViewById<Button>(Resource.Id.BtnSetCamUrl);
            BtnSetCamUrl.Click += (s, e) =>
            {
                var activity = Activity as ControllerActivity;
                Preference.CamUrl = TextCamUrl.Text;
                activity?.BeginStream();
            };

            SeekServoLeft = view.FindViewById<SeekBar>(Resource.Id.SeekLeftCamPos);
            SeekServoLeft.Progress = Preference.ServoPosLeft;
            BtnSetServoLeft = view.FindViewById<Button>(Resource.Id.BtnSetServoLeft);
            BtnSetServoLeft.Click += (s, e) => Controller?.SetCamLeftPos(SeekServoLeft.Progress);

            SeekServoRight = view.FindViewById<SeekBar>(Resource.Id.SeekRightCamPos);
            SeekServoRight.Progress = Preference.ServoPosRight;
            BtnSetServoRight = view.FindViewById<Button>(Resource.Id.BtnSetServoRight);
            BtnSetServoRight.Click += (s, e) => Controller?.SetCamRightPos(SeekServoRight.Progress);

            return view;
        }

    }
}