using System;
using Android.Content;
using Android.Database.Sqlite;
using Android.Graphics;
using Android.Net.Wifi.P2p;
using Android.Runtime;
using Android.Util;
using Android.Views;
using GaganCameraController.Model;

namespace GaganCameraController.Views
{
    class ControllerView : Android.Views.View
    {
        private static readonly Paint BorderPaint = new Paint() {Color = Color.LightGray, StrokeWidth = 5};
        private static readonly Paint InactiveAreaPaint = new Paint() {Color = Color.Black};
        private static readonly Paint InactiveArrowPaint = new Paint() {Color = Color.LightGray, StrokeWidth = 5};
        private static readonly Paint ActiveAreaPaint = new Paint() {Color = Color.White};
        private static readonly Paint ActiveArrowPaint = new Paint() {Color = Color.Blue};
        private EState _state;

        private enum EState
        {
            None,
            Fore,
            Left,
            Rignt,
        }

        private float ActualWidth { get; set; }
        private float ActualHeight { get; set; }

        private Path PathArrowUp { get; } = new Path();
        private Path PathArrowLeft { get; } = new Path();
        private Path PathArrowRight { get; } = new Path();
        private Path PathBorderUp { get; } = new Path();
        private Path PathBorderLeft { get; } = new Path();
        private Path PathBorderRight { get; } = new Path();

        public GaganController Controller { get; set; }

        private EState State
        {
            get { return _state; }
            set
            {
                if (_state == value) return;
                _state = value;
                SendCommand();
                Invalidate();
            }
        }

        static ControllerView()
        {
            BorderPaint.SetStyle(Paint.Style.Stroke);
            InactiveArrowPaint.SetStyle(Paint.Style.Stroke);
        }

        public ControllerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public ControllerView(Context context) : base(context)
        {
        }

        public ControllerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public ControllerView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public ControllerView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            ActualWidth = w;
            ActualHeight = h;

            PathArrowUp.MoveTo((float)(w / 2.0), 4);
            PathArrowUp.LineTo((float)(w * 3 / 8.0), (float)(h / 5.0));
            PathArrowUp.LineTo((float)(w * 7 / 16.0), (float)(h / 5.0));
            PathArrowUp.LineTo((float)(w * 7 / 16.0), (float)(h / 2.0-2));
            PathArrowUp.LineTo((float)(w * 9 / 16.0), (float)(h / 2.0-2));
            PathArrowUp.LineTo((float)(w * 9 / 16.0), (float)(h / 5.0));
            PathArrowUp.LineTo((float)(w * 5 / 8.0), (float)(h / 5.0));
            PathArrowUp.LineTo((float)(w / 2.0), 4);

            PathArrowRight.MoveTo((float)(w / 2.0 + 2), (float)(h *11 / 16.0));
            PathArrowRight.LineTo((float)(w / 2.0 + 2), (float)(h * 13 / 16.0));
            PathArrowRight.LineTo((float)(w * 4 / 5.0), (float)(h * 13 / 16.0));
            PathArrowRight.LineTo((float)(w * 4 / 5.0), (float)(h * 7 / 8.0));
            PathArrowRight.LineTo((float)(w - 2), (float)(h * 3 / 4.0));
            PathArrowRight.LineTo((float)(w * 4 / 5.0), (float)(h * 5 / 8.0));
            PathArrowRight.LineTo((float)(w * 4 / 5.0), (float)(h * 11/ 16.0));
            PathArrowRight.LineTo((float)(w / 2.0 + 2), (float)(h * 11 / 16.0));

            PathArrowLeft.MoveTo((float)(w / 2.0 - 2), (float)(h * 11 / 16.0));
            PathArrowLeft.LineTo((float)(w / 2.0 - 2), (float)(h * 13 / 16.0));
            PathArrowLeft.LineTo((float)(w / 5.0), (float)(h * 13 / 16.0));
            PathArrowLeft.LineTo((float)(w / 5.0), (float)(h * 7 / 8.0));
            PathArrowLeft.LineTo((float)(2), (float)(h * 3 / 4.0));
            PathArrowLeft.LineTo((float)(w / 5.0), (float)(h * 5/ 8.0));
            PathArrowLeft.LineTo((float)(w / 5.0), (float)(h * 11 / 16.0));
            PathArrowLeft.LineTo((float)(w / 2.0 - 2), (float)(h * 11 / 16.0));

            PathBorderUp.MoveTo((float)(w/4.0), 2);
            PathBorderUp.LineTo((float)(w/4.0), (float)(h/2.0));
            PathBorderUp.LineTo((float)(w*3/4.0), (float)(h/2.0));
            PathBorderUp.LineTo((float)(w*3/4.0), 2);
            PathBorderUp.LineTo((float)(w/4.0),2);

            PathBorderLeft.MoveTo(2,(float)(h/2.0));
            PathBorderLeft.LineTo((float)(w/2.0), (float)(h/2.0));
            PathBorderLeft.LineTo((float)(w/2.0), (float)(h-2));
            PathBorderLeft.LineTo(2, h-2);
            PathBorderLeft.LineTo(2,(float)(h/2.0));

            PathBorderRight.MoveTo((float)(w/2.0), (float)(h/2.0));
            PathBorderRight.LineTo((float)w-2, (float)(h/2.0));
            PathBorderRight.LineTo(w-2, h-2);
            PathBorderRight.LineTo((float)(w/2.0), h-2);
            PathBorderRight.LineTo((float)(w/2.0), (float)(h/2.0));
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (State == EState.Fore)
            {
                canvas.DrawPath(PathBorderUp, ActiveAreaPaint);
                canvas.DrawPath(PathArrowUp, ActiveArrowPaint);
            }
            else
            {
                canvas.DrawPath(PathBorderUp, InactiveAreaPaint);
                canvas.DrawPath(PathArrowUp, InactiveArrowPaint);
            }

            if (State == EState.Left)
            {
                canvas.DrawPath(PathBorderLeft, ActiveAreaPaint);
                canvas.DrawPath(PathArrowLeft, ActiveArrowPaint);
            }
            else
            {
                canvas.DrawPath(PathBorderLeft, InactiveAreaPaint);
                canvas.DrawPath(PathArrowLeft, InactiveArrowPaint);
            }

            if (State == EState.Rignt)
            {
                canvas.DrawPath(PathBorderRight, ActiveAreaPaint);
                canvas.DrawPath(PathArrowRight, ActiveArrowPaint);
            }
            else
            {
                canvas.DrawPath(PathBorderRight, InactiveAreaPaint);
                canvas.DrawPath(PathArrowRight, InactiveArrowPaint);
            }

            canvas.DrawPath(PathBorderUp, BorderPaint);
            canvas.DrawPath(PathBorderLeft, BorderPaint);
            canvas.DrawPath(PathBorderRight, BorderPaint);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            var nextState = EState.None;

            if (e.Action == MotionEventActions.Down || e.Action == MotionEventActions.Move)
            {
                var x = e.GetX(0);
                var y = e.GetY(0);

                if (y < ActualHeight/2)
                {
                    if (ActualWidth/4 < x && x < ActualWidth*3/4)
                    {
                        nextState = EState.Fore;
                    }
                    else
                    {
                        nextState = EState.None;
                    }
                }
                else if (x < ActualWidth/2)
                {
                    nextState = EState.Left;
                }
                else
                {
                    nextState = EState.Rignt;
                }
            }

            if (nextState != State)
            {
                State = nextState;
                Invalidate();
            }
            return true;
        }

        private void SendCommand()
        {
            if (!(Controller?.IsConnected ?? false)) return;

            switch (State)
            {
                case EState.Fore:
                    Controller.Streight();
                    break;
                case EState.Left:
                    Controller.Left();
                    break;
                case EState.Rignt:
                    Controller.Right();
                    break;
                default:
                    Controller.Stop();
                    break;
            }
        }
    }
}