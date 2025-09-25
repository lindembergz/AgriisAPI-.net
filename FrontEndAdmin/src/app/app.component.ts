import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastModule],
  template: `
    <router-outlet></router-outlet>
    <p-toast position="top-right" [breakpoints]="{'920px': {width: '100%', right: '0', left: '0'}}"></p-toast>
  `,
})
export class AppComponent {}
