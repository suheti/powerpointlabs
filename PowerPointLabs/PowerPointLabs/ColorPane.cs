﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using PowerPointLabs.Models;
using System.Drawing.Drawing2D;
using PPExtraEventHelper;

namespace PowerPointLabs
{
    public partial class ColorPane : UserControl
    {
        private Color _selectedColor;

        LMouseListener _native;

        PowerPoint.ShapeRange _selectedShapes;
        ColorDataSource dataSource = new ColorDataSource();
        public ColorPane()
        {
            InitializeComponent();
            this.panel1.DataBindings.Add(new Binding(
                "BackColor", 
                dataSource, 
                "selectedColor", 
                false, 
                DataSourceUpdateMode.OnPropertyChanged));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Start();

            _native = new LMouseListener();
            _native.LButtonClicked +=
                 new EventHandler<SysMouseEventInfo>(_native_LButtonClicked);
            //DisableMouseClicks();

            SelectShapes();
        }

        private void SelectShapes()
        {
            try
            {
                _selectedShapes = Globals.ThisAddIn.Application.ActiveWindow.Selection.ShapeRange;
            }
            catch (Exception exception)
            {
                _selectedShapes = null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panel1.BackColor;
            colorDialog1.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Point p = Control.MousePosition;
            IntPtr dc = Native.GetDC(IntPtr.Zero);
            dataSource.selectedColor = ColorTranslator.FromWin32(Native.GetPixel(dc, p.X, p.Y));
            UpdatePanelsForColor(panel1.BackColor);
            ColorSelectedShapesWithColor(panel1.BackColor);
        }

        private void ColorSelectedShapesWithColor(Color selectedColor)
        {
            SelectShapes();
            if (_selectedShapes != null)
            {
                foreach (PowerPoint.Shape s in _selectedShapes)
                {
                    var r = selectedColor.R;
                    var g = selectedColor.G;
                    var b = selectedColor.B;

                    var rgb = (b << 16) | (g << 8) | (r);
                    s.Fill.ForeColor.RGB = rgb;
                }
            }
        }

        private void MatchingColorPanel_DoubleClick(object sender, EventArgs e)
        {
            Color selectedColor = ((Panel)sender).BackColor;

            UpdatePanelsForColor(selectedColor);
            ColorSelectedShapesWithColor(selectedColor);
        }

        void _native_LButtonClicked(object sender, SysMouseEventInfo e)
        {
            _native.Close();
            UpdateUIForNewColor();
            timer1.Stop();
            
            //EnableMouseClicks();
        }

        private void UpdateUIForNewColor()
        {
            _selectedColor = panel1.BackColor;
            brightnessBar.Value = (int)(_selectedColor.GetBrightness() * 240.0f);
            saturationBar.Value = (int)(_selectedColor.GetSaturation() * 240.0f);
            UpdatePanelsForColor(_selectedColor);
            ColorSelectedShapesWithColor(_selectedColor);
            DrawSaturationGradient();
            DrawBrightnessGradient();
        }

        protected override void OnPaint(PaintEventArgs paintEvnt)
        {
            DrawBrightnessGradient();
            DrawSaturationGradient();
        }

        private void DrawBrightnessGradient()
        {
            var dis = brightnessPanel.DisplayRectangle;
            var screenRec = brightnessPanel.RectangleToScreen(dis);
            var rec = brightnessPanel.Parent.RectangleToClient(screenRec);
            brightnessPanel.Visible = false;
            LinearGradientBrush brush = new LinearGradientBrush(
                rec,
                ColorHelper.ColorFromAhsb(
                255,
                _selectedColor.GetHue(),
                _selectedColor.GetSaturation(),
                0.00f),
                ColorHelper.ColorFromAhsb(
                255,
                _selectedColor.GetHue(),
                _selectedColor.GetSaturation(),
                1.0f),
                LinearGradientMode.Horizontal);
            ColorBlend blend = new ColorBlend();
            Color[] blendColors = {
                ColorHelper.ColorFromAhsb(
                255, 
                _selectedColor.GetHue(),
                _selectedColor.GetSaturation(),
                0.0f),
                _selectedColor,
                ColorHelper.ColorFromAhsb(
                255, 
                _selectedColor.GetHue(),
                _selectedColor.GetSaturation(),
                1.0f)};
            float[] positions = { 0.0f, 0.5f, 1.0f };
            blend.Colors = blendColors;
            blend.Positions = positions;

            brush.InterpolationColors = blend;

            using (Graphics g = this.CreateGraphics())
            {
                g.FillRectangle(brush, rec);
            }
        }

        private void DrawSaturationGradient()
        {
            var dis = saturationPanel.DisplayRectangle;
            var screenRec = saturationPanel.RectangleToScreen(dis);
            var rec = saturationPanel.Parent.RectangleToClient(screenRec);
            saturationPanel.Visible = false;
            LinearGradientBrush brush = new LinearGradientBrush(
                rec,
                ColorHelper.ColorFromAhsb(
                255,
                _selectedColor.GetHue(),
                0.0f,
                _selectedColor.GetBrightness()),
                ColorHelper.ColorFromAhsb(
                255,
                _selectedColor.GetHue(),
                1.0f,
                _selectedColor.GetBrightness()),
                LinearGradientMode.Horizontal);
            ColorBlend blend = new ColorBlend();
            Color[] blendColors = {
                ColorHelper.ColorFromAhsb(
                255, 
                _selectedColor.GetHue(),
                0.0f,
                _selectedColor.GetBrightness()),
                _selectedColor,
                ColorHelper.ColorFromAhsb(
                255, 
                _selectedColor.GetHue(),
                1.0f,
                _selectedColor.GetBrightness())};
            float[] positions = { 0.0f, 0.5f, 1.0f };
            blend.Colors = blendColors;
            blend.Positions = positions;

            brush.InterpolationColors = blend;

            using (Graphics g = this.CreateGraphics())
            {
                g.FillRectangle(brush, rec);
            }
        } 

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            UpdatePanelsForColor(panel1.BackColor);
        }

        private void UpdatePanelsForColor(Color selectedColor)
        {
            panel1.BackColor = selectedColor;
            Color complementaryColor = ColorHelper.GetComplementaryColor(selectedColor);

            List<Color> analogousColors = ColorHelper.GetAnalogousColorsForColor(selectedColor);
            AnalogousLighter.BackColor = analogousColors[0];
            AnalogousDarker.BackColor = analogousColors[1];
            AnalogousSelected.BackColor = selectedColor;

            List<Color> complementaryColors = ColorHelper.GetSplitComplementaryColorsForColor(selectedColor);
            ComplementaryLighter.BackColor = complementaryColors[0];
            ComplementaryDarker.BackColor = complementaryColors[1];
            ComplementarySelected.BackColor = complementaryColor;

            List<Color> triadicColors = ColorHelper.GetTriadicColorsForColor(selectedColor);
            TriadicLower.BackColor = triadicColors[0];
            TriadicHigher.BackColor = triadicColors[1];
            TriadicSelected.BackColor = selectedColor;

            List<Color> tetradicColors = ColorHelper.GetTetradicColorsForColor(selectedColor);
            Tetradic1.BackColor = tetradicColors[0];
            Tetradic2.BackColor = tetradicColors[1];
            Tetradic3.BackColor = tetradicColors[2];
            TetradicSelected.BackColor = selectedColor;
        }

        private void MatchingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            DataObject colorObject = new DataObject();
            colorObject.SetData(((Panel)sender).BackColor);
            DoDragDrop(colorObject, DragDropEffects.All);
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            panel1.BackColor = (Color)e.Data.GetData(panel1.BackColor.GetType());
            UpdateUIForNewColor();
        }

        private void DisableMouseClicks()
        {
            if (this.Filter == null)
            {
                this.Filter = new MouseClickMessageFilter();
                Application.AddMessageFilter(this.Filter);
            }
        }

        private void EnableMouseClicks()
        {
            if ((this.Filter != null))
            {
                Application.RemoveMessageFilter(this.Filter);
                this.Filter = null;
            }
        }

        private MouseClickMessageFilter Filter;

        private const int LButtonDown = 0x0201;

        public class MouseClickMessageFilter : IMessageFilter
        {

            public bool PreFilterMessage(ref System.Windows.Forms.Message m)
            {
                switch (m.Msg)
                {
                    case LButtonDown:
                        return true;
                }
                return false;
            }
        }

        private void brightnessBar_ValueChanged(object sender, EventArgs e)
        {
            if (!timer1.Enabled)
            {
                float newBrightness = brightnessBar.Value / 240.0f;
                Color newColor = new Color();
                try
                {
                    newColor = ColorHelper.ColorFromAhsb(
                    255,
                    _selectedColor.GetHue(),
                    _selectedColor.GetSaturation(),
                    newBrightness);
                } 
                catch(Exception exception)
                {
                }

                UpdatePanelsForColor(newColor);
                ColorSelectedShapesWithColor(newColor);
            }
        }

        private void saturationBar_ValueChanged(object sender, EventArgs e)
        {
            float newSaturation = saturationBar.Value / 240.0f;
            Color newColor = new Color();
            try
            {
                newColor = ColorHelper.ColorFromAhsb(
                255,
                _selectedColor.GetHue(),
                newSaturation,
                _selectedColor.GetBrightness());
            }
            catch (Exception exception)
            {
            }

            UpdatePanelsForColor(newColor);
            ColorSelectedShapesWithColor(newColor);
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(_selectedColor.GetType().ToString()))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }

    public class SysMouseEventInfo : EventArgs
    {
        public string WindowTitle { get; set; }
    }
    public class LMouseListener
    {
        public LMouseListener()
        {
            this.CallBack += new HookProc(MouseEvents);
            //Module mod = Assembly.GetExecutingAssembly().GetModules()[0];
            //IntPtr hMod = Marshal.GetHINSTANCE(mod);
            using (System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess())
            using (System.Diagnostics.ProcessModule module = process.MainModule)
            {
                IntPtr hModule = GetModuleHandle(module.ModuleName);
                _hook = SetWindowsHookEx(WH_MOUSE_LL, this.CallBack, hModule, 0);
                //if (_hook != IntPtr.Zero)
                //{
                //    Console.WriteLine("Started");
                //}
            }
        }
        int WH_MOUSE_LL = 14;
        int HC_ACTION = 0;
        HookProc CallBack = null;
        IntPtr _hook = IntPtr.Zero;

        public event EventHandler<SysMouseEventInfo> LButtonClicked;

        int MouseEvents(int code, IntPtr wParam, IntPtr lParam)
        {
            //Console.WriteLine("Called");

            if (code < 0)
                return CallNextHookEx(_hook, code, wParam, lParam);

            if (code == this.HC_ACTION)
            {
                // Left button pressed somewhere
                if (wParam.ToInt32() == (uint)WM.WM_LBUTTONUP)
                {
                    MSLLHOOKSTRUCT ms = new MSLLHOOKSTRUCT();
                    ms = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    IntPtr win = WindowFromPoint(ms.pt);
                    string title = GetWindowTextRaw(win);
                    if (LButtonClicked != null)
                    {
                        LButtonClicked(this, new SysMouseEventInfo { WindowTitle = title });
                    }
                }
            }
            return CallNextHookEx(_hook, code, wParam, lParam);
        }

        public void Close()
        {
            if (_hook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hook);
            }
        }
        public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowsHookEx", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(int xPoint, int yPoint);

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [Out] StringBuilder lParam);

        public static string GetWindowTextRaw(IntPtr hwnd)
        {
            // Allocate correct string length first
            //int length = (int)SendMessage(hwnd, (int)WM.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
            StringBuilder sb = new StringBuilder(65535);//THIS COULD BE BAD. Maybe you shoudl get the length
            SendMessage(hwnd, (int)WM.WM_GETTEXT, (IntPtr)sb.Capacity, sb);
            return sb.ToString();
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public int mouseData;
        public int flags;
        public int time;
        public UIntPtr dwExtraInfo;
    }
    enum WM : uint
    {//all windows messages here
        WM_LBUTTONUP = 0x0202,
        WM_GETTEXT = 0x000D,
        WM_GETTEXTLENGTH = 0x000E
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
