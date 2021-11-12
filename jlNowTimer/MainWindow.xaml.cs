using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace jlNowTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int MAX_TIME = 60;

        private System.Windows.Threading.DispatcherTimer _minuteTimer = new System.Windows.Threading.DispatcherTimer();
        private System.Windows.Threading.DispatcherTimer _seccondTimer = new System.Windows.Threading.DispatcherTimer();
        private System.Windows.Threading.DispatcherTimer _milliSeccondTimer = new System.Windows.Threading.DispatcherTimer();

        private double _currentTimeValue = 30;      //Startup timer value.
        private double _timeDiskStepPerMillisecond; //Amount of degrees the timer-disk should move per millisecond.
        private double _timeDiskStepPerSecond;      //Amount of degrees the timer-disk will move per millisecond.
        private double _correctedDiskSizeInDegrees; //Holds the correct size the disk should have grown to per second.


        public MainWindow()
        {
            InitializeComponent();

            #region Add envent handlers for the timer ticks.
            // - - - - - - - - - - - - - - - - - - - - - - -

            _minuteTimer.Tick += _minuteTimer_Tick;

            _seccondTimer.Tick += (s, e) =>
            {
                //Every seccond:
                //Alternate the visibility of the power led.
                PowerLed.Visibility = (Visibility)Math.Abs((int)PowerLed.Visibility - 1);

                //Correct the timedisk to next second-step.
                _correctedDiskSizeInDegrees += _timeDiskStepPerSecond;
                TimeDisk.VisibleDegrees = _correctedDiskSizeInDegrees;
            };

            _milliSeccondTimer.Tick += (s, e) =>
            {
                //Move the timedisk one tiny step.
                TimeDisk.VisibleDegrees += _timeDiskStepPerMillisecond;
            };

            // - - - - - - - - - - - - - - - - - - - - - - -
            #endregion Add envent handlers for the timer ticks.

        }


        private void _minuteTimer_Tick(object sender, EventArgs e)
        {
            _currentTimeValue -= 1;

            if (_currentTimeValue == 0)
            {
                _minuteTimer.Stop();
                _seccondTimer.Stop();
                PowerLed.Visibility = Visibility.Hidden;
                TimeDisk.VisibleDegrees += 360;

                // Finnished -> Play fanfare! 🎺🎉
                Task.Run(() => PlayTada());
                

                //TODO:
                // Her kan det legges inn forskjellig annet snax.
                // Pause øvelser, ordtak, Haiku, 360-Tao, etc...
                // Annet?
            }

            lblTime.Content = _currentTimeValue.ToString();
        }


        private void PlayTada()
        {
            // Fanfare.
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            MediaPlayer player = new MediaPlayer();
            player.Open(new Uri(@"file://" + System.IO.Path.Combine(baseDirectory, Properties.Settings.Default.wavFileName), UriKind.RelativeOrAbsolute));
            player.Volume = 1f;
            player.Play();
        }


        private void Window_Initialized(object sender, EventArgs e)
        {
            _minuteTimer.Interval = TimeSpan.FromMinutes(1);
            _seccondTimer.Interval = TimeSpan.FromSeconds(1);
            _milliSeccondTimer.Interval = TimeSpan.FromMilliseconds(20);

            lblTime.Content = _currentTimeValue.ToString();
        }


        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _minuteTimer.Stop();
            _seccondTimer.Stop();
            _milliSeccondTimer.Stop();
            TimeDisk.VisibleDegrees = 0;
            _correctedDiskSizeInDegrees = 0;
            PowerLed.Visibility = Visibility.Hidden;
                       
            _currentTimeValue = double.Parse(lblTime.Content.ToString());
            // Resets to lowest possible start time if _currentTimeValue had reached 0.
            // There is no point in turning the timer down to 0 -> 1 is the lowest.
            if (_currentTimeValue == 0) _currentTimeValue = 1;

            if (e.Delta > 0) 
            {
                if (_currentTimeValue < MAX_TIME) _currentTimeValue += 1;
                lblTime.Content = _currentTimeValue.ToString();
            }
            else 
            {
                if (_currentTimeValue > 1) _currentTimeValue -= 1; // (!) there is no point in turning the timer down to 0 -> 1 is the lowest.
                lblTime.Content = _currentTimeValue.ToString();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Make sure we can move the time-disk window around the screen by klikking and draging it.
            DragMove();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_minuteTimer.IsEnabled)
            {
                _minuteTimer.Stop();
                _seccondTimer.Stop();
                _milliSeccondTimer.Stop();
                TimeDisk.VisibleDegrees = 0;
                PowerLed.Visibility = Visibility.Hidden;
            }
            else
            {
                // Note how big the increase in disk size must be per second, based on the start value for the timer.
                _timeDiskStepPerSecond = 360 / (_currentTimeValue * 60);

                // If the timer has reacht 0 (full turn) and it is started again in that position -> we need to set the start value to a positiv integer => 1
                if (lblTime.Content.ToString() == "0") lblTime.Content = "1";

                // Calculate how many steps the time disk must be divided up in when the disk ticks one step every second.:
                // The disk is 360° -> divide it up in the amount of choosen minutes times (60 seconds * (milisecond-tick-interval / 1000))  (timer ticks every 20 milliseconds).
                // * 0.575 -> adjust the divider with 0.575 (makes bigger chunks of time disk to display per millisecondTimer tick) to compensate
                // for lag when updating the grafics (it does not ceep track with the ticking of milliseconds).
                // Note: it may be that this factor will vary from PC to PC -> due to the capasity of the PC --> if this turns out to be true => make a setting panel where
                // the user may alter this factor to finetune the setting.
                _timeDiskStepPerMillisecond = _currentTimeValue > 0 ? 360 / (_currentTimeValue * 3000 * 0.575) : 0; 
                _minuteTimer.Start();
                _seccondTimer.Start();
                _milliSeccondTimer.Start();
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Configure open file dialog box
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "🎺🎉 Choose fanfare file.";
            openFileDialog.Filter = "Sound files (.wav)|*.wav"; // Filter files by extension
            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Opens the open file dialog.
            if (openFileDialog.ShowDialog() == true)
            {
                // Store the filePath (without the file name) into Property.settings so that
                // the last opened folder is remembered.
                Properties.Settings.Default.wavFileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                Properties.Settings.Default.Save();
            }
        }
    }
}
