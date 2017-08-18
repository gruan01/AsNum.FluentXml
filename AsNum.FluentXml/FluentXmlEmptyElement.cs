using System.Xml.Linq;

namespace AsNum.FluentXml
{

    /// <summary>
    /// 
    /// </summary>
    public class FluentXmlEmptyElement : FluentXmlBase<object>
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static FluentXmlEmptyElement Create()
        {
            return new FluentXmlEmptyElement()
            {
                NullVisible = true
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        protected override XObject BuildXml(string name, XNamespace ns)
        {
            return new XElement(ns + name);
        }
    }
}
