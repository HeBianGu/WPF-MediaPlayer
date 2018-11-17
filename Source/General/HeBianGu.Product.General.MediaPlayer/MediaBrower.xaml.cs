using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace HeBianGu.Product.General.MediaPlayer
{
    /// <summary>
    /// MediaBrower.xaml 的交互逻辑
    /// </summary>
    public partial class MediaBrower : UserControl
    {
        public MediaBrower()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
           

        }


        public ObservableCollection<FileInfo> FileSource
        {
            get { return (ObservableCollection<FileInfo>)GetValue(FileSourceProperty); }
            set { SetValue(FileSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileSourceProperty =
            DependencyProperty.Register("FileSource", typeof(ObservableCollection<FileInfo>), typeof(MediaBrower), new PropertyMetadata(new ObservableCollection<FileInfo>(), (d, e) =>
             {
                 MediaBrower control = d as MediaBrower;

                 if (control == null) return;

                 ObservableCollection<FileInfo> config = e.NewValue as ObservableCollection<FileInfo>;

                 config.CollectionChanged += (l, k) =>
             {
                 control.FileSource = config;
             };



             }));


        public Uri SelectFile
        {
            get { return (Uri)GetValue(SelectFileProperty); }
            set { SetValue(SelectFileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectFileProperty =
            DependencyProperty.Register("SelectFile", typeof(Uri), typeof(MediaBrower), new PropertyMetadata(default(Uri), (d, e) =>
             {
                 MediaBrower control = d as MediaBrower;

                 if (control == null) return;

                 Uri config = e.NewValue as Uri;

             }));


        //声明和注册路由事件
        public static readonly RoutedEvent PlayClickRoutedEvent =
            EventManager.RegisterRoutedEvent("PlayClick", RoutingStrategy.Bubble, typeof(EventHandler<RoutedEventArgs>), typeof(MediaBrower));
        //CLR事件包装
        public event RoutedEventHandler PlayClick
        {
            add { this.AddHandler(PlayClickRoutedEvent, value); }
            remove { this.RemoveHandler(PlayClickRoutedEvent, value); }
        }

        //激发路由事件,借用Click事件的激发方法

        protected void OnPlayClick()
        {
            RoutedEventArgs args = new RoutedEventArgs(PlayClickRoutedEvent, this);
            this.RaiseEvent(args);
        }


        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void CommandBinding_Executed_Play(object sender, ExecutedRoutedEventArgs e)
        {
            this.BeginPlay();
        }

        void BeginPlay()
        {
            FileInfo file = list_files.SelectedItem as FileInfo;

            if (file == null) return;

            this.SelectFile = new Uri(file.FullName, UriKind.Absolute);

            this.OnPlayClick();
        }

        private void CommandBinding_CanExecute_Play(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.list_files.SelectedItem != null;
        }

        private void list_files_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.BeginPlay();
        }

        private void CommandBinding_Executed_OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();

            var result = open.ShowDialog();

            if (!result.HasValue) return;

            if (!result.Value) return;

            FileInfo file = new FileInfo(open.FileName);

            this.FileSource.Add(file);
        }

        private void CommandBinding_CanExecute_OpenFile(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }

    class ModiaFile
    {

        public string FileName { get; set; }

        public ModiaFile(FileInfo file)
        {
            FileName = file.Name;
        }
    }
}
