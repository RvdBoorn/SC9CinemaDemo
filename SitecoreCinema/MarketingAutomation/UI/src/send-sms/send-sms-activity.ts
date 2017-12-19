import { SingleItem} from '@sitecore/ma-core';

export class SendSmsActivity extends SingleItem {
   getVisual(): string {
       const subTitle = 'Send Sms';
       const cssClass = this.isDefined ? '' : 'undefined';
       
       return `
           <div class="viewport-readonly-editor marketing-action ${cssClass}">
               <span class="icon">
                   <img src="/~/icon/OfficeWhite/32x32/ma_sms_link_clicked.png" />
             </span>
               <p class="text with-subtitle" title="Send Sms">
                   Send Sms
                   <small class="subtitle" title="${subTitle}">${subTitle}</small>
               </p>
           </div>
       `;
   }

   get isDefined(): boolean {
       if(this.editorParams.smsmessagetext == undefined)
       {
           return false;
       }
       if(this.editorParams.smsmessagetext == "")
       {
           return false;
       }

       return true;
   }
}