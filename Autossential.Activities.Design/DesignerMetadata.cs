using Autossential.Activities.Design.Designers;
using System.Activities.Presentation.Metadata;
using System.ComponentModel;

namespace Autossential.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            var builder = new AttributeTableBuilder();

            const string main = "Autossential";
            var dataTableCategory = new CategoryAttribute($"{main}.DataTable");
            var fileCategory = new CategoryAttribute($"{main}.File");
            //var fileCompressionCategory = new CategoryAttribute($"{main}.File.Compression");
            //var programmingCategory = new CategoryAttribute($"{main}.Programming");
            //var workflowCategory = new CategoryAttribute($"{main}.Workflow");
            //var securityCategory = new CategoryAttribute($"{main}.Security");

            builder.AddCustomAttributes(typeof(Aggregate), dataTableCategory);
            builder.AddCustomAttributes(typeof(Aggregate), new DesignerAttribute(typeof(AggregateDesigner)));

            builder.AddCustomAttributes(typeof(WaitFile), fileCategory);
            builder.AddCustomAttributes(typeof(WaitFile), new DesignerAttribute(typeof(WaitFileDesigner)));


            builder.ValidateTable();
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}