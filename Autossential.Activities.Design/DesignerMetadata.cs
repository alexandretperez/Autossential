using Autossential.Activities.Design.Designers;
using System;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;

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

        public void Register()
        {
            var builder = new AttributeTableBuilder();

            var dataTable = new CategoryAttribute(DATA_TABLE_CATEGORY);
            AddCustomAttributes(builder, dataTable, typeof(Aggregate), typeof(AggregateDesigner));
            AddCustomAttributes(builder, dataTable, typeof(PromoteHeaders), typeof(PromoteHeadersDesigner));
            AddCustomAttributes(builder, dataTable, typeof(DataRowToDictionary), typeof(DataRowToDictionaryDesigner));
            AddCustomAttributes(builder, dataTable, typeof(DictionaryToDataTable), typeof(DictionaryToDataTableDesigner));

            var file = new CategoryAttribute(FILE_CATEGORY);
            AddCustomAttributes(builder, file, typeof(WaitFile), typeof(WaitFileDesigner));
            AddCustomAttributes(builder, file, typeof(EnumerateFiles), typeof(EnumerateFilesDesigner));

            var workflow = new CategoryAttribute(WORKFLOW_CATEGORY);
            AddCustomAttributes(builder, workflow, typeof(Exit), typeof(ExitDesigner));
            AddCustomAttributes(builder, workflow, typeof(Container), typeof(ContainerDesigner));
            AddCustomAttributes(builder, workflow, typeof(CheckPoint), typeof(CheckPointDesigner));

            var programming = new CategoryAttribute(PROGRAMMING_CATEGORY);
            AddCustomAttributes(builder, programming, typeof(Increment), typeof(IncrementDesigner));
            AddCustomAttributes(builder, programming, typeof(Decrement), typeof(DecrementDesigner));
            AddCustomAttributes(builder, programming, typeof(CultureScope), typeof(CultureScopeDesigner));
            

            builder.ValidateTable();
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        private void AddCustomAttributes(AttributeTableBuilder builder, CategoryAttribute category, Type activityType, Type designerType)
        {
            builder.AddCustomAttributes(activityType, category);
            builder.AddCustomAttributes(activityType, new DesignerAttribute(designerType));
        }
    }
}