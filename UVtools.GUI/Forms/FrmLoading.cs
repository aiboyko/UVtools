﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using UVtools.Core;
using UVtools.Core.Operations;
using UVtools.GUI.Controls;

namespace UVtools.GUI.Forms
{
    public partial class FrmLoading : Form
    {
        public Stopwatch StopWatch { get; } = new Stopwatch();
        public OperationProgress Progress { get; private set; }

        private LogItem OperationLog;


        //public Task RunningTask { get; set; }

        public string Description
        {
            get => Text;
            set => Text = lbDescription.Text = value;
        }

        public bool CanCancel => Progress?.CanCancel ?? false;

        public FrmLoading()
        {
            InitializeComponent();
        }

        public FrmLoading(string description) : this()
        {
            SetDescription(description);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            e.Handled = true;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            e.Handled = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            lbOperation.Text = string.Empty;
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Step = 10;
            StopWatch.Restart();
            timer_Tick(timer, null);
            timer.Start();

            btnCancel.Enabled = CanCancel;
            btnCancel.Text = "Cancel";

            Cursor = Cursors.AppStarting;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            timer.Stop();
            StopWatch.Stop();
            Progress = null;
            OperationLog.ElapsedTime = (uint) StopWatch.ElapsedMilliseconds / 1000m;
            Cursor = Cursors.Arrow;
        }

        public OperationProgress RestartProgress(bool canCancel = true)
        {
            Progress = new OperationProgress(canCancel);
            return Progress;
        }

        public void SetDescription(string description)
        {
            Text =
            lbDescription.Text = description;
            OperationLog = new LogItem(description);
            Program.FrmMain.AddLog(OperationLog);
        }

        public void SetProgress(int value)
        {
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = value;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var ts = TimeSpan.FromMilliseconds(StopWatch.ElapsedMilliseconds);
            lbElapsedTime.Text = $"Elapsed Time: {ts.Minutes}m {ts.Seconds}s {ts.Milliseconds}ms";
            btnCancel.Enabled = CanCancel;
            if (ReferenceEquals(Progress, null)) return;

            lbOperation.Text = Progress.ToString();
            if (Progress.ItemCount == 0)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.Step = 10;
            }
            else
            {
                SetProgress(Progress.ProgressStep);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (ReferenceEquals(Progress, null) || Progress.TokenSource.IsCancellationRequested) return;
            Progress.TokenSource.Cancel();
            Progress.CanCancel = false;
            btnCancel.Enabled = false;
            btnCancel.Text = "Canceling...";
        }
    }
}
