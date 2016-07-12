using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace GaganCameraController.Views
{
    class ControllerArrowView : View
    {
        private static readonly Paint BorderPaint = new Paint() { Color = Color.LightGray, StrokeWidth = 5 };
        private static readonly Paint InactiveAreaPaint = new Paint() { Color = Color.Black };
        private static readonly Paint InactiveArrowPaint = new Paint() { Color = Color.LightGray, StrokeWidth = 5 };
        private static readonly Paint ActiveAreaPaint = new Paint() { Color = Color.White };
        private static readonly Paint ActiveArrowPaint = new Paint() { Color = Color.Blue };

        public enum EControlType
        {
            Left = 0,
            Right,
        }

        private bool _isActivie;

        public EControlType ArrowType { get; set; }

        private float ActualWidth { get; set; }
        private float ActualHeight { get; set; }

        private Path PathArrow { get; } = new Path();
        private Path PathBorder { get; } = new Path();

        public bool IsActive
        {
            get { return _isActivie; }
            set { _isActivie = value; }
        }

        public event EventHandler<EControlType> TouchController;

        public ControllerArrowView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public ControllerArrowView(Context context) : base(context)
        {
        }

        public ControllerArrowView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            InitAttrs(attrs);
        }

        public ControllerArrowView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            InitAttrs(attrs);
        }

        public ControllerArrowView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            InitAttrs(attrs);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            ActualWidth = w;
            ActualHeight = h;

            if (ArrowType == EControlType.Right)
            {
                PathArrow.MoveTo((float) (2.0), (float) (h*2/5.0));
                PathArrow.LineTo((float) (2.0), (float) (h*3/5.0));
                PathArrow.LineTo((float) (w*3/5.0), (float) (h*3/5.0));
                PathArrow.LineTo((float) (w*3/5.0), (float) (h*7/10.0));
                PathArrow.LineTo((float) (w - 2), (float) (h/2.0));
                PathArrow.LineTo((float) (w*3/5.0), (float) (h*3/10.0));
                PathArrow.LineTo((float) (w*3/5.0), (float) (h*2/5.0));
                PathArrow.LineTo((float) (2.0), (float) (h*2/5.0));
            }
            else
            {
                PathArrow.MoveTo((float) (w - 2), (float) (h*2/5.0));
                PathArrow.LineTo((float) (w - 2), (float) (h*3/5.0));
                PathArrow.LineTo((float) (w*2/5.0), (float) (h*3/5.0));
                PathArrow.LineTo((float) (w*2/5.0), (float) (h*7/10.0));
                PathArrow.LineTo((float) (2.0), (float) (h/2.0));
                PathArrow.LineTo((float) (w*2/5.0), (float) (h*3/10.0));
                PathArrow.LineTo((float) (w*2/5.0), (float) (h*2/5.0));
                PathArrow.LineTo((float) (w/2.0 - 2), (float) (h*2/5.0));
            }

            PathBorder.MoveTo(2, (float)(2.0));
            PathBorder.LineTo((float)(w - 2.0), (float)(2.0));
            PathBorder.LineTo((float)(w - 2.0), (float)(h - 2));
            PathBorder.LineTo(2, h - 2);
            PathBorder.LineTo(2, (float)(2.0));
        }

        private void InitAttrs(IAttributeSet attrs)
        {
            var a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.ControllerArrowView);
            var type = a.GetInt(Resource.Styleable.ControllerArrowView_type, 0);
            if (type < 0) type = 0;
            else if (type > 1) type = 1;

            ArrowType = (EControlType) type;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (IsActive)
            {
                canvas.DrawPath(PathBorder, ActiveAreaPaint);
                canvas.DrawPath(PathArrow, ActiveArrowPaint);
            }
            else
            {
                canvas.DrawPath(PathBorder, InactiveAreaPaint);
                canvas.DrawPath(PathArrow, InactiveArrowPaint);
            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    IsActive = true;
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                case MotionEventActions.HoverExit:
                case MotionEventActions.Outside:
                case MotionEventActions.PointerUp:
                    IsActive = false;
                    break;
            }

            Invalidate();

            return true;
        }
    }
}