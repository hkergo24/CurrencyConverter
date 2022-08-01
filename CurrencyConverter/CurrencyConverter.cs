
using System.Globalization;

public class Node
{
    public string ICurrency;
    public string FCurrency;
    public double Rate;
    public int Distance;
    public Node(string startCurrency, string finalCurrency, double rate)
    {
        ICurrency = startCurrency;
        FCurrency = finalCurrency;
        Rate = rate;
        Distance = int.MaxValue;
    }
    public void SwapRate()
    {
        string swap = ICurrency;
        ICurrency = FCurrency;
        FCurrency = swap;
        Rate = Math.Round(1 / Rate,4, MidpointRounding.AwayFromZero);
    }

}

public class CurrencyConverter
{
    public static HashSet<Node> BuildConverterRates(List<string> lines)
    {
        CultureInfo culture = new CultureInfo("en-US");
        HashSet<Node> nodes = new HashSet<Node>();
        foreach (var line in lines)
        {
            string[] words = line.Split(';');
            var node = new Node(words[0], words[1], Convert.ToDouble(words[2], culture));
            nodes.Add(node);
        }
        return nodes;
    }

    public static void BuildGraph(HashSet<Node> ConverterRates, Node startNode, string finalCurrency)
    {
        //we reach the final node
        if (startNode.FCurrency == finalCurrency)
        {
            return;
        }

        var nextNodes = ConverterRates.Where(n => (n.ICurrency == startNode.FCurrency || n.FCurrency == startNode.FCurrency) && n.Distance!=startNode.Distance);
        if (!nextNodes.Any())//then the path is wrong
                              //so we delete all the nodes of this path that are the only way to get there
                              //because we don't want to reach these nodes again
        {
            CurrencyConverter.RemoveNodesFromConverterRates(ConverterRates, startNode);
        }
        foreach (var nextNode in nextNodes)
        {
            // on inverse le rapport si besoin
            if (nextNode.ICurrency != startNode.FCurrency)
                nextNode.SwapRate();
            var distance = startNode.Distance + 1;
            if (nextNode.Distance > distance)
                nextNode.Distance = distance;
            BuildGraph(ConverterRates, nextNode, finalCurrency);
        }
    }

    public static void RemoveNodesFromConverterRates(HashSet<Node> ConverterRates, Node lastNode)
    {
        var previousNode = ConverterRates.Where(n => n.FCurrency == lastNode.ICurrency);
        if (previousNode.Count() == 1)
        {
            RemoveNodesFromConverterRates(ConverterRates, previousNode.FirstOrDefault());
        }
        ConverterRates.Remove(lastNode);
    }

    public static List<Node> GetPath(HashSet<Node> ConverterRates, Node lastNode, string D1, List<Node> path)
    {
        path.Add(lastNode);
        if (lastNode.ICurrency == D1)
        {
            return path;
        }
        var previousNodes = ConverterRates.Where(n => n.FCurrency == lastNode.ICurrency);
        var nextNode = previousNodes.FirstOrDefault(n => n.Distance == previousNodes.Min(o => o.Distance));
        if (nextNode == null)
        {
            return path;
        }
        return GetPath(ConverterRates, nextNode, D1, path);
    }

    public static int CalculateFinalAmount(List<Node> path, int amount)
    {
        double result = amount;
        path.Reverse();
        foreach (var n in path)
        {
            result = Math.Round(result * n.Rate, 4, MidpointRounding.AwayFromZero);
        }
        int intResult = (int)Math.Round(result);
        return intResult;
    }

    public static void Main(string[] args)
    {
        string text = System.IO.File.ReadAllText(args[0]);

        List<string> lines = System.IO.File.ReadAllLines(args[0]).ToList();
        string[] line1 = lines[0].Split(';');
        string D1 = line1[0];
        string D2 = line1[2];
        int M = Int32.Parse(line1[1]);
        int N = Int32.Parse(lines[1]);
        lines.RemoveAt(0);
        lines.RemoveAt(0);
        var ConverterRates = BuildConverterRates(lines);

        var immediateRate = ConverterRates.FirstOrDefault(n => (n.ICurrency == D1 && n.FCurrency == D2) || (n.ICurrency == D2 && n.FCurrency == D1));
        // cas trivial
        if (immediateRate != null)
        {
            if (immediateRate.ICurrency != D1)
                immediateRate.SwapRate();
            double result = Math.Round(M * immediateRate.Rate, 4, MidpointRounding.AwayFromZero);
            Console.WriteLine(result);
        }
        else
        {
            var firstNodes = ConverterRates.Where(n => (n.ICurrency == D1 || n.FCurrency == D1));
            foreach (var firstNode in firstNodes)
            {
                if (firstNode.ICurrency != D1)
                    firstNode.SwapRate();
                firstNode.Distance = 1;
                CurrencyConverter.BuildGraph(ConverterRates, firstNode, D2);
            }
            var finalNodes = ConverterRates.Where(n => n.FCurrency == D2);
            var lastNode = finalNodes.FirstOrDefault(n => n.Distance == finalNodes.Min(o => o.Distance));
            var path = new List<Node>();
            GetPath(ConverterRates, lastNode, D1, path);
            Console.WriteLine(CalculateFinalAmount(path, M));
        }
    }



}