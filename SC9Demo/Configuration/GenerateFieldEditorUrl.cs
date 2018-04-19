using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ExperienceEditor.Speak.Server.Contexts;
using Sitecore.ExperienceEditor.Speak.Server.Requests;
using Sitecore.ExperienceEditor.Speak.Server.Responses;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Text;
using System;
using System.Collections.Generic;

namespace SC9Demo.Configuration
{
    public class GenerateFieldEditorUrl : PipelineProcessorRequest<ItemContext>
    {
        public GenerateFieldEditorUrl()
        {
        }

        private List<FieldDescriptor> CreateFieldDescriptors(string fields)
        {
            List<FieldDescriptor> fieldList = new List<FieldDescriptor>();
            foreach (string field in new ListString(new ListString(fields)))
            {
                if (base.RequestContext.Item.Fields[field] == null)
                {
                    Log.Debug(string.Format("Item {0} does not contain field {1}", base.RequestContext.Item.Paths.Path, field), this);
                }
                else
                {
                    fieldList.Add(new FieldDescriptor(base.RequestContext.Item, field));
                }
            }
            return fieldList;
        }

        public string GenerateUrl()
        {
            List<FieldDescriptor> fieldList = this.CreateFieldDescriptors(base.RequestContext.Argument);
            FieldEditorOptions fieldEditorOption = new FieldEditorOptions(fieldList)
            {
                SaveItem = true
            };
            return fieldEditorOption.ToUrlString().ToString();
        }

        public override PipelineProcessorResponseValue ProcessRequest()
        {
            return new PipelineProcessorResponseValue()
            {
                Value = this.GenerateUrl()
            };
        }
    }
}