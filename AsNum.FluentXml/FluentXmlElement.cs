using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AsNum.FluentXml {
    public class FluentXmlElement<T> : FluentXmlBase<T>
    {

        public string Name
        {
            get;
            set;
        }

        protected override XObject BuildXml(string name)
        {
            return FluentXmlHelper.Build(
                this.GetFormattedValue()
                , this.Name ?? name
                );
        }
    }
}
