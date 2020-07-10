using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;

namespace HeBianGu.General.VLCMediaPlayer
{
    /// <summary>
    /// VLCPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class VLCPlayer : UserControl
    {
        public VLCPlayer()
        {
            InitializeComponent();

            CommandBinding binding = new CommandBinding(VLCPlayer.OpenFile, (l, k) =>
            {
                OpenFileDialog dialog = new OpenFileDialog();

                var r = dialog.ShowDialog();

                if (r.HasValue && r.Value)
                {
                    this.VedioSource = new Uri(dialog.FileName, UriKind.Absolute);
                }

            });

            this.CommandBindings.Add(binding);


        }

        /// <summary> 当前播放的路径 </summary>
        public Uri VedioSource
        {
            get { return (Uri)GetValue(VedioSourceProperty); }
            set { SetValue(VedioSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VedioSourceProperty =
            DependencyProperty.Register("VedioSource", typeof(Uri), typeof(VLCPlayer), new PropertyMetadata(default(Uri), (d, e) =>
            {
                VLCPlayer control = d as VLCPlayer;

                if (control == null) return;

                Uri config = e.NewValue as Uri;

                control.InitVlc();

                control.vlccontrol.SourceProvider.MediaPlayer.Play(config);

                control.toggle_play.IsChecked = false;

            }));


        VlcControl vlccontrol = null;

        void InitVlc()
        {
            this.vlccontrol = new VlcControl();

            this.ControlContainer.Content = this.vlccontrol;

            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var libDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            this.vlccontrol.SourceProvider.CreatePlayer(libDirectory/* pass your player parameters here */);


            this.vlccontrol.SourceProvider.MediaPlayer.PositionChanged += (l, k) =>
            {
                this.RefreshSlider();
            };

            this.vlccontrol.SourceProvider.MediaPlayer.LengthChanged += (l, k) =>
            {
                this.InitSlider();
            };
        }

        /// <summary> 初始化播放进度条 </summary>
        void InitSlider()
        {
            if (this.vlccontrol?.SourceProvider?.MediaPlayer == null) return;

            this.Dispatcher.Invoke(() =>
             {
                 this.media_slider.Maximum = this.vlccontrol.SourceProvider.MediaPlayer.Length;
             });

        }

        /// <summary> 刷新当前进度 </summary>
        void RefreshSlider()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.media_slider.ValueChanged -= this.media_slider_ValueChanged;

                this.media_slider.Value = this.vlccontrol.SourceProvider.MediaPlayer == null ? 0 : this.vlccontrol.SourceProvider.MediaPlayer.Time;

                this.media_slider.ValueChanged += this.media_slider_ValueChanged;
            });

        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            this.Rate = (float)Math.Round(this.vlccontrol.SourceProvider.MediaPlayer.Rate / (float)1.2, 2);

        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.Rate = (float)Math.Round(this.vlccontrol.SourceProvider.MediaPlayer.Rate * (float)1.2, 2);
        }

        public float Rate
        {
            get { return (float)GetValue(RateProperty); }
            set { SetValue(RateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RateProperty =
            DependencyProperty.Register("Rate", typeof(float), typeof(VLCPlayer), new PropertyMetadata(1.0f, (d, e) =>
             {
                 VLCPlayer control = d as VLCPlayer;

                 if (control == null) return;

                 control.vlccontrol.SourceProvider.MediaPlayer.Rate = (float)e.NewValue;

             }));





        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.Stop();
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

        void Play()
        {
            this.vlccontrol?.SourceProvider.MediaPlayer.Play();
        }

        void Pause()
        {
            this.vlccontrol?.SourceProvider.MediaPlayer.Pause();
        }

        void Stop()
        {
            this.toggle_play.IsChecked = true;

            this.vlccontrol?.Dispose();
        }

        private void media_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.vlccontrol.SourceProvider.MediaPlayer.Time = (long)this.media_slider.Value;
        }

        private void slider_sound_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.RefreshAudio();
        }

        void RefreshAudio()
        {
            if (this.vlccontrol?.SourceProvider?.MediaPlayer?.Audio == null) return;

            this.vlccontrol.SourceProvider.MediaPlayer.Audio.Volume = (int)this.slider_sound.Value;
        }


        private void FButton_Click_Full(object sender, RoutedEventArgs e)
        {
            this.vlccontrol.SourceProvider.MediaPlayer.Video.FullScreen = true;
        }
    }

    public partial class VLCPlayer
    {
        public static RoutedUICommand OpenFile = new RoutedUICommand();
    }

    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var d = double.Parse(value.ToString());

            var sp = TimeSpan.FromMilliseconds((long)d);

            return sp.ToString().Split('.')[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
