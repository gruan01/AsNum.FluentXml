using System.Xml.Linq;

namespace AsNum.FluentXml
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FluentXmlIgnore<T> : FluentXmlBase<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        protected override XObject BuildXml(string name, XNamespace ns)
        {
            return null;
        }
    }
}
