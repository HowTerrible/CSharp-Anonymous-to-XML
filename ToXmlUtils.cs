using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace XmlTest
{
    public static class ToXmlUtils
    {
        private static readonly Type[] WriteTypes = new[] {
        typeof(string),
        typeof(Enum),
        typeof(DateTime), typeof(DateTime?),
        typeof(DateTimeOffset), typeof(DateTimeOffset?),
        typeof(int), typeof(int?),
        typeof(decimal), typeof(decimal?),
        typeof(Guid), typeof(Guid?),
        typeof(long), typeof(long?),
    };
        public static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive || WriteTypes.Contains(type);
        }
        public static object ToXml(this object input)
        {
            return input.ToXml(null);
        }

        /// <summary>
        /// Get Property Value
        /// 获得属性值
        /// GetPropertyValue
        /// </summary>
        /// <param name="input"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private static object GetPropertyValue(object input, PropertyInfo info)
        {
            object result;
            if (input.GetType() == typeof(string) || input.GetType() == typeof(int))
            {
                result = input.ToString();
            }
            else
            {
                if (info.PropertyType.IsEnum)
                {
                    result = (int)info.GetValue(input, null);
                }
                else
                {
                    result = info.GetValue(input, null);
                }
            }

            return result;
        }

        /// <summary>
        /// Get Field Value
        /// 获得字段值
        /// </summary>
        /// <param name="input"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private static object GetFieldValue(object input, FieldInfo info)
        {
            object result;
            if (input.GetType() == typeof(string) || input.GetType() == typeof(int))
            {
                result = input.ToString();
            }
            else
            {
                result = info.GetValue(input);
            }
            return result;
        }

        /// <summary>
        /// Get Object Name
        /// 获得对象节点名称
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static string GetObjectName(object info, string AlternativeName)
        {
            var type = info.GetType();
            string attributeName = GetXMLElementAttrNameFromAttribute(info.GetType().GetCustomAttributes(true));
            return string.IsNullOrEmpty(attributeName) ? AlternativeName : attributeName;
        }

        /// <summary>
        /// Get Property Name
        /// 获取属性节点名称
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static string GetPropertyName(PropertyInfo info)
        {

            string attributeName = GetXMLElementAttrNameFromAttribute(info.GetCustomAttributes(true));
            return string.IsNullOrEmpty(attributeName) ? info.Name : attributeName;
        }

        /// <summary>
        /// Get Field Name
        /// 获取字段节点名称
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static string GetFieldName(FieldInfo info)
        {
            string attributeName = GetXMLElementAttrNameFromAttribute(info.GetCustomAttributes(true));
            return string.IsNullOrEmpty(attributeName) ? info.Name : attributeName;
        }

        /// <summary>
        /// 通过XML特性获得节点名称
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private static string GetXMLElementAttrNameFromAttribute(object[] attributes)
        {
            string attrName = "";

            if (attributes != null && attributes.Length > 0)
            {

                foreach (var item in attributes)
                {
                    if (item is XmlElementAttribute)
                    {
                        var temp = item as XmlElementAttribute;
                        attrName = temp.ElementName;


                    }
                    else if (item is XmlRootAttribute)
                    {
                        var temp = item as XmlRootAttribute;
                        attrName = temp.ElementName;
                    }
                }
            }
            return attrName;
        }

        public static object ToXml(this object input, string element)
        {
            if (input == null)  
                return null;

            if (string.IsNullOrEmpty(element))
                element = "object";
            element = XmlConvert.EncodeName(element);
            var ret = new XElement(element);

            if (input != null)
            {
                var type = input.GetType();

                if (input is IEnumerable && !type.IsSimpleType())
                {
                    var elements = input as IEnumerable<object>;

                    var tempData = new XElement(GetObjectName(input, element));
                    foreach(var i in elements)
                    {
                        Type tempType = i.GetType();
                        tempData.Add(i.ToXml(tempType.Name));
                    }
                    return tempData;
                }
                else
                {
                    var props = type.GetProperties();
                    var propElements = from prop in props
                                   let name = XmlConvert.EncodeName(GetPropertyName(prop))
                                   let val = GetPropertyValue(input, prop)
                                   let value = prop.PropertyType.IsSimpleType()
                                        ? new XElement(name, val)
                                        : val.ToXml(name)
                                   where value != null
                                   select value;
                    ret.Add(propElements);

                    var fields = type.GetFields();
                    var fieldElements = from field in fields
                                        let name = XmlConvert.EncodeName(GetFieldName(field))
                               let val = GetFieldValue(input, field)
                               let value = field.FieldType.IsSimpleType()
                                    ? new XElement(name, val)
                                    : val.ToXml(name)
                               where value != null
                               select value;

                    ret.Add(fieldElements);
                }
            }

            return ret;
        }
    }
}
