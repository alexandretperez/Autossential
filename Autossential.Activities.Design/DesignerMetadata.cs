using Autossential.Activities.Design.Designers;
using Autossential.Activities.Design.PropertyEditors;
using Autossential.Shared.Activities.Localization;
using Autossential.Activities.Properties;
using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Autossential.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public const string MAIN_CATEGORY = "Autossential";
        public const string DATA_TABLE_CATEGORY = MAIN_CATEGORY + ".DataTable";
        public const string FILE_CATEGORY = MAIN_CATEGORY + ".File";
        public const string FILE_COMPRESSION_CATEGORY = FILE_CATEGORY + ".Compression";
        public const string PROGRAMMING_CATEGORY = MAIN_CATEGORY + ".Programming";
        public const string WORKFLOW_CATEGORY = MAIN_CATEGORY + ".Workflow";
        public const string SECURITY_CATEGORY = MAIN_CATEGORY + ".Security";
        public const string DIAGNOSTICS_CATEGORY = MAIN_CATEGORY + ".Diagnostics";

        public void Register()
        {
            var builder = new AttributeTableBuilder();

            var dataTable = new CategoryAttribute(DATA_TABLE_CATEGORY);
            AddCustomAttributes(builder, dataTable, typeof(Aggregate), typeof(AggregateDesigner));
            AddCustomAttributes(builder, dataTable, typeof(PromoteHeaders), typeof(PromoteHeadersDesigner));
            AddCustomAttributes(builder, dataTable, typeof(DataRowToDictionary), typeof(DataRowToDictionaryDesigner));
            AddCustomAttributes(builder, dataTable, typeof(DictionaryToDataTable), typeof(DictionaryToDataTableDesigner));
            AddCustomAttributes(builder, dataTable, typeof(RemoveEmptyRows), typeof(RemoveEmptyRowsDesigner));
            AddCustomAttributes(builder, dataTable, typeof(RemoveDataColumns), typeof(RemoveDataColumnsDesigner));
            AddCustomAttributes(builder, dataTable, typeof(RemoveDuplicateRows), typeof(RemoveDuplicateRowsDesigner));

            var file = new CategoryAttribute(FILE_CATEGORY);
            AddCustomAttributes(builder, file, typeof(WaitFile), typeof(WaitFileDesigner));
            AddCustomAttributes(builder, file, typeof(EnumerateFiles), typeof(EnumerateFilesDesigner));
            AddCustomAttributes(builder, file, typeof(CleanUpFolder), typeof(CleanUpFolderDesigner));

            var fileCompression = new CategoryAttribute(FILE_COMPRESSION_CATEGORY);
            AddCustomAttributes(builder, fileCompression, typeof(Zip), typeof(ZipDesigner));
            AddCustomAttributes(builder, fileCompression, typeof(Unzip), typeof(UnzipDesigner));
            AddCustomAttributes(builder, fileCompression, typeof(ZipEntriesCount), typeof(ZipEntriesCountDesigner));

            var workflow = new CategoryAttribute(WORKFLOW_CATEGORY);
            AddCustomAttributes(builder, workflow, typeof(Exit), typeof(ExitDesigner));
            AddCustomAttributes(builder, workflow, typeof(Next), typeof(NextDesigner));
            AddCustomAttributes(builder, workflow, typeof(Container), typeof(ContainerDesigner));
            AddCustomAttributes(builder, workflow, typeof(CheckPoint), typeof(CheckPointDesigner));
            AddCustomAttributes(builder, workflow, typeof(Iterate), typeof(IterateDesigner));

            var programming = new CategoryAttribute(PROGRAMMING_CATEGORY);
            AddCustomAttributes(builder, programming, typeof(Increment), typeof(IncrementDesigner));
            AddCustomAttributes(builder, programming, typeof(Decrement), typeof(DecrementDesigner));
            AddCustomAttributes(builder, programming, typeof(CultureScope), typeof(CultureScopeDesigner));

            var security = new CategoryAttribute(SECURITY_CATEGORY);
            AddCustomAttributes(builder, security, typeof(EncryptText), typeof(EncryptTextDesigner));
            AddCustomAttributes(builder, security, typeof(DecryptText), typeof(DecryptTextDesigner));
            AddCustomAttributes(builder, security, typeof(EncryptDataTable), typeof(EncryptDataTableDesigner));
            AddCustomAttributes(builder, security, typeof(DecryptDataTable), typeof(DecryptDataTableDesigner));

            var diagnostics = new CategoryAttribute(DIAGNOSTICS_CATEGORY);
            AddCustomAttributes(builder, diagnostics, typeof(Stopwatch), typeof(StopwatchDesigner));

            foreach (var activityType in GetActivities())
                ApplyPropertyAttributes(builder, activityType);

            AddEditorAttributes(builder);

            builder.ValidateTable();
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        private void AddEditorAttributes(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(CheckPoint), nameof(CheckPoint.Data), new EditorAttribute(typeof(ArgumentDictionaryPropertyEditor), typeof(DialogPropertyValueEditor)));
        }

        private static IEnumerable<Type> GetActivities()
        {
            yield return typeof(Aggregate);
            yield return typeof(CheckPoint);
            yield return typeof(CleanUpFolder);
            yield return typeof(Container);
            yield return typeof(CultureScope);
            yield return typeof(DataRowToDictionary);
            yield return typeof(Decrement);
            yield return typeof(DecryptDataTable);
            yield return typeof(DecryptText);
            yield return typeof(DictionaryToDataTable);
            yield return typeof(EncryptDataTable);
            yield return typeof(EncryptText);
            yield return typeof(EnumerateFiles);
            yield return typeof(Exit);
            yield return typeof(Increment);
            yield return typeof(Iterate);
            yield return typeof(Next);
            yield return typeof(PromoteHeaders);
            yield return typeof(RemoveDataColumns);
            yield return typeof(RemoveDuplicateRows);
            yield return typeof(RemoveEmptyRows);
            yield return typeof(Stopwatch);
            yield return typeof(Unzip);
            yield return typeof(WaitFile);
            yield return typeof(Zip);
            yield return typeof(ZipEntriesCount);
        }

        private void ApplyPropertyAttributes(AttributeTableBuilder builder, Type activityType)
        {
            if (!activityType.GetCustomAttributes().Any(attr => attr is LocalDescriptionAttribute))
            {
                var key = activityType.Name + "_Description";
                if (Resources.ResourceManager.GetString(key) != null)
                    builder.AddCustomAttributes(activityType, new LocalDescriptionAttribute(key));
            }

            foreach (var prop in activityType.GetProperties())
            {
                var attrs = prop.GetCustomAttributes();

                if (!attrs.Any(attr => attr is LocalDisplayNameAttribute))
                {
                    var key = $"{activityType.Name}_{prop.Name}_DisplayName";
                    if (Resources.ResourceManager.GetString(key) != null)
                    {
                        builder.AddCustomAttributes(activityType, prop, new LocalDisplayNameAttribute(key));
                    }
                    else if (prop.Name.StartsWith("Input") && prop.Name.Length > 5)
                    {
                        builder.AddCustomAttributes(activityType, prop, new LocalDisplayNameAttribute(prop.Name.Substring(5)));
                    }
                    else if (prop.Name.StartsWith("Output") && prop.Name.Length > 6)
                    {
                        builder.AddCustomAttributes(activityType, prop, new LocalDisplayNameAttribute(prop.Name.Substring(6)));
                    }
                }

                if (!attrs.Any(attr => attr is LocalDescriptionAttribute))
                {
                    var key = $"{activityType.Name}_{prop.Name}_Description";
                    if (Resources.ResourceManager.GetString(key) != null)
                        builder.AddCustomAttributes(activityType, prop, new LocalDescriptionAttribute(key));
                }

                if (!attrs.Any(attr => attr is LocalCategAttribute))
                {
                    if (typeof(InArgument).IsAssignableFrom(prop.PropertyType))
                    {
                        builder.AddCustomAttributes(activityType, prop, new LocalCategAttribute(Resources.Input_Category));
                        continue;
                    }

                    if (typeof(OutArgument).IsAssignableFrom(prop.PropertyType))
                    {
                        builder.AddCustomAttributes(activityType, prop, new LocalCategAttribute(Resources.Output_Category));
                        continue;
                    }

                    if (typeof(InOutArgument).IsAssignableFrom(prop.PropertyType))
                        builder.AddCustomAttributes(activityType, prop, new LocalCategAttribute(Resources.InputOutput_Category));
                }
            }
        }

        private void AddCustomAttributes(AttributeTableBuilder builder, CategoryAttribute category, Type activityType, Type designerType)
        {
            builder.AddCustomAttributes(activityType, category);
            builder.AddCustomAttributes(activityType, new DesignerAttribute(designerType));
        }
    }
}