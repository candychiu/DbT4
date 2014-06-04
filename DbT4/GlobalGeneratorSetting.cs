using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbT4Lib
{
    /// <summary>
    /// A class to capture code generation settings.  It can be broken up into multiple 
    /// configuration classes.
    /// 
    /// Code borrowed from Reverse POCO
    /// </summary>
    public class UserSettings
    {
        // Misc settings **********************************************************************************************************************
        public string Namespace { get; set; } // Override the default namespace here
        public string DbContextName { get; set; }
        public string ConnectionStringName { get; set; }  // Searches for this connection string in config files listed below
        public string ConfigurationClassName { get; set; } // Configuration, Mapping, Map, etc. This is appended to the Poco class name to configure the mappings.
        public string[] ConfigFilenameSearchOrder { get; set; } // Add more here if required. The config files are searched for in the local project first, then the whole solution second.
        public bool MakeClassesPartial { get; set; }
        public bool GenerateSeparateFiles { get; set; }
        public bool UseCamelCase { get; set; }   // This will rename the tables & fields to use CamelCase. If false table & field names will be left alone.
        public bool IncludeComments { get; set; } // Adds comments the generated code
        public bool IncludeViews { get; set; }
        public bool DisableGeographyTypes { get; set; } // Turns off use of System.Data.Entity.Spatial.DbGeography and System.Data.Entity.Spatial.DbGeometry as OData doesn't support entities with geometry/geography types.
        public string CollectionType { get; set; }  // Determines the type of collection for the Navigation Properties. "ObservableCollection" for example
        public string CollectionTypeNamespace { get; set; } // "System.Collections.ObjectModel" is required if setting the CollectionType = "ObservableCollection"


        // Pluralization **********************************************************************************************************************
        // To turn off pluralization, use:
        //      Inflector.PluralizationService = null;
        // Default pluralization, use:
        //      Inflector.PluralizationService = new EnglishPluralizationService();
        // For Spanish pluralization:
        //      1. Intall the "EF6.Contrib" Nuget Package.
        //      2. Add the following to the top of this file and adjust path, and remove the space between the angle bracket and # at the beginning and end.
        //         < #@ assembly name="your full path to \EntityFramework.Contrib.dll" # >
        //      3. Change the line below to: Inflector.PluralizationService = new SpanishPluralizationService();
        //Inflector.PluralizationService = new EnglishPluralizationService();
        public EntityNameService EntityNameService { get; set; }

        // Elements to generate ***************************************************************************************************************
        // Add the elements that should be generated when the template is executed.
        // Multiple projects can now be used that separate the different concerns.
        [Flags]
        public enum Elements
        {
            Poco = 1,
            Context = 2,
            UnitOfWork = 4,
            PocoConfiguration = 8
        };
        public Elements ElementsToGenerate { get; set; }

        // Use these namespaces to specify where the different elements of the template are being generated.
        public string PocoNamespace { get; set; }
        public string ContextNamespace { get; set; }
        public string UnitOfWorkNamespace { get; set; }
        public string PocoConfigurationNamespace { get; set; }

        // Example of separate concerns:
        //PocoNamespace = "MyProject.Model";
        //ContextNamespace = "MyProject.Data";
        //UnitOfWorkNamespace = "MyProject.Data";	
        //PocoConfigurationNamespace = "MyProject.Data";
        //ElementsToGenerate = Elements.Poco;   // Model Project
        //ElementsToGenerate = Elements.Context | Elements.UnitOfWork | Elements.PocoConfiguration; //  Data project


        // Schema *****************************************************************************************************************************
        // If there are multiple schema, then the table name is prefixed with the schema, except for dbo.
        // Ie. dbo.hello will be Hello.
        //     abc.hello will be AbcHello.
        // To only include a single schema, specify it below.
        public string SchemaName { get; set; }
        public bool PrependSchemaName { get; set; }   // Control if the schema name is prepended to the table name

        // Filtering **************************************************************************************************************************
        // Use the following table/view name regex filters to include or exclude tables/views
        // Exclude filters are checked first and tables matching filters are removed.
        //  * If left null, none are excluded.
        //  * If not null, any tables matching the regex are excluded.
        // Include filters are checked second.
        //  * If left null, all are included.
        //  * If not null, only the tables matching the regex are included.
        //  Example:    TableFilterExclude = new Regex(".*auto.*");
        //              TableFilterInclude = new Regex("(.*_FR_.*)|(data_.*)");
        //              TableFilterInclude = new Regex("^table_name1$|^table_name2$|etc");
        //TableFilterExclude = null;
        //TableFilterInclude = null;

        // Table renaming *********************************************************************************************************************
        // Use the following regex to rename tables such as tblOrders to Orders, AB_Shipments to Shipments, etc.
        // TableRenameFilter = new Regex("^tbl|^AB_");
        //TableRenameFilter = null;
        //TableRenameReplacement = "";


        // Callbacks **********************************************************************************************************************
        // This method will be called right before we write the POCO header.
        public Action<TableWrapper> WritePocoClassAttributes { get; set; }
        public Func<TableWrapper, string> WritePocoBaseClasses { get; set; }    // Writes optional base classes // t => "IMyBaseClass";
        public Action<TableWrapper> WritePocoBaseClassBody { get; set; } // Writes any boilerplate stuff
        public Func<ColumnWrapper, string> WritePocoColumn { get; set; }

        /// <summary>
        /// Constructor which contains default settings.
        /// </summary>
        public UserSettings()
        {
            Namespace = "";
            DbContextName = "MyDb";
            ConnectionStringName = "";  // Searches for this connection string in config files listed below
            ConfigurationClassName = "Configuration"; // Configuration, Mapping, Map, etc. This is appended to the Poco class name to configure the mappings.
            ConfigFilenameSearchOrder = new string[] { "app.config", "web.config", "app.config.transform", "web.config.transform" }; // Add more here if required. The config files are searched for in the local project first, then the whole solution second.
            MakeClassesPartial = false;
            GenerateSeparateFiles = false;
            UseCamelCase = true;    // This will rename the tables & fields to use CamelCase. If false table & field names will be left alone.
            IncludeComments = true; // Adds comments the generated code
            IncludeViews = true;
            DisableGeographyTypes = false; // Turns off use of System.Data.Entity.Spatial.DbGeography and System.Data.Entity.Spatial.DbGeometry as OData doesn't support entities with geometry/geography types.
            CollectionType = "List";  // Determines the type of collection for the Navigation Properties. "ObservableCollection" for example
            CollectionTypeNamespace = "";

            EntityNameService = new EntityNameService(EntityNameService.EN);

            ElementsToGenerate = Elements.Poco;

            PocoNamespace = "";
            ContextNamespace = "";
            UnitOfWorkNamespace = "";
            PocoConfigurationNamespace = "";

            SchemaName = null;
            PrependSchemaName = true;

            WritePocoClassAttributes = t =>
            {
                // Do nothing by default
                // Example:
                // if(t.ClassName.StartsWith("Order"))
                //     WriteLine("    [SomeAttribute]");
            };
            WritePocoBaseClasses = null;
            WritePocoBaseClassBody = t =>
            {
                // Do nothing by default
                // Example:
                // WriteLine("        // " + t.ClassName);
            };
            WritePocoColumn = c => c.Entity;
        }
    }
}
