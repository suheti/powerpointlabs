﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PowerPointLabs.Views
{
    public partial class AutoNarrateDialogBox : Form
    {
        public delegate void UpdateSettingsDelegate(String voiceName, bool preview);
        public UpdateSettingsDelegate SettingsHandler;

        public AutoNarrateDialogBox()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }

        public AutoNarrateDialogBox(int selectedVoice, List<String> voices, bool preview) : this()
        {
            defaultVoice.DataSource = voices;
            defaultVoice.SelectedIndex = selectedVoice;

            this.preview.Checked = preview;
        }

        private void AutoNarrateDialogBox_Load(object sender, EventArgs e)
        {
            ToolTip voiceToolTip = new ToolTip();
            voiceToolTip.SetToolTip(defaultVoice, 
                "The voice to be used when generating synthesized audio. Use [Voice] tags to specify a different voice for a particular section of text.");

            ToolTip previewToolTip = new ToolTip();
            previewToolTip.SetToolTip(preview,
                "If checked, the current slide's audio and animations will play after the Add Audio button is clicked.");
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ok_Click(object sender, EventArgs e)
        {
            SettingsHandler(defaultVoice.Text, preview.Checked);

            Close();
        }
    }
}
