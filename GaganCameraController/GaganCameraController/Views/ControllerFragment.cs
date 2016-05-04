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
using GaganCameraController.Model;

namespace GaganCameraController.Views
{
    public class ControllerFragment : Fragment
    {
        public static ControllerFragment NewInstance()
        {
            var fragment = new ControllerFragment();
            return fragment;
        }

        public GaganController Controller { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            var view = inflater.Inflate(Resource.Layout.Controller, container, false);
            var controllerView = view.FindViewById<ControllerView>(Resource.Id.ControllerView);
            controllerView.Controller = Controller;
            return view;
        }
    }
}