using System.Xml.Linq;

namespace AsNum.FluentXml
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FluentXmlAttribute<T> : FluentXmlBase<T>
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        protected override XObject BuildXml(string name, XNamespace ns)
        {
            var nn = string.IsNullOrWhiteSpace(this.Name) ? name : this.Name;
            var n = this.NS; //?? ns;
            return new XAttribute(n != null ? n + nn : nn, this.GetFormattedValue());
        }

    }
}
