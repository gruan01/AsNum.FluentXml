using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AsNum.FluentXml {
    public static class FluentXmlHelper
    {
        public static FluentXmlAttribute<T> AsAttribute<T>(this T value, string format = null)
        {
            return new FluentXmlAttribute<T>()
            {
                Value = value,
                Format = format
            };
        }


        public static FluentXmlElement<T> AsElement<T>(this T value, string format = null)
        {
            return new FluentXmlElement<T>()
            {
                Value = value,
                Format = format
            };
        }

        public static FluentXmlElementValue<T> AsElementValue<T>(this T value, string format = null)
        {
            return new FluentXmlElementValue<T>()
            {
                Value = value,
                Format = format
            };
        }

        public static IEnumerable<FluentXmlElement<T>> AsElementArray<T>(this IEnumerable<T> values, string subElementName, string format = null)
        {
            foreach (var v in values)
            {
                yield return new FluentXmlElement<T>()
                {
                    Value = v,
                    Format = format,
                    Name = subElementName
                };
            }
        }

        public static FluentXmlIgnore<T> AsIgnore<T>(this T value)
        {
            return new FluentXmlIgnore<T>()
            {
                Value = value
            };
        }


        public static T SetFormat<T>(this T fx, string format) where T : FluentXmlBase
        {
            fx.Format = format;
            return fx;
        }

        public static T SetOrder<T>(this T fx, int? order) where T : FluentXmlBase
        {
            fx.Order = order;
            return fx;
        }

        public static T SetNullVisible<T>(this T fx, bool visible) where T : FluentXmlBase
        {
            fx.NullVisible = visible;
            return fx;
        }



        public static XObject Build(object obj, string name)
        {
            if (obj == null)
                return null;


            var type = obj.GetType();
            if (type.Equals(typeof(string)) || type.IsPrimitive || type.IsValueType)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return null;
                else
                {
                    return new XElement(name, obj);
                }
            }
            // String 也实现了 IEnumerable
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var ele = new XElement(name);
                foreach (var o in (IEnumerable)obj)
                {
                    var sub = Build(o, "");
                    if (sub != null)
                        ele.Add(sub);
                }

                return ele;
            }
            else if (typeof(FluentXmlBase).IsAssignableFrom(type))
            {
                var f = (FluentXmlBase)obj;
                return f.Build(name);
            }
            else
            {
                var ele = new XElement(name);
                var ps = type.GetProperties();
                foreach (var p in ps)
                {
                    var v = p.GetValue(obj, null);
                    ele.Add(Build(v, p.Name));
                }

                return ele;
            }
        }
    }
}
