using Microsoft.Win32;
using System;
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

        const int TIMER_INTERVAL = 1;
        const int MAX_TIME = 60;
        const int DEFAULT_START_TIME = 30;

        private System.Windows.Threading.DispatcherTimer _minuteTimer = new System.Windows.Threading.DispatcherTimer();
        private System.Windows.Threading.DispatcherTimer _seccondTimer = new System.Windows.Threading.DispatcherTimer();


        private double _currentTimeValue = 30;
        private double _timeDiskStep;


        public MainWindow()
        {
            InitializeComponent();

            _timeDiskStep = 360 / _currentTimeValue;

            _minuteTimer.Tick += _minuteTimer_Tick;

            _seccondTimer.Tick += (s, e) =>
            {
                //Every seccond:
                //Alternate the visibility of the power led.
                PowerLed.Visibility = (Visibility)Math.Abs((int)PowerLed.Visibility - 1);
                //Move the timedisk one step.
                TimeDisk.VisibleDegrees += _timeDiskStep;
            };
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
                PlayTada();


                //TODO:
                // Her kan det legges inn forskjellig annet snax.
                // Pause øvelser, ordtak, Haiku, 360-Tao, etc...
                // Annet?
            }

            lblTime.Content = _currentTimeValue.ToString();
            //TimeDisk.VisibleDegrees += _timeDiskStep;
        }


        private void PlayTada()
        {
            // Alarm
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            MediaPlayer player = new MediaPlayer();
            player.Open(new Uri(@"file://" + System.IO.Path.Combine(baseDirectory, Properties.Settings.Default.wavFileName), UriKind.RelativeOrAbsolute));
            player.Volume = 1f;
            player.Play();
            //player.Close();
        }


        private void Window_Initialized(object sender, EventArgs e)
        {
            _minuteTimer.Interval = TimeSpan.FromMinutes(TIMER_INTERVAL);
            _seccondTimer.Interval = TimeSpan.FromSeconds(TIMER_INTERVAL);

            lblTime.Content = DEFAULT_START_TIME.ToString();
        }


        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _minuteTimer.Stop();
            _seccondTimer.Stop();
            TimeDisk.VisibleDegrees = 0;
            PowerLed.Visibility = Visibility.Hidden;

            int currentTime = Int32.Parse(lblTime.Content.ToString());

            if (e.Delta > 0) 
            {
                if (currentTime < MAX_TIME) currentTime += 1;
                lblTime.Content = currentTime.ToString();
                _currentTimeValue = currentTime;
            }
            else 
            {
                if (currentTime > 1) currentTime -= 1; // (!) there is now point in turning the timer down to 0 -> 1 is the lowest.
                lblTime.Content = currentTime.ToString();
                _currentTimeValue = currentTime;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Make sure we can moove the time-disk window.
            DragMove();

            //TODO: Remove this -> only for debuging.
            PlayTada();

        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_minuteTimer.IsEnabled)
            {
                _minuteTimer.Stop();
                _seccondTimer.Stop();
                TimeDisk.VisibleDegrees = 0;
                PowerLed.Visibility = Visibility.Hidden;
            }
            else
            {
                // Calculate how many steps the time disk must be divided up in when the disk ticks one step every second.:
                // The disk is 360° -> divide it up in the amount of shoosen minutes times 60 seconds per minute.
                _timeDiskStep = _currentTimeValue > 0 ? 360 / (_currentTimeValue * 60) : 0;
                _minuteTimer.Start();
                _seccondTimer.Start();
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
