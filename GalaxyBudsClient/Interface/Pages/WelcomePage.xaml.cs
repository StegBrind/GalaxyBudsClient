﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Items;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Serilog;

namespace GalaxyBudsClient.Interface.Pages
{
 	public class WelcomePage : AbstractPage
	{
		public override Pages PageType => Pages.Welcome;

		private readonly SwitchDetailListItem _darkMode;
		private readonly Border _locale;

		private ContextMenu? _localeMenu;

		public WelcomePage()
		{   
			AvaloniaXamlLoader.Load(this);

			_darkMode = this.FindControl<SwitchDetailListItem>("DarkMode");
			_locale = this.FindControl<Border>("Locales");
		}

		public override void OnPageShown()
		{
			_darkMode.IsChecked = SettingsProvider.Instance.DarkMode == DarkModes.Dark;
			
			var localeMenuActions =
				new Dictionary<string,EventHandler<RoutedEventArgs>?>();
			
#pragma warning disable 8605
			foreach (int value in Enum.GetValues(typeof(Locales)))
#pragma warning restore 8605
			{
				if (value == (int)Locales.custom && !Loc.IsTranslatorModeEnabled())
					continue;

				var locale = (Locales)value;
				localeMenuActions[locale.GetDescription()] = (sender, args) =>
				{
					SettingsProvider.Instance.Locale = locale;
					Loc.Load();
				};
			}
			
			_localeMenu = MenuFactory.BuildContextMenu(localeMenuActions, _locale);
		}
		
		private void Next_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			// Only search for the Buds app on Windows 10 and above
			if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 10) {
				// Using PowerShell because I couldn't find a native way to get a list of UWP apps
				ProcessStartInfo si = new ProcessStartInfo {
					FileName = "powershell",
					Arguments = "Get-AppxPackage SAMSUNGELECTRONICSCO.LTD.GalaxyBuds",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};
				var pro = Process.Start(si);
				pro.WaitForExit();
				var result = pro.StandardOutput.ReadToEnd();
				if (result.Contains("SAMSUNGELECTRONICSCO.LTD.GalaxyBuds")) {
					MainWindow.Instance.Pager.SwitchPage(Pages.BudsAppDetected);
				} else {
					MainWindow.Instance.Pager.SwitchPage(Pages.DeviceSelect);
				}
			}
		}

		private void DarkMode_OnToggled(object? sender, bool e)
		{
			SettingsProvider.Instance.DarkMode = e ? DarkModes.Dark : DarkModes.Light;
			
			if (Application.Current is App app)
			{
				app.RestartApp(Pages.Welcome);
			}
		}

		private void Locales_OnPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			_localeMenu?.Open(_locale);
		}
	}
}
