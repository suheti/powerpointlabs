﻿using PPExtraEventHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PowerPointLabs.ColorPicker
{
    public partial class ColorInformationDialog : Form
    {
        private HSLColor _selectedColor;
        
        public ColorInformationDialog(HSLColor selectedColor)
        {
            _selectedColor = selectedColor;
            InitializeComponent();
            SetUpUI();
        }

        private void textBox_Enter(object sender, EventArgs e)
        {
            var textBox = (TextBox) sender;
            Clipboard.SetText(textBox.Text.Substring(5));
            if (textBox.Equals(HSLTextBox))
            {
                label1.Text = "HSL value copied";
            } 
            else if (textBox.Equals(rgbTextBox))
            {
                label1.Text = "RGB value copied";
            } 
            else if (textBox.Equals(hexTextBox))
            {
                label1.Text = "HEX value copied";
            }
        }

        private void SetUpUI()
        {
            selectedColorPanel.BackColor = _selectedColor;
            UpdateHexTextBox();
            UpdateRGBTextBox();
            UpdateHSLTextBox();
            SetUpToolTips();
        }

        private void SetUpToolTips()
        {
            toolTip1.SetToolTip(this.rgbTextBox, "Red, Blue, Green");
            toolTip1.SetToolTip(this.HSLTextBox, "Hue, Saturation, Luminance");
            toolTip1.SetToolTip(this.hexTextBox, "Hex Triplet");
        }

        private void UpdateHSLTextBox()
        {
            HSLTextBox.Text = String.Format("HSL: {0}, {1}, {2}", (int)_selectedColor.Hue,
            (int) (_selectedColor.Saturation), (int) (_selectedColor.Luminosity));
            HSLTextBox.Enter += textBox_Enter;
        }

        private void UpdateRGBTextBox()
        {
            rgbTextBox.Text = String.Format("RGB: {0}, {1}, {2}", ((Color)_selectedColor).R,
            ((Color)_selectedColor).G, ((Color)_selectedColor).B);
            rgbTextBox.Enter += textBox_Enter;
        }

        private void UpdateHexTextBox()
        {
            byte[] rgbArray = { ((Color)_selectedColor).R, ((Color)_selectedColor).G, ((Color)_selectedColor).B };
            hexTextBox.Text = "HEX: #" + ByteArrayToString(rgbArray);
            hexTextBox.Enter += textBox_Enter;
        }

        private string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }
    }
}
