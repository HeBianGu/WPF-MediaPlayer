using HeBianGu.Base.WpfBase;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Vlc.DotNet.Core.Interops.Signatures;
using Vlc.DotNet.Wpf;
using Path = System.IO.Path;

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

            {
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

            {
                CommandBinding binding = new CommandBinding(VLCPlayer.ShootCutCommand, async (l, k) =>
                {
                    string filePath = await this.BeginShootCut();

                    this.OnShootCut(filePath);

                });

                //, (l, k) =>
                //  {
                //      if (this.vlccontrol?.SourceProvider?.MediaPlayer == null)
                //      {
                //          k.CanExecute = false;
                //          return;
                //      }
                //      k.CanExecute = this.vlccontrol.SourceProvider.MediaPlayer.State == MediaStates.Paused || this.vlccontrol.SourceProvider.MediaPlayer.State == MediaStates.Playing;
                //  }

                this.CommandBindings.Add(binding);
            }


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

        /// <summary> 设置当前显示时间 </summary>
        public TimeSpan Time
        {
            get { return (TimeSpan)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(TimeSpan), typeof(VLCPlayer), new PropertyMetadata(default(TimeSpan), async (d, e) =>
             {
                 VLCPlayer control = d as VLCPlayer;

                 if (control == null) return;

                 TimeSpan config = (TimeSpan)e.NewValue;

                 await Task.Run(() => control.vlccontrol.SourceProvider.MediaPlayer.Time = (long)config.TotalMilliseconds);

             }));


        void InitVlc()
        {

            if (this.vlccontrol?.SourceProvider?.MediaPlayer != null)
            {
                this.vlccontrol.SourceProvider.MediaPlayer.PositionChanged -= MediaPlayer_PositionChanged;

                this.vlccontrol.SourceProvider.MediaPlayer.LengthChanged -= MediaPlayer_LengthChanged;
            }

            this.vlccontrol = new VlcControl();

            this.ControlContainer.Content = this.vlccontrol;

            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var libDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            this.vlccontrol.SourceProvider.CreatePlayer(libDirectory/* pass your player parameters here */);

            //this.vlccontrol.SourceProvider.MediaPlayer.Video.IsMouseInputEnabled = false;
            //this.vlccontrol.SourceProvider.MediaPlayer.Video.IsKeyInputEnabled = false;

            this.vlccontrol.SourceProvider.MediaPlayer.PositionChanged += MediaPlayer_PositionChanged;

            this.vlccontrol.SourceProvider.MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
        }

        private void MediaPlayer_LengthChanged(object sender, VlcMediaPlayerLengthChangedEventArgs e)
        {
            this.InitSlider();
        }

        private void MediaPlayer_PositionChanged(object sender, VlcMediaPlayerPositionChangedEventArgs e)
        {
            this.RefreshSlider();
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
            if (this.vlccontrol?.SourceProvider?.MediaPlayer == null) return;

            this.vlccontrol?.SourceProvider.MediaPlayer.Play();
        }

        void Pause()
        {
            if (this.vlccontrol?.SourceProvider?.MediaPlayer == null) return;

            this.vlccontrol?.SourceProvider.MediaPlayer.Pause();
        }

        public void Stop()
        {
            this.toggle_play.IsChecked = true;

            if (this.vlccontrol?.SourceProvider?.MediaPlayer == null) return;

            this.vlccontrol.SourceProvider.MediaPlayer.PositionChanged -= MediaPlayer_PositionChanged;

            this.vlccontrol.SourceProvider.MediaPlayer.LengthChanged -= MediaPlayer_LengthChanged;

            this.media_slider.Value = 0;

            Task.Run(()=> this.vlccontrol?.Dispose());
        }

        private async void media_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            long value = (long)this.media_slider.Value;

            await Task.Run(() => this.vlccontrol.SourceProvider.MediaPlayer.Time = value);
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


        public IEnumerable ItemSource
        {
            get { return (IEnumerable)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register("ItemSource", typeof(IEnumerable), typeof(VLCPlayer), new PropertyMetadata(default(IEnumerable), (d, e) =>
             {
                 VLCPlayer control = d as VLCPlayer;

                 if (control == null) return;

                 IEnumerable config = e.NewValue as IEnumerable;

             }));


        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(VLCPlayer), new PropertyMetadata(default(object), (d, e) =>
             {
                 VLCPlayer control = d as VLCPlayer;

                 if (control == null) return;

                 object config = e.NewValue as object;

             }));




        public object ContentControl
        {
            get { return (object)GetValue(ContentControlProperty); }
            set { SetValue(ContentControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentControlProperty =
            DependencyProperty.Register("ContentControl", typeof(object), typeof(VLCPlayer), new PropertyMetadata(default(object), (d, e) =>
             {
                 VLCPlayer control = d as VLCPlayer;

                 if (control == null) return;

                 object config = e.NewValue as object;

             }));

        public TimeSpan GetTime()
        {
            if (this.vlccontrol?.SourceProvider == null) return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(this.vlccontrol.SourceProvider.MediaPlayer.Time);
        }

        public ImageSource GetVlc()
        {
            return this.vlccontrol.SourceProvider.VideoSource;
        }

        string ShotCutPat { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HeBianGu", Assembly.GetExecutingAssembly().GetName().Name, "ShootCut");
         
        //声明和注册路由事件
        public static readonly RoutedEvent ShootCutRoutedEvent =
            EventManager.RegisterRoutedEvent("ShootCut", RoutingStrategy.Bubble, typeof(EventHandler<ObjectRoutedEventArgs<string>>), typeof(VLCPlayer));
        //CLR事件包装
        public event RoutedEventHandler ShootCut
        {
            add { this.AddHandler(ShootCutRoutedEvent, value); }
            remove { this.RemoveHandler(ShootCutRoutedEvent, value); }
        }

        //激发路由事件,借用Click事件的激发方法

        protected void OnShootCut(string uri)
        {
            ObjectRoutedEventArgs<string> args = new ObjectRoutedEventArgs<string>(ShootCutRoutedEvent, this);

            args.Entity = uri;

            this.RaiseEvent(args);
        }

        public async Task<string> BeginShootCut()
        {
            string name = Path.GetFileNameWithoutExtension(this.VedioSource?.LocalPath);

            string timespan = TimeSpan.FromMilliseconds(this.vlccontrol.SourceProvider.MediaPlayer.Time).Ticks.ToString();

            string filePath = Path.Combine(ShotCutPat, name, timespan + ".jpg");

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            return await Task.Run(() =>
              {
                  this.vlccontrol.SourceProvider.MediaPlayer.TakeSnapshot(new FileInfo(filePath));

                  return filePath;
              });
        }


    }

    public partial class VLCPlayer
    {
        public static RoutedUICommand OpenFile = new RoutedUICommand();

        public static RoutedUICommand ShootCutCommand = new RoutedUICommand();
    }

    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var d = long.Parse(value.ToString());

            var sp = TimeSpan.FromMilliseconds(d);

            return sp.ToString().Split('.')[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
