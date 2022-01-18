namespace cswm.TaskBarApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool traceEnabled = args.Any(arg => arg.Contains("--trace-enabled"));

            ApplicationConfiguration.Initialize();
            Application.Run(new TaskBarApp(traceEnabled: traceEnabled));
        }
    }
}