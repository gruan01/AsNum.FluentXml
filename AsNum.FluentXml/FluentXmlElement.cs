using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AsNum.FluentXml
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FluentXmlElement<T> : FluentXmlBase<T>
    {

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, XNamespace> AdditionalNamespace
        {
            get;
        } = new Dictionary<string, XNamespace>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        protected override XObject BuildXml(string name, XNamespace ns)
        {
            var os = FluentXmlHelper.Build(
                this.GetFormattedValue()
                , this.Name ?? name
                , this.NS ?? ns
                ).FirstOrDefault();

            var o = (XElement)FluentXmlHelper.Build(
                this.GetFormattedValue()
                , this.Name ?? name
                , this.NS ?? ns
                ).FirstOrDefault();

            var nss = this.AdditionalNamespace.Select(a => new XAttribute(XNamespace.Xmlns + a.Key, a.Value));
            o.Add(nss);

            return o;
        }
    }
}
