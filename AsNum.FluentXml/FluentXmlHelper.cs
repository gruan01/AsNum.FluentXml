using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        /// 简单值类型集合：直接序列化为文本，不需要展开属性
        /// </summary>
        private static readonly HashSet<Type> _simpleTypes = new()
        {
            typeof(bool), typeof(char),
            typeof(sbyte), typeof(byte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal),
            typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
            typeof(Guid), typeof(Uri), typeof(Version),
        };

        /// <summary>
        /// 属性访问器：缓存 PropertyInfo 和编译后的 getter 委托，避免反射调用 GetValue
        /// </summary>
        private sealed class TypeAccessor
        {
            public PropertyInfo[] Properties { get; set; } = Array.Empty<PropertyInfo>();
            public Func<object, object>[] Getters { get; set; } = Array.Empty<Func<object, object>>();
        }

        /// <summary>
        /// 类型访问器缓存，一次查询获取 Properties + 编译后的 Getters
        /// </summary>
        private static readonly ConcurrentDictionary<Type, TypeAccessor> _accessorCache = new();

        /// <summary>
        /// 使用 Expression.Compile() 创建属性 getter 委托。
        /// 首次构造 ~50μs，运行时调用 ~2ns。线程安全，兼容所有平台。
        /// </summary>
        private static Func<object, object> CompileGetter(PropertyInfo p)
        {
            // 跳过只写属性（无 getter）
            if (p.GetGetMethod(nonPublic: true) == null)
                return _ => null!;

            var param = Expression.Parameter(typeof(object));
            var cast = Expression.Convert(param, p.DeclaringType!);
            var prop = Expression.Property(cast, p);
            var box = Expression.Convert(prop, typeof(object));
            return Expression.Lambda<Func<object, object>>(box, param).Compile();
        }

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
                else if (type == typeof(string))
                {
                    yield return new XElement(xn, obj);
                }
                else if (type.IsPrimitive || type.IsEnum || _simpleTypes.Contains(type))
                {
                    yield return new XElement(xn, obj);
                }
                else if (obj is IEnumerable enumerable)
                {
                    foreach (var o in enumerable)
                    {
                        if (o == null)
                            continue;
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
                    var accessor = _accessorCache.GetOrAdd(type, t =>
                    {
                        var ps = t.GetProperties();
                        return new TypeAccessor
                        {
                            Properties = ps,
                            Getters = ps.Select(CompileGetter).ToArray()
                        };
                    });

                    var props = accessor.Properties;
                    var getters = accessor.Getters;
                    for (int i = 0; i < props.Length; i++)
                    {
                        var v = getters[i](obj);
                        var subs = Build(v, props[i].Name, ns);
                        if (subs != null)
                            ele.Add(subs);
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
