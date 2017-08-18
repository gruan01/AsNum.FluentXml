using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AsNum.FluentXml;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AsNum.FluentXml.Test
{


    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            var names = new List<string>() {
                "Mr aa bb",
                "Mrs cc bb"
            };

            var roomCount = 2;


            var data = new
            {
                JustTest = FluentXmlEmptyElement.Create(),
                RequestOn = DateTime.Now.AsIgnore(),
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
                            BookingDepartureDate = DateTime.Now.AsElement(format: "yyyy-MM-dd"),
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
                                        CheckInDate = DateTime.Now.AddDays(10).AsElement(format: "yyyy-MM-dd"),
                                        CheckOutDate = DateTime.Now.AddDays(12).AsElement(format: "yyyy-MM-dd")
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
        }

        [TestMethod]
        public void ArrayWithoutParentTest()
        {
            var data = new
            {
                Items = Enumerable.Range(0, 10).Select(i => i).AsElementArray("i"),
                Count = 10.AsAttribute()
            };
            var xml = FluentXmlHelper.GetXml(data, "root", XNamespace.None);
        }

        [TestMethod]
        public void ArrayWithParentTest()
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
        }
    }
}
