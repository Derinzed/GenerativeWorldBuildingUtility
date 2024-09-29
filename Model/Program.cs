using GenerativeWorldBuildingUtility.Model;
using System.Windows;
using System.IO;
using GenerativeWorldBuildingUtility.View;
using System.Runtime.InteropServices;
using JohnUtilities.Classes;
using JohnUtilities.Services.Interfaces;
using JohnUtilities.Services.Adapters;
using JohnUtilities.Model.Classes;
using GenerativeWorldBuildingUtility.ViewModel;
using System.Windows.Forms;

#if RELEASE
using AutoUpdaterDotNET;
#endif

public class Program
{
    public Program()
    {

    }
    public static System.Windows.Application? WinApp { get; private set; }
    public static Window? MainWindow { get; private set; }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool AllocConsole(); // Create console window

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow(); // Get console window handle

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;

    static void InitializeWindows()
    {
        WinApp = new System.Windows.Application();
        WinApp.Run(MainWindow); // note: blocking call
    }
    static void ShowConsole()
    {
        var handle = GetConsoleWindow();
        if (handle == IntPtr.Zero)
            AllocConsole();
        else
            ShowWindow(handle, SW_SHOW);
    }

    static void HideConsole()
    {
        var handle = GetConsoleWindow();
        if (handle != null)
            ShowWindow(handle, SW_HIDE);
    }

    [STAThread]
    public static void Main()
    {

#if RELEASE
        AutoUpdater.Start(@"https://github.com/Derinzed/GenerativeWorldBuildingUtility/blob/Pre-Alpha_DEV/Update.xml");
        AutoUpdater.DownloadPath = System.Windows.Forms.Application.StartupPath;
#endif

        Logging.GetLogger().Init(new JU_StreamWriter("Log.txt", true), null);
        MainWindow = new MainWindow();
        
        //ShowConsole();

        var Events = new GeneratorBaseEvents();
        Events.SetupEvents();
        var EventManager = EventReporting.GetEventReporter();

        var generator = new TextGenerator();

        PromptGenerator PromptGen = new PromptGenerator(new JohnUtilities.Classes.ConfigurationManager(new ConfigLoading(new JU_XMLService()), new FileManager(new JU_FileService(), new ProcessesManager()), new List<ConfigurationElement>()), generator);
        PromptGen.LoadPrompts();
        PromptGen.LoadRandomizedData();

        var result = PromptGen.GetRandomData("Quest", "LocationTypes", 3, true);

        var Generator = new Generator(PromptGen);

        //var result = Generator.Generate("NPC");

        var AllDataLists = PromptGen.GetPromptDataLists("NPC");

        var ViewModel = new MainWindowViewModel(Generator);

        //ViewModel.RandomElements[0].Children[0].Active = false;

        ViewModel.PropertyChanged += PromptGen.OnRandomElementUpdated;

        MainWindow.DataContext = ViewModel;

        //if (!File.Exists("appsettings.json"))
        //{
           // generator.InitialSetup();
        //}

        InitializeWindows();
    }

}