using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AfPerformance;
using CommandLine;
using log4net;
using log4net.Config;
using log4net.Util;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.Diagnostics;
using OSIsoft.AF.EventFrame;
using OSIsoft.AF.Search;
using OSIsoft.AF.Time;

namespace AFEventFramePerformanceTest
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        public static void Main(string[] args)
        {
            var argsString = $"Call arguments {string.Join(" ", args)}";
            log.InfoFormat(argsString);
            Console.WriteLine(argsString);

            var configOptions = new ConfigOptions();
            Parser.Default.ParseArguments(args, configOptions);

            var afPerformanceTest = new AfPerformanceTest(configOptions);

            afPerformanceTest.Run();

            if (configOptions.CloseOnExit.Equals("yes")) return;

            Console.WriteLine("Press any key to close ...");
            Console.ReadKey();
        }
    }

    public class AfPerformanceTest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ConfigOptions _configOptions;
        private AFDatabase _afDatabase;

        public AfPerformanceTest(ConfigOptions configOptions)
        {
            _configOptions = configOptions;
        }

        public void Run()
        {
            ConnectToAfDatabase(_configOptions.Server, _configOptions.Database);

            Log.InfoFormat(TestOutput.OutputFormat);
            Console.WriteLine(TestOutput.OutputFormat);

            var testRunThreadCount = _configOptions.TestRunThreadCount;
            
            var tasks = new List<Task>();
            var testOutputs = new List<TestOutput>();

            var testCaseSubstringNames = _configOptions.TestCases.ToLowerInvariant().Split(',').ToList();

            foreach (var type in GetTestCaseTypesToRun(testCaseSubstringNames))
            {
                    if (tasks.Count >= testRunThreadCount)
                    {
                        var indexOfCompletedTask = Task.WaitAny(tasks.ToArray());
                        tasks.Remove(tasks[indexOfCompletedTask]);
                    }
                    tasks.Add(Task.Run(() =>
                    {
                        var testCase = CreateTestCaseInstance(type);
                        if (testCase == null) return;

                        var testOutput = testCase.Run();
                        testOutput.TestRunThreadCount = _configOptions.TestRunThreadCount;
                        testOutput.TestCaseThreadCount = _configOptions.TestCaseThreadcount;
                        testOutput.Output();
                        testOutputs.Add(testOutput);
                    }));

            }
            Task.WaitAll(tasks.ToArray());
            var totals = GetTotals(testOutputs, _configOptions.TestRunThreadCount, _configOptions.TestCaseThreadcount);
            Console.WriteLine(totals);
            Log.InfoFormat(totals);
        }

        private string GetTotals(List<TestOutput> testOutputs, int testRunThreadCount, int testCaseThreadcount)
        {
            var totalSearchCount = testOutputs.Select(o => o.GetSearchCount()).Sum();
            var totalValuesCount = testOutputs.Select(o => o.GetTotalResultCount).Sum();
            var totalAvg = testOutputs.Select(o => o.GetTotalActionMillisOverSearchCount).Sum();
            var totalSearchTime = testOutputs.Select(o => o.GetTotalActionSecs).Sum();
            var totalTime = testOutputs.Select(o => o.GetTotalTestSecs()).Sum();

            var totals = $"Total;{testRunThreadCount};{testCaseThreadcount};{totalSearchCount};{totalValuesCount};{totalAvg};{totalSearchTime};{totalTime}";
            return totals;
        }

        private static List<Type> GetTestCaseTypesToRun(List<string> testCaseSubStrings)
        {
            var allTestCaseTypes = GetOrderedTestCaseTypes();
            var returnTypes = new List<Type>();
            if (testCaseSubStrings.First().Equals("all"))
                return allTestCaseTypes.ToList();

            foreach (var testCaseSubString in testCaseSubStrings)
            {
                returnTypes.AddRange(allTestCaseTypes.Where(testCaseType => testCaseType.Name.ToLowerInvariant().Contains(testCaseSubString)));
            }
            return returnTypes;
        }

        private BaseTestCase CreateTestCaseInstance(Type type)
        {
            return Activator.CreateInstance(type, _afDatabase, _configOptions.TestCaseThreadcount) as BaseTestCase;
        }

        private static IOrderedEnumerable<Type> GetOrderedTestCaseTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(BaseTestCase).IsAssignableFrom(t)).OrderBy(t => t.Name);
        }

        public void ConnectToAfDatabase(string afServer, string afDatabase)
        {
            XmlConfigurator.Configure();

            PISystems myPISystems = new PISystems();

            PISystem myPISystem = string.IsNullOrEmpty(afServer) ? myPISystems.DefaultPISystem : myPISystems[afServer];
            myPISystem.Connect();
            Log.InfoFormat("SDK: {0}, Server: {1}", myPISystems.Version, myPISystem.ServerVersion);

            _afDatabase = string.IsNullOrEmpty(afDatabase) ? myPISystem.Databases.DefaultDatabase : myPISystem.Databases[afDatabase];

            Log.InfoFormat("PI System {0} ; Database {1}", myPISystem.Name, _afDatabase.Name);

        }
    }

    //public string I_ElementName_AFEventFrameSearch_TimeTest(int threadNum = 0)
    //{

    //    //_log.Info($"$ReviewStatus_AFEventFrameSearch on {_environment}");
    //    return $"I{threadNum}; Avg time = " +
    //        Math.Round((double)
    //        (
    //            ElementName_AFEventFrameSearch("ANH01F01") +
    //            ElementName_AFEventFrameSearch("BKR01G02") +
    //            ElementName_AFEventFrameSearch("GFS01B04") +
    //            ElementName_AFEventFrameSearch("HR2A06") +
    //            ElementName_AFEventFrameSearch("LAW01M17") +
    //            ElementName_AFEventFrameSearch("NHPC02") +
    //            ElementName_AFEventFrameSearch("WDS01I08") +
    //            ElementName_AFEventFrameSearch("HR2*") +
    //            ElementName_AFEventFrameSearch("LAW01*") +
    //            ElementName_AFEventFrameSearch("NHP*") +
    //            ElementName_AFEventFrameSearch("WDS01*")
    //        ) / 11) + "ms";
    //}


    //public string J_ClassificationValue_ElementName_AFEventFrameSearch_TimeTest(int threadNum = 0)
    //{

    //    //_log.Info($"$ReviewStatus_AFEventFrameSearch on {_environment}");
    //    return $"J{threadNum}; Avg time = " +
    //        Math.Round((double)
    //        (
    //            ClassificationValue_AFEventFrameSearch("1.4.6", "ANH01A05") +
    //            ClassificationValue_AFEventFrameSearch("1.2.0", "BKR01G02") +
    //            ClassificationValue_AFEventFrameSearch("1.2.0", "GFS01B04") +
    //            ClassificationValue_AFEventFrameSearch("1.4.1", "HR2A06") +
    //            ClassificationValue_AFEventFrameSearch("1.3.0", "LAW01M17") +
    //            ClassificationValue_AFEventFrameSearch("1.1.6", "NHPC02") +
    //            ClassificationValue_AFEventFrameSearch("1.4.1", "WDS01I08") +
    //            ClassificationValue_AFEventFrameSearch("1.1.1", "HR2*") +
    //            ClassificationValue_AFEventFrameSearch("1.1.15","LAW01*") +
    //            ClassificationValue_AFEventFrameSearch("1.3.0", "NHP*") +
    //            ClassificationValue_AFEventFrameSearch("1.1.6", "WDS01*") +
    //            ClassificationValue_AFEventFrameSearch("1.1.6", "ANH01*")
    //        ) / 12) + "ms";
    //}

    //public string L_ClassificationValue_AFEventFrameSearch_TimeTest(int threadNum = 0)
    //{

    //    //_log.Info($"$ReviewStatus_AFEventFrameSearch on {_environment}");
    //    return $"J{threadNum}; Avg time = " +
    //        Math.Round((double)
    //        (
    //            ClassificationValue_AFEventFrameSearch("1.1.9") +
    //            ClassificationValue_AFEventFrameSearch("1.2.0") +
    //            ClassificationValue_AFEventFrameSearch("1.4.1") +
    //            ClassificationValue_AFEventFrameSearch("1.3.0") +
    //            ClassificationValue_AFEventFrameSearch("1.4.1") +
    //            ClassificationValue_AFEventFrameSearch("1.1.1") +
    //            ClassificationValue_AFEventFrameSearch("1.1.15") +
    //            ClassificationValue_AFEventFrameSearch("1.3.0") +
    //            ClassificationValue_AFEventFrameSearch("1.1.6")
    //        ) / 9) + "ms";
    //}

    #region TestCases
    public class AllEventFramesInDatabase : BaseTestCase
    {
        public AllEventFramesInDatabase(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
        }


        protected override List<Action> GetActions()
        {
            List<Action> actions = Constants.SourceSystemUniqueIds.Select(sourceSystemUniqueId => (Action)(() =>
            {
                var actionStopwatch = new AFStopwatch();
                actionStopwatch.Start();

                var eventFrames = AFEventFrame.FindEventFrames(AfDatabase, null, "*", AFSearchField.Name, true,
                    AFSortField.Name, AFSortOrder.Ascending, 0, int.MaxValue);
                TestOutput.AddActionResult(
                    new ActionPerformanceResult
                    {
                        ActionMillis = actionStopwatch.ElapsedMilliseconds,
                        ResultCount = eventFrames.Count
                    });
            })).ToList();
            return actions;
        }

    }

    public class BiRecordedValuesWindFarm : BaseTestCase
    {
        private readonly string[] _attributeNames = new string[]
        {
                "WMET.WdSpd2|WMET.WdSpd2.Av",
                "WNAC.WdSpdAct|WNAC.WdSpdAct.Av",
                "WTUR.PwrAt|WTUR.PwrAt.Tot|WRPT.MxTmq",
                "WSLG.TurTmLog|WSLG.TurTmTotLog",
                "WRPT.EgyCap",
                "WRPT.EgyEst",
                "WSLG.TurTmLog|WSLG.TurTmTotLog",
                "WRPT.TurTmLog|WRPT.TurTmFltLog",
                "WRPT.TurTmLog|WRPT.TurTmProdLog",
                "WRPT.TurTmLog|WRPT.TurDnLog",
                "WRPT.EgyCap|WRPT.EgyTotCap",
                "WRPT.EgyCap|WRPT.EgyProdCap",
                "WRPT.EgyLossBaselineCalc|WRPT.EgyLossBaselineTotCalc",
                "WRPT.EgyCap|WRPT.EgyFltCap",
                "WRPT.EgyEst"
        };

        public BiRecordedValuesWindFarm(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
            WindFarms = "ANH01,BKR01";
            Start = new AFTime("2016-08-01");
            End = new AFTime("2016-08-05");
        }

        public string WindFarms { get; set; }

        public AFTime End { get; set; }

        public AFTime Start { get; set; }

        public AFTimeRange TimeRange => new AFTimeRange(Start, End);

        protected override List<Action> GetActions()
        {
            if (string.IsNullOrEmpty(WindFarms))
                return new List<Action>();

            var splittedWindFarms = WindFarms.Split(',');

            var actionz = new List<Action>();

            foreach (var windFarm in splittedWindFarms)
            {
                var afEventFrameSearch = GetEventFramesFromAfEventFrameSearch("EFOutageClassification",
                    $"{windFarm}*", "Deleted", false);
                var eventFramesGroupedByElement = afEventFrameSearch.GroupBy(ef => ef.PrimaryReferencedElement);

                foreach (var attributeName in _attributeNames)
                {
                    actionz.AddRange(eventFramesGroupedByElement.Select(group => (Action)(() =>
                    {
                        var actionStopwatch = new AFStopwatch();
                        actionStopwatch.Start();

                        AFValues afValues = GetRecordedValues(group.Key, attributeName, TimeRange);
                        TestOutput.AddActionResult(
                            new ActionPerformanceResult
                            {
                                ActionMillis = actionStopwatch.ElapsedMilliseconds,
                                ResultCount = afValues?.Count ?? 0
                            });
                    })));
                }

            }
            return actionz;
        }


        private AFValues GetRecordedValues(AFElement afElement, string attributeName, AFTimeRange timeRange)
        {
            AFAttribute afAttribute = GetAfAttributeByPath(afElement, attributeName);

            if (afAttribute == null)
            {
                Console.WriteLine($"{attributeName} missing on {afElement.Name} in {AfDatabase.Name}");
            }
            var afValues = afAttribute?.Data.RecordedValues(timeRange, AFBoundaryType.Inside, afAttribute.DefaultUOM,
                string.Empty, true, 0);
            return afValues;
        }

        private AFAttribute GetAfAttributeByPath(AFElement afElement, string attributeName)
        {
            var attributePath = $@"\\CSI00812\{AfDatabase.Name}\{afElement.Name}|{attributeName}";
            return AFObject.FindObject(attributePath) as AFAttribute;
        }

        private List<AFEventFrame> GetEventFramesFromAfEventFrameSearch(string templateName, string elementName,
            string attributeName,
            object value)
        {
            string query = $"Template:{templateName} SortField:ID";
            if (elementName != null)
            {
                query += $" ElementName:{elementName}";
            }

            if (attributeName != null)
            {
                query += $" |{attributeName}:={value}";
            }

            var afEventFrameSearch = new AFEventFrameSearch(AfDatabase, "MyQuery" + value, AFSearchMode.StartInclusive, Start, End,
                query);

            return afEventFrameSearch.FindEventFrames().ToList();
        }
    }

    public class EfsCheckExistingSourceSystemUniqueIdExtendedProperty : BaseTestCase
    {
        public EfsCheckExistingSourceSystemUniqueIdExtendedProperty(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
        }


        protected override List<Action> GetActions()
        {
            List<Action> actions = Constants.SourceSystemUniqueIds.Select(sourceSystemUniqueId => (Action)(() =>
            {
                var actionStopwatch = new AFStopwatch();
                actionStopwatch.Start();

                var eventFrames = AFEventFrame.FindEventFramesByExtendedProperty(AfDatabase, "SourceSystemId",
                    new[] { sourceSystemUniqueId }, 1);
                TestOutput.AddActionResult(
                    new ActionPerformanceResult
                    {
                        ActionMillis = actionStopwatch.ElapsedMilliseconds,
                        ResultCount = eventFrames.Count
                    });
            })).ToList();
            return actions;
        }

    }

    public class WtlEventFrameReviewStatusCount : BaseTestCase
    {
        public WtlEventFrameReviewStatusCount(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
        }


        protected override List<Action> GetActions()
        {
            var attribute = "WSLG.CtVibMxVibEvtStNew";
            List<Action> actions = Constants.WindFarms.Select(windFarm => (Action)(() =>
            {
                var actionStopwatch = new AFStopwatch();
                actionStopwatch.Start();

                var afElementSearch = new AFElementSearch(AfDatabase, "MyQuery",
                    "Name:" + windFarm + " CategoryName:WTG");

                var elements = afElementSearch.FindElements().ToList();

                //Console.WriteLine("Done : " + elements.Count() + ", " + stopwatch.ElapsedMilliseconds + "ms");

                AFElement.LoadAttributes(elements,
                    new List<AFAttributeTemplate>
                    {
                            AfDatabase.ElementTemplates["WtgBaseTemplate"].AttributeTemplates["WSLG.EvtCt"]
                                .AttributeTemplates[attribute]
                    });

                //var countDictionary = new Dictionary<string, int>();

                //foreach (var afElement in elements)
                //{
                //    var count = afElement.Attributes["WSLG.EvtCt"].Attributes[attribute].GetValue().ValueAsInt32();

                //    if (count > 0)
                //    {
                //        countDictionary[afElement.Name] = count;
                //    }
                //}

                TestOutput.AddActionResult(
                    new ActionPerformanceResult
                    {
                        ActionMillis = actionStopwatch.ElapsedMilliseconds,
                        ResultCount = elements.Count
                    });
            })).ToList();
            return actions;
        }

    }

    public class EfsCheckExistingSourceSystemUniqueIdAfSearch : BaseTestCase
    {
        public EfsCheckExistingSourceSystemUniqueIdAfSearch(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
        }


        protected override List<Action> GetActions()
        {
            List<Action> actions = Constants.SourceSystemUniqueIds.Select(sourceSystemUniqueId => (Action)(() =>
            {
                var actionStopwatch = new AFStopwatch();
                actionStopwatch.Start();

                string query = $"Template:EFTCMBaseTemplate |SourceSystemUniqueId:={sourceSystemUniqueId}  SortField:ID";
                // Sometimes we use AFSearchMode.StartInclusive other times AFSearchMode.Overlapped. StartInclusive we use when a start and end time is provided
                // Is there a performance difference?
                var afEventFrameSearch = new AFEventFrameSearch(AfDatabase, "ASearch", query);
                var maxCount = 1000;
                var eventFrames = afEventFrameSearch.FindEventFrames(0, true, maxCount).Take(maxCount).ToList();
                var count = eventFrames.Count;

                TestOutput.AddActionResult(
                    new ActionPerformanceResult
                    {
                        ActionMillis = actionStopwatch.ElapsedMilliseconds,
                        ResultCount = count
                    });
            })).ToList();
            return actions;
        }

    }

    public class OcsCheckExistingClassificationIdGuidAfSearch : BaseTestCase
    {
        public OcsCheckExistingClassificationIdGuidAfSearch(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
        }


        protected override List<Action> GetActions()
        {
            List<Action> actions = Constants.ClassificationIds.Select(classificationId => (Action)(() =>
            {
                var actionStopwatch = new AFStopwatch();
                actionStopwatch.Start();

                string query = $"Template:EFOutageClassification |Id:={classificationId}  SortField:ID";
                // Sometimes we use AFSearchMode.StartInclusive other times AFSearchMode.Overlapped. StartInclusive we use when a start and end time is provided
                // Is there a performance difference?
                var afEventFrameSearch = new AFEventFrameSearch(AfDatabase, "ASearch", query);
                var maxCount = 1000;
                var eventFrames = afEventFrameSearch.FindEventFrames(0, true, maxCount).Take(maxCount).ToList();
                var count = eventFrames.Count;

                TestOutput.AddActionResult(
                    new ActionPerformanceResult
                    {
                        ActionMillis = actionStopwatch.ElapsedMilliseconds,
                        ResultCount = count
                    });
            })).ToList();
            return actions;
        }

    }

    public class WtlEventListTurbinesNotReviewed : WtlEventListTurbines
    {
        public WtlEventListTurbinesNotReviewed(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|ReviewStatus:<>2";
        }
    }

    public class WtlEventListTurbinesReviewed : WtlEventListTurbines
    {
        public WtlEventListTurbinesReviewed(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|ReviewStatus:=2";
        }
    }

    public class WtlEventListTurbinesNew : WtlEventListTurbines
    {
        public WtlEventListTurbinesNew(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
            ValueQuery = "|ReviewStatus:=0";
        }
    }

    public class WtlEventListTurbinesObservation : WtlEventListTurbines
    {
        public WtlEventListTurbinesObservation(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|ReviewStatus:=-1";
        }
    }

    public class WtlEventListWindFarmsNotReviewed : WtlEventListWindFarms
    {
        public WtlEventListWindFarmsNotReviewed(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|ReviewStatus:<>2";
        }
    }

    public class WtlEventListWindFarmsReviewed : WtlEventListWindFarms
    {
        public WtlEventListWindFarmsReviewed(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|ReviewStatus:=2";
        }
    }

    public class WtlEventListWindFarmsNew : WtlEventListWindFarms
    {
        public WtlEventListWindFarmsNew(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
            ValueQuery = "|ReviewStatus:=0";
        }
    }

    public class WtlEventListWindFarmsObservation : WtlEventListWindFarms
    {
        public WtlEventListWindFarmsObservation(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|ReviewStatus:=-1";
        }
    }

    public class WtlIncidentListTurbinesCurrent : WtlIncidentListTurbines
    {
        public WtlIncidentListTurbinesCurrent(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|SAPNotificationStatus:<>Closed";
        }
    }

    public class WtlIncidentListTurbinesOpen : WtlIncidentListTurbines
    {
        public WtlIncidentListTurbinesOpen(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|SAPNotificationStatus:=Open";
        }
    }

    public class WtlIncidentListTurbinesClosed : WtlIncidentListTurbines
    {
        public WtlIncidentListTurbinesClosed(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|SAPNotificationStatus:=Closed";
        }
    }

    public class WtlIncidentListWindFarmsCurrent : WtlIncidentListWindFarms
    {
        public WtlIncidentListWindFarmsCurrent(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|SAPNotificationStatus:<>Closed";
        }
    }

    public class WtlIncidentListWindFarmsOpen : WtlIncidentListWindFarms
    {
        public WtlIncidentListWindFarmsOpen(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|SAPNotificationStatus:=Open";
        }
    }

    public class WtlIncidentListWindFarmsClosed : WtlIncidentListWindFarms
    {
        public WtlIncidentListWindFarmsClosed(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|SAPNotificationStatus:=Closed";
        }
    }

    public class WtlDiagnosisEvaluationReady : WtlIncidentListTestCase
    {
        public WtlDiagnosisEvaluationReady(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|ReadyForDiagnosisEvaluation:<>false |DiagnosisEvaluationStatus:=0";
        }
    }

    public class WtlDiagnosisEvaluationAll : WtlIncidentListTestCase
    {
        public WtlDiagnosisEvaluationAll(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
            ValueQuery = "|ReadyForDiagnosisEvaluation:<>false";
        }
    }

    public class WtlDiagnosisEvaluationCompleted : WtlIncidentListTestCase
    {
        public WtlDiagnosisEvaluationCompleted(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|ReadyForDiagnosisEvaluation:<>false |DiagnosisEvaluationStatus:=1";
        }
    }

    public class WtlVibrationFleetOverviewOpen : WtlIncidentListTestCase
    {
        public WtlVibrationFleetOverviewOpen(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|SAPNotificationStatus:=Open";
        }
    }

    public class WtlVibrationFleetOverviewClosed : WtlIncidentListTestCase
    {
        public WtlVibrationFleetOverviewClosed(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|SAPNotificationStatus:=Closed";
        }
    }

    public class WtlVibrationFleetOverviewAll : WtlIncidentListTestCase
    {
        public WtlVibrationFleetOverviewAll(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "";
        }
    }

    public class WtlSystemFleetOverviewOpen : WtlIncidentListTestCase
    {
        public WtlSystemFleetOverviewOpen(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|SAPNotificationStatus:=Open";
        }
    }

    public class WtlSystemFleetOverviewClosed : WtlIncidentListTestCase
    {
        public WtlSystemFleetOverviewClosed(AFDatabase afDatabase, int threadCount = 1)
            : base(afDatabase, threadCount)
        {
            ValueQuery = "|SAPNotificationStatus:=Closed";
        }
    }

    public class WtlSystemFleetOverviewAll : WtlIncidentListTestCase
    {
        public WtlSystemFleetOverviewAll(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
            ValueQuery = "";
        }
    }

    public class WtlEventListWindFarms : WtlEventListTestCase
    {
        public WtlEventListWindFarms(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
            ElementNames = Constants.WindFarms;
        }
    }

    public class WtlEventListTurbines : WtlEventListTestCase
    {
        public WtlEventListTurbines(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
            ElementNames = Constants.Turbines;
        }
    }

    public class WtlEventListTestCase : WtlTestCase
    {
        public WtlEventListTestCase(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
            TemplateName = "EFTCMBaseEvent";
        }
    }


    public class WtlIncidentListTurbines : WtlIncidentListTestCase
    {
        public WtlIncidentListTurbines(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
            ElementNames = Constants.Turbines;
        }
    }

    public class WtlIncidentListWindFarms : WtlIncidentListTestCase
    {
        public WtlIncidentListWindFarms(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
            ElementNames = Constants.WindFarms;
        }
    }

    public class WtlIncidentListTestCase : WtlTestCase
    {
        public WtlIncidentListTestCase(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
            TemplateName = "EFTCMVibrationIncident";
        }
    }


    public class Constants
    {
        public static string[] Turbines = new[]
        {
                "ANH01A28", "AVN01T03", "BBW01B24", "BKR01J02", "GFS01C04", "GFS02G07", "GFS03F00", "HR2A01", "LAW01B09",
                "MGRT02",
                "NHPH09", "WDS01D02", "WMR01F04", "WOW01D04", "WOW02E11"
            };

        public static string[] WindFarms = new[]
        {
                "ANH01*", "AVN01*", "BBW01*", "BKR01*", "GFS01*", "GFS02*", "GFS03*", "HR2*", "LAW01*", "MGR*",
                "NHP*", "WDS01*", "WMR01*", "WOW01*", "WOW02*"
            };

        public static string[] SourceSystemUniqueIds = new[]
        {
                "TcmVib_LAW01_1335",
                "TcmVib_NHP_16742",
                "TcmVib_GFS01_2299",
                "TcmVib_BKR01_1093",
                "TcmVib_WDS01_890",
                "TcmVib_WMR01_725",
                "TcmVib_MGR_2179"
            };

        public static string[] ClassificationIds = new[]
        {
                "a3ffe39a-f751-4fdc-95ea-8b39913f60d7",
                "bbe5514e-d28d-4e07-9cd1-b3b22be233b3",
                "21e414ba-bb3d-4064-b2ed-796ecf4a4cea",
                "dddf49de-43ae-4f60-bf40-0759a856a063",
                "200bdbdb-ab68-45c8-bb30-89412f1bd213",
                "188c0712-4e9f-4850-9278-722ee537ece3",
                "468e526e-5683-45c3-9dcd-6a3156bacea0"
            };
    }

    public class WtlTestCase : BaseTestCase
    {

        public WtlTestCase(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
        }

        public string ValueQuery { get; set; }

        public string TemplateName { get; set; }

        public string[] ElementNames { get; set; }

        protected override List<Action> GetActions()
        {
            if (ElementNames == null)
            {
                ElementNames = new[] { string.Empty };
            }

            var templateFilter = string.IsNullOrEmpty(TemplateName) ? string.Empty : $"Template:{TemplateName}";
            List<Action> actions = ElementNames.Select(elementName => (Action)(() =>
            {
                var actionStopwatch = new AFStopwatch();
                actionStopwatch.Start();

                var elementNameFilter = elementName != String.Empty ? $"ElementName:{elementName}" : String.Empty;
                string query = $"{elementNameFilter} {templateFilter} {ValueQuery}  SortField:ID";
                // Sometimes we use AFSearchMode.StartInclusive other times AFSearchMode.Overlapped. StartInclusive we use when a start and end time is provided
                // Is there a performance difference?
                var afEventFrameSearch = new AFEventFrameSearch(AfDatabase, "ASearch",  query);
                var maxCount = 1000;
                var eventFrames = afEventFrameSearch.FindEventFrames(0, true, maxCount).Take(maxCount).ToList();
                var count = eventFrames.Count;

                TestOutput.AddActionResult(
                    new ActionPerformanceResult
                    {
                        ActionMillis = actionStopwatch.ElapsedMilliseconds,
                        ResultCount = count
                    });
            })).ToList();
            return actions;
        }
    }

    #endregion

    #region BaseTestCase, TestOutput and more
    public class BaseTestCase : TestCase
    {
        public BaseTestCase(AFDatabase afDatabase, int threadCount = 1) : base(afDatabase, threadCount)
        {
        }

        protected override List<Action> GetActions()
        {
            return new List<Action>();
        }
    }

    public abstract class TestCase
    {
        private readonly int _threadCount;
        protected readonly TestOutput TestOutput;
        protected readonly AFDatabase AfDatabase;

        protected TestCase(AFDatabase afDatabase, int threadCount = 1)
        {
            AfDatabase = afDatabase;
            _threadCount = threadCount;
            TestOutput = new TestOutput(GetType().Name);
        }

        public TestOutput Run()
        {

            var actions = GetActions();

            AFStopwatch totalStopwatch = new AFStopwatch();

            RunTestActions(actions);

            TestOutput.AddTestResult(
                new TestResult
                {
                    TotalMillis = totalStopwatch.ElapsedMilliseconds
                });

            return TestOutput;
        }

        private void RunTestActions(List<Action> actions)
        {
            var tasks = new List<Task>();

            foreach (var action in actions)
            {
                if (tasks.Count >= _threadCount)
                {
                    var indexOfCompletedTask = Task.WaitAny(tasks.ToArray());
                    tasks.Remove(tasks[indexOfCompletedTask]);
                }
                tasks.Add(Task.Run(action));
            }
            Task.WaitAll(tasks.ToArray());
        }

        protected abstract List<Action> GetActions();
    }

    public class TestOutput
    {
        private int _searchCount;
        private int _totalSearchMillis;
        private int _totalLoadMillis;
        private int _totalResultCount;
        private double _totalActionMillisOverSearchCount;
        private int _totalTestMillis;
        private readonly string _testName;
        private int _totalActionMillis;
        private static readonly ILog log = LogManager.GetLogger(typeof(TestOutput));

        public int GetSearchCount() => _searchCount;
        public int GetTotalResultCount => _totalResultCount;
        public double GetTotalActionMillisOverSearchCount => _totalActionMillisOverSearchCount;
        public double GetTotalActionSecs => GetSeconds(_totalActionMillis);
        public double GetTotalTestSecs() => GetSeconds(_totalTestMillis);

        public TestOutput(string testName)
        {
            _testName = testName;
            Console.WriteLine($"Run test: {testName}");
        }

        public int TestCaseThreadCount { get; set; }

        public int TestRunThreadCount { get; set; }

        public void AddActionResult(ActionPerformanceResult result)
        {
            lock (this)
            {
                _searchCount++;
                _totalSearchMillis += result.SearchMillis;
                _totalLoadMillis += result.LoadMillis;
                _totalActionMillis += result.ActionMillis;
                _totalResultCount += result.ResultCount;
            }
        }

        public void AddTestResult(TestResult testResult)
        {
            _totalTestMillis += testResult.TotalMillis;
        }

        public void Output()
        {
            _totalActionMillisOverSearchCount = Math.Round((double)_totalActionMillis / _searchCount);
            var output = $"{_testName};{TestRunThreadCount};{TestCaseThreadCount};{_searchCount};{_totalResultCount};{_totalActionMillisOverSearchCount};{GetSeconds(_totalActionMillis)};{GetSeconds(_totalTestMillis)};{DateTime.UtcNow.ToString("g")}";
            Console.WriteLine(output);
            log.InfoFormat(output);
        }

        public static string OutputFormat => $"Test Name;Run Thread Count; Case Thread Count;Search Count;Values Count;Avg;SearchTime;TotalTime;Date";

        private static double GetSeconds(int totalMillis)
        {
            return Math.Round((double)totalMillis / 1000, 1);
        }

    }

    public class TestResult
    {
        public int TotalMillis { get; set; }
    }

    public class ActionPerformanceResult
    {
        public int TotalMillis { get; set; }
        public int SearchMillis { get; set; }
        public int LoadMillis { get; set; }
        public int ResultCount { get; set; }
        public int ActionMillis { get; set; }
    }
    #endregion
}