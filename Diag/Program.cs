using System.Xml.Linq;
using AsNum.FluentXml;

var bObj = new { Flag = true };
var bXml = FluentXmlHelper.GetXml(bObj, ""Root"");
Console.WriteLine(""=== Boolean ==="");
Console.WriteLine(bXml);
Console.WriteLine();

var aObj = new { id = 42.AsAttribute(""id"") };
var aXml = FluentXmlHelper.GetXml(aObj, ""Root"");
Console.WriteLine(""=== Attribute ==="");
Console.WriteLine(aXml);
Console.WriteLine();

var dt = new DateTime(2026, 1, 15);
var dObj = new { date = dt.AsAttribute(""date"", ""yyyy-MM-dd"") };
var dXml = FluentXmlHelper.GetXml(dObj, ""Root"");
Console.WriteLine(""=== Date Attribute ==="");
Console.WriteLine(dXml);
Console.WriteLine();

var obj = new Customer { Id = 1, Name = ""Bob"", Items = new List<OrderItem> { new() { ProductName = ""Widget"", Quantity = 2, UnitPrice = 9.99m } } };
try
{
    var cXml = FluentXmlHelper.GetXml(obj, ""Order"");
    Console.WriteLine(""=== Collection ==="");
    Console.WriteLine(cXml);
}
catch (Exception ex)
{
    Console.WriteLine(""=== Collection ERROR ==="");
    Console.WriteLine(ex.Message);
}

public enum OrderStatus { Pending, Shipped, Delivered }
public class OrderItem { public string ProductName { get; set; } = """"; public int Quantity { get; set; } public decimal UnitPrice { get; set; } }
public class Customer { public int Id { get; set; } public string Name { get; set; } = """"; public List<OrderItem>? Items { get; set; } }
