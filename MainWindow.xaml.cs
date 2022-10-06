using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;

namespace CanaryLauncherUpdate
{

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		readonly WebClient webClient = new();

		bool clientDownloaded = false;
		bool needUpdate = false;
		readonly string clientName = "client.exe";
		readonly string urlClient = "https://github.com/lucasgiovannibr/clientlauncherupdate/archive/refs/heads/main.zip";
		readonly string urlVersion = "https://raw.githubusercontent.com/lucasgiovannibr/clientlauncherupdate/main/version.txt";
		string currentVersion = "";

		public MainWindow()
		{
			InitializeComponent();
		}

		private void TibiaLauncher_Load(object sender, RoutedEventArgs e)
		{
			currentVersion = webClient.DownloadString(urlVersion);
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
				StreamReader reader = new(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt");
				string? myVersion = reader.ReadLine();
				reader.Close();

				labelVersion.Text = "v" + currentVersion;

				if (currentVersion == myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_play.png")));
					buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/icon_play.png"));
					labelDownloadPercent.Content = GetClientVersion();
					labelDownloadPercent.Visibility = Visibility.Visible;
					needUpdate = false;
				}

				if (currentVersion != myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_update.png")));
					buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/icon_update.png"));
					labelDownloadPercent.Content = "Download";
					buttonPlay.Visibility = Visibility.Visible;
					needUpdate = true;
				}
			}
			if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt"))
			{
				labelVersion.Text = "v" + currentVersion;
				buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_update.png")));
				buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/icon_update.png"));
				labelDownloadPercent.Content = "Download";
				buttonPlay.Visibility = Visibility.Visible;
				needUpdate = true;
			}
		}

		static string GetClientVersion()
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string json = path + "/CanaryClient/clientlauncherupdate-main/package.json";

			using (StreamReader stream = new(json))
			{
				string jsonString = stream.ReadToEnd();
				dynamic versionclient = JsonConvert.DeserializeObject(jsonString);
				foreach (string version in versionclient)
				{
					return version;
				}
			}
			return "Play Game";
		}

		private void ButtonPlay_Click(object sender, RoutedEventArgs e)
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
			buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/button_play.png")));
			buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Assets/icon_play.png"));

			Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient");
			ZipFile.ExtractToDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/tibia.zip", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient", true);
			File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/tibia.zip");
			progressbarDownload.Value = 100;
			needUpdate = false;
			clientDownloaded = true;
			labelDownloadPercent.Content = "Play Game";
			buttonPlay.Visibility = Visibility.Visible;
			progressbarDownload.Visibility = Visibility.Collapsed;
			labelDownloadPercent.Visibility = Visibility.Visible;
		}

		private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			progressbarDownload.Value = e.ProgressPercentage;
			labelDownloadPercent.Content = SizeSuffix(e.BytesReceived) + " / " + SizeSuffix(e.TotalBytesToReceive);
		}

		static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
		static string SizeSuffix(Int64 value, int decimalPlaces = 1)
		{
			if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException(nameof(decimalPlaces)); }
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

		private void ButtonPlay_MouseEnter(object sender, MouseEventArgs e)
		{
			if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt"))
			{
				StreamReader reader = new(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt");
				string? myVersion = reader.ReadLine();
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

		private void ButtonPlay_MouseLeave(object sender, MouseEventArgs e)
		{
			if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt"))
			{
				StreamReader reader = new(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient/clientlauncherupdate-main/version.txt");
				string? myVersion = reader.ReadLine();
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

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void RestoreButton_Click(object sender, RoutedEventArgs e)
		{
			if (ResizeMode != ResizeMode.NoResize)
			{
				if (WindowState == WindowState.Normal)
					WindowState = WindowState.Maximized;
				else
					WindowState = WindowState.Normal;
			}
		}

		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}
	}
}
