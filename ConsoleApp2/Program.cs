using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
namespace ConsoleApp2;

internal class Program
{
    static void Main(string[] args)
    {
        if (args[0] == null)
        {
            throw new Exception("Invalid arguments");
        }

        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();


        List<string> files = Directory.GetFiles(args[0]).ToList();
        var settings = Config();
        
        if ( files.Count > 2 && args[1] == "-s") 
        {
            var segments = Segmentation(files, 3, 1);
            var tasks = segments.Select(segment => Task.Run(() => Run(segment, settings)));
            Task.WhenAll(tasks).Wait();
        }

        stopwatch.Stop();

        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        Console.WriteLine(elapsedMilliseconds);
    }
    public static void SearchAndDestory(List<string> files, Setting setting)
    {
        var DOM = new XmlDocument();
        for (int i = 0; i < files.Count; i++)
        {
            DOM.Load(files[i]);

            XmlNode parentNode = DOM.SelectSingleNode(setting.Xpath);
            XmlNodeList children = parentNode.ChildNodes;

            HashSet<string> set = new HashSet<string>();

            foreach (XmlNode child in children)
            {
                var culture = child.Attributes[setting.Operation].Value;

                if (set.Add(culture) == false)
                {
                    parentNode.RemoveChild(child);
                    Console.WriteLine($"Removed {child.Attributes[setting.Operation].Value}: Total of nodes {children.Count} and set contains {set.Count}");
                }
            }
            DOM.Save(files[i]);
        }
    }
    public static List<Setting> Config()
    {
        List<Setting> settings = new List<Setting>();   
        string json = File.ReadAllText("C:/Users/aktiv/Desktop/projekt/ConsoleApp2/Config.json");
        var DOM = JsonDocument.Parse(json);

        IEnumerator<JsonElement> deletionsArray = DOM.RootElement.GetProperty("deletions").EnumerateArray();

        while (deletionsArray.MoveNext())
        {
            var element = deletionsArray.Current;
            settings.Add(new Setting(element.GetProperty("xpath").GetString(), element.GetProperty("operation").GetString()));
        }

        return settings;
    }
    public static void Run(List<string> files, List<Setting> settings)
    {
        for (int i = 0; i < settings.Count; i++)
        {
            SearchAndDestory(files, settings[i]);
        }
    }
    public static List<List<string>> Segmentation(List<string> files, int segmentNumber, int size)
    {
        List<List<string>> segments = new List<List<string>>();
        int p = segmentNumber;
        for (int i = 0; i < p; i++)
        {
            // Calculate start and end indices for the portion
            int startIndex = i * size;
            int endIndex = (i == p - 1) ? files.Count : (i + 1) * size;

            // Extract files for the current portion
            List<string> portion = files.GetRange(startIndex, endIndex - startIndex);
            segments.Add(portion);
        }

        return segments;
    }
    public class Setting
    {
        public Setting(string xpath, string operation)
        {
            Xpath = xpath;
            Operation = operation;  
        }

        public string Xpath = default!;
        public string Operation = default!;
    }
}
