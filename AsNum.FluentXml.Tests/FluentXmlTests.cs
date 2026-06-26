using System.Xml.Linq;
using AsNum.FluentXml;

namespace AsNum.FluentXml.Tests;

// ============================================================
// 测试用数据模型
// ============================================================

public enum OrderStatus { Pending, Shipped, Delivered }

public class Address
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string ZipCode { get; set; } = "";
}

public class OrderItem
{
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime? Birthday { get; set; }
    public Address? HomeAddress { get; set; }
    public List<OrderItem>? Items { get; set; }
    public OrderStatus Status { get; set; }
}

public class DeepNested
{
    public string Level1 { get; set; } = "";
    public DeepNestedChild? Child { get; set; }
}

public class DeepNestedChild
{
    public string Level2 { get; set; } = "";
    public DeepNestedGrandChild? GrandChild { get; set; }
}

public class DeepNestedGrandChild
{
    public string Level3 { get; set; } = "";
}

public struct Point2D
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class ContainerWithStruct
{
    public Point2D Position { get; set; }
    public string Label { get; set; } = "";
}

// ============================================================
// 辅助方法：解析 XML 并定位元素
// ============================================================

internal static class TestHelper
{
    /// <summary>
    /// 解析 GetXml 输出字符串为根 XElement
    /// </summary>
    public static XElement ParseXml(string xml)
    {
        var idx = xml.IndexOf("?>");
        var content = idx >= 0 ? xml.Substring(idx + 2).TrimStart() : xml;
        return XElement.Parse(content);
    }
}

// ============================================================
// AsElement 测试 — 注意：AsElement 指定 Name 时，元素取代父名成为根
// ============================================================

public class AsElementTests
{
    [Fact]
    public void String_Value_Creates_Element()
    {
        var xml = FluentXmlHelper.GetXml("hello".AsElement("Message"), "Root");
        var root = TestHelper.ParseXml(xml);
        // FluentXmlElement 的 Name="Message" 取代了 "Root"，所以根就是 <Message>
        Assert.Equal("Message", root.Name.LocalName);
        Assert.Equal("hello", root.Value);
    }

    [Fact]
    public void Int_Value_Creates_Element()
    {
        var xml = FluentXmlHelper.GetXml(42.AsElement("Count"), "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Count", root.Name.LocalName);
        Assert.Equal("42", root.Value);
    }

    [Fact]
    public void Decimal_Value_Creates_Element()
    {
        var xml = FluentXmlHelper.GetXml(99.95m.AsElement("Price"), "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Price", root.Name.LocalName);
        Assert.Equal("99.95", root.Value);
    }

    [Fact]
    public void DateTime_Value_Creates_Element()
    {
        var dt = new DateTime(2026, 6, 26, 12, 0, 0);
        var xml = FluentXmlHelper.GetXml(dt.AsElement("Date"), "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Date", root.Name.LocalName);
        Assert.NotNull(root.Value);
    }

    [Fact]
    public void Guid_Value_Creates_Element()
    {
        var g = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var xml = FluentXmlHelper.GetXml(g.AsElement("Id"), "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Id", root.Name.LocalName);
        Assert.Equal("12345678-1234-1234-1234-123456789abc", root.Value);
    }

    [Fact]
    public void Enum_Value_Creates_Element()
    {
        var xml = FluentXmlHelper.GetXml(OrderStatus.Shipped.AsElement("Status"), "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Status", root.Name.LocalName);
        Assert.Equal("Shipped", root.Value);
    }

    [Fact]
    public void Null_Value_Not_Rendered()
    {
        string? nullStr = null;
        var result = FluentXmlHelper.Build(nullStr!.AsElement("Missing"), "Root", null)
            .Where(x => x != null).ToList();
        Assert.Empty(result);
    }
}

// ============================================================
// AsAttribute — 经匿名对象包装，通过 Build 管道验证
// ============================================================

public class AsAttributeTests
{
    [Fact]
    public void Int_Value_Creates_Attribute()
    {
        var xml = FluentXmlHelper.GetXml(new { id = 42.AsAttribute(name: "id") }, "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("42", root.Attribute("id")?.Value);
    }

    [Fact]
    public void DateTime_With_Format_Creates_Formatted_Attribute()
    {
        var dt = new DateTime(2026, 1, 15);
        var xml = FluentXmlHelper.GetXml(new { date = dt.AsAttribute("yyyy-MM-dd", name: "date") }, "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("2026-01-15", root.Attribute("date")?.Value);
    }

    [Fact]
    public void Null_Value_Not_Rendered()
    {
        string? nullStr = null;
        var xml = FluentXmlHelper.GetXml(new { missing = nullStr.AsAttribute(name: "missing") }, "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Null(root.Attribute("missing"));
    }

    [Fact]
    public void With_Namespace_Creates_Namespaced_Attribute()
    {
        XNamespace ns = "http://example.com";
        var xml = FluentXmlHelper.GetXml(new { attr = "v".AsAttribute(name: "attr", ns: ns) }, "Root", ns);
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("v", root.Attribute(ns + "attr")?.Value);
    }
}

// ============================================================
// AsElementValue (CDATA) 测试
// ============================================================

public class AsElementValueTests
{
    [Fact]
    public void Creates_CDATA_Section()
    {
        var obj = new { Payload = "<data>&amp;</data>".AsElementValue() };
        var xml = FluentXmlHelper.GetXml(obj, "Root");
        Assert.Contains("<![CDATA[<data>&amp;</data>]]>", xml);
    }

    [Fact]
    public void Null_Value_Not_Rendered()
    {
        string? nullStr = null;
        var obj = new { Payload = nullStr.AsElementValue() };
        var xml = FluentXmlHelper.GetXml(obj, "Root");
        Assert.DoesNotContain("<Payload", xml);
    }
}

// ============================================================
// AsElementArray 测试
// ============================================================

public class AsElementArrayTests
{
    [Fact]
    public void Creates_Multiple_Elements()
    {
        var items = new[] { "A", "B", "C" };
        var obj = new { Items = items.AsElementArray("Item") };
        var xml = FluentXmlHelper.GetXml(obj, "List");
        Assert.Contains("<Item>A</Item>", xml);
        Assert.Contains("<Item>B</Item>", xml);
        Assert.Contains("<Item>C</Item>", xml);
    }

    [Fact]
    public void Null_Collection_Skipped()
    {
        string[]? items = null;
        var obj = new { Items = items.AsElementArray("Item") };
        var xml = FluentXmlHelper.GetXml(obj, "List");
        Assert.DoesNotContain("<Item>", xml);
    }

    [Fact]
    public void Empty_Collection_Skipped()
    {
        var items = Array.Empty<string>();
        var obj = new { Items = items.AsElementArray("Item") };
        var xml = FluentXmlHelper.GetXml(obj, "List");
        Assert.DoesNotContain("<Item>", xml);
    }
}

// ============================================================
// AsIgnore 测试
// ============================================================

public class AsIgnoreTests
{
    [Fact]
    public void Ignored_Property_Not_In_Output()
    {
        var xml = FluentXmlHelper.GetXml(new
        {
            Visible = "show",
            Hidden = "hide".AsIgnore()
        }, "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("show", root.Element("Visible")?.Value);
        Assert.Null(root.Element("Hidden"));
    }
}

// ============================================================
// SetNullVisible 测试
// ============================================================

public class SetNullVisibleTests
{
    [Fact]
    public void Null_Value_Rendered_When_Flag_Set()
    {
        string? nullStr = null;
        var obj = new { Empty = nullStr.AsElement("Empty").SetNullVisible(true) };
        var xml = FluentXmlHelper.GetXml(obj, "Root");
        var root = TestHelper.ParseXml(xml);
        var empty = root.Element("Empty");
        Assert.NotNull(empty);
        Assert.Equal("", empty!.Value);
    }
}

// ============================================================
// 命名空间测试
// ============================================================

public class NamespaceTests
{
    [Fact]
    public void SetNameSpace_Applies_Namespace()
    {
        XNamespace ns = "http://example.com";
        var xml = FluentXmlHelper.GetXml("test".AsElement("Data").SetNameSpace(ns), "Root");
        Assert.Contains("http://example.com", xml);
    }

    [Fact]
    public void AddNameSpace_Adds_Additional_Namespace()
    {
        XNamespace ns = "http://extra.com";
        var xml = FluentXmlHelper.GetXml(
            "test".AsElement("Data").AddNameSpace("ex", ns), "Root");
        Assert.Contains("xmlns:ex=\"http://extra.com\"", xml);
    }
}

// ============================================================
// FluentXmlEmptyElement 测试
// ============================================================

public class EmptyElementTests
{
    [Fact]
    public void Creates_Self_Closing_Element()
    {
        var obj = new { Empty = FluentXmlEmptyElement.Create() };
        var xml = FluentXmlHelper.GetXml(obj, "Root");
        var root = TestHelper.ParseXml(xml);
        var empty = root.Element("Empty");
        Assert.NotNull(empty);
        Assert.True(empty!.IsEmpty);
    }
}

// ============================================================
// SetFormat 测试
// ============================================================

public class SetFormatTests
{
    [Fact]
    public void DateTime_Formatted_In_Element()
    {
        var dt = new DateTime(2026, 3, 8, 14, 30, 0);
        var xml = FluentXmlHelper.GetXml(dt.AsElement("When").SetFormat("yyyy-MM-dd"), "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("When", root.Name.LocalName);
        Assert.Equal("2026-03-08", root.Value);
    }

    [Fact]
    public void Numeric_Formatted_In_Element()
    {
        var xml = FluentXmlHelper.GetXml(3.14159.AsElement("Pi").SetFormat("F2"), "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Pi", root.Name.LocalName);
        Assert.Equal("3.14", root.Value);
    }
}

// ============================================================
// 复杂对象序列化测试
// ============================================================

public class ComplexObjectTests
{
    [Fact]
    public void Serializes_Anonymous_Object()
    {
        var xml = FluentXmlHelper.GetXml(new { Name = "Alice", Age = 30 }, "Person");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Person", root.Name.LocalName);
        Assert.Equal("Alice", root.Element("Name")?.Value);
        Assert.Equal("30", root.Element("Age")?.Value);
    }

    [Fact]
    public void Serializes_Nested_Object()
    {
        var obj = new Customer
        {
            Id = 1,
            Name = "Bob",
            Status = OrderStatus.Delivered,
            HomeAddress = new Address { Street = "123 Main St", City = "NYC", ZipCode = "10001" }
        };
        var xml = FluentXmlHelper.GetXml(obj, "Customer");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Customer", root.Name.LocalName);
        Assert.Equal("1", root.Element("Id")?.Value);
        Assert.Equal("Bob", root.Element("Name")?.Value);
        Assert.Equal("Delivered", root.Element("Status")?.Value);
        var addr = root.Element("HomeAddress");
        Assert.NotNull(addr);
        Assert.Equal("123 Main St", addr!.Element("Street")?.Value);
        Assert.Equal("NYC", addr.Element("City")?.Value);
    }

    [Fact]
    public void Serializes_Object_With_Null_SubObject()
    {
        var obj = new Customer { Id = 1, Name = "NoAddr", HomeAddress = null };
        var xml = FluentXmlHelper.GetXml(obj, "Customer");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("NoAddr", root.Element("Name")?.Value);
        Assert.Null(root.Element("HomeAddress"));
    }

    [Fact]
    public void Serializes_Deep_Nested_Objects()
    {
        var obj = new DeepNested
        {
            Level1 = "A",
            Child = new DeepNestedChild
            {
                Level2 = "B",
                GrandChild = new DeepNestedGrandChild { Level3 = "C" }
            }
        };
        var xml = FluentXmlHelper.GetXml(obj, "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Root", root.Name.LocalName);
        Assert.Equal("A", root.Element("Level1")?.Value);
        Assert.Equal("B", root.Element("Child")?.Element("Level2")?.Value);
        Assert.Equal("C", root.Element("Child")?.Element("GrandChild")?.Element("Level3")?.Value);
    }
}

// ============================================================
// 值类型/Struct 处理测试
// ============================================================

public class ValueTypeTests
{
    [Fact]
    public void Custom_Struct_Expands_Properties()
    {
        var obj = new ContainerWithStruct
        {
            Position = new Point2D { X = 10, Y = 20 },
            Label = "origin"
        };
        var xml = FluentXmlHelper.GetXml(obj, "Point");
        var root = TestHelper.ParseXml(xml);
        // Point2D 是自定义 struct，属性展开到子元素
        Assert.Equal("10", root.Element("Position")?.Element("X")?.Value);
        Assert.Equal("20", root.Element("Position")?.Element("Y")?.Value);
        Assert.Equal("origin", root.Element("Label")?.Value);
    }

    [Fact]
    public void DateTime_Serialized_As_Text()
    {
        var dt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var xml = FluentXmlHelper.GetXml(new { When = dt }, "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.NotNull(root.Element("When")?.Value);
    }

    [Fact]
    public void Guid_Serialized_As_Text()
    {
        var g = Guid.NewGuid();
        var xml = FluentXmlHelper.GetXml(new { Id = g }, "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal(g.ToString(), root.Element("Id")?.Value);
    }
}

// ============================================================
// Build 方法直接测试
// ============================================================

public class BuildMethodTests
{
    [Fact]
    public void Build_String_Returns_XElement()
    {
        var result = FluentXmlHelper.Build("hello", "Tag", null).ToList();
        Assert.Single(result);
        var xe = Assert.IsType<XElement>(result[0]);
        Assert.Equal("Tag", xe.Name.LocalName);
        Assert.Equal("hello", xe.Value);
    }

    [Fact]
    public void Build_Int_Returns_XElement()
    {
        var result = FluentXmlHelper.Build(123, "Num", null).ToList();
        Assert.Single(result);
        Assert.Equal("123", ((XElement)result[0]).Value);
    }

    [Fact]
    public void Build_Null_Returns_Empty()
    {
        var result = FluentXmlHelper.Build(null!, "Tag", null).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void Build_Collection_With_FluentXmlElement_Works()
    {
        var items = new[] { "a", "b" }.Select(s => s.AsElement("Item")).ToList();
        var result = FluentXmlHelper.Build(items, "Items", null).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal("Item", ((XElement)result[0]).Name.LocalName);
        Assert.Equal("Item", ((XElement)result[1]).Name.LocalName);
    }

    [Fact]
    public void Build_Anonymous_Object_Returns_Element_With_Children()
    {
        var result = FluentXmlHelper.Build(new { X = 1, Y = 2 }, "Point", null).ToList();
        Assert.Single(result);
        var xe = Assert.IsType<XElement>(result[0]);
        Assert.Equal("Point", xe.Name.LocalName);
        Assert.Equal(2, xe.Elements().Count());
    }
}

// ============================================================
// GetXml / GetXmlData 测试
// ============================================================

public class GetXmlTests
{
    [Fact]
    public void GetXml_Returns_Valid_Xml()
    {
        var xml = FluentXmlHelper.GetXml("hello".AsElement("Msg"), "Root");
        Assert.StartsWith("<?xml", xml);
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Msg", root.Name.LocalName);
        Assert.Equal("hello", root.Value);
    }

    [Fact]
    public void GetXml_With_Namespace()
    {
        XNamespace ns = "http://test.com";
        var xml = FluentXmlHelper.GetXml("v".AsElement("E", ns: ns), "Root", ns);
        Assert.Contains("http://test.com", xml);
    }

    [Fact]
    public void GetXml_No_BOM()
    {
        var xml = FluentXmlHelper.GetXml("test".AsElement("E"), "Root");
        Assert.StartsWith("<?xml", xml);
    }

    [Fact]
    public void GetXmlData_Returns_ByteArray_No_BOM()
    {
        var data = FluentXmlHelper.GetXmlData("x".AsElement("E"), "Root");
        Assert.NotEmpty(data);
        Assert.Equal((byte)'<', data[0]);
    }
}

// ============================================================
// 性能回归测试
// ============================================================

public class PerformanceRegressionTests
{
    [Fact]
    public void Compiled_Getters_Produce_Correct_Values()
    {
        var obj = new Customer
        {
            Id = 42,
            Name = "Test",
            Status = OrderStatus.Shipped,
            Birthday = new DateTime(2000, 1, 1),
            HomeAddress = new Address { Street = "S", City = "C", ZipCode = "Z" }
        };
        var xml = FluentXmlHelper.GetXml(obj, "Customer");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Customer", root.Name.LocalName);
        Assert.Equal("42", root.Element("Id")?.Value);
        Assert.Equal("Test", root.Element("Name")?.Value);
        Assert.Equal("Shipped", root.Element("Status")?.Value);
        var addr = root.Element("HomeAddress");
        Assert.NotNull(addr);
        Assert.Equal("S", addr!.Element("Street")?.Value);
        Assert.Equal("C", addr.Element("City")?.Value);
        Assert.Equal("Z", addr.Element("ZipCode")?.Value);
    }

    [Fact]
    public void Repeated_Calls_Use_Cached_Getters()
    {
        for (int i = 0; i < 100; i++)
        {
            var obj = new Customer { Id = i, Name = $"User{i}", Status = OrderStatus.Pending };
            var xml = FluentXmlHelper.GetXml(obj, "Customer");
            var root = TestHelper.ParseXml(xml);
            Assert.Equal(i.ToString(), root.Element("Id")?.Value);
        }
    }

    [Fact]
    public void Format_Cache_Works()
    {
        for (int i = 0; i < 50; i++)
        {
            var dt = DateTime.Now;
            var xml = FluentXmlHelper.GetXml(dt.AsElement("D").SetFormat("yyyy-MM-dd"), "Root");
            var root = TestHelper.ParseXml(xml);
            Assert.Equal("D", root.Name.LocalName);
            Assert.NotNull(root.Value);
        }
    }

    [Fact]
    public void Implicit_Operator_Works()
    {
        FluentXmlElement<int> fx = 42.AsElement("N");
        int value = fx;
        Assert.Equal(42, value);
    }
}

// ============================================================
// 边缘情况测试
// ============================================================

public class EdgeCaseTests
{
    [Fact]
    public void Empty_String_Rendered()
    {
        var xml = FluentXmlHelper.GetXml("".AsElement("Empty"), "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("Empty", root.Name.LocalName);
        Assert.Equal("", root.Value);
    }

    [Fact]
    public void Boolean_True_Rendered()
    {
        var obj = new { Flag = true };
        var xml = FluentXmlHelper.GetXml(obj, "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("true", root.Element("Flag")?.Value);
    }

    [Fact]
    public void Boolean_False_Rendered()
    {
        var obj = new { Flag = false };
        var xml = FluentXmlHelper.GetXml(obj, "Root");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("false", root.Element("Flag")?.Value);
    }

    [Fact]
    public void Many_Properties_All_Rendered()
    {
        var obj = new
        {
            A = "a", B = "b", C = "c", D = "d", E = "e",
            F = "f", G = "g", H = "h", I = "i", J = "j"
        };
        var xml = FluentXmlHelper.GetXml(obj, "Many");
        var root = TestHelper.ParseXml(xml);
        Assert.Equal("a", root.Element("A")?.Value);
        Assert.Equal("j", root.Element("J")?.Value);
        Assert.Equal(10, root.Elements().Count());
    }

    [Fact]
    public void GetXmlData_With_Custom_Settings()
    {
        var settings = new System.Xml.XmlWriterSettings
        {
            Encoding = System.Text.Encoding.UTF8,
            Indent = true,
        };
        var data = FluentXmlHelper.GetXmlData("x".AsElement("E"), "Root", setting: settings);
        Assert.NotEmpty(data);
    }
}
