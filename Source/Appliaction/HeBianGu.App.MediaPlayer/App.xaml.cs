// Copyright © 2022 By HeBianGu(QQ:908293466) https://github.com/HeBianGu/WPF-ControlBase

using HeBianGu.Base.WpfBase;
using HeBianGu.Control.Guide;
using HeBianGu.Control.ThemeSet;
using HeBianGu.General.WpfControlLib;
using HeBianGu.Service.Mvp;
using HeBianGu.Systems.About;
using HeBianGu.Systems.Setting;
using HeBianGu.Systems.Upgrade;
using System;
using System.Windows;
using System.Windows.Media;

namespace HeBianGu.App.MediaPlayer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : ApplicationBase
    {
        protected override MainWindowBase CreateMainWindow(StartupEventArgs e)
        {
            return new MainWindow();
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddWindowDialog();
            services.AddWindowAnimation();
            services.AddMessageProxy();
            services.AddXmlSerialize();
            services.AddXmlMeta();
            services.AddSetting();
            services.AddXmlWebSerializerService();
            services.AddSettingPath();

            //  Do ：启用启动窗口 需要添加引用HeBianGu.Window.Start
            services.AddStart(l =>
            {
                l.Title = "H影音";
                l.ProductFontSize = 100;
                l.Copyright = "Copyright @ HeBianGu 2019-2023";
            });
            services.AddAutoUpgrade(l =>
            {
                l.Uri = "https://gitee.com/hebiangu/wpf-auto-update/raw/master/Install/MediaPlayer/AutoUpdate.xml";
                l.UseIEDownload = true;
            });

            #region - More -
            services.AddUpgradeViewPresenter();
            services.AddAboutViewPresenter();
            services.AddMoreViewPresenter(x =>
            {
                x.AddPersenter(UpgradeViewPresenter.Instance);
                x.AddPersenter(AboutViewPresenter.Instance);

            });
            #endregion 

            #region - WindowCaption -
            services.AddGuideViewPresenter();
            services.AddSettingViewPrenter();
            services.AddThemeRightViewPresenter();
            services.AddWindowCaptionViewPresenter(x =>
            {
                x.AddPersenter(MoreViewPresenter.Instance);
                x.AddPersenter(GuideViewPresenter.Instance);
                x.AddPersenter(SettingViewPresenter.Instance);
                x.AddPersenter(ThemeRightToolViewPresenter.Instance);
            });
            #endregion
        }

        protected override void Configure(IApplicationBuilder app)
        {
            base.Configure(app);

            app.UseVlc(x =>
            {

            });

            //  Do：设置默认主题
            app.UseLocalTheme(l =>
            {
                l.AccentColor = (Color)ColorConverter.ConvertFromString("#FF0093FF");
                //l.ForegroundColor = (Color)ColorConverter.ConvertFromString("#000000");
                l.DefaultFontSize = 15D;
                l.FontSize = FontSize.Small;
                l.ItemHeight = 35;
                //l.ItemWidth = 120;
                l.ItemCornerRadius = 5;
                l.AnimalSpeed = 5000;
                l.AccentColorSelectType = 0;
                l.IsUseAnimal = true;
                l.ThemeType = ThemeType.Light;
                l.Language = Language.Chinese;
                l.AccentBrushType = AccentBrushType.LinearGradientBrush;
            });
        }
    }
}
