using System.Xml.Linq;

namespace AsNum.FluentXml
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class FluentXmlBase
    {
        /// <summary>
        /// 
        /// </summary>
        internal string Format
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal int? Order
        {
            get;
            set;
        }

        /// <summary>
        /// 当为 Null 或 空字符串的时候，是否显示，默认不显示
        /// </summary>
        internal bool NullVisible
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal XNamespace NS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        internal abstract XObject Build(string name, XNamespace ns);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FluentXmlBase<T> : FluentXmlBase
    {
        /// <summary>
        /// 
        /// </summary>
        internal T Value
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fluent"></param>
        public static implicit operator T(FluentXmlBase<T> fluent)
        {
            return fluent.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        internal override XObject Build(string name, XNamespace ns)
        {
            if (this.Value != null || (this.Value == null && this.NullVisible))
            {
                return this.BuildXml(name, ns);
            }
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        protected abstract XObject BuildXml(string name, XNamespace ns);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected object GetFormattedValue()
        {
            object value = this.Value;

            if (value != null && !string.IsNullOrEmpty(this.Format))
            {
                var fmt = string.Format("{{0:{0}}}", this.Format);// $"{{0:{this.Format}}}";
                value = string.Format(fmt, value);
            }

            return value ?? "";
        }
    }
}
