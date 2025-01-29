using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SPICA.WinForms.GUI
{
    class SUIVScroll : Control
    {
        //Private fields
        private Color CurrBarColor;

        private int Scroll;
        private bool MouseDrag;

        private int Max;
        private int ScrollY;
        private int BarY;
        private int BarHeight;

        private Color _BarColor = Color.White;
        private Color _BarColorHover = Color.Gray;

        //Public properties
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string Text { get; set; }

        [Category("外观"), Description("正常条形颜色.")]
        public Color BarColor
        {
            get
            {
                return _BarColor;
            }
            set
            {
                _BarColor = value;

                Invalidate();
            }
        }

        [Category("外观"), Description("鼠标悬停时的条形颜色.")]
        public Color BarColorHover
        {
            get
            {
                return _BarColorHover;
            }
            set
            {
                _BarColorHover = value;

                Invalidate();
            }
        }

        [Category("行为学"), Description("条可以具有的最大滚动值.")]
        public int Maximum
        {
            get
            {
                return Max;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(MaxTooLowEx);
                }

                Max = value;

                if (ScrollY > Max)
                    Value = Max; 
                else
                    CalculateBarHeight();
            }
        }

        [Category("行为学"), Description("条的当前滚动值（位置）.")]
        public int Value
        {
            get
            {
                return ScrollY;
            }
            set
            {
                if (value < 0 || value > Max)
                {
                    throw new ArgumentOutOfRangeException(string.Format(ValueOutOfRangeEx, Max));
                }

                ScrollY = value;

                ScrollChanged?.Invoke(this, EventArgs.Empty);

                CalculateBarHeight();
            }
        }

        //Public events
        public event EventHandler ScrollChanged;

        //Integer constants
        private const int MinBarHeight = 32;

        //String constants (Exception messages)
        private const string MaxTooLowEx = "最大值无效!期望一个值>= 0!";
        private const string ValueOutOfRangeEx = "无效值!预期 >= 0 and <= {0}!";

        public SUIVScroll()
        {
            SetStyle(
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor, true);

            CurrBarColor = BarColor;

            Max = 100;

            BarHeight = MinBarHeight;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(CurrBarColor), new Rectangle(0, BarY, Width, BarHeight));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Rectangle Rect = new Rectangle(0, BarY, Width, BarHeight);

                if (Rect.Contains(e.Location))
                {
                    Scroll = e.Y - BarY;
                    MouseDrag = true;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            Focus();

            if (e.Button == MouseButtons.Left) MouseDrag = false;

            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Rectangle Rect = new Rectangle(0, BarY, Width, BarHeight);


            if (Rect.Contains(e.Location))
            {
                if (CurrBarColor != BarColorHover)
                {
                    CurrBarColor = BarColorHover;

                    Invalidate();
                }
            }
            else if (!MouseDrag)
            {
                if (CurrBarColor != BarColor)
                {
                    CurrBarColor = BarColor;

                    Invalidate();
                }
            }

            if (e.Button == MouseButtons.Left && MouseDrag)
            {
                BarY = Math.Max(Math.Min(e.Y - Scroll, Height - BarHeight), 0);

                ScrollY = (int)(((float)BarY / Math.Max(Height - BarHeight, 1)) * Max);

                ScrollChanged?.Invoke(this, EventArgs.Empty);

                Invalidate();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!MouseDrag) CurrBarColor = BarColor;

            Invalidate();

            base.OnMouseLeave(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            CalculateBarHeight();

            base.OnLayout(levent);
        }

        private void CalculateBarHeight()
        {
            BarHeight = Math.Max(Height - Max, MinBarHeight);

            if (Max > 0)
                BarY = (int)(((float)ScrollY / Max) * (Height - BarHeight));
            else
                BarY = 0;

            Invalidate();
        }
    }
}
