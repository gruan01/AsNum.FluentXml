using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AsNum.FluentXml;
using System.Collections.Generic;
using System.Linq;

namespace AsNum.FluentXml.Test {


    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void TestMethod1() {

            var names = new List<string>() {
                "Mr aa bb",
                "Mrs cc bb"
            };

            var roomCount = 2;


            var data = new {
                JustTest = FluentXmlEmptyElement.Create(),
                RequestOn = DateTime.Now.AsIgnore(),
                Request = new {
                    Source = new {
                        RequestorID = new {
                            RequestorID = "1483".AsAttribute(),
                            EMailAddress = "M@C.IE".AsAttribute(),
                            Password = "XXX".AsAttribute(),
                        },
                        RequestorPreferences = new {
                            Language = "en".AsAttribute(),
                            Currency = "USD".AsAttribute(),
                            RequestMode = "SYNCHRONOUS"
                        }
                    },
                    RequestDetails = new {
                        AddBookingRequest = new {
                            Currency = "USD".AsAttribute(),
                            BookingName = "MC20031301",
                            BookingReference = "MC20031301",
                            BookingDepartureDate = DateTime.Now.AsElement("yyyy-MM-dd"),
                            PaxNames = names.Select((n, i) => new {
                                PaxId = (i + 1).AsAttribute(),
                                Value = n.AsElementValue()
                            }).AsElementArray("PaxName"),
                            BookingItem = new {
                                ItemType = "hotel".AsAttribute(),
                                ExpectedPrice = 50.0M.AsAttribute(),
                                ItemReference = 1,
                                ItemCity = new {
                                    Code = "AMS".AsAttribute()
                                },
                                Item = new {
                                    Code = "NAD".AsAttribute()
                                },
                                HotelItem = new {
                                    PeriodOfStay = new {
                                        CheckInDate = DateTime.Now.AddDays(10).AsElement("yyyy-MM-dd"),
                                        CheckOutDate = DateTime.Now.AddDays(12).AsElement("yyyy-MM-dd")
                                    },
                                    HotelRooms = Enumerable.Range(1, roomCount).Select(i => new {
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


            var xml = FluentXmlHelper.Build(data, "Root");
            var xmlStr = xml.ToString();
        }
    }
}
