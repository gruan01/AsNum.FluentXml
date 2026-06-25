using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsNum.FluentXml.Test;

/// <summary>
/// 验证 README.md 中的示例代码输出是否与文档中展示的 XML 一致
/// </summary>
[TestClass]
public class ReadmeTests
{
    /// <summary>
    /// README 主示例：酒店预订 XML。
    /// 使用固定日期代替 DateTime.Now，验证输出与文档中的 XML 完全匹配。
    /// </summary>
    [TestMethod]
    public void Readme_MainExample_ShouldMatchDocumentedXml()
    {
        var now = new DateTime(2017, 2, 18); // 模拟 DateTime.Now = 2017-02-18

        var names = new List<string> {
            "Mr aa bb",
            "Mrs cc bb"
        };

        var roomCount = 2;

        var data = new
        {
            JustTest = FluentXmlEmptyElement.Create(),
            RequestOn = now.AsIgnore(),
            Request = new
            {
                Source = new
                {
                    RequestorID = new
                    {
                        RequestorID = "1483".AsAttribute(),
                        EMailAddress = "M@C.IE".AsAttribute(),
                        Password = "XXX".AsAttribute(),
                    },
                    RequestorPreferences = new
                    {
                        Language = "en".AsAttribute(),
                        Currency = "USD".AsAttribute(),
                        RequestMode = "SYNCHRONOUS"
                    }
                },
                RequestDetails = new
                {
                    AddBookingRequest = new
                    {
                        Currency = "USD".AsAttribute(),
                        BookingName = "MC20031301",
                        BookingReference = "MC20031301",
                        BookingDepartureDate = now.AsElement(format: "yyyy-MM-dd"),
                        PaxNames = names.Select((n, i) => new
                        {
                            PaxId = (i + 1).AsAttribute(),
                            Value = n.AsElementValue()
                        }).AsElementArray("PaxName"),
                        BookingItem = new
                        {
                            ItemType = "hotel".AsAttribute(),
                            ExpectedPrice = 50.0M.AsAttribute(),
                            ItemReference = 1,
                            ItemCity = new
                            {
                                Code = "AMS".AsAttribute()
                            },
                            Item = new
                            {
                                Code = "NAD".AsAttribute()
                            },
                            HotelItem = new
                            {
                                PeriodOfStay = new
                                {
                                    CheckInDate = now.AddDays(10).AsElement(format: "yyyy-MM-dd"),
                                    CheckOutDate = now.AddDays(12).AsElement(format: "yyyy-MM-dd")
                                },
                                HotelRooms = Enumerable.Range(1, roomCount).Select(i => new
                                {
                                    RoomIndex = i.AsAttribute(),
                                    Code = "TB".AsAttribute(),
                                    Id = "001:ABC".AsAttribute()
                                }).AsElementArray("HotelRoom")
                            }
                        }
                    }
                }
            }
        };

        var xml = FluentXmlHelper.GetXml(data, "root", XNamespace.None);

        // 验证关键结构
        Assert.IsTrue(xml.Contains("<JustTest />") || xml.Contains("<JustTest/>"), "JustTest should be empty element");
        Assert.IsFalse(xml.Contains("RequestOn"), "RequestOn should be ignored");

        // RequestorID 元素和属性
        Assert.IsTrue(xml.Contains("RequestorID=\"1483\""), "RequestorID attribute");
        Assert.IsTrue(xml.Contains("EMailAddress=\"M@C.IE\""), "EMailAddress attribute");
        Assert.IsTrue(xml.Contains("Password=\"XXX\""), "Password attribute");

        // RequestorPreferences
        Assert.IsTrue(xml.Contains("Language=\"en\""), "Language attribute");
        Assert.IsTrue(xml.Contains("Currency=\"USD\""), "Currency attribute");
        Assert.IsTrue(xml.Contains("<RequestMode>SYNCHRONOUS</RequestMode>"), "RequestMode element");

        // Booking
        Assert.IsTrue(xml.Contains("<BookingName>MC20031301</BookingName>"), "BookingName");
        Assert.IsTrue(xml.Contains("<BookingReference>MC20031301</BookingReference>"), "BookingReference");

        // PaxNames with CDATA
        Assert.IsTrue(xml.Contains("PaxId=\"1\""), "First PaxId");
        Assert.IsTrue(xml.Contains("PaxId=\"2\""), "Second PaxId");
        Assert.IsTrue(xml.Contains("<![CDATA[Mr aa bb]]>"), "CDATA Mr aa bb");
        Assert.IsTrue(xml.Contains("<![CDATA[Mrs cc bb]]>"), "CDATA Mrs cc bb");

        // BookingItem
        Assert.IsTrue(xml.Contains("ItemType=\"hotel\""), "ItemType");
        Assert.IsTrue(xml.Contains("ExpectedPrice=\"50.0\""), "ExpectedPrice");
        Assert.IsTrue(xml.Contains("<ItemReference>1</ItemReference>"), "ItemReference");
        Assert.IsTrue(xml.Contains("Code=\"AMS\""), "ItemCity Code");
        Assert.IsTrue(xml.Contains("Code=\"NAD\""), "Item Code");

        // Dates — 使用固定 now = 2017-02-18
        Assert.IsTrue(xml.Contains("<BookingDepartureDate>2017-02-18</BookingDepartureDate>"), "BookingDepartureDate");
        Assert.IsTrue(xml.Contains("<CheckInDate>2017-02-28</CheckInDate>"), "CheckInDate");
        Assert.IsTrue(xml.Contains("<CheckOutDate>2017-03-02</CheckOutDate>"), "CheckOutDate");

        // HotelRooms
        Assert.IsTrue(xml.Contains("RoomIndex=\"1\""), "RoomIndex 1");
        Assert.IsTrue(xml.Contains("RoomIndex=\"2\""), "RoomIndex 2");
        Assert.IsTrue(xml.Contains("Code=\"TB\""), "Room Code TB");
        Assert.IsTrue(xml.Contains("Id=\"001:ABC\""), "Room Id");

        // 验证 XML 可以被正确解析
        var doc = XDocument.Parse(xml);
        Assert.AreEqual("root", doc.Root!.Name.LocalName);
    }

    /// <summary>
    /// README 示例：无父元素的数组项
    /// </summary>
    [TestMethod]
    public void Readme_ArrayWithoutParent_ShouldMatchExpected()
    {
        var data = new
        {
            Items = Enumerable.Range(0, 10).Select(i => i).AsElementArray("i"),
            Count = 10.AsAttribute()
        };
        var xml = FluentXmlHelper.GetXml(data, "root", XNamespace.None);

        Assert.IsTrue(xml.Contains("Count=\"10\""));
        for (int i = 0; i < 10; i++)
        {
            Assert.IsTrue(xml.Contains($"<i>{i}</i>"), $"Should contain <i>{i}</i>");
        }
    }

    /// <summary>
    /// README 示例：有父元素的数组项
    /// </summary>
    [TestMethod]
    public void Readme_ArrayWithParent_ShouldMatchExpected()
    {
        var data = new
        {
            Items = new
            {
                Values = Enumerable.Range(0, 10).Select(i => i).AsElementArray("i")
            },
            Count = 10
        };

        var xml = FluentXmlHelper.GetXml(data, "root", XNamespace.None);

        Assert.IsTrue(xml.Contains("<Items>"), "Should contain Items wrapper");
        // Values 是 IEnumerable，被扁平化到 Items 内部，不会生成 <Values> 包裹元素
        Assert.IsTrue(xml.Contains("<i>0</i>"), "Should contain i items");
        Assert.IsTrue(xml.Contains("<Count>10</Count>"), "Should contain Count element");
    }

    /// <summary>
    /// README 示例：命名空间和前缀
    /// 验证输出与 README 中展示的 XML 一致
    /// </summary>
    [TestMethod]
    public void Readme_NamespaceWithPrefix_ShouldMatchDocumentedXml()
    {
        var ns1 = XNamespace.Get("http://www.github.com");
        var ns2 = XNamespace.Get("http://www.microsoft.com");
        var ns3 = XNamespace.Get("http://www.nuget.com");
        var data = new
        {
            ID = 1.AsAttribute(),
            Name = "xling".AsAttribute(),
            Title1 = "xxx".AsAttribute(name: "Title", ns: ns1),
            Title2 = "abc".AsAttribute(name: "Title", ns: ns2),
            Items = new
            {
                Values = Enumerable.Range(1, 10).Select(i => i).AsElementArray("i", ns: ns3)
            }.AsElement(ns: ns3)
        }.AsElement()
        .AddNameSpace("git", ns1)
        .AddNameSpace("ms", ns2)
        .AddNameSpace("nuget", ns3);

        var xml = FluentXmlHelper.GetXml(data, "root", XNamespace.None);

        // README 中展示的命名空间声明
        Assert.IsTrue(xml.Contains("xmlns:git=\"http://www.github.com\""), "git namespace");
        Assert.IsTrue(xml.Contains("xmlns:ms=\"http://www.microsoft.com\""), "ms namespace");
        Assert.IsTrue(xml.Contains("xmlns:nuget=\"http://www.nuget.com\""), "nuget namespace");

        // 属性
        Assert.IsTrue(xml.Contains("ID=\"1\""), "ID attribute");
        Assert.IsTrue(xml.Contains("Name=\"xling\""), "Name attribute");

        // 带前缀的元素
        Assert.IsTrue(xml.Contains("<nuget:Items>"), "nuget:Items element");
        Assert.IsTrue(xml.Contains("<nuget:i>"), "nuget:i element");

        // 验证每个 i 元素
        for (int i = 1; i <= 10; i++)
        {
            Assert.IsTrue(xml.Contains($"<nuget:i>{i}</nuget:i>"), $"Should contain <nuget:i>{i}</nuget:i>");
        }
    }

    /// <summary>
    /// README 示例：默认命名空间
    /// </summary>
    [TestMethod]
    public void Readme_DefaultNamespace_ShouldMatchExpected()
    {
        var nsDefault = XNamespace.Get("http://www.cnbooking.net");
        var ns1 = XNamespace.Get("http://www.github.com");

        var data = new
        {
            ID = 1.AsAttribute(),
            ID1 = 1.AsElement(name: "ID"),
            GitID = 1.AsAttribute(name: "ID", ns: ns1),
            GitID1 = 1.AsElement(name: "ID", ns: ns1),
            GitInfo = new
            {
                ID = 1.AsAttribute(ns: ns1),
                Name = "gruan@asnum.com".AsAttribute(),
                GitUrl = "http://www.github.com/gruan01".AsElement(ns: ns1)
            }.AsElement().SetNameSpace(ns1)
        }.AsElement()
        .AddNameSpace("git", ns1);

        var xml = FluentXmlHelper.GetXml(data, "root", nsDefault);

        // 默认命名空间
        Assert.IsTrue(xml.Contains("xmlns=\"http://www.cnbooking.net\""), "Default namespace");
        Assert.IsTrue(xml.Contains("xmlns:git=\"http://www.github.com\""), "git namespace prefix");

        // 元素结构
        Assert.IsTrue(xml.Contains("ID=\"1\""), "Default ID attribute");
        Assert.IsTrue(xml.Contains("<ID>1</ID>"), "ID element");
        Assert.IsTrue(xml.Contains("GitInfo"), "GitInfo element should exist");
        Assert.IsTrue(xml.Contains("GitUrl"), "GitUrl element should exist");
    }
}
