using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaroGame.Play
{
    public class TimeManager
    {
        #region Properties

        private Timer timerCoolDown;
        private ProgressBar progressBarCoolDown;
        private TextBox textBoxTimeLeft;

        public Timer TimerCoolDown { get => timerCoolDown; set => timerCoolDown = value; }
        public ProgressBar ProgressBarCoolDown { get => progressBarCoolDown; set => progressBarCoolDown = value; }
        public TextBox TextBoxTimeLeft { get => textBoxTimeLeft; set => textBoxTimeLeft = value; }

        private event EventHandler timeUp;
        public event EventHandler TimeUp
        {
            add { timeUp += value; }
            remove { timeUp -= value; }
        }

        #endregion

        #region Methods

        // Constructor
        public TimeManager(Form1 mainForm, Timer timerCoolDown, ProgressBar progressBarCoolDown, TextBox textBoxTimeLeft)
        {
            TimerCoolDown = timerCoolDown;
            TimerCoolDown.Interval = Constants.COOL_DOWN_INTERVAL;
            TimerCoolDown.Tick += TimerCoolDown_Tick;

            ProgressBarCoolDown = progressBarCoolDown;
            ProgressBarCoolDown.Maximum = mainForm.MySettings.WaitingTime;
            ProgressBarCoolDown.Step = Constants.COOL_DOWN_STEP;

            TextBoxTimeLeft = textBoxTimeLeft;
        }

        // Khi bộ đếm đang chạy, cứ sau một khoảng thời gian Interval thì làm việc này
        private void TimerCoolDown_Tick(object sender, EventArgs e)
        {
            ProgressBarCoolDown.PerformStep();

            // Hết 1s = 1000ms
            if (ProgressBarCoolDown.Value % Constants.SECOND_TO_MILISECOND == 0)
            {
                TextBoxTimeLeft.Text = (ProgressBarCoolDown.Value / Constants.SECOND_TO_MILISECOND).ToString();
            }

            if (ProgressBarCoolDown.Value <= 0)
            {
                TimerCoolDown.Stop();
                NotifyTimeUp();
                //socketManager.Send(new SocketData((int)SocketCommand.TIME_OUT, "", new Point()));
            }
        }

        // Thông báo người chơi hiện tại hết thời gian
        private void NotifyTimeUp()
        {
            if (timeUp != null)
            {
                timeUp(this, new EventArgs());
            }
        }

        // Chuẩn bị
        public void Ready()
        {
            TimerCoolDown.Stop();
            ProgressBarCoolDown.Value = 0;
            TextBoxTimeLeft.Text = (ProgressBarCoolDown.Maximum / Constants.SECOND_TO_MILISECOND).ToString();
        }

        // Bắt đầu lại
        public void Restart()
        {
            ProgressBarCoolDown.Value = ProgressBarCoolDown.Maximum;
            TextBoxTimeLeft.Text = (ProgressBarCoolDown.Maximum / Constants.SECOND_TO_MILISECOND).ToString();
            TimerCoolDown.Start();
        }

        // Tiếp tục
        public void Continue()
        {
            if (ProgressBarCoolDown.Value > 0)
            {
                TimerCoolDown.Start();
            }
        }

        // Tạm dừng
        public void Pause()
        {
            TimerCoolDown.Stop();
        }

        #endregion
    }
}
