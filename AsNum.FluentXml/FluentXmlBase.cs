using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AsNum.FluentXml {

    public abstract class FluentXmlBase
    {
        public string Format
        {
            get;
            set;
        }

        public int? Order
        {
            get;
            set;
        }

        /// <summary>
        /// 当为 Null 或 空字符串的时候，是否显示，默认不显示
        /// </summary>
        public bool NullVisible
        {
            get;
            set;
        }

        public abstract XObject Build(string name);
    }

    public abstract class FluentXmlBase<T> : FluentXmlBase
    {
        public T Value
        {
            get;
            set;
        }

        public static implicit operator T(FluentXmlBase<T> fluent)
        {
            return fluent.Value;
        }

        public override XObject Build(string name)
        {
            if (this.Value != null || (this.Value == null && this.NullVisible))
            {
                return this.BuildXml(name);
            }
            else
                return null;
        }

        protected abstract XObject BuildXml(string name);

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
