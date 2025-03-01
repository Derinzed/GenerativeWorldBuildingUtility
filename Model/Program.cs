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
using UpdaterLibrary;



public class Program
{
    public Program()
    {

    }
    public static System.Windows.Application? WinApp { get; private set; }
    public static MainWindow? MainWindow { get; private set; }

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

        
        Logging.GetLogger().Init(new JU_StreamWriter("Log.txt", true), null);
        MainWindow = new MainWindow();

        Logging.WriteLogLine("Begining Updates:");
        Logging.WriteLogLine("Execution path: " + System.Windows.Forms.Application.ExecutablePath);
        Logging.WriteLogLine("XML path: " + @"https://raw.githubusercontent.com/Derinzed/GenerativeWorldBuildingUtility/refs/heads/Pre-Alpha_DEV/Update.xml");

#if RELEASE || RELEASEFULLSUBSCRIPTION
        try
        {
            Updater updater = new Updater(System.Windows.Forms.Application.ExecutablePath, @"https://raw.githubusercontent.com/Derinzed/GenerativeWorldBuildingUtility/refs/heads/Pre-Alpha_DEV/Update.xml");
            if (updater.UpdateAvailable())
            {
                Logging.WriteLogLine("Updates are available. Obtaining them.");
                updater.GetUpdates();
                updater.StartUpdating();
            }
            
        }
        catch (Exception ex)
        {
            Logging.WriteLogLine("There was an error during updating: " + ex.Message);
            Logging.WriteLogLine(ex.InnerException.Message);
        }
#endif

        //ShowConsole();

        var Events = new GeneratorBaseEvents();
        Events.SetupEvents();
        var EventManager = EventReporting.GetEventReporter();

        var generator = new TextGenerator();

        PromptGenerator PromptGen = new PromptGenerator(new JohnUtilities.Classes.ConfigurationManager(new ConfigLoading(new JU_XMLService()), new FileManager(new JU_FileService(), new ProcessesManager()), new List<ConfigurationElement>()), generator);
        PromptGen.LoadPrompts();
        //PromptGen.LoadRandomizedData();
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OpenAIAPIKey")))
        {
            PromptGen.UseServer = true;
        }

        var result = PromptGen.GetRandomData("Quest", "LocationTypes", 3, true);

        var Generator = new Generator(PromptGen, generator);

        //var result = Generator.Generate("NPC");

        var AllDataLists = PromptGen.GetPromptDataLists("NPC");

        var ViewModel = new MainWindowViewModel(Generator);

        //ViewModel.RandomElements[0].Children[0].Active = false;

        ViewModel.PropertyChanged += PromptGen.OnRandomElementUpdated;

        MainWindow.DataContext = ViewModel;
        MainWindow.VM = ViewModel;
        MainWindow.SetupAIModelSettingsView();

        //if (!File.Exists("appsettings.json"))
        //{
           // generator.InitialSetup();
        //}

        InitializeWindows();
    }

}