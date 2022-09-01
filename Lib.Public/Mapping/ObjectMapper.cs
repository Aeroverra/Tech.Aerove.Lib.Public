using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace LibPublic.Mapping
{
    public class ObjectMapper<TOutput>
    {
        private readonly Type Type;
        private readonly List<PropertyInfo> Properties;

        public ObjectMapper()
        {
            Type = typeof(TOutput);
            Properties = typeof(TOutput).GetProperties().ToList();
            //GetProperties(BindingFlags.Public | BindingFlags.Instance)
        }

        public TOutput Map(object input)
        {
            var output = Activator.CreateInstance<TOutput>();
            var inputProperties = input.GetType().GetProperties();
            foreach (var prop in inputProperties)
            {
                var outputProp = ToOutputProperty(prop);
                if (outputProp == null)
                {
                    continue;
                }
                var value = prop.GetValue(input);
                outputProp.SetValue(output, value);
            }
            return output;
        }


        public List<TOutput> Map<T>(List<T> items)
        {
            var output = Activator.CreateInstance<List<TOutput>>();

            foreach (var i in items)
            {
                output.Add(Map(i));
            }
            return output;
        }

        /// <summary>
        /// Updates an object with values from another
        /// </summary>
        /// <typeparam name="TSourceData"></typeparam>
        /// <param name="output">The object you want to update</param>
        /// <param name="source">Source of the data to update the output with</param>
        public void Patch<TSourceData>(TOutput output, TSourceData source)
        {
            var sourceProperties = source.GetType().GetProperties();
            foreach (var sourceProp in sourceProperties)
            {
                if (sourceProp.IsReadOnly())
                {
                    continue;
                }
                var value = sourceProp.GetValue(source);
                if (value == null)
                {
                    continue;
                }

                var outputProperty = ToOutputProperty(sourceProp);
                if (outputProperty == null || outputProperty.IsKey())
                {
                    continue;
                }

                outputProperty.SetValue(output, value);

            }
        }

        /// <summary>
        /// returns a an object source property of any type to the output property
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private PropertyInfo? ToOutputProperty(PropertyInfo source)
        {
            var property = Properties.FirstOrDefault(x => x.Name == source.Name);
            if (property == null)
            {
                return null;
            }
            var sourceType = property.PropertyType.GetUnderlyingType();
            var outputType = property.PropertyType.GetUnderlyingType();

            if (outputType != sourceType)
            {
                return null;
            }

            return property;
        }

    }

    internal static class ObjectMapperExtensions
    {
        public static bool IsReadOnly(this Type type, bool defaultValue = false)
        {
            var isReadOnly = false;
            var sourceTypeAttributes = type.GetCustomAttributes();
            var schemaAttribute = sourceTypeAttributes.SingleOrDefault(x => x.GetType() == typeof(ReadOnlyAttribute));
            if (schemaAttribute != null)
            {
                isReadOnly = (schemaAttribute as ReadOnlyAttribute).IsReadOnly;
            }
            return isReadOnly;
        }

        /// <summary>
        /// returns the base type if its nullable or itself if its note.
        /// Used for comparison of nullable and non nullable types
        /// </summary>
        /// <returns></returns>
        public static Type GetUnderlyingType(this Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                // It's nullable
                return nullableType;
            }
            return type;
        }

        public static bool IsReadOnly(this object obj)
        {
            return obj.GetType().IsReadOnly();
        }

        public static bool IsReadOnly(this PropertyInfo propertyInfo)
        {
            var isReadOnly = propertyInfo.DeclaringType.IsReadOnly();
            isReadOnly = propertyInfo.PropertyType.IsReadOnly(isReadOnly);
            return isReadOnly;
        }

        public static bool IsKey(this PropertyInfo propertyInfo)
        {
            var outputAttributes = propertyInfo.GetCustomAttributes();
            if (outputAttributes.Any(x => x.GetType() == typeof(KeyAttribute)))
            {
                return true;
            }
            return false;
        }
    }
}
