# FluentXml

# Generate Xml without complex class structor!

Think about the following xml ：
~~~xml
<Root>
  <JustTest />
  <Request>
    <Source>
      <RequestorID RequestorID="1483" EMailAddress="M@C.IE" Password="XXX" />
      <RequestorPreferences Language="en" Currency="USD">
        <RequestMode>SYNCHRONOUS</RequestMode>
      </RequestorPreferences>
    </Source>
    <RequestDetails>
      <AddBookingRequest Currency="USD">
        <BookingName>MC20031301</BookingName>
        <BookingReference>MC20031301</BookingReference>
        <BookingDepartureDate>2017-02-28</BookingDepartureDate>
        <PaxNames>
          <PaxName PaxId="1"><![CDATA[Mr aa bb]]></PaxName>
          <PaxName PaxId="2"><![CDATA[Mrs cc bb]]></PaxName>
        </PaxNames>
        <BookingItem ItemType="hotel" ExpectedPrice="50.0">
          <ItemReference>1</ItemReference>
          <ItemCity Code="AMS" />
          <Item Code="NAD" />
          <HotelItem>
            <PeriodOfStay>
              <CheckInDate>2017-03-10</CheckInDate>
              <CheckOutDate>2017-03-12</CheckOutDate>
            </PeriodOfStay>
            <HotelRooms>
              <HotelRoom RoomIndex="1" Code="TB" Id="001:ABC" />
              <HotelRoom RoomIndex="2" Code="TB" Id="001:ABC" />
            </HotelRooms>
          </HotelItem>
        </BookingItem>
      </AddBookingRequest>
    </RequestDetails>
  </Request>
</Root>
~~~

How many class need you define one by one  ?
Joing string ?? Too lower!!

Now, let me show you how to do this by programm:
~~~c#
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
~~~

Array items without parent Element Example:
~~~c#
var data = new
{
    Items = Enumerable.Range(0, 10).Select(i => i).AsElementArray("i"),
    Count = 10.AsAttribute()
};
var xml = FluentXmlHelper.GetXml(data, "root", XNamespace.None);
~~~

Array items with parent element example:
~~~c#
var data = new
{
    Items = new
    {
        Values = Enumerable.Range(0, 10).Select(i => i).AsElementArray("i")
    },
    Count = 10
};

var xml = FluentXmlHelper.GetXml(data, "root", XNamespace.None);
~~~

With namespace and prefix:
~~~c#
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
~~~

XML:
~~~xml
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<root ID="1" Name="xling" git:Title1="xxx" ms:Title2="abc" 
    xmlns:git="http://www.github.com" 
    xmlns:ms="http://www.microsoft.com" 
    xmlns:nuget="http://www.nuget.com">
    <nuget:Items>
        <nuget:i>1</nuget:i>
        <nuget:i>2</nuget:i>
        <nuget:i>3</nuget:i>
        <nuget:i>4</nuget:i>
        <nuget:i>5</nuget:i>
        <nuget:i>6</nuget:i>
        <nuget:i>7</nuget:i>
        <nuget:i>8</nuget:i>
        <nuget:i>9</nuget:i>
        <nuget:i>10</nuget:i>
    </nuget:Items>
</root>
~~~

Default Namespace:
~~~c#
            var nsDefault = XNamespace.Get("http://www.abc.net");
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
~~~
xml:
~~~xml
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<root ID="1" git:ID="1" xmlns:git="http://www.github.com" xmlns="http://www.abc.net">
    <ID>1</ID>
    <git:ID>1</git:ID>
    <git:GitInfo git:ID="1" Name="gruan@asnum.com">
        <git:GitUrl>http://www.github.com/gruan01</git:GitUrl>
    </git:GitInfo>
</root>
~~~

FluentXml provide the following extension methods:
* AsAttribute : Value will be show as a xml attribute.
* AsElement : Value will be show as a element.
* AsElementValue : Value will be show as value of element.
* AsElementArray : show as sub element list.
* AsIgnore : not show it.
* SetFormat 
* SetNullVisible : If value is null, default not show it.
* Build

And **FluentXmlEmptyElement.Create()** used to show a empty element, without value , without any attribute.
