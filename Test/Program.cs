using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace Test
{
    class Program
    {
        private static string strLogDirectory = Environment.GetEnvironmentVariable("ININ_LOGS");
        //private static string strLogDirectory = Environment.GetEnvironmentVariable("ININ_TRACE_ROOT");
        private static string strLogDate;
        private static string strCallID;
        private static string strLog = "CallLog.ininlog";
        //private static string strIndexExtension = ".ininlog_journal";
        private static List<string> callidList = new List<string>();
        private static List<string> callLogs = new List<string>();
        private static string strCallDate;
        private static string strCallTime;
        [STAThread]
        static void Main()
        {
            Console.Clear();
            Console.WriteLine("---------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Enter Date to Search (yyyy-mm-dd)");
            Console.ResetColor();
            strLogDate = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Enter CallID");
            Console.ResetColor();
            strCallID = Console.ReadLine();
            Console.WriteLine("---------------------------------");
            Console.WriteLine("");
            ReaderControl   control = new ReaderControl();
            LogMessage      msg     = new LogMessage();
            LogReader       reader  = new LogReader(new LogReaderContext());

            reader.open_log(strLogDirectory + "\\" + strLogDate + "\\" + strLog, new LogProperties());

            BinLogHeader header = reader.nested_log_header_at_idx(0);

            
            while (reader.next(msg, control))
            {
                ActiveAttributesVect_t attribs = msg.get_active_context_attributes();
                ActiveAttributeValuesVect_t attribVals = msg.get_active_context_attribute_values();
                int numAttribs = ((attribs != null) && (attribVals != null)) ? Math.Min(attribs.Count, attribVals.Count) : 0;
                for (int i = 0; i < numAttribs; ++i)
                {
                    if (attribVals[i].ToString().Contains(strCallID))
                    {
                        callidList.Add(msg.expand_format_message());
                        strCallDate = msg.timestamp().as_creator_time(header.tz_offset()).ToString().Substring(0, 10);
                        //strCallDate = msg.timestamp().as_creator_time(header.tz_offset()).ToString().Substring(0, 10).Replace("-", "/");
                        strCallTime = msg.timestamp().as_creator_time(header.tz_offset()).ToString().Substring(11, 8);

                        
                        
                        Console.WriteLine("Call Time : {0}", strCallTime);
                        Console.WriteLine("Call Date : {0}", strCallDate);
                        Console.WriteLine("");
                        foreach (var entry in callidList)
                        {
                            Console.WriteLine(entry);
                        }
                        Console.WriteLine("");


                        OutputLogLinesBeforeTime(strLogDirectory, strCallDate, strCallTime);

                    }
                }
            }
            //Console.WriteLine("");
            //Console.WriteLine("Press any key to exit");
            //Console.ReadKey();
            //Console.Clear();
        }
        static TimeSpan? ExtractTime(string logLine)
        {
            var tokens = logLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
                return null;
            TimeSpan time;
            if (!TimeSpan.TryParse(tokens[1], out time))
                return null;
            return time;
            
        }

        static DateTime? ExtractDate(string logLine)
        {
            var tokens = logLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 1)
                return null;
            DateTime date;
            if (!DateTime.TryParse(tokens[0], out date))
                return null;
            return date;
        }

        static void OutputLogLinesBeforeTime(string LogDir, string LogDate, string LogTime)
        {
            try
            {
                var time = TimeSpan.Parse(LogTime);
                var date = DateTime.Parse(LogDate).Date;
                //Console.WriteLine(date.ToString("yyyy/MM/dd"));
                DirectoryInfo d = new DirectoryInfo(LogDir + "\\" + LogDate + "\\");
                foreach (var file in d.GetFiles("*.ininlog_journal"))
                {
                    try
                    {
                        using (Stream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (StreamReader sReader = new StreamReader(stream))
                        {

                            //foreach (var line in sReader.EnumerateLines().Where(l => ExtractTime(l) <= time && ExtractDate(l) == date))
                            //    callLogs.Add(line);
                            var line = sReader.EnumerateLines().LastOrDefault(l => ExtractTime(l) <= time && ExtractDate(l) == date);
                            callLogs.Add(line);
                        }
                    }
                    catch (UnauthorizedAccessException ae)
                    {
                        Console.WriteLine(ae.Message);
                    }
                    catch (SystemException se)
                    {
                        Console.WriteLine(se.Message);
                    }
                    catch (ApplicationException ape)
                    {
                        Console.WriteLine(ape.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                foreach (var e in callLogs)
                    Console.WriteLine(e);
            }
            catch (UnauthorizedAccessException ae)
            {
                Console.WriteLine(ae.Message);
            }
            catch (SystemException se)
            {
                Console.WriteLine(se.Message);
            }
            catch (ApplicationException ape)
            {
                Console.WriteLine(ape.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}