using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AsNum.FluentXml
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class FluentXmlElement<T> : FluentXmlBase<T>
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
            XElement o = null!;
            foreach (XObject xo in FluentXmlHelper.Build(
                this.GetFormattedValue()
                , this.Name ?? name
                , this.NS ?? ns))
            {
                o = (XElement)xo;
                break;
            }

            if (o == null)
                return null!;

            if (this.AdditionalNamespace.Count > 0)
            {
                foreach (var kv in this.AdditionalNamespace)
                    o.Add(new XAttribute(XNamespace.Xmlns + kv.Key, kv.Value));
            }
            return o;
        }
    }
}
