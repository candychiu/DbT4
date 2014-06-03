using DatabaseSchemaReader.DataSchema;
using System;
using System.Text.RegularExpressions;

namespace DbT4Lib
{
    public class TableWrapper
    {
        // Properties
        public string Name { get; private set; }
        public string NameHumanCase { get; private set; }
        public string ClassName { get; private set; }
        public string CleanName { get; private set; }
        //public string Schema { get { return _table.SchemaOwner; } }
        //public string Type;
        //public bool IsMapping;
        //public bool IsView;
        //public bool HasForeignKey;
        //public bool HasNullableColumns;

        private DatabaseTable _table;
        public DatabaseTable Table { get { return _table; } }

        public TableWrapper(DatabaseTable table, UserSettings settings)
        {
            var nameHelper = settings.EntityNameService;
            var useCamelCase = settings.UseCamelCase;
            var prependSchemaName = settings.PrependSchemaName;

            _table = table;

            Name = _table.Name;
            //if (tableRenameFilter != null)
            //    Name = tableRenameFilter.Replace(Name, tableRenameReplacement);
            CleanName = nameHelper.CleanName(Name);
            ClassName = nameHelper.MakeSingular(CleanName);
            string singular = nameHelper.MakeSingular(Name);
            NameHumanCase = (useCamelCase ? nameHelper.ToTitleCase(singular) : singular).Replace(" ", "").Replace("$", "");
            if (prependSchemaName &&
                !string.IsNullOrEmpty(_table.SchemaOwner) && 
                string.Compare(_table.SchemaOwner, "dbo", StringComparison.OrdinalIgnoreCase) != 0)
                NameHumanCase = _table.SchemaOwner + "_" + NameHumanCase;
            NameHumanCase = nameHelper.ResolveNameConflict(NameHumanCase);
        }
    }

    public class ColumnWrapper
    {
        private static readonly Regex RxClean = new Regex("^(event|Equals|GetHashCode|GetType|ToString|repo|Save|IsNew|Insert|Update|Delete|Exists|SingleOrDefault|Single|First|FirstOrDefault|Fetch|Page|Query)$");

        public ColumnWrapper(DatabaseColumn column, TableWrapper tableWrapper, UserSettings settings)
        {
            var nameHelper = settings.EntityNameService;
            var useCamelCase = settings.UseCamelCase;
            var prependSchemaName = settings.PrependSchemaName;

            _column = column;

            //string typename = rdr["TypeName"].ToString().Trim();

            //var col = new Column
            //{
            //    Name = rdr["ColumnName"].ToString().Trim(),
            //    PropertyType = GetPropertyType(typename),
            //    MaxLength = (int)rdr["MaxLength"],
            //    Precision = (int)rdr["Precision"],
            //    Default = rdr["Default"].ToString().Trim(),
            //    DateTimePrecision = (int)rdr["DateTimePrecision"],
            //    Scale = (int)rdr["Scale"],
            //    Ordinal = (int)rdr["Ordinal"],
            //    IsIdentity = rdr["IsIdentity"].ToString().Trim().ToLower() == "true",
            //    IsNullable = rdr["IsNullable"].ToString().Trim().ToLower() == "true",
            //    IsStoreGenerated = rdr["IsStoreGenerated"].ToString().Trim().ToLower() == "true",
            //    IsPrimaryKey = rdr["PrimaryKey"].ToString().Trim().ToLower() == "true"
            //};

            //col.IsRowVersion = col.IsStoreGenerated && !col.IsNullable && typename == "timestamp";
            //if (col.IsRowVersion)
            //    col.MaxLength = 8;

            //col.CleanUpDefault();
            PropertyName = EntityNameService.CleanUp(Name);
            PropertyName = RxClean.Replace(PropertyName, "_$1");

            //// Make sure property name doesn't clash with class name
            if (PropertyName == tableWrapper.NameHumanCase)
                PropertyName = PropertyName + "_";

            PropertyNameHumanCase = (useCamelCase ? nameHelper.ToTitleCase(PropertyName) : PropertyName).Replace(" ", "");
            if (PropertyNameHumanCase == string.Empty)
                PropertyNameHumanCase = PropertyName;

            // Make sure property name doesn't clash with class name
            if (PropertyNameHumanCase == tableWrapper.NameHumanCase)
                PropertyNameHumanCase = PropertyNameHumanCase + "_";

            if (char.IsDigit(PropertyNameHumanCase[0]))
                PropertyNameHumanCase = "_" + PropertyNameHumanCase;

            //if (CheckNullable(col) != string.Empty)
            //    table.HasNullableColumns = true;

            // If PropertyType is empty, return null. Most likely ignoring a column due to legacy (such as OData not supporting spatial types)
            if (string.IsNullOrEmpty(PropertyType))
                throw new InvalidOperationException(string.Format("Unable to create Helper for column {0}", _column.Name));
        }

        private DatabaseColumn _column;

        public bool IncludeComment { get; set; }

        public string Name { get { return _column.Name; } }
        //public int DateTimePrecision;
        //public string Default;
        public int? MaxLength { get { return _column.Length.HasValue ? _column.Length.Value : 0; } }
        public int Precision { get { return _column.Precision.HasValue ? _column.Precision.Value : 0; } }
        public string PropertyName { get; private set; }
        public string PropertyNameHumanCase { get; private set; }
        public string PropertyType { get { return _column.DataType.NetDataTypeCSharpName; } }
        public int Scale { get { return _column.Scale.HasValue ? _column.Scale.Value : 0; } }
        //public int Ordinal;

        public bool IsIdentity { get { return _column.IsAutoNumber; } }
        public bool IsNullable { get { return _column.Nullable; } }
        public bool IsPrimaryKey { get { return _column.IsPrimaryKey; } }
        public bool IsStoreGenerated { get { return _column.IsComputed; } }
        //public bool IsRowVersion;

        public string Config
        {
            get
            {
                bool hasDatabaseGeneratedOption = false;
                string propertyType = PropertyType.ToLower();
                switch (propertyType)
                {
                    case "long":
                    case "short":
                    case "int":
                    case "double":
                    case "float":
                    case "decimal":
                    case "string":
                        hasDatabaseGeneratedOption = true;
                        break;
                }
                string databaseGeneratedOption = string.Empty;
                if (hasDatabaseGeneratedOption)
                {
                    if (IsIdentity)
                        databaseGeneratedOption = ".HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)";
                    if (IsStoreGenerated)
                        databaseGeneratedOption = ".HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed)";
                    if (IsPrimaryKey && !IsIdentity && !IsStoreGenerated)
                        databaseGeneratedOption = ".HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)";
                }
                return string.Format("Property(x => x.{0}).HasColumnName(\"{1}\"){2}{3}{4}{5}{6};", PropertyNameHumanCase, Name,
                                    (IsNullable) ? ".IsOptional()" : ".IsRequired()",
                                    (MaxLength > 0) ? ".HasMaxLength(" + MaxLength + ")" : string.Empty,
                                    (Scale > 0) ? ".HasPrecision(" + Precision + "," + Scale + ")" : string.Empty,
                                    //(IsRowVersion) ? ".IsFixedLength().IsRowVersion()" : string.Empty,
                                    databaseGeneratedOption);
            }
        }
        //public string ConfigFk;
        public string Entity
        {
            get
            {
                string comments = string.Empty;
                if (IncludeComment)
                {
                    comments = " // " + Name;
                    if (IsPrimaryKey)
                        comments += " (Primary key)";
                }
                return string.Format("public {0}{1} {2} {3}{4}", PropertyType, CheckNullable(_column), PropertyNameHumanCase, IsStoreGenerated ? "{ get; internal set; }" : "{ get; set; }", comments);
            }
        }
        //public string EntityFk;

        private static string CheckNullable(DatabaseColumn col)
        {
            string result = "";
            if (col.Nullable &&
                col.DataType.NetDataTypeCSharpName != "byte[]" &&
                col.DataType.NetDataTypeCSharpName != "string" &&
                col.DataType.NetDataTypeCSharpName != "Microsoft.SqlServer.Types.SqlGeography" &&
                col.DataType.NetDataTypeCSharpName != "Microsoft.SqlServer.Types.SqlGeometry" &&
                col.DataType.NetDataTypeCSharpName != "System.Data.Entity.Spatial.DbGeography" &&
                col.DataType.NetDataTypeCSharpName != "System.Data.Entity.Spatial.DbGeometry")
                result = "?";
            return result;
        }
    }
}
