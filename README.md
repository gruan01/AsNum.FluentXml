# FluentXml

# Generate Xml without complex class structor!

Think about the following xml ：
~~~
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
~~~
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