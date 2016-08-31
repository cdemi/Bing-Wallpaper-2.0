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

        private Configuration configuration;
        protected override void OnLoad(EventArgs e)
        {
            hideForm();
            base.OnLoad(e);
        }
        private bool showing = false;
        private void hideForm()
        {
            Visible = showing = ShowInTaskbar = false;
            Opacity = 0;
        }

        private void showForm()
        {
            Visible = showing = ShowInTaskbar = true;
            Opacity = 1;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (showing)
                hideForm();
            else
                showForm();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("config.json"))
            {
                configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
            }
            else
            {
                //Load Default Configuration
                configuration = new Configuration
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
            hideForm();
        }

        private string description;
        private string detailsURL;

        private bool updateWallpaper(bool force = false)
        {
            try
            {
                BingImage bingResponse;
                string imageURL;
                using (WebClient bingClient = new WebClient())
                {
                    bingResponse = JsonConvert.DeserializeObject<BingImage>(bingClient.DownloadString("http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US"));
                    imageURL = $"http://www.bing.com{bingResponse.images.FirstOrDefault().url}";
                    detailsURL = bingResponse.images.FirstOrDefault().copyrightlink;
                    description = Regex.Match(bingResponse.images.FirstOrDefault().copyright, @"(.+?)(\s\(.+?\))").Groups[1].Value;
                }
                toolStripMenuItem2.Visible = true;
                string wallpapersPath = configuration.Path;
                string picturePath = $"{wallpapersPath}\\{bingResponse.images.FirstOrDefault().hsh}.jpg";
                if ((!File.Exists(picturePath)) || force)
                {
                    if (!Directory.Exists(wallpapersPath))
                        Directory.CreateDirectory(wallpapersPath);
                    using (WebClient imageClient = new WebClient())
                    {
                        imageClient.DownloadFile(imageURL, picturePath);
                    }
                    Wallpaper.Set(picturePath);

                    if (configuration.ShowNotification)
                        notifyIcon1.ShowBalloonTip(10000, "Today's Bing Wallpaper", description, ToolTipIcon.None);

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
            if (updateWallpaper())
            {
                timer1.Interval = 3600000;
            }
            else
            {
                timer1.Interval = 600000;
            }
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            Process.Start(detailsURL);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(10000, "Today's Bing Wallpaper", description, ToolTipIcon.None);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            updateWallpaper(true);
        }
    }
}
