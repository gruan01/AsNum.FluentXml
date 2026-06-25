using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsNum.FluentXml.Test;

/// <summary>
/// 针对审计发现 Bug/热点的修复验证测试
/// </summary>
[TestClass]
public class AuditFixTests
{
    /// <summary>
    /// Bug #1: FluentXmlElement.BuildXml() — 值=null + NullVisible=false + AddNameSpace → 不应 NRE
    /// </summary>
    [TestMethod]
    public void NullValue_WithNamespace_ShouldNotThrow()
    {
        var ns = XNamespace.Get("http://example.com");
        string? nullStr = null;

        // 修复前：o 为 null，AddNameSpace 使 AdditionalNamespace 非空，o.Add() 触发 NRE
        var data = new
        {
            Item = nullStr!.AsElement().AddNameSpace("ex", ns)
        };

        // 不应抛出异常
        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsNotNull(xml);

        // null 值默认不渲染，所以命名空间声明也不应出现
        Assert.IsFalse(xml.Contains("xmlns:ex"), "Null element should produce no output");
        Assert.IsFalse(xml.Contains("<Item"), "Null element should not render");
    }

    /// <summary>
    /// Bug #1 变体: 值=null + NullVisible=true + AddNameSpace → 应渲染空元素 + 命名空间
    /// </summary>
    [TestMethod]
    public void NullVisible_WithNamespace_ShouldRenderEmptyWithNamespace()
    {
        var ns = XNamespace.Get("http://example.com");
        string? nullStr = null;

        var data = new
        {
            Item = nullStr!.AsElement().SetNullVisible(true).AddNameSpace("ex", ns)
        };

        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("xmlns:ex=\"http://example.com\""), "Namespace should be declared");
        Assert.IsTrue(xml.Contains("<Item"), "Empty element should render");
    }

    /// <summary>
    /// Bug #2: 字符串类型不应落入 IEnumerable&lt;char&gt; 分支逐字符渲染
    /// 验证：字符串值始终渲染为完整文本节点
    /// </summary>
    [TestMethod]
    public void StringValue_ShouldRenderAsWholeText_NotAsChars()
    {
        var data = new
        {
            Greeting = "Hello World".AsElement()
        };

        var xml = FluentXmlHelper.GetXml(data, "root");

        // 应该渲染为 <Greeting>Hello World</Greeting>
        Assert.IsTrue(xml.Contains("<Greeting>Hello World</Greeting>"),
            "String should render as complete text, not character-by-character");

        // 不应该出现逐字符标签（如 <c>H</c>）
        Assert.IsFalse(xml.Contains("<c>H</c>"));
        Assert.IsFalse(xml.Contains("<c>e</c>"));
    }

    /// <summary>
    /// Bug #2: 字符串属性在嵌套对象中也应正确渲染
    /// </summary>
    [TestMethod]
    public void StringValue_InNestedObject_ShouldRenderCorrectly()
    {
        var data = new
        {
            Outer = new
            {
                Inner = "nested-value".AsElement()
            }
        };

        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<Inner>nested-value</Inner>"));
    }

    /// <summary>
    /// Bug #3: Order 属性已移除，SetOrder 方法已移除
    /// 此测试隐式验证：使用链式调用时不依赖 SetOrder 不会崩溃
    /// </summary>
    [TestMethod]
    public void FluentChaining_WithoutSetOrder_ShouldStillWork()
    {
        var data = new
        {
            Item = "test".AsElement()
                .SetFormat("X")
                .SetNullVisible(true)
                .SetNameSpace(XNamespace.Get("http://ns.com"))
        };

        var xml = FluentXmlHelper.GetXml(data, "root");
        // SetNameSpace 会添加命名空间，元素名可能有前缀，检查内容存在即可
        Assert.IsTrue(xml.Contains("Item") && xml.Contains(">test<"), "Element Item with value test should exist");
    }

    /// <summary>
    /// Bug #4/#5: type==typeof(string) 和移除无用 name 检查后，原语类型仍正确渲染
    /// </summary>
    [TestMethod]
    public void PrimitiveTypes_ShouldStillRenderCorrectly()
    {
        var data = new
        {
            IntVal = 42.AsElement(),
            DecimalVal = 12.5m.AsElement(),
            BoolVal = true.AsElement(),
            DoubleVal = 3.14.AsElement()
        };

        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<IntVal>42</IntVal>"));
        Assert.IsTrue(xml.Contains("<DecimalVal>"), "Decimal element should exist");
        Assert.IsTrue(xml.Contains("<BoolVal>"), "Bool element should exist");
        Assert.IsTrue(xml.Contains("<DoubleVal>"), "Double element should exist");
    }

    /// <summary>
    /// Bug #6: FluentXmlElementValue 冗余 ToString 已移除，CDATA 仍正确渲染
    /// </summary>
    [TestMethod]
    public void CData_IntAndString_ShouldRenderCorrectly()
    {
        var data = new
        {
            TextCData = "hello <world> & friends".AsElementValue(),
            IntCData = 42.AsElementValue()
        };

        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<![CDATA[hello <world> & friends]]>"));
        Assert.IsTrue(xml.Contains("<![CDATA[42]]>"));
    }

    /// <summary>
    /// 综合回归测试：README 主示例仍然正确
    /// </summary>
    [TestMethod]
    public void Regression_ReadmeMainExample_StillWorks()
    {
        var now = new DateTime(2017, 2, 18);
        var names = new List<string> { "Mr aa bb", "Mrs cc bb" };

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
                    }
                }
            }
        };

        var xml = FluentXmlHelper.GetXml(data, "root");
        Assert.IsTrue(xml.Contains("<JustTest />") || xml.Contains("<JustTest/>"));
        Assert.IsFalse(xml.Contains("RequestOn"));
        Assert.IsTrue(xml.Contains("RequestorID=\"1483\""));
    }
}
