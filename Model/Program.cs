using GenerativeWorldBuildingUtility.Model;
using System.Windows;
using System.IO;
using GenerativeWorldBuildingUtility.View;
using System.Runtime.InteropServices;

public class Program
{
    public Program()
    {

    }
    public static Application? WinApp { get; private set; }
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
        WinApp = new Application();
        WinApp.Run(MainWindow = new MainWindow()); // note: blocking call
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
        ShowConsole();
        //InitializeWindows();

        var generator = new TextGenerator();

        if (!File.Exists("appsettings.json"))
        {
            generator.InitialSetup();
        }

        PromptGenerator PromptGen = new PromptGenerator();
        PromptGen.LoadPrompts();

        PromptGen.LoadRandomizedElements();

        Console.WriteLine(PromptGen.GetRandomElementFromList("LocationTypes"));

        Utilities.GetTextBetweenCharacters("TEST;A $Age$ fantasy character of race $Race$ with long hair and wireframed glasses.  He weilds a $Weapon$.", "\\$", "\\$");

        //var result = generator.GenerateText("A Dungeons and Dragons quest for a level 5 party that starts in a tavern. There should be a named sorceror enemy.  The quest should happen in an icy underground cave.");
        //Console.WriteLine(result);
        Console.ReadLine();
    }
}