namespace SsmsDatabaseFolders
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Localization;

    using Microsoft.VisualStudio.Shell;

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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(CsvConverter))]
        public List<string> RegularExpressions { get; set; } = new List<string>();

        [Browsable(false)]
        [DefaultValue(null)]
        public string RegularExpressionsSerialized
        {
            get
            {
                if (RegularExpressions.Count < 1)
                    return null;
                return string.Join("|", RegularExpressions
                    .Select(_ => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(_))));
            }
            set
            {
                if (RegularExpressions == null)
                    RegularExpressions = new List<string>();
                else
                    RegularExpressions.Clear();
                if (value == null)
                {
                    return;
                }
                var base64Elements = value.Split('|');
                foreach (var base64Element in base64Elements)
                {
                    RegularExpressions.Add(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Element)));
                }
            }
        }

        [ReadOnly(true)]
        [Browsable(false)]
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(JsonLikeConverter))]
        public List<CustomFolderConfiguration> CustomFolderConfigurationsImpl { get; set; } = new List<CustomFolderConfiguration>();

        [Browsable(false)]
        [DefaultValue(null)]
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
        public bool IsEnabledAndAnOptionSet
        {
            get
            {
                return Enabled && (GroupDatabasesByName || SeparateReadonlyDatabases || this.GroupDatabasesByCustomFolder());
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
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
            get
            {
                if (RegularExpressions.Count < 1)
                    return null;
                return string.Join("|", RegularExpressions
                    .Select(_ => Convert.ToBase64String(Encoding.UTF8.GetBytes(_))));
            }
            set
            {
                if (RegularExpressions == null)
                    RegularExpressions = new List<string>();
                else
                    RegularExpressions.Clear();
                if (value == null)
                {
                    return;
                }
                var base64Elements = value.Split('|');
                foreach (var base64Element in base64Elements)
                {
                    RegularExpressions.Add(Encoding.UTF8.GetString(Convert.FromBase64String(base64Element)));
                }
            }
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
                result.Append('[');
                foreach (CustomFolderConfiguration configuration in configurations)
                {
                    result.Append("{\"CustomFolderName\":\"");
                    result.Append(configuration.CustomFolderName.Replace('"', '\''));
                    result.Append("\",\"UseOtherGroupingMethodsInside\":\"");
                    result.Append(configuration.UseOtherGroupingMethodsInside);
                    result.Append("\",\"RegularExpressions\":\"");
                    result.Append(configuration.RegularExpressionsSerialized);
                    result.Append("\"},");
                }
                if (result.Length > 1)
                    result.Length--;
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

            var state = JsonState.None;
            var currentName = new StringBuilder(32);
            var currentValue = new StringBuilder(32);
            CustomFolderConfiguration current = null;

            foreach (var ch in value.ToString())
            {
                switch (ch)
                {
                    case '[':
                        switch (state)
                        {
                            case JsonState.None:
                                state = JsonState.InSideArray;
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
                                current = new CustomFolderConfiguration();
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
                                if (current != null)
                                    result.Add(current);
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
                                if (current != null)
                                {
                                    switch (currentName.ToString())
                                    {
                                        case "CustomFolderName":
                                            current.CustomFolderName = currentValue.ToString();
                                            break;
                                        case "UseOtherGroupingMethodsInside":
                                            if (bool.TryParse(currentValue.ToString(), out var boolVal))
                                            {
                                                current.UseOtherGroupingMethodsInside = boolVal;
                                            }
                                            break;
                                        case "BackColor":
                                            break;
                                        case "RegularExpressions":
                                            current.RegularExpressionsSerialized = currentValue.ToString();
                                            break;
                                    }
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
            return result;
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
