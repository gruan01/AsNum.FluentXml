using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using AsNum.FluentXml;

namespace Benchmark
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class FluentXmlBenchmarks
    {
        private object _simpleData;
        private object _complexData;
        private object _namespaceData;

        [GlobalSetup]
        public void Setup()
        {
            _simpleData = new
            {
                ID = 1.AsAttribute(),
                Name = "Test".AsElement(),
                Value = "Hello World".AsElementValue()
            };

            var names = new List<string> { "Mr aa bb", "Mrs cc bb" };
            _complexData = new
            {
                Request = new
                {
                    Source = new
                    {
                        RequestorID = new
                        {
                            ID = "1483".AsAttribute(),
                            Email = "M@C.IE".AsAttribute()
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
                        AddBooking = new
                        {
                            Currency = "USD".AsAttribute(),
                            BookingName = "MC20031301",
                            PaxNames = names.Select((n, i) => new
                            {
                                PaxId = (i + 1).AsAttribute(),
                                Value = n.AsElementValue()
                            }).AsElementArray("PaxName"),
                            HotelItem = new
                            {
                                Stay = new
                                {
                                    CheckIn = DateTime.Now.AsElement(format: "yyyy-MM-dd"),
                                    CheckOut = DateTime.Now.AddDays(2).AsElement(format: "yyyy-MM-dd")
                                },
                                Rooms = Enumerable.Range(1, 5).Select(i => new
                                {
                                    Index = i.AsAttribute(),
                                    Code = "TB".AsAttribute()
                                }).AsElementArray("HotelRoom")
                            }
                        }
                    }
                }
            };

            var ns1 = XNamespace.Get("http://www.github.com");
            var ns2 = XNamespace.Get("http://www.microsoft.com");
            _namespaceData = new
            {
                ID = 1.AsAttribute(),
                Name = "xling".AsAttribute(),
                GitInfo = new
                {
                    ID = 2.AsAttribute(ns: ns1),
                    Name = "test".AsAttribute(),
                    Url = "http://github.com".AsElement(ns: ns1)
                }.AsElement().SetNameSpace(ns1)
            };
        }

        [Benchmark]
        public string SimpleXml()
        {
            return FluentXmlHelper.GetXml(_simpleData, "root");
        }

        [Benchmark]
        public string ComplexXml()
        {
            return FluentXmlHelper.GetXml(_complexData, "root");
        }

        [Benchmark]
        public string NamespaceXml()
        {
            return FluentXmlHelper.GetXml(_namespaceData, "root", XNamespace.Get("http://www.cnbooking.net"));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<FluentXmlBenchmarks>();
        }
    }
}
