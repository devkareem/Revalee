﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;

namespace Revalee.Service
{
	partial class Revalee : ServiceBase
	{
		private const int SERVICE_ACCEPT_PRESHUTDOWN = 0x100;
		private const int SERVICE_CONTROL_PRESHUTDOWN = 0xf;

		public Revalee()
		{
			InitializeComponent();

			if (Environment.OSVersion.Version.Major >= 6)
			{
				AcceptPreShutdown();
			}
		}

		protected override void OnContinue()
		{
			Supervisor.Resume();
			base.OnContinue();
		}

		protected override void OnPause()
		{
			Supervisor.Pause();
			base.OnPause();
		}

		protected override void OnStart(string[] args)
		{
			try
			{
				Supervisor.Start();
				base.OnStart(args);
			}
			catch (Exception ex)
			{
				Supervisor.LogException(ex, TraceEventType.Critical, "Service failed to start");

				using (var controller = new ServiceController(this.ServiceName))
				{
					controller.Stop();
				}

				this.ExitCode = 1;
			}
		}

		protected override void OnStop()
		{
			try
			{
				Supervisor.Stop();
			}
			catch { }

			base.OnStop();
		}

		protected override void OnShutdown()
		{
			try
			{
				Supervisor.Shutdown();
			}
			catch { }

			base.OnShutdown();

			this.Stop();
		}

		protected override void OnCustomCommand(int command)
		{
			if (command == SERVICE_CONTROL_PRESHUTDOWN)
			{
				try
				{
					Supervisor.Shutdown();
				}
				catch { }
			}

			base.OnCustomCommand(command);

			this.Stop();
		}


		[MTAThread]
		public static void Main()
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			if (commandLineArgs != null && commandLineArgs.Length == 2)
			{
				if (commandLineArgs[1] == "-?" || string.Equals(commandLineArgs[1], "-help", StringComparison.OrdinalIgnoreCase))
				{
					InteractiveExecution.Help();
					return;
				}
				else if (string.Equals(commandLineArgs[1], "-install", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						CommandLineInstaller installer = new CommandLineInstaller();
						installer.Install();
					}
					catch (Exception ex)
					{
						Environment.ExitCode = 1;
						Console.WriteLine();
						Console.WriteLine(ex.Message);
					}

					return;
				}
				else if (string.Equals(commandLineArgs[1], "-uninstall", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						CommandLineInstaller installer = new CommandLineInstaller();
						installer.Uninstall();
					}
					catch (Exception ex)
					{
						Environment.ExitCode = 1;
						Console.WriteLine();
						Console.WriteLine(ex.Message);
					}

					return;
				}
				else if (string.Equals(commandLineArgs[1], "-interactive", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						InteractiveExecution.Run();
					}
					catch (Exception ex)
					{
						try
						{
							Supervisor.LogException(ex, TraceEventType.Critical, "Service terminating on error");
						}
						catch { }

						Environment.ExitCode = 1;
					}
					return;
				}
				else if (string.Equals(commandLineArgs[1], "-export", StringComparison.OrdinalIgnoreCase))
				{
					TaskExporter.DumpToConsole();
					return;
				}
			}

			try
			{
				AppDomain.CurrentDomain.UnhandledException += ServiceCallbackUnhandledExceptionHandler;
				ServiceBase.Run(new Revalee());
			}
			catch (Exception ex)
			{
				try
				{
					Supervisor.LogException(ex, TraceEventType.Critical, "Service encountered a critical error");
				}
				catch
				{ }
			}
		}

		private static void ServiceCallbackUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			try
			{
				Supervisor.LogException((Exception)args.ExceptionObject, TraceEventType.Critical, "Service encountered a critical error");
			}
			catch
			{ }
		}

		private void AcceptPreShutdown()
		{
			FieldInfo acceptedCommandsFieldInfo = typeof(ServiceBase).GetField("acceptedCommands", BindingFlags.Instance | BindingFlags.NonPublic);
			if (acceptedCommandsFieldInfo != null)
			{
				acceptedCommandsFieldInfo.SetValue(this, ((int)acceptedCommandsFieldInfo.GetValue(this)) | SERVICE_ACCEPT_PRESHUTDOWN);
			}
		}
	}
}