using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.Resources;

namespace Autossential.Shared.Activities.Design
{
    public sealed class ActivitiesAttributesBuilder
    {
        private readonly AttributeTableBuilder _tableBuilder = new AttributeTableBuilder();
        private CategoryAttribute _inCategory;
        private CategoryAttribute _outCategory;
        private CategoryAttribute _inOutCategory;
        private CategoryAttribute _optionsCategory;
        private readonly ResourceManager _resourcesManager;

        private ActivitiesAttributesBuilder(ResourceManager manager)
        {
            _resourcesManager = manager;
        }

        public static void Build(ResourceManager manager, Action<ActivitiesAttributesBuilder> builder)
        {
            var activitesAttributesBuilder = new ActivitiesAttributesBuilder(manager);
            builder(activitesAttributesBuilder);
            activitesAttributesBuilder.ValidateAndCreate();
        }

        private void ValidateAndCreate()
        {
            _tableBuilder.ValidateTable();
            MetadataStore.AddAttributeTable(_tableBuilder.CreateTable());
        }

        public ActivitiesAttributesBuilder SetDefaultCategories(string inCategory, string outCategory, string inOutCategory, string optionsCategory)
        {
            _inCategory = new CategoryAttribute(inCategory);
            _outCategory = new CategoryAttribute(outCategory);
            _inOutCategory = new CategoryAttribute(inOutCategory);
            _optionsCategory = new CategoryAttribute(optionsCategory);
            return this;
        }

        public ActivitiesAttributesBuilder Register<TActivity, TActivityDesigner>(Attribute[] attributes, params Action<MembersAttributesBuilder<TActivity>>[] membersAttributesBuilder)
            where TActivity : class
            where TActivityDesigner : class
        {
            var type = typeof(TActivity);

            _tableBuilder.AddCustomAttributes(type, attributes);
            _tableBuilder.AddCustomAttributes(type, new DesignerAttribute(typeof(TActivityDesigner)));

            if (membersAttributesBuilder.Length > 0)
            {
                var amb = new MembersAttributesBuilder<TActivity>(_tableBuilder);
                foreach (var reg in membersAttributesBuilder)
                    reg(amb);
            }

            AppyCommonAttributes(type);
            return this;
        }

        public ActivitiesAttributesBuilder Register<TActivity, TActivityDesigner>(Attribute attribute, params Action<MembersAttributesBuilder<TActivity>>[] membersAttributesBuilder)
               where TActivity : class
            where TActivityDesigner : class => Register<TActivity, TActivityDesigner>(new[] { attribute }, membersAttributesBuilder);

        private bool TryGetFromResource(string key, out string value)
        {
            value = _resourcesManager.GetString(key);
            return value != null;
        }

        private readonly string[] _propertiesToIgnore = new[] { "ContinueOnError", "Timeout", "DisplayName" };

        private void AppyCommonAttributes(Type activityType)
        {
            if (TryGetFromResource(activityType.Name + "_DisplayName", out string value))
                _tableBuilder.AddCustomAttributes(activityType, new DisplayNameAttribute(value));

            if (TryGetFromResource(activityType.Name + "_Description", out value))
                _tableBuilder.AddCustomAttributes(activityType, new DescriptionAttribute(value));

            foreach (var prop in activityType.GetProperties())
            {
                if (Array.IndexOf(_propertiesToIgnore, prop.Name) > -1)
                    continue;

                // DISPLAY NAME ATTRIBUTES

                if (TryGetFromResource($"{activityType.Name}_{prop.Name}_DisplayName", out value))
                {
                    _tableBuilder.AddCustomAttributes(activityType, prop, new DisplayNameAttribute(value));
                }
                else if (prop.Name.StartsWith("Input") && prop.Name.Length > 5)
                {
                    _tableBuilder.AddCustomAttributes(activityType, prop, new DisplayNameAttribute(prop.Name.Substring(5)));
                }
                else if (prop.Name.StartsWith("Output") && prop.Name.Length > 6)
                {
                    _tableBuilder.AddCustomAttributes(activityType, prop, new DisplayNameAttribute(prop.Name.Substring(6)));
                }

                // DESCRIPTION ATTRIBUTES

                if (TryGetFromResource($"{activityType.Name}_{prop.Name}_Description", out value))
                    _tableBuilder.AddCustomAttributes(activityType, prop, new DescriptionAttribute(value));

                // CATEGORY ATTRIBUTES

                if (typeof(InArgument).IsAssignableFrom(prop.PropertyType))
                {
                    _tableBuilder.AddCustomAttributes(activityType, prop, _inCategory);
                    continue;
                }

                if (typeof(OutArgument).IsAssignableFrom(prop.PropertyType))
                {
                    _tableBuilder.AddCustomAttributes(activityType, prop, _outCategory);
                    continue;
                }

                if (typeof(InOutArgument).IsAssignableFrom(prop.PropertyType))
                {
                    _tableBuilder.AddCustomAttributes(activityType, prop, _inOutCategory);
                    continue;
                }

                _tableBuilder.AddCustomAttributes(activityType, prop, _optionsCategory);
            }
        }
    }
}