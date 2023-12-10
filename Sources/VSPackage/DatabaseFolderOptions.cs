namespace SsmsDatabaseFolders
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    using Localization;

    using Microsoft.VisualStudio.Shell;

    public class DatabaseFolderOptions : DialogPage, IDatabaseFolderOptions
    {
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

        public bool ShouldSerializeRegularExpressions()
        {
            return false;
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
}
