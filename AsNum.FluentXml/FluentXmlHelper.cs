using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AsNum.FluentXml
{
    /// <summary>
    /// 
    /// </summary>
    public static class FluentXmlHelper
    {

        /// <summary>
        /// 
        /// </summary>
        private static readonly XmlWriterSettings DefaultSetting = new XmlWriterSettings()
        {
            //禁止生成 BOM 字节序
            Encoding = new UTF8Encoding(false),
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
        };

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static FluentXmlAttribute<T> AsAttribute<T>(this T value, string format = null, string name = null, XNamespace ns = null)
        {
            return new FluentXmlAttribute<T>()
            {
                Value = value,
                Format = format,
                Name = name,
                NS = ns
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static FluentXmlElement<T> AsElement<T>(this T value, string name = null, string format = null, XNamespace ns = null)
        {
            return new FluentXmlElement<T>()
            {
                Value = value,
                Format = format,
                Name = name,
                NS = ns
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static FluentXmlElementValue<T> AsElementValue<T>(this T value, string format = null)
        {
            return new FluentXmlElementValue<T>()
            {
                Value = value,
                Format = format
            };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="itemName"></param>
        /// <param name="format"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static IEnumerable<FluentXmlElement<T>> AsElementArray<T>(this IEnumerable<T> values, string itemName, string format = null, XNamespace ns = null)
        {
            if (values != null)
            {
                foreach (var v in values)
                {
                    yield return new FluentXmlElement<T>()
                    {
                        Value = v,
                        Format = format,
                        Name = itemName,
                        NS = ns
                    };
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FluentXmlIgnore<T> AsIgnore<T>(this T value)
        {
            return new FluentXmlIgnore<T>()
            {
                Value = value
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fx"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static T SetFormat<T>(this T fx, string format) where T : FluentXmlBase
        {
            fx.Format = format;
            return fx;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fx"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static T SetOrder<T>(this T fx, int? order) where T : FluentXmlBase
        {
            fx.Order = order;
            return fx;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fx"></param>
        /// <param name="visible"></param>
        /// <returns></returns>
        public static T SetNullVisible<T>(this T fx, bool visible) where T : FluentXmlBase
        {
            fx.NullVisible = visible;
            return fx;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fx"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static T SetNameSpace<T>(this T fx, XNamespace ns) where T : FluentXmlBase
        {
            fx.NS = ns;
            return fx;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fx"></param>
        /// <param name="prefix"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static FluentXmlElement<T> AddNameSpace<T>(this FluentXmlElement<T> fx, string prefix, XNamespace ns)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new System.ArgumentException($"“{nameof(prefix)}”不能为 null 或空白。", nameof(prefix));
            }

            fx.AdditionalNamespace.Add(prefix, ns);
            return fx;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="ns">命名空间</param>
        /// <returns></returns>
        public static IEnumerable<XObject> Build(object obj, string name, XNamespace ns)
        {
            if (obj != null)
            {
                var xn = ns != null ? ns + name : name;

                var type = obj.GetType();

                if (obj is XElement element)
                {
                    yield return element;
                }
                // String 也实现了 IEnumerable
                else if (!string.IsNullOrWhiteSpace(name) && (type.Equals(typeof(string)) || type.IsPrimitive || type.IsValueType))
                {
                    yield return new XElement(xn, obj);
                }
                else if (obj is IEnumerable enumerable)
                {
                    foreach (var o in enumerable)
                    {
                        var n = o is FluentXmlBase @base ? @base.Name : "";
                        var sub = Build(o, n, ns);
                        if (sub != null)
                            foreach (var s in sub)
                                yield return s;
                    }
                }
                else if (obj is FluentXmlBase @base)
                {
                    yield return @base.Build(name, ns);
                }
                else
                {
                    var ele = new XElement(xn);
                    var ps = type.GetProperties();
                    foreach (var p in ps)
                    {
                        var v = p.GetValue(obj, null);
                        var sub = Build(v, p.Name, ns);
                        ele.Add(sub);
                    }

                    yield return ele;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static byte[] GetXmlData(object obj, string name, XNamespace ns = null, XmlWriterSettings setting = null)
        {

            var xos = Build(obj, name, ns);

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), xos);
            using var stm = new MemoryStream();
            using var writter = XmlWriter.Create(stm, setting ?? DefaultSetting);
            doc.Save(writter);
            writter.Flush();
            return stm.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static string GetXml(object obj, string name, XNamespace ns = null, XmlWriterSettings setting = null)
        {
            return Encoding.UTF8.GetString(GetXmlData(obj, name, ns, setting));
        }
    }
}
