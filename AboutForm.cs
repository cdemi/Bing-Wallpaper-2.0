using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Bing_Wallpaper
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private Configuration _configuration;
        protected override void OnLoad(EventArgs e)
        {
            HideForm();
            base.OnLoad(e);
        }
        private bool _showing;
        private void HideForm()
        {
            Visible = _showing = ShowInTaskbar = false;
            Opacity = 0;
        }

        private void ShowForm()
        {
            Visible = _showing = ShowInTaskbar = true;
            Opacity = 1;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_showing)
                HideForm();
            else
                ShowForm();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("config.json"))
            {
                _configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
            }
            else
            {
                //Load Default Configuration
                _configuration = new Configuration
                {
                    Path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\Bing Wallpapers\",
                    ShowNotification = true
                };
            }
            timer1_Tick(null, null);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            toolStripMenuItem1.PerformClick();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/cdemi/Bing-Wallpaper-2.0");
        }

        private void AboutForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            HideForm();
        }

        private string _description;
        private string _detailsUrl;

        private bool updateWallpaper(bool force = false)
        {
            try
            {
                BingImage bingResponse;
                string imageUrl;
                using (var bingClient = new WebClient())
                {
                    bingResponse = JsonConvert.DeserializeObject<BingImage>(bingClient.DownloadString("http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US"));
                    imageUrl = $"http://www.bing.com{bingResponse.images.FirstOrDefault()?.url}";
                    _detailsUrl = bingResponse.images.FirstOrDefault()?.copyrightlink;
                    _description = Regex.Match(bingResponse.images.FirstOrDefault()?.copyright, @"(.+?)(\s\(.+?\))").Groups[1].Value;
                }
                toolStripMenuItem2.Visible = true;
                var wallpapersPath = _configuration.Path;
                var picturePath = $"{wallpapersPath}\\{bingResponse.images.FirstOrDefault()?.hsh}.jpg";
                if (!File.Exists(picturePath) || force)
                {
                    if (!Directory.Exists(wallpapersPath))
                        Directory.CreateDirectory(wallpapersPath);

                    using (var imageClient = new WebClient())
                    {
                        imageClient.DownloadFile(imageUrl, picturePath);
                    }

                    Wallpaper.Set(picturePath);

                    if (_configuration.ShowNotification)
                        notifyIcon1.ShowBalloonTip(10000, "Today's Bing Wallpaper", _description, ToolTipIcon.None);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!updateWallpaper())
            {
                timer1.Interval = 600000;
            }
            else
            {
                timer1.Interval = 3600000;
            }
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            Process.Start(_detailsUrl);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(10000, "Today's Bing Wallpaper", _description, ToolTipIcon.None);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            updateWallpaper(true);
        }
    }
}
