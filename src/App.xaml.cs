using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CanaryLauncherUpdate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			App app = new App();
			app.InitializeComponent();
			app.Run();
		}
	}
}
