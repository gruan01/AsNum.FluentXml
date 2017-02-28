using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AsNum.FluentXml {
    public class FluentXmlElementValue<T> : FluentXmlBase<T>
    {
        protected override XObject BuildXml(string name)
        {
            return new XCData(this.GetFormattedValue().ToString());
        }
    }
}
