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
using System.Net.Http;

namespace CanaryLauncherUpdate
{

	public partial class MainWindow : Window
	{
		HttpClient httpClient = new HttpClient();
		WebClient webClient = new WebClient();

		bool clientDownloaded = false;
		bool needUpdate = false;
		string clientName = "client.exe";
		string urlClient = "https://github.com/lucasgiovannibr/clientlauncherupdate/archive/refs/heads/main.zip";
		string urlVersion = "https://raw.githubusercontent.com/lucasgiovannibr/clientlauncherupdate/main/version.txt";
		string currentVersion = "";
		string path = AppDomain.CurrentDomain.BaseDirectory.ToString();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void TibiaLauncher_Load(object sender, RoutedEventArgs e)
		{
			ImageLogoServer.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/logo.png"));
			ImageLogoCompany.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/logo_company.png"));

			currentVersion = httpClient.GetStringAsync(urlVersion).Result;
			progressbarDownload.Visibility = Visibility.Collapsed;
			labelClientVersion.Visibility = Visibility.Collapsed;
			labelDownloadPercent.Visibility = Visibility.Collapsed;

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			if (Directory.Exists(path + "/clientlauncherupdate-main"))
			{
				clientDownloaded = true;
			}

			if (File.Exists(path + "/clientlauncherupdate-main/version.txt"))
			{
				StreamReader reader = new StreamReader(path + "/clientlauncherupdate-main/version.txt");
				string? myVersion = reader.ReadLine();
				reader.Close();

				labelVersion.Text = "v" + currentVersion;

				if (currentVersion == myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/button_play.png")));
					buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/icon_play.png"));
					labelClientVersion.Content = GetClientVersion(path);
					labelClientVersion.Visibility = Visibility.Visible;
					buttonPlay_tooltip.Text = "Play Game";
					needUpdate = false;
				}

				if (currentVersion != myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/button_update.png")));
					buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/icon_update.png"));
					labelClientVersion.Content = "Download";
					labelClientVersion.Visibility = Visibility.Visible;
					buttonPlay.Visibility = Visibility.Visible;
					buttonPlay_tooltip.Text = "Update";
					needUpdate = true;
				}
			}
			if (!File.Exists(path + "/clientlauncherupdate-main/version.txt"))
			{
				labelVersion.Text = "v" + currentVersion;
				buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/button_update.png")));
				buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/icon_update.png"));
				labelClientVersion.Content = "Download";
				labelClientVersion.Visibility = Visibility.Visible;
				buttonPlay.Visibility = Visibility.Visible;
				buttonPlay_tooltip.Text = "Download";
				needUpdate = true;
			}
		}

		static string GetClientVersion(string path)
		{
			string json = path + "/clientlauncherupdate-main/package.json";
			StreamReader stream = new StreamReader(json);
			dynamic jsonString = stream.ReadToEnd();
			dynamic versionclient = JsonConvert.DeserializeObject(jsonString);
			foreach (string version in versionclient)
			{
				return version;
			}
			return "Play Game";
		}

		private void buttonPlay_Click(object sender, RoutedEventArgs e)
		{
			if (needUpdate == true)
			{
				try
				{
					labelDownloadPercent.Visibility = Visibility.Visible;
					progressbarDownload.Visibility = Visibility.Visible;
					labelClientVersion.Visibility = Visibility.Collapsed;
					buttonPlay.Visibility = Visibility.Collapsed;
					webClient.DownloadProgressChanged += Client_DownloadProgressChanged;
					webClient.DownloadFileCompleted += Client_DownloadFileCompleted;
					webClient.DownloadFileAsync(new Uri(urlClient), path + "/tibia.zip");
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
					Process.Start(path + "/clientlauncherupdate-main/bin/" + clientName);
					this.Close();
				}
				else
				{
					try
					{
						labelDownloadPercent.Visibility = Visibility.Visible;
						progressbarDownload.Visibility = Visibility.Visible;
						labelClientVersion.Visibility = Visibility.Collapsed;
						buttonPlay.Visibility = Visibility.Collapsed;
						webClient.DownloadProgressChanged += Client_DownloadProgressChanged;
						webClient.DownloadFileCompleted += Client_DownloadFileCompleted;
						webClient.DownloadFileAsync(new Uri(urlClient), path + "/tibia.zip");
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
			buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/button_play.png")));
			buttonPlayIcon.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/icon_play.png"));

			Directory.CreateDirectory(path);
			ZipFile.ExtractToDirectory(path + "/tibia.zip", path, true);
			File.Delete(path + "/tibia.zip");
			progressbarDownload.Value = 100;
			needUpdate = false;
			clientDownloaded = true;
			labelClientVersion.Content = "Play Game";
			buttonPlay_tooltip.Text = "Play Game";
			labelClientVersion.Visibility = Visibility.Visible;
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
			if (File.Exists(path + "/clientlauncherupdate-main/version.txt"))
			{
				StreamReader reader = new StreamReader(path + "/clientlauncherupdate-main/version.txt");
				string? myVersion = reader.ReadLine();
				reader.Close();

				if (currentVersion != myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/button_hover_update.png")));
				}
				if (currentVersion == myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/button_hover_play.png")));
				}
			}
			else
			{
				buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/button_hover_update.png")));
			}
		}

		private void buttonPlay_MouseLeave(object sender, MouseEventArgs e)
		{
			if (File.Exists(path + "/clientlauncherupdate-main/version.txt"))
			{
				StreamReader reader = new StreamReader(path + "/clientlauncherupdate-main/version.txt");
				string? myVersion = reader.ReadLine();
				reader.Close();

				if (currentVersion != myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/button_update.png")));
				}
				if (currentVersion == myVersion)
				{
					buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/button_play.png")));
				}
			}
			else
			{
				buttonPlay.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "pack://application:,,,/Assets/button_update.png")));
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
