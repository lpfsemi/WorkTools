using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkTools.Models;
using WorkTools.Services;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using WorkTools.Views.Pages;
using WorkTools.Log;
using System.Threading.Tasks;
using System;
using System.Reflection.Metadata;

namespace WorkTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
            .ConfigureServices((context, services) =>
            {
                // App Host
                services.AddHostedService<ApplicationHostService>();

                // Page resolver service
                services.AddSingleton<IPageService, PageService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddScoped<INavigationWindow, Views.Windows.MainWindow>();
                services.AddScoped<ViewModels.MainWindowViewModel>();

                // Views and ViewModels
                services.AddScoped<Views.Pages.DashboardPage>();
                services.AddScoped<ViewModels.DashboardViewModel>();
                services.AddScoped<Views.Pages.DataPage>();
                services.AddScoped<ViewModels.DataViewModel>();
                services.AddScoped<Views.Pages.PLCPage>();
                services.AddScoped<ViewModels.PLCViewModel>();
                services.AddScoped<Views.Pages.ControlInstrumentPage>();
                services.AddScoped<ViewModels.ControlInstrumentViewModel>();
                services.AddScoped<Views.Pages.SettingsPage>();
                services.AddScoped<ViewModels.SettingsViewModel>();
                services.AddScoped<RecipePage>();
                services.AddScoped<ViewModels.RecipeViewModel>();
                // Configuration
                services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();
            // 开启日志服务
            LogService.Register();
            LogService.Logger.Info($"App Opened...");
            ExceptionHandler();
        }
        /// <summary>
        ///     异常处理监听
        /// </summary>
        private void ExceptionHandler()
        {
            //UI线程未捕获异常处理事件（UI主线程）
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }
        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();
            LogService.Logger.Info($"App Closed...");
            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }

        //UI线程未捕获异常处理事件（UI主线程）
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            //异常信息 和 调用堆栈信息
            //string msg = String.Format("{0}\n\n{1}", ex.Message, ex.StackTrace);
            LogService.Logger.Error("UI线程异常", ex);
            e.Handled = true; //表示异常已处理，可以继续运行
        }

        //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
        //如果UI线程异常DispatcherUnhandledException未注册，则如果发生了UI线程未处理异常也会触发此异常事件
        //此机制的异常捕获后应用程序会直接终止。没有像DispatcherUnhandledException事件中的Handler=true的处理方式，可以通过比如Dispatcher.Invoke将子线程异常丢在UI主线程异常处理机制中处理
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                //string msg = String.Format("{0}\n\n{1}", ex.Message, ex.StackTrace);
                LogService.Logger.Error("非UI线程异常", ex);
        }

        //Task线程内未捕获异常处理事件
        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            var ex = e.Exception;
            //string msg = String.Format("{0}\n\n{1}", ex.Message, ex.StackTrace);
            LogService.Logger.Error("Task异常", ex);
        }
    }
}