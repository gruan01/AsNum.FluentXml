using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AsNum.FluentXml {
    public class FluentXmlEmptyElement : FluentXmlBase<object>
    {

        public static FluentXmlEmptyElement Create()
        {
            return new FluentXmlEmptyElement()
            {
                NullVisible = true
            };
        }

        protected override XObject BuildXml(string name)
        {
            return new XElement(name);
        }
    }
}
