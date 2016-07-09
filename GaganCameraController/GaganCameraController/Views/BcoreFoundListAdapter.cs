using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using LibBcore;
using Object = Java.Lang.Object;

namespace GaganCameraController.Views
{
    class BcoreFoundListAdapter : BaseAdapter<BcoreDeviceInfo>
    {
        private IList<BcoreDeviceInfo> Items { get; }
        private LayoutInflater Inflater { get; }

        public override int Count => Items?.Count ?? 0;

        public override BcoreDeviceInfo this[int position]
        {
            get
            {
                if (Items == null || position < 0 || Items.Count <= position) return null;
                return Items[position];
            }
        }

        public BcoreFoundListAdapter(Context context, IList<BcoreDeviceInfo> items)
        {
            Inflater = LayoutInflater.From(context);
            Items = items;
        }

        public override Object GetItem(int position)
        {
            if (Items == null || position < 0 || Items.Count <= position) return null;
            return Items[position];
        }

        public override long GetItemId(int position)
        {
            if (Items == null || position < 0 || Items.Count <= position) return -1;
            return position;
        }

        public override Android.Views.View GetView(int position, Android.Views.View convertView, ViewGroup parent)
        {
            var view = convertView ?? Inflater.Inflate(Android.Resource.Layout.SimpleListItemActivated2, parent, false);

            var item = GetItem(position) as BcoreDeviceInfo;

            if (item == null) return convertView;
            
            var textName = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            textName.Text = item.Name;

            var textAddr = view.FindViewById<TextView>(Android.Resource.Id.Text2);
            textAddr.Text = item.Addr;

            return view;
        }
    }
}