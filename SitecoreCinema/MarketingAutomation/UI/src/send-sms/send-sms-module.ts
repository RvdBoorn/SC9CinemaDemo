import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SmsEditorComponent } from './editor/sms-editor.component';

@NgModule({
    imports: [
        CommonModule, 
        FormsModule
    ],
    declarations: [SmsEditorComponent],
    entryComponents: [SmsEditorComponent]
})
export class SendSmsModule { }
 