import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { filter } from 'rxjs/operators';

import { ToolbarModule } from 'primeng/toolbar';
import { AvatarModule } from 'primeng/avatar';
import { MenuModule } from 'primeng/menu';
import { PanelMenuModule } from 'primeng/panelmenu';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    ToolbarModule,
    AvatarModule,
    MenuModule,
    PanelMenuModule,
  ],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
})
export class LayoutComponent implements OnInit {
  authService = inject(AuthService);
  user = this.authService.currentUser;
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);

  userMenuItems: MenuItem[] = [
    { label: 'Perfil', icon: 'pi pi-user' },
    { label: 'Configurações', icon: 'pi pi-cog' },
    { separator: true },
    { label: 'Sair', icon: 'pi pi-sign-out', command: () => this.authService.logout() },
  ];

  mainMenuItems: MenuItem[] = [
    {
      label: 'Home',
      icon: 'pi pi-home',
      routerLink: ['/home'],
    },
    {
      label: 'Clientes',
      items: [
        { label: 'Fornecedores', routerLink: ['/fornecedores'] },
        { label: 'Produtores', routerLink: ['/produtores'] },
      ],
    },
    {
      label: 'Tabelas de sistema',
      items: [
        { label: 'Ano Safra', routerLink: [] },
        { label: 'Culturas', routerLink: [] },
      ],
    },
    {
      label: 'Produtos',
      items: [
        { label: 'Categorias', routerLink: [] },
        { label: 'Produtos', routerLink: [] },
      ],
    },
    {
      label: 'Equipe',
      icon: 'pi pi-users',
      items: [],
    },
  ];

  ngOnInit() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.updateMenuState();
    });
    this.updateMenuState(); // Initial update
  }

  private updateMenuState() {
    const currentUrl = this.router.url;
    this.mainMenuItems.forEach(item => this.setExpandedState(item, currentUrl));
  }

  private setExpandedState(item: MenuItem, currentUrl: string) {
    if (item.routerLink && currentUrl.startsWith(item.routerLink[0])) {
      item.expanded = true;
    } else if (item.items) {
      item.items.forEach(subItem => {
        this.setExpandedState(subItem, currentUrl);
        if (subItem.expanded) {
          item.expanded = true; // Expand parent if any child is expanded
        }
      });
    }
  }
}
