import { Plugin } from '@sitecore/ma-core';
import { SendSmsActivity } from './send-sms/send-sms-activity';
import { SendSmsModuleNgFactory } from '../codegen/send-sms/send-sms-module.ngfactory';
import { SmsEditorComponent } from '../codegen/send-sms/editor/sms-editor.component';
 
// Use the @Plugin decorator to define all the activities the module contains.
@Plugin({
    activityDefinitions: [
        {
            // The ID must match the ID of the activity type description definition item in the CMS.
           
        id: '1973d9ef-806c-41ef-af6c-29adf90ae3f3',
            activity: SendSmsActivity,
            editorComponenet: SmsEditorComponent,
            editorModuleFactory: SendSmsModuleNgFactory
        }
    ]
})
export default class SendSmsPlugin {}