using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    public class DiagnosticListenerOptions
    {
        private readonly DefaultTags _defaultTags = new DefaultTags();
        
        public Dictionary<(string, Type), Func<PropertyInfo, IFormatter>> CustomFieldFormatters { get; } = new Dictionary<(string, Type), Func<PropertyInfo, IFormatter>>();
        public Dictionary<(string, Type), Func<PropertyInfo, IFormatter>> CustomTagFormatters { get; } = new Dictionary<(string, Type), Func<PropertyInfo, IFormatter>>();
        
        public Func<string, string> NameFixer { get; set; }
        public Action<DiagnosticListener, Exception> OnError { get; set; }

        public byte[] DefaultTags => _defaultTags.Bytes;

        public void AddCustomFieldFormatter(string propertyName, Type propertyType, Func<PropertyInfo, IFormatter> formatter)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (propertyType == null) throw new ArgumentNullException(nameof(propertyType));
            CustomFieldFormatters[(propertyName, propertyType)] = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }
        
        public void AddCustomFieldFormatter(Type propertyType, Func<PropertyInfo, IFormatter> formatter)
        {
            AddCustomFieldFormatter(string.Empty, propertyType, formatter);
        }

        public void AddCustomTagFormatter(Type propertyType, Func<PropertyInfo, IFormatter> formatter) => AddCustomTagFormatter(string.Empty, propertyType, formatter);

        public void AddCustomTagFormatter(string propertyName, Type propertyType, Func<PropertyInfo, IFormatter> formatter)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (propertyType == null) throw new ArgumentNullException(nameof(propertyType));
            CustomTagFormatters[(propertyName, propertyType)] = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public void AddDefaultTag(string name, string value)
        {
            _defaultTags.Add(name, value);
        }
    }
}