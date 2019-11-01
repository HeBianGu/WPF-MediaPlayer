using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HeBianGu.Product.General.MediaPlayer
{
    /// <summary>
    /// MediaPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class MediaPlayer : UserControl
    {
        public MediaPlayer()
        {
            InitializeComponent();

            this.media_media.MediaEnded += Player_MediaEnded;
            this.media_media.MediaOpened += Player_MediaOpened;
            this.media_media.MediaFailed += Player_MediaFailed;
            this.media_media.Loaded += Player_Loaded;

            _timer.Elapsed += Timer_Elapsed;
            _timer.Interval = 1000;

        }
      

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.RefreshSlider();
        }

        Timer _timer = new Timer();

        private void Player_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Player_Loaded");
        }

        void InitSlider()
        {
            if (this.media_media.Source == null) return;

            if (this.media_media.NaturalDuration.HasTimeSpan)
            {
                this.media_slider.Maximum = this.media_media.NaturalDuration.TimeSpan.Ticks;
            }
        }

        void RefreshSlider()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.media_slider.Value = this.media_media.Position.Ticks;
            });

        }

        private void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show(e.ErrorException.Message);
            Debug.WriteLine("Player_MediaFailed");

        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Player_MediaOpened");

            this.InitSlider();

            this.InitSound();

            this._timer.Start();
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Player_MediaEnded");

            this.Stop();
        }


        private void media_slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (this.media_media == null) return;

            this.media_media.Position = TimeSpan.FromTicks((long)this.media_slider.Value);

            this._timer.Start();
        }

        void InitSound()
        {
            this.slider_sound.Value = this.media_media.Volume;
        }

        private void slider_sound_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            this.media_media.Volume = this.slider_sound.Value;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.media_media.SpeedRatio = this.media_media.SpeedRatio / 2;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.media_media.SpeedRatio = this.media_media.SpeedRatio * 2;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.Stop();
        }

        private void media_slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            this._timer.Stop();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.toggle_play.IsChecked.Value)
            {
                this.Pause();
            }
            else
            {

                this.Play();
            }
        }

        private void CommandBinding_Executed_Play(object sender, ExecutedRoutedEventArgs e)
        {

            Debug.WriteLine("CommandBinding_Executed_Play");

        }

        private void CommandBinding_CanExecute_Play(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MediaBrower_PlayClick(object sender, RoutedEventArgs e)
        {
            this.media_media.Stop();
            this.media_media.Play();
            this.toggle_play.IsChecked = false;
        }

        void Play()
        {
            this.media_media.Play();
            this._timer.Start();
        }

        void Pause()
        {
            this.media_media.Pause();
            this._timer.Stop();
        }

        void Stop()
        {
            this.media_media.Position = TimeSpan.FromTicks(0);
            this.media_slider.Value = 0;
            this.media_media.Stop();
            this._timer.Stop();
            this.toggle_play.IsChecked = true;
            this.media_media.LoadedBehavior = MediaState.Manual;
        }




        //public string VedioSource
        //{
        //    get { return (string)GetValue(VedioSourceProperty); }
        //    set { SetValue(VedioSourceProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty VedioSourceProperty =
        //    DependencyProperty.Register("VedioSource", typeof(string), typeof(MediaPlayer), new PropertyMetadata(default(string), (d, e) =>
        //     {
        //         MediaPlayer control = d as MediaPlayer;

        //         if (control == null) return;

        //         string config = e.NewValue as string;

        //         //control.media_media.Source = new Uri(config, UriKind.RelativeOrAbsolute);

        //         //control.Play();

        //     }));


        public Uri VedioSource
        {
            get { return (Uri)GetValue(VedioSourceProperty); }
            set { SetValue(VedioSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VedioSourceProperty =
            DependencyProperty.Register("VedioSource", typeof(Uri), typeof(MediaPlayer), new PropertyMetadata(default(Uri), (d, e) =>
             {
                 MediaPlayer control = d as MediaPlayer;

                 if (control == null) return;

                 Uri config = e.NewValue as Uri;

                 control.media_media.Source = config;

                 control.Stop();

             }));



    }


    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            //if (value.ToString() == "0") return "0";
            //if (value.ToString() == "100") return "100";

            var d = double.Parse(value.ToString());

            var sp = TimeSpan.FromTicks((long)d);

            return sp.ToString().Split('.')[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
