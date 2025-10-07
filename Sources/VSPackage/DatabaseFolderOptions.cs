namespace SsmsDatabaseFolders
{
    using Localization;
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class DatabaseFolderOptions : DialogPage, IDatabaseFolderOptions
    {
        [CategoryResources(nameof(DatabaseFolderOptions) + "Active")]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(Enabled))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(Enabled))]
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        [CategoryResources(nameof(DatabaseFolderOptions) + "DatabaseFolderDisplayOptions")]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(GroupDatabasesByName))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(GroupDatabasesByName))]
        [DefaultValue(true)]
        public bool GroupDatabasesByName { get; set; } = true;

        [CategoryResources(nameof(DatabaseFolderOptions) + "DatabaseFolderDisplayOptions")]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(SeparateReadonlyDatabases))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(SeparateReadonlyDatabases))]
        [DefaultValue(false)]
        public bool SeparateReadonlyDatabases { get; set; } = false;

        [CategoryResources(nameof(DatabaseFolderOptions) + "DatabaseFolderDisplayOptions")]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(EnableDebugOutput))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(EnableDebugOutput))]
        [DefaultValue(false)]
        public bool EnableDebugOutput { get; set; } = false;

        [CategoryResources(nameof(DatabaseFolderOptions) + "DatabaseFolderDisplayOptions")]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(RegularExpressions))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(RegularExpressions))]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(CsvConverter))]
        public List<string> RegularExpressions { get; set; } = new List<string>();

        [Browsable(false)]
        [DefaultValue(null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string RegularExpressionsSerialized
        {
            get => CsvConverter.ConvertToString(RegularExpressions);
            set => RegularExpressions = CsvConverter.ConvertFromString(value, RegularExpressions);
        }

        [ReadOnly(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<ICustomFolderConfiguration> CustomFolderConfigurations
        {
            get
            {
                return CustomFolderConfigurationsImpl.OfType<ICustomFolderConfiguration>().ToList();
            }
        }

        [CategoryResources(nameof(DatabaseFolderOptions) + "DatabaseFolderDisplayOptions")]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(CustomFolderConfigurations))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(CustomFolderConfigurations))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(JsonLikeConverter))]
        public List<CustomFolderConfiguration> CustomFolderConfigurationsImpl { get; set; } = new List<CustomFolderConfiguration>();

        [Browsable(false)]
        [DefaultValue(null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string CustomFolderConfigurationsSerialized
        {
            get
            {
                return (new JsonLikeConverter().ConvertTo(CustomFolderConfigurationsImpl, typeof(string)) ?? string.Empty).ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    CustomFolderConfigurationsImpl = new List<CustomFolderConfiguration>();
                    return;
                }
                CustomFolderConfigurationsImpl = (List<CustomFolderConfiguration>)(new JsonLikeConverter().ConvertFrom(value));
            }
        }

        [ReadOnly(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsEnabledAndAnOptionSet
        {
            get
            {
                return Enabled && (GroupDatabasesByName || SeparateReadonlyDatabases || this.GroupDatabasesByCustomFolder());
            }
        }

        [CategoryResources(nameof(DatabaseFolderOptions))]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(Settings))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(Settings))]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Settings
        {
            get
            {
                var regExprFolder = RegularExpressionsSerialized;
                var customFolders = CustomFolderConfigurationsSerialized;
                var builder = new StringBuilder();
                builder.AppendFormat(@"{{
   ""Enabled"": ""{0}"",
   ""GroupDatabasesByName"": ""{1}"",
   ""SeparateReadonlyDatabases"": ""{2}"",
   ""EnableDebugOutput"": ""{3}"",
   ""RegularExpressions"": ""{4}"",
   ""CustomFolderConfigurations"": {5}
}}
", Enabled, GroupDatabasesByName, SeparateReadonlyDatabases, EnableDebugOutput, regExprFolder ?? string.Empty, customFolders ?? "[]");
                return builder.ToString();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        JsonLikeParser.ParseJsonText(value, (parent, name, isArray) =>
                        {
                            if (parent == null)
                                return this;
                            if (name == "CustomFolderConfigurations" && isArray)
                            {
                                this.CustomFolderConfigurationsImpl.Clear();
                                return this.CustomFolderConfigurationsImpl;
                            }

                            if (object.ReferenceEquals(this.CustomFolderConfigurationsImpl, parent))
                            {
                                var obj = new CustomFolderConfiguration();
                                this.CustomFolderConfigurationsImpl.Add(obj);
                                return obj;
                            }
                            return new object();
                        }, (current, propertyName, propertyValue) =>
                        {
                            if (object.ReferenceEquals(current, this))
                            {
                                bool boolVal;
                                switch (propertyName)
                                {
                                    case "Enabled":
                                        if (bool.TryParse(propertyValue, out boolVal))
                                        {
                                            this.Enabled = boolVal;
                                        }
                                        break;
                                    case "GroupDatabasesByName":
                                        if (bool.TryParse(propertyValue, out boolVal))
                                        {
                                            this.GroupDatabasesByName = boolVal;
                                        }
                                        break;
                                    case "SeparateReadonlyDatabases":
                                        if (bool.TryParse(propertyValue, out boolVal))
                                        {
                                            this.SeparateReadonlyDatabases = boolVal;
                                        }
                                        break;
                                    case "EnableDebugOutput":
                                        if (bool.TryParse(propertyValue, out boolVal))
                                        {
                                            this.EnableDebugOutput = boolVal;
                                        }
                                        break;
                                    case "RegularExpressions":
                                        this.RegularExpressionsSerialized = propertyValue;
                                        break;
                                    case "CustomFolderConfigurations":
                                        this.CustomFolderConfigurationsSerialized = propertyValue;
                                        break;
                                }
                            }
                            else
                            {
                                JsonLikeConverter.SetProperties(current, propertyName, propertyValue);
                            }
                        });
                    }
                    catch
                    {
                        // ignore it
                    }
                }
            }
        }

        public bool ShouldSerializeRegularExpressions()
        {
            return false;
        }

        public bool ShouldSerializeCustomFolderConfigurations()
        {
            return false;
        }

        public bool ShouldSerializeCustomFolderConfigurationsImpl()
        {
            return false;
        }

        public bool ShouldSerializeSettings()
        {
            return false;
        }

        public bool ShouldIsEnabledAndAnOptionSet()
        {
            return false;
        }
    }

    public class CustomFolderConfiguration : ICustomFolderConfiguration
    {
        [CategoryResources(nameof(DatabaseFolderOptions) + "DatabaseFolderDisplayOptions")]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(CustomFolderConfiguration) + nameof(CustomFolderName))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(CustomFolderConfiguration) + nameof(CustomFolderName))]
        public string CustomFolderName { get; set; }

        [CategoryResources(nameof(DatabaseFolderOptions) + "DatabaseFolderDisplayOptions")]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(CustomFolderConfiguration) + nameof(RegularExpressions))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(CustomFolderConfiguration) + nameof(RegularExpressions))]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [TypeConverter(typeof(CsvConverter))]
        public List<string> RegularExpressions { get; set; } = new List<string>();

        // doesn't work; setting color or font of the treenodes doesn't change anything
        //[CategoryResources(nameof(DatabaseFolderOptions) + "DatabaseFolderDisplayOptions")]
        //[DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(CustomFolderConfiguration) + nameof(BackColor))]
        //[DescriptionResources(nameof(DatabaseFolderOptions) + nameof(CustomFolderConfiguration) + nameof(BackColor))]
        //[Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        //[DefaultValue(null)]
        //public Color? BackColor { get; set; }

        [CategoryResources(nameof(DatabaseFolderOptions) + "DatabaseFolderDisplayOptions")]
        [DisplayNameResources(nameof(DatabaseFolderOptions) + nameof(CustomFolderConfiguration) + nameof(UseOtherGroupingMethodsInside))]
        [DescriptionResources(nameof(DatabaseFolderOptions) + nameof(CustomFolderConfiguration) + nameof(UseOtherGroupingMethodsInside))]
        [DefaultValue(false)]
        public bool UseOtherGroupingMethodsInside { get; set; } = false;

        [Browsable(false)]
        [DefaultValue(null)]
        public string RegularExpressionsSerialized
        {
            get => CsvConverter.ConvertToString(RegularExpressions);
            set => RegularExpressions = CsvConverter.ConvertFromString(value, RegularExpressions);
        }

        public CustomFolderConfiguration()
        {
            CustomFolderName = Resources.Resources.DefaultCustomFolderName;
        }

        public bool ShouldSerializeRegularExpressions()
        {
            return false;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(CustomFolderName))
                return Resources.Resources.DefaultCustomFolderName;
            return CustomFolderName;
        }
    }

    public class CsvConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            List<String> v = value as List<String>;
            if (destinationType == typeof(string))
            {
                return String.Join(",", v.ToArray());
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public static string ConvertToString(IList<string> source)
        {
            if (source.Count < 1)
                return null;
            return string.Join("|", source
                .Select(_ => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(_))));
        }

        public static List<string> ConvertFromString(string source, List<string> dest)
        {
            if (dest == null)
                dest = new List<string>();
            else
                dest.Clear();
            if (source == null)
            {
                return dest;
            }
            var base64Elements = source.Split('|');
            foreach (var base64Element in base64Elements)
            {
                dest.Add(Encoding.UTF8.GetString(Convert.FromBase64String(base64Element)));
            }

            return dest;
        }
    }

    public class JsonLikeConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var configurations = value as List<CustomFolderConfiguration>;
            if (configurations == null)
                return base.ConvertTo(context, culture, value, destinationType);

            if (destinationType == typeof(string))
            {
                var result = new StringBuilder(256);
                var delimiter = "";
                result.Append('[');
                foreach (CustomFolderConfiguration configuration in configurations)
                {
                    result.AppendFormat(@"{0}
   {{
      ""CustomFolderName"": ""{1}"",
      ""UseOtherGroupingMethodsInside"": ""{2}"",
      ""RegularExpressions"": ""{3}""
   }}", delimiter, configuration.CustomFolderName.Replace('"', '\''), configuration.UseOtherGroupingMethodsInside, configuration.RegularExpressionsSerialized);
                    delimiter = @",
";
                    //result.Append("{\"CustomFolderName\":\"");
                    //result.Append(configuration.CustomFolderName.Replace('"', '\''));
                    //result.Append("\",\"UseOtherGroupingMethodsInside\":\"");
                    //result.Append(configuration.UseOtherGroupingMethodsInside);
                    //result.Append("\",\"RegularExpressions\":\"");
                    //result.Append(configuration.RegularExpressionsSerialized);
                    //result.Append("\"},");
                }
                result.Append(']');
                return result.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var result = new List<CustomFolderConfiguration>();
            if (value == null)
                return result;

            JsonLikeParser.ParseJsonText(value.ToString(), (parent, name, isArray) =>
            {
                if (parent == null)
                    return result;
                var obj = new CustomFolderConfiguration();
                if (object.ReferenceEquals(result, parent))
                    result.Add(obj);
                return obj;
            }, SetProperties);

            return result;
        }

        internal static void SetProperties(object current, string propertyName, string propertyValue)
        {
            if (current is CustomFolderConfiguration customFolderConfig)
            {
                switch (propertyName)
                {
                    case "CustomFolderName":
                        customFolderConfig.CustomFolderName = propertyValue;
                        break;
                    case "UseOtherGroupingMethodsInside":
                        if (bool.TryParse(propertyValue, out var boolVal))
                        {
                            customFolderConfig.UseOtherGroupingMethodsInside = boolVal;
                        }
                        break;
                    case "BackColor":
                        break;
                    case "RegularExpressions":
                        customFolderConfig.RegularExpressionsSerialized = propertyValue;
                        break;
                }
            }
        }
    }

    internal class JsonLikeParser
    {
        internal static object ParseJsonText(string jsonText, Func<object, string, bool, object> createNewObject, Action<object, string, string> setValue)
        {
            var state = JsonState.None;
            var currentName = new StringBuilder(32);
            var currentValue = new StringBuilder(32);
            var currentObject = (object)null;
            var objectStack = new Stack<object>();

            foreach (var ch in jsonText)
            {
                switch (ch)
                {
                    case '[':
                        switch (state)
                        {
                            case JsonState.None:
                            case JsonState.OutSideName:
                                state = JsonState.InSideArray;
                                objectStack.Push(currentObject);
                                currentObject = createNewObject(currentObject, currentName.ToString(), true);
                                currentName.Length = 0;
                                break;
                            case JsonState.InSideName:
                                currentName.Append(ch);
                                break;
                            case JsonState.InSideValue:
                                currentValue.Append(ch);
                                break;
                        }
                        break;
                    case ']':
                        switch (state)
                        {
                            case JsonState.InSideName:
                                currentName.Append(ch);
                                break;
                            case JsonState.InSideValue:
                                currentValue.Append(ch);
                                break;
                        }
                        break;
                    case '{':
                        switch (state)
                        {
                            case JsonState.InSideName:
                                currentName.Append(ch);
                                break;
                            case JsonState.InSideValue:
                                currentValue.Append(ch);
                                break;
                            default:
                                state = JsonState.InSideObject;
                                objectStack.Push(currentObject);
                                currentObject = createNewObject(currentObject, currentName.ToString(), false);
                                currentName.Length = 0;
                                break;
                        }
                        break;
                    case '}':
                        switch (state)
                        {
                            case JsonState.InSideName:
                                currentName.Append(ch);
                                break;
                            case JsonState.InSideValue:
                                currentValue.Append(ch);
                                break;
                            default:
                                if (objectStack.Count > 1)
                                    currentObject = objectStack.Pop();
                                break;
                        }
                        break;
                    case '"':
                        switch (state)
                        {
                            case JsonState.InSideObject:
                                state = JsonState.InSideName;
                                break;
                            case JsonState.InSideName:
                                state = JsonState.OutSideName;
                                break;
                            case JsonState.OutSideName:
                                state = JsonState.InSideValue;
                                break;
                            case JsonState.InSideValue:
                                if (currentObject != null)
                                {
                                    setValue(currentObject, currentName.ToString(), currentValue.ToString());
                                    currentName.Length = 0;
                                    currentValue.Length = 0;
                                }
                                state = JsonState.InSideObject;
                                break;
                        }
                        break;
                    default:
                        switch (state)
                        {
                            case JsonState.InSideName:
                                currentName.Append(ch);
                                break;
                            case JsonState.InSideValue:
                                currentValue.Append(ch);
                                break;
                        }
                        break;
                }
            }
            while (objectStack.Count > 1)
                currentObject = objectStack.Pop();

            return currentObject;
        }

        private enum JsonState
        {
            None,
            InSideArray,
            InSideObject,
            InSideName,
            OutSideName,
            InSideValue
        }
    }
}
