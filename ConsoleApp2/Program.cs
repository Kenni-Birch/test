using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using static ConsoleApp2.Program;
namespace ConsoleApp2;

internal class Program
{
    static void Main(string[] args)
    {
        List<string> files = Directory.GetFiles("C:/Users/aktiv/Desktop/content").ToList();

        var settings = Config("search");

        SearchAndReport(files, settings[0]);
        //Run(files, settings);

    }
    public static void SearchAndDestory(List<string> files, Setting setting)
    {

        for (int i = 0; i < files.Count; i++)
        {
            var DOM = new XmlDocument();

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
    public static void SearchAndReport(List<string> files, Setting setting)
    {
        List<int> counter = new List<int>();
        foreach (var filePath in files)
        {
            var document = new XmlDocument();
            document.Load(filePath);

            var navigator = document.CreateNavigator();
            var nodes = navigator.Select(setting.Xpath);

            HashSet<string> set = new HashSet<string>();

            foreach (XPathNavigator node in nodes)
            {
                Recursive(node, setting.Operation, set, counter);
            }
            document.Save(filePath);
        }

        Console.WriteLine("Total Count: " + counter.Count);
    }

    public static void Recursive(XPathNavigator node, string operation, HashSet<string> set, List<int> counter)
    {
        var attributeValue = node.GetAttribute(operation, "");

        if (!string.IsNullOrEmpty(attributeValue) && !set.Add(attributeValue))
        {
            counter.Add(1);
        }

        foreach (XPathNavigator childNode in node.SelectChildren(XPathNodeType.Element))
        {
            Recursive(childNode, operation, set, counter);
        }
    }
    public static List<Setting> Config(string op)
    {
        List<Setting> settings = new List<Setting>();   
        string json = File.ReadAllText("C:/Users/aktiv/Desktop/projekt/ConsoleApp2/Config.json");
        using (var DOM = JsonDocument.Parse(json))
        {
            IEnumerator<JsonElement> deletionsArray = DOM.RootElement.GetProperty(op).EnumerateArray();

            while (deletionsArray.MoveNext())
            {
                var element = deletionsArray.Current;
                settings.Add(new Setting(element.GetProperty("xpath").GetString(), element.GetProperty("operation").GetString()));
            }
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
