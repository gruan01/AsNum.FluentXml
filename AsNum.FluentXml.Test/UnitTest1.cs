using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsNum.FluentXml.Test;

[TestClass]
public class FluentXmlAttributeTests
{
    [TestMethod]
    public void AsAttribute_Int_ShouldRenderCorrectly()
    {
        var data = new { Id = 1.AsAttribute() };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("Id=\"1\""));
    }

    [TestMethod]
    public void AsAttribute_String_ShouldRenderCorrectly()
    {
        var data = new { Name = "test".AsAttribute() };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("Name=\"test\""));
    }

    [TestMethod]
    public void AsAttribute_WithCustomName_ShouldUseCustomName()
    {
        var data = new { Value = "hello".AsAttribute(name: "Title") };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("Title=\"hello\""));
        Assert.IsFalse(xml.Contains("Value=\"hello\""));
    }

    [TestMethod]
    public void AsAttribute_WithNamespace_ShouldIncludeNamespace()
    {
        var ns = XNamespace.Get("http://example.com");
        var data = new { Id = "1".AsAttribute(ns: ns) };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("Id=\"1\""));
        Assert.IsTrue(xml.Contains("xmlns"));
    }

    [TestMethod]
    public void AsAttribute_WithFormat_ShouldFormatValue()
    {
        var dt = new DateTime(2024, 1, 15);
        var data = new { Date = dt.AsAttribute(format: "yyyy-MM-dd") };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("Date=\"2024-01-15\""));
    }
}

[TestClass]
public class FluentXmlElementTests
{
    [TestMethod]
    public void AsElement_Simple_ShouldRenderCorrectly()
    {
        var data = new { Name = "test".AsElement() };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Name>test</Name>"));
    }

    [TestMethod]
    public void AsElement_WithCustomName_ShouldUseCustomName()
    {
        var data = new { Value = "hello".AsElement(name: "Greeting") };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Greeting>hello</Greeting>"));
    }

    [TestMethod]
    public void AsElement_WithFormat_ShouldFormatValue()
    {
        var price = 12.5m;
        var data = new { Price = price.AsElement(format: "F2") };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Price>12.50</Price>"));
    }

    [TestMethod]
    public void AsElement_FluentChaining_ShouldWork()
    {
        var data = new { Value = "test".AsElement().SetFormat("X").SetNullVisible(true) };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Value>test</Value>"));
    }

    [TestMethod]
    public void AsElement_WithAdditionalNamespace_ShouldRenderXmlns()
    {
        var ns = XNamespace.Get("http://example.com");
        var data = new { Item = "value".AsElement().AddNameSpace("ex", ns) };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("xmlns:ex=\"http://example.com\""));
    }
}

[TestClass]
public class FluentXmlElementValueTests
{
    [TestMethod]
    public void AsElementValue_ShouldRenderAsCData()
    {
        var data = new { Content = "hello <world>".AsElementValue() };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<![CDATA[hello <world>]]>"));
    }

    [TestMethod]
    public void AsElementValue_Int_ShouldRenderAsCData()
    {
        var data = new { Count = 42.AsElementValue() };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<![CDATA[42]]>"));
    }
}

[TestClass]
public class FluentXmlEmptyElementTests
{
    [TestMethod]
    public void EmptyElement_ShouldRenderSelfClosingTag()
    {
        var data = new { Empty = FluentXmlEmptyElement.Create() };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Empty />") || xml.Contains("<Empty/>"));
    }

    [TestMethod]
    public void EmptyElement_WithNamespace_ShouldIncludeNamespace()
    {
        var ns = XNamespace.Get("http://example.com");
        var data = new { Empty = FluentXmlEmptyElement.Create().SetNameSpace(ns) };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("Empty"));
    }
}

[TestClass]
public class FluentXmlIgnoreTests
{
    [TestMethod]
    public void AsIgnore_ShouldNotRenderInOutput()
    {
        var data = new
        {
            Visible = "yes".AsElement(),
            Hidden = "no".AsIgnore()
        };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Visible>yes</Visible>"));
        Assert.IsFalse(xml.Contains("Hidden"));
        Assert.IsFalse(xml.Contains("no"));
    }
}

[TestClass]
public class FluentXmlArrayTests
{
    [TestMethod]
    public void AsElementArray_ShouldRenderRepeatingElements()
    {
        var data = new
        {
            Items = Enumerable.Range(0, 3).Select(i => i).AsElementArray("i")
        };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<i>0</i>"));
        Assert.IsTrue(xml.Contains("<i>1</i>"));
        Assert.IsTrue(xml.Contains("<i>2</i>"));
    }

    [TestMethod]
    public void AsElementArray_WithParent_ShouldNestCorrectly()
    {
        var data = new
        {
            Items = new
            {
                Values = Enumerable.Range(0, 2).Select(i => i).AsElementArray("v")
            }
        };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Items>"));
        Assert.IsTrue(xml.Contains("<v>0</v>"));
        Assert.IsTrue(xml.Contains("<v>1</v>"));
    }

    [TestMethod]
    public void AsElementArray_EmptyArray_ShouldNotCrash()
    {
        var data = new
        {
            Items = Enumerable.Empty<int>().Select(i => i).AsElementArray("i")
        };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsNotNull(xml);
    }

    [TestMethod]
    public void AsElementArray_WithAttributes_ShouldRenderAttributes()
    {
        var names = new List<string> { "Alice", "Bob" };
        var data = new
        {
            Persons = names.Select((n, i) => new
            {
                Index = (i + 1).AsAttribute(),
                Name = n.AsElement()
            }).AsElementArray("Person")
        };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("Index=\"1\""));
        Assert.IsTrue(xml.Contains("Index=\"2\""));
        Assert.IsTrue(xml.Contains("<Name>Alice</Name>"));
        Assert.IsTrue(xml.Contains("<Name>Bob</Name>"));
    }
}

[TestClass]
public class FluentXmlNestedObjectTests
{
    [TestMethod]
    public void NestedAnonymousObject_ShouldRenderAsNestedXml()
    {
        var data = new
        {
            Parent = new
            {
                Child = "value".AsElement()
            }
        };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Parent>"));
        Assert.IsTrue(xml.Contains("<Child>value</Child>"));
        Assert.IsTrue(xml.Contains("</Parent>"));
    }

    [TestMethod]
    public void DeeplyNested_ShouldRenderCorrectly()
    {
        var data = new
        {
            L1 = new
            {
                L2 = new
                {
                    L3 = new
                    {
                        Value = "deep".AsElement()
                    }
                }
            }
        };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Value>deep</Value>"));
    }
}

[TestClass]
public class FluentXmlNamespaceTests
{
    [TestMethod]
    public void DefaultNamespace_ShouldApplyToRoot()
    {
        var ns = XNamespace.Get("http://default.com");
        var data = new { Id = "1".AsAttribute() };
        var xml = FluentXmlHelper.GetXml(data, "root", ns);
        Assert.IsTrue(xml.Contains("xmlns=\"http://default.com\""));
    }

    [TestMethod]
    public void MixedNamespaces_ShouldRenderCorrectly()
    {
        var ns1 = XNamespace.Get("http://a.com");
        var ns2 = XNamespace.Get("http://b.com");
        var data = new
        {
            A = "val1".AsAttribute(ns: ns1),
            B = "val2".AsElement(ns: ns2)
        };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("A=\"val1\""));
        Assert.IsTrue(xml.Contains("<B"));
    }

    [TestMethod]
    public void SetNameSpace_OnElement_ShouldApplyToElement()
    {
        var ns = XNamespace.Get("http://ns.com");
        var data = new
        {
            Item = new
            {
                Id = "1".AsAttribute()
            }.AsElement().SetNameSpace(ns)
        };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("http://ns.com"));
    }
}

[TestClass]
public class FluentXmlEdgeCaseTests
{
    [TestMethod]
    public void NullValue_ShouldNotRenderByDefault()
    {
        string? nullStr = null;
        var data = new { Value = nullStr!.AsElement() };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsFalse(xml.Contains("<Value>"));
    }

    [TestMethod]
    public void NullValue_WithNullVisible_ShouldRenderEmpty()
    {
        string? nullStr = null;
        var data = new { Value = nullStr!.AsElement().SetNullVisible(true) };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Value"));
    }

    [TestMethod]
    public void GetXmlData_ShouldReturnByteArray()
    {
        var data = new { Id = "1".AsAttribute() };
        var bytes = FluentXmlHelper.GetXmlData(data, "root");
        Assert.IsNotNull(bytes);
        Assert.IsTrue(bytes.Length > 0);
        var str = System.Text.Encoding.UTF8.GetString(bytes);
        Assert.IsTrue(str.Contains("<?xml"));
        Assert.IsTrue(str.Contains("<root"));
    }

    [TestMethod]
    public void XmlDeclaration_ShouldBeUtf8()
    {
        var data = new { Id = "1".AsAttribute() };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("utf-8") || xml.Contains("UTF-8"));
    }

    [TestMethod]
    public void NoBom_ShouldNotContainBom()
    {
        var data = new { Id = "1".AsAttribute() };
        var bytes = FluentXmlHelper.GetXmlData(data, "root");
        // UTF-8 BOM is 0xEF, 0xBB, 0xBF
        Assert.AreNotEqual(0xEF, bytes[0]);
    }

    [TestMethod]
    public void ComplexXml_WithAllFeatures_ShouldRenderCorrectly()
    {
        var names = new List<string> { "Mr aa bb", "Mrs cc bb" };
        var ns = XNamespace.Get("http://cnbooking.net");

        var data = new
        {
            Empty = FluentXmlEmptyElement.Create(),
            Ignored = DateTime.Now.AsIgnore(),
            Request = new
            {
                Source = new
                {
                    RequestorID = new
                    {
                        Id = "1483".AsAttribute(),
                        Email = "M@C.IE".AsAttribute(),
                    },
                    Preferences = new
                    {
                        Language = "en".AsAttribute(),
                        Currency = "USD".AsAttribute(),
                        RequestMode = "SYNCHRONOUS"
                    }
                },
                Details = new
                {
                    Booking = new
                    {
                        Currency = "USD".AsAttribute(),
                        BookingName = "MC20031301",
                        DepartureDate = new DateTime(2017, 2, 28).AsElement(format: "yyyy-MM-dd"),
                        PaxNames = names.Select((n, i) => new
                        {
                            PaxId = (i + 1).AsAttribute(),
                            Value = n.AsElementValue()
                        }).AsElementArray("PaxName")
                    }
                }
            }
        };

        var xml = FluentXmlHelper.GetXml(data, "root", ns);

        Assert.IsTrue(xml.Contains("<Empty />") || xml.Contains("<Empty/>"), "Empty element should be present");
        Assert.IsFalse(xml.Contains("Ignored"), "Ignored property should not appear");
        Assert.IsTrue(xml.Contains("<RequestorID"), "RequestorID should exist");
        Assert.IsTrue(xml.Contains("Id=\"1483\""), "Id attribute should be 1483");
        Assert.IsTrue(xml.Contains("Email=\"M@C.IE\""), "Email attribute should match");
        Assert.IsTrue(xml.Contains("<RequestMode>SYNCHRONOUS</RequestMode>"), "RequestMode should be SYNCHRONOUS");
        Assert.IsTrue(xml.Contains("<BookingName>MC20031301</BookingName>"), "BookingName should match");
        Assert.IsTrue(xml.Contains("<DepartureDate>2017-02-28</DepartureDate>"), "Date format should be correct");
        Assert.IsTrue(xml.Contains("<![CDATA[Mr aa bb]]>"), "CDATA should contain first name");
        Assert.IsTrue(xml.Contains("<![CDATA[Mrs cc bb]]>"), "CDATA should contain second name");
        Assert.IsTrue(xml.Contains("PaxId=\"1\""), "First PaxId should be 1");
        Assert.IsTrue(xml.Contains("PaxId=\"2\""), "Second PaxId should be 2");
        Assert.IsTrue(xml.Contains("xmlns=\"http://cnbooking.net\""), "Default namespace should be present");
    }

    [TestMethod]
    public void FluentChaining_MultipleSetters_ShouldAllApply()
    {
        var data = new
        {
            Item = "test".AsElement()
                .SetFormat("X")
                .SetNullVisible(true)
        };
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Item>test</Item>"));
    }

    [TestMethod]
    public void GetXml_ShouldReturnValidXml()
    {
        var data = new { Id = "1".AsAttribute() };
        var xml = FluentXmlHelper.GetXml(data, "root");
        var doc = XDocument.Parse(xml);
        Assert.AreEqual("root", doc.Root!.Name.LocalName);
    }
}
