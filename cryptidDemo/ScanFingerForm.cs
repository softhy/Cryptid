﻿#region

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cryptid.Scanners;
using Cryptid.Utils;

#endregion

namespace CryptidDemo {
    public partial class ScanFingerForm : Form {
        private State _currState;

        public ScanFingerForm() {
            InitializeComponent();
        }

        private State CurrentState {
            get { return _currState; }
            set {
                _currState = value;
                OnStateChange(value);
            }
        }

        public Bitmap Fingerprint { get; set; }

        private void ScanFingerForm_Load(object sender, EventArgs e) {
            var t = Task.Factory.StartNew(() => {
                CurrentState = State.FingerNotPressed;
                FpsGt511C1R.SetCmosLed(true);
                while (!FpsGt511C1R.IsPressingFinger()) Task.Delay(1000);
                CurrentState = State.FingerPressed;
                CurrentState = State.TransferingData;
                Fingerprint = FpsGt511C1R.GetRawImage();
                FpsGt511C1R.SetCmosLed(false);
                CurrentState = State.TransferComplete;
                DialogResult = DialogResult.OK;
                SafeClose();
            });
            t.Start();
        }

        private void OnStateChange(State s) {
            switch (s) {
                case State.FingerNotPressed:
                    BackColor = Color.Red;
                    stateText.SetPropertyThreadSafe(() => stateText.Text, "Place Finger on Sensor");
                    break;
                case State.FingerPressed:
                    BackColor = Color.Blue;
                    stateText.SetPropertyThreadSafe(() => stateText.Text, "Begining Transfer");
                    break;
                case State.TransferingData:
                    BackColor = Color.Green;
                    stateText.SetPropertyThreadSafe(() => stateText.Text, "Transfering Data...");
                    break;
                case State.TransferComplete:
                    BackColor = Color.Green;
                    stateText.SetPropertyThreadSafe(() => stateText.Text, "Transfering Complete");
                    break;
            }
        }

        public void SafeClose() {
            if (InvokeRequired) {
                BeginInvoke(new Action(SafeClose));
                return;
            }
            Close();
        }

        private enum State {
            FingerNotPressed,
            FingerPressed,
            TransferingData,
            TransferComplete
        }
    }
}