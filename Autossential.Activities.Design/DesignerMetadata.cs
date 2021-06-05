using Autossential.Activities.Base;
using Autossential.Activities.Design.Designers;
using Autossential.Activities.Design.PropertyEditors;
using Autossential.Activities.Properties;
using Autossential.Shared.Activities.Design;
using System;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
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
        public const string DIAGNOSTICS_CATEGORY = MAIN_CATEGORY + ".Diagnostics";

        public void Register()
        {
            var dataTable = new CategoryAttribute(DATA_TABLE_CATEGORY);
            var workflow = new CategoryAttribute(WORKFLOW_CATEGORY);
            var file = new CategoryAttribute(FILE_CATEGORY);
            var fileCompression = new CategoryAttribute(FILE_COMPRESSION_CATEGORY);
            var programming = new CategoryAttribute(PROGRAMMING_CATEGORY);
            var security = new CategoryAttribute(SECURITY_CATEGORY);
            var diagnostics = new CategoryAttribute(DIAGNOSTICS_CATEGORY);

            ActivitiesAttributesBuilder.Build(Resources.ResourceManager, builder =>
            {
                builder.SetDefaultCategories(
                    Resources.Input_Category,
                    Resources.Output_Category,
                    Resources.InputOutput_Category,
                    Resources.Options_Category);

                // DATA TABLE
                builder
                    .Register<Aggregate, AggregateDesigner>(dataTable, m =>
                    {
                        m.Register(new CategoryAttribute(Resources.Output_Category), p => p.Detached);
                        m.Register(new CategoryAttribute(Resources.Options_Category), p => p.Columns);
                    })
                    .Register<DataRowToDictionary, DataRowToDictionaryDesigner>(dataTable)
                    .Register<DictionaryToDataTable, DictionaryToDataTableDesigner>(dataTable)
                    .Register<RemoveEmptyRows, RemoveEmptyRowsDesigner>(dataTable, m =>
                    {
                        m.Register(new CategoryAttribute(Resources.RemoveEmptyRows_CustomOptions_Category),
                            p => p.Columns,
                            p => p.Operator);
                    })
                    .Register<RemoveDataColumns, RemoveDataColumnsDesigner>(dataTable)
                    .Register<RemoveDuplicateRows, RemoveDuplicateRowsDesigner>(dataTable, m => m.Register(p => p.Columns, new CategoryAttribute(Resources.Options_Category)))
                    .Register<PromoteHeaders, PromoteHeadersDesigner>(dataTable, m => m.Register(p => p.EmptyColumnName, new CategoryAttribute(Resources.Options_Category)));

                // FILE
                builder
                    .Register<CleanUpFolder, CleanUpFolderDesigner>(file, m =>
                    {
                        m.Register(new CategoryAttribute(Resources.Options_Category),
                            p => p.LastWriteTime,
                            p => p.SearchPattern);
                    })
                    .Register<EnumerateFiles, EnumerateFilesDesigner>(file, m => m.Register(new CategoryAttribute(Resources.Options_Category), p => p.SearchPattern))
                    .Register<WaitFile, WaitFileDesigner>(file, m => m.Register(p => p.Timeout, new CategoryAttribute()));

                // FILE COMPRESSION
                builder
                    .Register<Zip, ZipDesigner>(fileCompression)
                    .Register<ZipEntriesCount, ZipEntriesCountDesigner>(fileCompression)
                    .Register<Unzip, UnzipDesigner>(fileCompression);

                // CHECKPOINT
                builder
                    .Register<CheckPoint, CheckPointDesigner>(workflow, m =>
                        m.Register(new EditorAttribute(typeof(ArgumentDictionaryPropertyEditor), typeof(DialogPropertyValueEditor)), p => p.Data))
                    .Register<Container, ContainerDesigner>(workflow)
                    .Register<Exit, ExitDesigner>(workflow)
                    .Register<Next, NextDesigner>(workflow)
                    .Register<Iterate, IterateDesigner>(workflow, m => m.Register(new CategoryAttribute(Resources.Options_Category), p => p.Reverse));

                // PROGRAMMING
                builder
                    .Register<CultureScope, CultureScopeDesigner>(programming)
                    .Register<Decrement, DecrementDesigner>(programming)
                    .Register<Increment, IncrementDesigner>(programming);

                // SECURITY
                Action<MembersAttributesBuilder> cryptoBase = m =>
                {
                    m.Register<CryptographyBaseActivity>(new CategoryAttribute(Resources.Options_Category),
                        p => p.Iterations,
                        p => p.TextEncoding
                    );
                    m.Register<CryptographyBaseActivity>(p => p.TextEncoding, new DisplayNameAttribute("Encoding"));
                };

                builder
                    .Register<DecryptDataTable, DecryptDataTableDesigner>(security, m =>
                    {
                        cryptoBase(m);
                        m.Register(new CategoryAttribute(Resources.Options_Category), p => p.Sort, p => p.Columns);
                    })
                    .Register<DecryptText, DecryptTextDesigner>(security, cryptoBase)
                    .Register<EncryptDataTable, EncryptDataTableDesigner>(security, m =>
                    {
                        cryptoBase(m);
                        m.Register(new CategoryAttribute(Resources.Options_Category), p => p.Sort, p => p.Columns);
                    })
                    .Register<EncryptText, EncryptTextDesigner>(security, cryptoBase);

                // DIAGNOSTICS
                builder
                    .Register<Stopwatch, StopwatchDesigner>(diagnostics);
            });
        }
    }
}