import { Component, OnInit, Injector } from '@angular/core';
import {
               EditorBase
    } from '@sitecore/ma-core';
 
@Component({
    selector: 'readonly-editor',
            template: `
        <section class="content">
            <div class="form-group">
                <div class="row readonly-editor">
                    <label class="col-6 title">Send SMS</label>
                </div>
                <div class="row">
                    <div class="col-12">
                        <input type="text" class="form-control" [(ngModel)]="smsmessagetext"/>
                    </div>
                </div>
            </div>
        </section>
    `,
    //CSS Styles are ommitted for brevity
    styles: ['']
})
 
export class ReadonlyEditorComponent extends EditorBase implements OnInit {

    constructor(private injector: Injector) {
        super();
    }

    smsmessagetext: Text;
   
    ngOnInit(): void { 
        this.smsmessagetext = this.model.smsmessagetext;
    }
 
    serialize(): any {
        return {    
            smsmessagetext : this.smsmessagetext
        };
    }
}