using System.Security.Cryptography.X509Certificates;
using CommandLine;
using CommandLine.Text;

namespace AfPerformance
{
    /// <summary>
    ///     Class that contains command line options, and associated variables.
    ///     see http://commandline.codeplex.com/
    ///     see https://github.com/gsscoder/commandline/wiki
    /// </summary>
    public class ConfigOptions
    {

        [Option('s', "server", HelpText = "AF Server Name", DefaultValue = "CSI00812", Required = true)]
        public string Server { get; set; }

        [Option('d', "database", HelpText = "AF Database name", DefaultValue = "DongEnergyWind", Required = true)]
        public string Database { get; set; }

        [Option('t', "casethreadcount", HelpText = "Number of available threads in each test case", DefaultValue = 1, Required = true)]
        public int TestCaseThreadcount { get; set; }

        [Option('u', "runthreadcount", HelpText = "Number of available threads in the test run (number of parallel test cases)", DefaultValue = 1, Required = true)]
        public int TestRunThreadCount { get; set; }

        [Option('n', "testcasenames", HelpText = "Comma separated list of testcase name substrings of test cases to run. (all will run all test cases)", DefaultValue = "all", Required = false)]
        public string TestCases { get; set; }

        [Option('c', "closeoneexit", HelpText = "yes/no to pause before closing console", DefaultValue = "no", Required = false)]
        public string CloseOnExit { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
