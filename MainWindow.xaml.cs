using System;
using System.Collections.Generic;
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
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;
using System.Windows.Threading;

namespace CanaryLauncherUpdate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		WebClient webClient = new WebClient();
		bool clientDownloaded = false;
		bool needUpdate = false;
		string clientName = "client.exe";
		string urlClient = "https://github.com/lucasgiovannibr/clientlauncherupdate/archive/refs/heads/main.zip";
		string urlVersion = "https://raw.githubusercontent.com/lucasgiovannibr/clientlauncherupdate/main/version.txt";
		string currentVersion = "";

		public MainWindow()
        {
            InitializeComponent();
        }

		private void TibiaLauncher_Load(object sender, RoutedEventArgs e)
		{
			currentVersion = webClient.DownloadString(urlVersion);
			labelDownloadPercent.Visibility = Visibility.Collapsed;
			progressbarDownload.Visibility = Visibility.Collapsed;

			if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient"))
			{
				Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient");
			}
			if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main"))
			{
				clientDownloaded = true;
			}

			if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt"))
			{
				StreamReader reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt");
				string myVersion = reader.ReadLine();
				reader.Close();

				labelVersion.Text = "My: " + myVersion + " Server: " + currentVersion;

				if (currentVersion == myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_play.png")));
					buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/icon_play.png"));
					needUpdate = false;
				}

				if (currentVersion != myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_update.png")));
					buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/icon_update.png"));
					needUpdate = true;
				}
			}
			if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt"))
			{
				labelVersion.Text = "My: None Server: " + currentVersion;
				buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_update.png")));
				buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/icon_update.png"));
				needUpdate = true;
			}
		}

		private void buttonPlay_Click(object sender, RoutedEventArgs e)
		{
			if (needUpdate == true)
			{
				try
				{
					labelDownloadPercent.Visibility = Visibility.Visible;
					progressbarDownload.Visibility = Visibility.Visible;
					buttonPlay.Visibility = Visibility.Collapsed;
					webClient.DownloadProgressChanged += Client_DownloadProgressChanged;
					webClient.DownloadFileCompleted += Client_DownloadFileCompleted;
					webClient.DownloadFileAsync(new Uri(urlClient), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/tibia.zip");
				}
				catch (Exception ex)
				{
					labelVersion.Text = ex.ToString();
				}
			}
			else
			{
				if (clientDownloaded == true)
				{
					Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/bin/" + clientName);
					this.Close();
				}
				else
				{
					try
					{
						labelDownloadPercent.Visibility = Visibility.Visible;
						progressbarDownload.Visibility = Visibility.Visible;
						buttonPlay.Visibility = Visibility.Collapsed;
						webClient.DownloadProgressChanged += Client_DownloadProgressChanged;
						webClient.DownloadFileCompleted += Client_DownloadFileCompleted;
						webClient.DownloadFileAsync(new Uri(urlClient), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/tibia.zip");
					}
					catch (Exception ex)
					{
						labelVersion.Text = ex.ToString();
					}
				}
			}
		}

		private void Client_DownloadFileCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			labelDownloadPercent.Visibility = Visibility.Collapsed;
			progressbarDownload.Visibility = Visibility.Collapsed;
			buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_play.png")));
			buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/icon_play.png"));
			buttonPlay.Visibility = Visibility.Visible;
			clientDownloaded = true;

			Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient");
			ZipFile.ExtractToDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/tibia.zip", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient", true);
			File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/tibia.zip");
			progressbarDownload.Value = 100;
			needUpdate = false;
			clientDownloaded = true;
			buttonPlay.Visibility = Visibility.Visible;
			progressbarDownload.Visibility = Visibility.Collapsed;
			labelDownloadPercent.Visibility = Visibility.Collapsed;
		}

		private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			progressbarDownload.Value = e.ProgressPercentage;
			labelDownloadPercent.Content = SizeSuffix(e.BytesReceived) + " / " + SizeSuffix(e.TotalBytesToReceive);
		}

		static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
		static string SizeSuffix(Int64 value, int decimalPlaces = 1)
		{
			if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
			if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
			if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

			int mag = (int)Math.Log(value, 1024);
			decimal adjustedSize = (decimal)value / (1L << (mag * 10));

			if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
			{
				mag += 1;
				adjustedSize /= 1024;
			}
			return string.Format("{0:n" + decimalPlaces + "} {1}",
				adjustedSize,
				SizeSuffixes[mag]);
		}

		private void buttonPlay_MouseEnter(object sender, MouseEventArgs e)
		{
			if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt"))
			{
				StreamReader reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt");
				string myVersion = reader.ReadLine();
				reader.Close();

				if (currentVersion != myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_hover_update.png")));
				}
				if (currentVersion == myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_hover_play.png")));
				}
			}
			else
			{
				buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_hover_update.png")));
			}
		}

		private void buttonPlay_MouseLeave(object sender, MouseEventArgs e)
		{
			if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt"))
			{
				StreamReader reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt");
				string myVersion = reader.ReadLine();
				reader.Close();

				if (currentVersion != myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_update.png")));
				}
				if (currentVersion == myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_play.png")));
				}
			}
			else
			{
				buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_update.png")));
			}
		}
	}
}
