using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Diagnostics;
using OSIsoft.AF.EventFrame;
using OSIsoft.AF.Search;
using OSIsoft.AF.Time;

namespace TechSup735146//AFEventFramePerformanceTest
{
    class Program
    {

        AFDatabase _afDatabase;
        static AFTime start;
        static AFTime end;
        static int pagesize;
        private static int pageCount;

        public void EFPerform1()
        {
            PISystems myPISystems = new PISystems();
            PISystem myPISystem = myPISystems.DefaultPISystem;
            myPISystem.Connect();
            Console.WriteLine("SDK: {0}, Server: {1}", myPISystems.Version, myPISystem.ServerVersion);

            _afDatabase = myPISystem.Databases["DongEnergyWind_performance"];//myPISystem.Databases.DefaultDatabase;

            Console.WriteLine(_afDatabase.Name);

        }
        public static void Main(string[] args)
        {
            Program a = new Program();
            a.EFPerform1();

            start = AFTime.MinValue;
            end = AFTime.MaxValue;
            pagesize = Int32.MaxValue;
            pageCount = 0;


            if (args.Length == 0)
            {


                Console.Write("Select (a,b,c,d,e,f,g,h) : ");
                ConsoleKeyInfo select = Console.ReadKey();
                Console.WriteLine();

                if (select.Key == ConsoleKey.A)
                    a.A_FindEventFrames();
                else if (select.Key == ConsoleKey.B)
                    a.B_SourceSystemUniqueId_FindEventFramesByAttribute_TimeTest();
                else if (select.Key == ConsoleKey.C)
                    a.C_SourceSystemUniqueId_AFEventFrameSearch_TimeTest();
                else if (select.Key == ConsoleKey.D)
                    a.D_ReviewStatus_ElementName_FindEventFramesByAttribute_TimeTest();
                else if (select.Key == ConsoleKey.E)
                    a.E_ReviewStatus_ElementName_AFEventFrameSearch_TimeTest();
                else if (select.Key == ConsoleKey.F)
                    a.F_ReviewStatus_ElementName_AFEventFrameSearch_GetTotalCount_TimeTest();
                else if (select.Key == ConsoleKey.G) //"Brute force" count of EF by WTG
                    a.G_ReviewStatusCount_AFEventFrameSearch_CalculateCountOnAllTurbines_TimeTest();
                else if (select.Key == ConsoleKey.H) //Compares to Count of EF of status by WTG (using Custom Data Reference)
                    a.H_ReviewStatusCountCustomDataReference_AFElementSearch_TimeTest();
                else if (select.Key == ConsoleKey.I)
                    a.I_ElementName_AFEventFrameSearch_TimeTest();
                else if (select.Key == ConsoleKey.J)
                    a.J_ClassificationValue_ElementName_AFEventFrameSearch_TimeTest();
                else if (select.Key == ConsoleKey.K)
                    a.K_SourceSystemId_ExtendedProperty_TimeTest();

            }
            else
            {
                if (args.Length > 1)
                    start = new AFTime(args[1]);
                if (args.Length > 2)
                    end = new AFTime(args[2]);

                int threadCount = 1;

                if (args.Length > 3)
                {
                    threadCount = Convert.ToInt32(args[3]);

                }

                if (args.Length > 4)
                {
                    pagesize = Convert.ToInt32(args[4]);
                }

                if (args.Length > 5)
                {
                    pageCount = Convert.ToInt32(args[5]);
                }

                Console.WriteLine($"start: {start}, end: {end}, threadCount: {threadCount}, pagesize: {pagesize}, pagecount: {pageCount}");

                if (args[0].ToLowerInvariant() == "t")
                {
                    var tasks = new List<Task>();
                    for (int i = 0; i < threadCount; i++)
                    {
                        var runNum = i + 1;
                        tasks.Add(Task.Run(() => { Console.WriteLine(a.A_FindEventFrames(runNum)); }));
                        tasks.Add(Task.Run(() => { Console.WriteLine(a.B_SourceSystemUniqueId_FindEventFramesByAttribute_TimeTest(runNum)); }));
                        tasks.Add(Task.Run(() => { Console.WriteLine(a.C_SourceSystemUniqueId_AFEventFrameSearch_TimeTest(runNum)); }));
                        tasks.Add(Task.Run(() => { Console.WriteLine(a.D_ReviewStatus_ElementName_FindEventFramesByAttribute_TimeTest(runNum)); }));
                        tasks.Add(Task.Run(() => { Console.WriteLine(a.E_ReviewStatus_ElementName_AFEventFrameSearch_TimeTest(runNum)); }));
                        tasks.Add(Task.Run(() => { Console.WriteLine(a.F_ReviewStatus_ElementName_AFEventFrameSearch_GetTotalCount_TimeTest(runNum)); }));
                        tasks.Add(Task.Run(() => { Console.WriteLine(a.G_ReviewStatusCount_AFEventFrameSearch_CalculateCountOnAllTurbines_TimeTest(runNum)); }));
                        tasks.Add(Task.Run(() => { Console.WriteLine(a.H_ReviewStatusCountCustomDataReference_AFElementSearch_TimeTest(runNum)); }));
                        tasks.Add(Task.Run(() => { Console.WriteLine(a.I_ElementName_AFEventFrameSearch_TimeTest(runNum)); }));
                        tasks.Add(Task.Run(() => { Console.WriteLine(a.J_ClassificationValue_ElementName_AFEventFrameSearch_TimeTest(runNum)); }));
                        tasks.Add(Task.Run(() => { Console.WriteLine(a.K_SourceSystemId_ExtendedProperty_TimeTest(runNum)); }));
                    }

                    Task.WaitAll(tasks.ToArray());
                }
                else if (args[0].ToLowerInvariant().Equals("all"))
                {
                    Console.WriteLine(a.A_FindEventFrames());
                    Console.WriteLine(a.B_SourceSystemUniqueId_FindEventFramesByAttribute_TimeTest());
                    Console.WriteLine(a.C_SourceSystemUniqueId_AFEventFrameSearch_TimeTest());
                    Console.WriteLine(a.D_ReviewStatus_ElementName_FindEventFramesByAttribute_TimeTest());
                    Console.WriteLine(a.E_ReviewStatus_ElementName_AFEventFrameSearch_TimeTest());
                    Console.WriteLine(a.F_ReviewStatus_ElementName_AFEventFrameSearch_GetTotalCount_TimeTest());
                    Console.WriteLine(a.G_ReviewStatusCount_AFEventFrameSearch_CalculateCountOnAllTurbines_TimeTest());
                    Console.WriteLine(a.H_ReviewStatusCountCustomDataReference_AFElementSearch_TimeTest());
                    Console.WriteLine(a.I_ElementName_AFEventFrameSearch_TimeTest());
                    Console.WriteLine(a.J_ClassificationValue_ElementName_AFEventFrameSearch_TimeTest());
                    Console.WriteLine(a.K_SourceSystemId_ExtendedProperty_TimeTest());

                }
                else
                {
                    var tasks = new List<Task>();
                    for (int i = 0; i < threadCount; i++)
                    {
                        var runNum = i + 1;
                        switch (args[0])
                        {
                            case "a":
                                tasks.Add(Task.Run(() => { Console.WriteLine(a.A_FindEventFrames(runNum)); }));
                                break;
                            case "b":
                                tasks.Add(Task.Run(() => { Console.WriteLine(a.B_SourceSystemUniqueId_FindEventFramesByAttribute_TimeTest(runNum)); }));
                                break;
                            case "c":
                                tasks.Add(Task.Run(() => { Console.WriteLine(a.C_SourceSystemUniqueId_AFEventFrameSearch_TimeTest(runNum)); }));
                                break;
                            case "d":
                                tasks.Add(Task.Run(() => { Console.WriteLine(a.D_ReviewStatus_ElementName_FindEventFramesByAttribute_TimeTest(runNum)); }));
                                break;
                            case "e":
                                tasks.Add(Task.Run(() => { Console.WriteLine(a.E_ReviewStatus_ElementName_AFEventFrameSearch_TimeTest(runNum)); }));
                                break;
                            case "f":
                                tasks.Add(Task.Run(() => { Console.WriteLine(a.F_ReviewStatus_ElementName_AFEventFrameSearch_GetTotalCount_TimeTest(runNum)); }));
                                break;
                            case "g":
                                tasks.Add(Task.Run(() => { Console.WriteLine(a.G_ReviewStatusCount_AFEventFrameSearch_CalculateCountOnAllTurbines_TimeTest(runNum)); }));
                                break;
                            case "h":
                                tasks.Add(Task.Run(() => { Console.WriteLine(a.H_ReviewStatusCountCustomDataReference_AFElementSearch_TimeTest(runNum)); }));
                                break;
                            case "i":
                                tasks.Add(Task.Run(() => { Console.WriteLine(a.I_ElementName_AFEventFrameSearch_TimeTest(runNum)); }));
                                break;
                            case "j":
                                tasks.Add(Task.Run(() => { Console.WriteLine(a.J_ClassificationValue_ElementName_AFEventFrameSearch_TimeTest(runNum)); }));
                                break;
                            case "k":
                                tasks.Add(Task.Run(() => { Console.WriteLine(a.K_SourceSystemId_ExtendedProperty_TimeTest(runNum)); }));
                                break;

                        }
                    }

                    Task.WaitAll(tasks.ToArray());
                }
            }
            Console.WriteLine("Press any key to close ...");
            Console.ReadKey();
        }

        public string A_FindEventFrames(int threadNum = 0)
        {
            AFStopwatch myWatch = new AFStopwatch();

            AFNamedCollectionList<AFEventFrame> myEFs = AFEventFrame.FindEventFrames(_afDatabase, null, "*", AFSearchField.Name, true, AFSortField.Name, AFSortOrder.Ascending, 0, int.MaxValue);

            return $"A{threadNum}; Avg time = " + Math.Round((double)myWatch.ElapsedMilliseconds / myEFs.Count, 3) +
                   "ms per event frame, NumberofEFs = " + myEFs.Count + ", Time = " + myWatch.ElapsedMilliseconds;
        }

        /*
        public void A_FindEventFrameByExtendedProperty_SourceSystemId_TimeTest()
        {
            //_log.Info($"SourceSystemUniqueId_FindByExtendedProperties on {_environment}");
            SourceSystemUniqueId_FindByExtendedProperties("TcmVib_LAW01_1335");
            SourceSystemUniqueId_FindByExtendedProperties("TcmVib_NHP_16742");
            SourceSystemUniqueId_FindByExtendedProperties("TcmVib_GFS01_2299");
            SourceSystemUniqueId_FindByExtendedProperties("TcmVib_BKR01_1093");
            SourceSystemUniqueId_FindByExtendedProperties("TcmVib_WDS01_890");
            SourceSystemUniqueId_FindByExtendedProperties("TcmVib_WMR01_725");
            SourceSystemUniqueId_FindByExtendedProperties("TcmVib_MGR_2179");
            SourceSystemUniqueId_FindByExtendedProperties("TcmVib_MGR_Unknown1");
            SourceSystemUniqueId_FindByExtendedProperties("TcmVib_MGR_Unknown2");
            SourceSystemUniqueId_FindByExtendedProperties("TcmVib_MGR_Unknown3");
            SourceSystemUniqueId_FindByExtendedProperties("TcmVib_MGR_Unknown4");
        }
        */

        public string B_SourceSystemUniqueId_FindEventFramesByAttribute_TimeTest(int threadNum = 0)
        {
            //_log.Info($"SourceSystemUniqueId_FindByAttributes on {_environment}");
            return $"B{threadNum}; Avg time = " +
                Math.Round((double)
                (
                    SourceSystemUniqueId_FindByAttributes("TcmVib_LAW01_1335") +
                    SourceSystemUniqueId_FindByAttributes("TcmVib_NHP_16742") +
                    SourceSystemUniqueId_FindByAttributes("TcmVib_GFS01_2299") +
                    SourceSystemUniqueId_FindByAttributes("TcmVib_BKR01_1093") +
                    SourceSystemUniqueId_FindByAttributes("TcmVib_WDS01_890") +
                    SourceSystemUniqueId_FindByAttributes("TcmVib_WMR01_725") +
                    SourceSystemUniqueId_FindByAttributes("TcmVib_MGR_2179")
                ) / 7) + "ms";
        }

        public string C_SourceSystemUniqueId_AFEventFrameSearch_TimeTest(int threadNum = 0)
        {
            //_log.Info($"SourceSystemUniqueId_AFEventFrameSearch on {_environment}");
            return $"C{threadNum}; Avg time = " +
                Math.Round((double)
                (
                    SourceSystemUniqueId_AFEventFrameSearch("TcmVib_LAW01_1335") +
                    SourceSystemUniqueId_AFEventFrameSearch("TcmVib_NHP_16742") +
                    SourceSystemUniqueId_AFEventFrameSearch("TcmVib_GFS01_2299") +
                    SourceSystemUniqueId_AFEventFrameSearch("TcmVib_BKR01_1093") +
                    SourceSystemUniqueId_AFEventFrameSearch("TcmVib_WDS01_890") +
                    SourceSystemUniqueId_AFEventFrameSearch("TcmVib_WMR01_725") +
                    SourceSystemUniqueId_AFEventFrameSearch("TcmVib_MGR_2179")
                ) / 7) + "ms";
        }

        public string K_SourceSystemId_ExtendedProperty_TimeTest(int threadNum = 0)
        {
            //_log.Info($"SourceSystemUniqueId_AFEventFrameSearch on {_environment}");
            return $"K{threadNum}; Avg time = " +
                Math.Round((double)
                (
                    SourceSystemUniqueId_FindByExtendedProperties("TcmVib_LAW01_1335") +
                    SourceSystemUniqueId_FindByExtendedProperties("TcmVib_NHP_16742") +
                    SourceSystemUniqueId_FindByExtendedProperties("TcmVib_GFS01_2299") +
                    SourceSystemUniqueId_FindByExtendedProperties("TcmVib_BKR01_1093") +
                    SourceSystemUniqueId_FindByExtendedProperties("TcmVib_WDS01_890") +
                    SourceSystemUniqueId_FindByExtendedProperties("TcmVib_WMR01_725") +
                    SourceSystemUniqueId_FindByExtendedProperties("TcmVib_MGR_2179")
                ) / 7) + "ms";
        }

        public string D_ReviewStatus_ElementName_FindEventFramesByAttribute_TimeTest(int threadNum = 0)
        {
            //_log.Info($"ReviewStatus_FindByAttributes on {_environment}");
            return $"D{threadNum}; Avg time = " +
                Math.Round((double)
                (
                    ReviewStatus_FindByAttributes(0, "ANH01A01") +
                    ReviewStatus_FindByAttributes(0, "BKR01G01") +
                    ReviewStatus_FindByAttributes(0, "GFS01A01") +
                    ReviewStatus_FindByAttributes(2, "HR2A01") +
                    ReviewStatus_FindByAttributes(2, "LAW01C13") +
                    ReviewStatus_FindByAttributes(2, "NHPA05") +
                    ReviewStatus_FindByAttributes(2, "WDS01G13") +
                    ReviewStatus_FindByAttributes(0, "HR2*") +
                    ReviewStatus_FindByAttributes(0, "LAW01*") +
                    ReviewStatus_FindByAttributes(2, "NHP*") +
                    ReviewStatus_FindByAttributes(2, "WDS01*")
                ) / 11) + "ms";
        }

        public string E_ReviewStatus_ElementName_AFEventFrameSearch_TimeTest(int threadNum = 0)
        {

            //_log.Info($"$ReviewStatus_AFEventFrameSearch on {_environment}");
            return $"E{threadNum}; Avg time = " +
                Math.Round((double)
                (
                    ReviewStatus_AFEventFrameSearch(0, "ANH01A01") +
                    ReviewStatus_AFEventFrameSearch(0, "BKR01G01") +
                    ReviewStatus_AFEventFrameSearch(0, "GFS01A01") +
                    ReviewStatus_AFEventFrameSearch(2, "HR2A01") +
                    ReviewStatus_AFEventFrameSearch(2, "LAW01C13") +
                    ReviewStatus_AFEventFrameSearch(2, "NHPA05") +
                    ReviewStatus_AFEventFrameSearch(2, "WDS01G13") +
                    ReviewStatus_AFEventFrameSearch(0, "HR2*") +
                    ReviewStatus_AFEventFrameSearch(0, "LAW01*") +
                    ReviewStatus_AFEventFrameSearch(2, "NHP*") +
                    ReviewStatus_AFEventFrameSearch(2, "WDS01*")
                ) / 11) + "ms";
        }

        public string F_ReviewStatus_ElementName_AFEventFrameSearch_GetTotalCount_TimeTest(int threadNum = 0)
        {
            return $"F{threadNum}; Avg time = " +
                Math.Round((double)
                (
                    ReviewStatus_AFEventFrameSearch_GetTotalCount(0, "ANH01A01") +
                    ReviewStatus_AFEventFrameSearch_GetTotalCount(0, "BKR01G01") +
                    ReviewStatus_AFEventFrameSearch_GetTotalCount(0, "GFS01A01") +
                    ReviewStatus_AFEventFrameSearch_GetTotalCount(2, "HR2A01") +
                    ReviewStatus_AFEventFrameSearch_GetTotalCount(2, "LAW01C13") +
                    ReviewStatus_AFEventFrameSearch_GetTotalCount(2, "NHPA05") +
                    ReviewStatus_AFEventFrameSearch_GetTotalCount(2, "WDS01G13") +
                    ReviewStatus_AFEventFrameSearch_GetTotalCount(0, "HR2*") +
                    ReviewStatus_AFEventFrameSearch_GetTotalCount(0, "LAW01*") +
                    ReviewStatus_AFEventFrameSearch_GetTotalCount(2, "NHP*") +
                    ReviewStatus_AFEventFrameSearch_GetTotalCount(2, "WDS01*")
                 ) / 11) + "ms";
        }

        public string G_ReviewStatusCount_AFEventFrameSearch_CalculateCountOnAllTurbines_TimeTest(int threadNum = 0)
        {
            return $"G{threadNum}; Avg time = " +
                Math.Round((double)
                (
                    //_log.Info($"ReviewStatusCountOnElements_AFEventFrameSearch_GroupBy on {_environment}");
                    ReviewStatus_AFEventFrameSearch_CalculateCountOnAllTurbines(0, "ANH01*") +
                    ReviewStatus_AFEventFrameSearch_CalculateCountOnAllTurbines(0, "BKR01*") +
                    ReviewStatus_AFEventFrameSearch_CalculateCountOnAllTurbines(0, "HR2*") +
                    ReviewStatus_AFEventFrameSearch_CalculateCountOnAllTurbines(0, "LAW01*") +
                    ReviewStatus_AFEventFrameSearch_CalculateCountOnAllTurbines(0, "NHP*") +
                    ReviewStatus_AFEventFrameSearch_CalculateCountOnAllTurbines(0, "WDS01*") +
                    ReviewStatus_AFEventFrameSearch_CalculateCountOnAllTurbines(0, "WOW02*")
                 ) / 7) + "ms";
        }

        public string H_ReviewStatusCountCustomDataReference_AFElementSearch_TimeTest(int threadNum = 0)
        {
            //_log.Info($"ReviewStatusCountOnElementsCustomDataReference_AFElementSearch on {_environment}");

            return $"H{threadNum}; Avg time = " +
                Math.Round((double)
                (
                    ReviewStatus_AFElementSearch_CountOnAllTurbinesCDR("WSLG.CtVibMxVibEvtStNew", "ANH01*") +
                    ReviewStatus_AFElementSearch_CountOnAllTurbinesCDR("WSLG.CtVibMxVibEvtStNew", "BKR01*") +
                    ReviewStatus_AFElementSearch_CountOnAllTurbinesCDR("WSLG.CtVibMxVibEvtStNew", "HR2*") +
                    ReviewStatus_AFElementSearch_CountOnAllTurbinesCDR("WSLG.CtVibMxVibEvtStNew", "LAW01*") +
                    ReviewStatus_AFElementSearch_CountOnAllTurbinesCDR("WSLG.CtVibMxVibEvtStNew", "NHP*") +
                    ReviewStatus_AFElementSearch_CountOnAllTurbinesCDR("WSLG.CtVibMxVibEvtStNew", "WDS01*") +
                    ReviewStatus_AFElementSearch_CountOnAllTurbinesCDR("WSLG.CtVibMxVibEvtStNew", "WOW02*")
                 ) / 7) + "ms";
        }


        public string I_ElementName_AFEventFrameSearch_TimeTest(int threadNum = 0)
        {

            //_log.Info($"$ReviewStatus_AFEventFrameSearch on {_environment}");
            return $"I{threadNum}; Avg time = " +
                Math.Round((double)
                (
                    ElementName_AFEventFrameSearch("ANH01F01") +
                    ElementName_AFEventFrameSearch("BKR01G02") +
                    ElementName_AFEventFrameSearch("GFS01B04") +
                    ElementName_AFEventFrameSearch("HR2A06") +
                    ElementName_AFEventFrameSearch("LAW01M17") +
                    ElementName_AFEventFrameSearch("NHPC02") +
                    ElementName_AFEventFrameSearch("WDS01I08") +
                    ElementName_AFEventFrameSearch("HR2*") +
                    ElementName_AFEventFrameSearch("LAW01*") +
                    ElementName_AFEventFrameSearch("NHP*") +
                    ElementName_AFEventFrameSearch("WDS01*")
                ) / 11) + "ms";
        }


        public string J_ClassificationValue_ElementName_AFEventFrameSearch_TimeTest(int threadNum = 0)
        {

            //_log.Info($"$ReviewStatus_AFEventFrameSearch on {_environment}");
            return $"J{threadNum}; Avg time = " +
                Math.Round((double)
                (
                    ClassificationValue_AFEventFrameSearch("1.1.9", "ANH01F01") +
                    ClassificationValue_AFEventFrameSearch("1.2.0", "BKR01G02") +
                    ClassificationValue_AFEventFrameSearch("1.2.0", "GFS01B04") +
                    ClassificationValue_AFEventFrameSearch("1.4.1", "HR2A06") +
                    ClassificationValue_AFEventFrameSearch("1.3.0", "LAW01M17") +
                    ClassificationValue_AFEventFrameSearch("1.1.6", "NHPC02") +
                    ClassificationValue_AFEventFrameSearch("1.4.1", "WDS01I08") +
                    ClassificationValue_AFEventFrameSearch("1.1.1", "HR2*") +
                    ClassificationValue_AFEventFrameSearch("1.1.15", "LAW01*") +
                    ClassificationValue_AFEventFrameSearch("1.3.0", "NHP*") +
                    ClassificationValue_AFEventFrameSearch("1.1.6", "WDS01*")
                ) / 11) + "ms";
        }//TODO: Evolve to current test and tests a/b on individual and across farms

        private int SourceSystemUniqueId_FindByExtendedProperties(string sourceSystemUniqueId)
        {
            AFStopwatch Stopwatch = new AFStopwatch();
            Stopwatch.Start();

            // Act
            var eventFrame = AFEventFrame.FindEventFramesByExtendedProperty(_afDatabase, "SourceSystemId", new[] { sourceSystemUniqueId }, 1).FirstOrDefault().Value;

            //Console.WriteLine(Stopwatch.ElapsedMilliseconds);

            return Stopwatch.ElapsedMilliseconds;

            //_log.Info($"time: {stopwatch.Elapsed.TotalSeconds}, count: {count}");
        }

        private int SourceSystemUniqueId_FindByAttributes(string sourceSystemUniqueId, string elementName = null)
        {
            return FindByAttributes("EFTCMBaseTemplate", "SourceSystemUniqueId", sourceSystemUniqueId, elementName);
        }

        private int SourceSystemUniqueId_AFEventFrameSearch(string sourceSystemUniqueId, string elementName = null)
        {
            return AFEventFrameSearch("EFTCMBaseTemplate", elementName, "SourceSystemUniqueId", value: sourceSystemUniqueId);
        }

        private int ReviewStatus_FindByAttributes(int reviewStatus, string elementName = null)
        {
            return FindByAttributes("EFTCMBaseEvent", "ReviewStatus", reviewStatus, elementName);
        }

        private int ReviewStatus_AFEventFrameSearch(int reviewStatus, string elementName = null)
        {
            return AFEventFrameSearch("EFTCMBaseEvent", elementName, "ReviewStatus", reviewStatus);
        }

        private int ElementName_AFEventFrameSearch(string elementName = null)
        {
            return AFEventFrameSearch("EFTCMBaseEvent", elementName);
        }

        private int ClassificationValue_AFEventFrameSearch(string classificationValue, string elementName = null)
        {
            return AFEventFrameSearch("EFOutageClassification", elementName, "OCSClassification", classificationValue);
        }


        private int FindByAttributes(string templateName, string attributeName, object value, string elementName = null)
        {
            AFStopwatch stopwatch = new AFStopwatch();

            var afAttributeValueQuery = GetAfAttributeValueQuery(templateName, attributeName, value);

            if (pageCount != 0)
            {
                for (int page = 0; page < pageCount; page++)
                {
                    var afEventFrames = FindEventFramesByAttribute(elementName, afAttributeValueQuery, page, pagesize);
                    //Console.WriteLine($"Page:{page+1}, PageSize:{pagesize}, EFCount:{afEventFrames.Count}");
                    //AFEventFrame.LoadEventFrames(afEventFrames);
                    //var uniqueIds = afEventFrames.Select(ef => ef.Attributes["SourceSystemUniqueId"]);
                }
            }
            else
            {
                var afEventFrames = FindEventFramesByAttribute(elementName, afAttributeValueQuery);
                //AFEventFrame.LoadEventFrames(afEventFrames);
                //var uniqueIds = afEventFrames.Select(ef => ef.Attributes["SourceSystemUniqueId"]);
                //Console.WriteLine($"EFCount:{afEventFrames.Count}");
            }
            //Console.WriteLine("Done : " + afEventFrames.Count + ", " + stopwatch.ElapsedMilliseconds + "ms");
            return stopwatch.ElapsedMilliseconds;

            //_log.Info($"time:{stopwatch.Elapsed.TotalSeconds}, Template:{templateName}, {attributeName}:{value}, Element:{elementName}, count:{afEventFrames.Count}");
        }

        private static List<AFEventFrame> FindEventFramesByAttribute(string elementName, AFAttributeValueQuery afAttributeValueQuery, int page = 0, int maxCount = Int32.MaxValue)
        {
            return AFEventFrame.FindEventFramesByAttribute(
                null, // Search root
                AFSearchMode.Inclusive, // Search mode
                AFTime.MinValue, // Start time
                AFTime.MaxValue, // End time
                null, // Name filter
                elementName, // Referenced name filter
                null, // Duration query
                new[] { afAttributeValueQuery }, // Value query
                true, // Search full hierarchy
                AFSortField.ID, // Sort field
                AFSortOrder.Ascending, // Sort order
                page * pagesize, // Start index
                maxCount).ToList(); // Max count
        }

        private AFAttributeValueQuery GetAfAttributeValueQuery(string templateName, string attributeName, object value)
        {
            AFAttributeTemplate attributeTemplate =
                _afDatabase.ElementTemplates[templateName].AttributeTemplates[attributeName];
            var afAttributeValueQuery = new AFAttributeValueQuery(attributeTemplate, AFSearchOperator.Equal, value);
            return afAttributeValueQuery;
        }

        private int AFEventFrameSearch(string templateName, string elementName = null, string attributeName = null, object value = null)
        {
            AFStopwatch stopwatch = new AFStopwatch();

            string elementNameFilter = elementName != null ? "ElementName:" + elementName + " " : string.Empty;
            string query = $"Template:{templateName} SortField:ID";
            if (elementName != null)
            {
                query += $" ElementName:{elementName}";
            }

            if (attributeName != null)
            {
                query += $" |{attributeName}:={value}";
            }

            var afEventFrameSearch = new AFEventFrameSearch(_afDatabase, "MyQuery" + value, query);
            if (pageCount != 0)
            {
                afEventFrameSearch.CacheTimeout = TimeSpan.FromSeconds(30);
                for (int page = 0; page < pageCount; page++)
                {
                    var afEventFrames = afEventFrameSearch.FindEventFrames(startIndex: page * pagesize, fullLoad: false, pageSize: pagesize)
                        .ToList();
                    //var uniqueIds = afEventFrames.Select(ef => ef.Attributes["SourceSystemUniqueId"]);
                    //Console.WriteLine($"Page:{page + 1}, PageSize:{pagesize}, EFCount:{afEventFrames.Count}");
                }
            }
            else
            {
                var afEventFrames = afEventFrameSearch.FindEventFrames(fullLoad: false, pageSize: pagesize).ToList();
                //var uniqueIds = afEventFrames.Select(ef => ef.Attributes["SourceSystemUniqueId"]);
                //Console.WriteLine($"EFCount:{afEventFrames.Count}");
            }

            //Console.WriteLine("Done : " + afEventFrames.Count + ", " + stopwatch.ElapsedMilliseconds + "ms");
            return stopwatch.ElapsedMilliseconds;

            //_log.Info($"time:{stopwatch.Elapsed.TotalSeconds}, Template:{templateName}, {attributeName}:{value}, Element:{elementName}, count:{afEventFrames.Count}");
        }

        private int ReviewStatus_AFEventFrameSearch_GetTotalCount(object value, string elementName = null)
        {
            var templateName = "EFTCMBaseEvent";
            var attributeName = "ReviewStatus";

            AFStopwatch stopwatch = new AFStopwatch();

            string elementNameFilter = elementName != null ? "ElementName:" + elementName + " " : string.Empty;
            var afEventFrameSearch = new AFEventFrameSearch(_afDatabase, "MyQuery" + value, elementNameFilter + "Template:" + templateName + " |" + attributeName + ":=" + value/* + " SortField:ID"*/);

            var count = afEventFrameSearch.GetTotalCount();

            //Console.WriteLine("Done : " + count + ", " + stopwatch.ElapsedMilliseconds + "ms");
            return stopwatch.ElapsedMilliseconds;
            //_log.Info($"time:{stopwatch.Elapsed.TotalSeconds}, Template:{templateName}, {attributeName}: {value}, Element:{elementName}, count:{count}");
        }


        private int ReviewStatus_AFEventFrameSearch_CalculateCountOnAllTurbines(int reviewStatus, string windFarmName)
        {
            var templateName = "EFTCMVibrationEvent";
            var attributeName = "ReviewStatus";

            AFStopwatch stopwatch = new AFStopwatch();
            var afEventFrameSearch = new AFEventFrameSearch(_afDatabase, "MyQuery", "ElementName:" + windFarmName + " Template:" + templateName + " |" + attributeName + ":=" + reviewStatus + " SortField:ID");

            var countDictionary = new Dictionary<string, int>();

            IEnumerable<AFEventFrame> efs = afEventFrameSearch.FindEventFrames(pageSize: pagesize);

            //Console.WriteLine("Done : " + efs.Count() + ", " + stopwatch.ElapsedMilliseconds + "ms");

            var eventFramesGroupedByPrimaryReferencedElementNames = efs.GroupBy(ef => ef.PrimaryReferencedElement.Name);

            foreach (var eventFrameGroup in eventFramesGroupedByPrimaryReferencedElementNames)
            {
                countDictionary[eventFrameGroup.Key] = eventFrameGroup.Count();
            }

            //Console.WriteLine("Done2 : " + efs.Count() + ", " + stopwatch.ElapsedMilliseconds + "ms");
            return stopwatch.ElapsedMilliseconds;

            //_log.Info($"Time:{stopwatch.Elapsed.TotalSeconds}, Template:{templateName}, {attributeName}:{reviewStatus}, Element:{windFarmName}, Element Count: {countDictionary.Keys.Count}, EventFrame Count:{countDictionary.Sum(e => e.Value)}");

            //foreach (var counts in countDictionary)
            //{
            //    _log.Info($"{counts.Key}:{counts.Value}");
            //}
        }

        private int ReviewStatus_AFElementSearch_CountOnAllTurbinesCDR(string attribute, string windFarmName)
        {

            AFStopwatch stopwatch = new AFStopwatch();

            var afElementSearch = new AFElementSearch(_afDatabase, "MyQuery", "Name:" + windFarmName + " CategoryName:WTG");

            var elements = afElementSearch.FindElements();

            //Console.WriteLine("Done : " + elements.Count() + ", " + stopwatch.ElapsedMilliseconds + "ms");

            AFElement.LoadAttributes(elements.ToList(), new List<AFAttributeTemplate> { _afDatabase.ElementTemplates["WtgBaseTemplate"].AttributeTemplates["WSLG.EvtCt"].AttributeTemplates[attribute] });

            var countDictionary = new Dictionary<string, int>();

            foreach (var afElement in elements)
            {
                var count = afElement.Attributes["WSLG.EvtCt"].Attributes[attribute].GetValue().ValueAsInt32();

                if (count > 0)
                {
                    countDictionary[afElement.Name] = count;
                }
            }

            //Console.WriteLine("Done2 : " + elements.Count() + ", " + stopwatch.ElapsedMilliseconds + "ms");
            return stopwatch.ElapsedMilliseconds;
            //_log.Info($"Time:{stopwatch.Elapsed.TotalSeconds}, Element:{windFarmName}, Element Count: {countDictionary.Keys.Count}, EventFrame Count:{countDictionary.Sum(e => e.Value)}");

            //foreach (var counts in countDictionary)
            //{
            //    _log.Info($"{counts.Key}:{counts.Value}");
            //}
        }
    }

}
