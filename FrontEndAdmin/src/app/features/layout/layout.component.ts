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
        { label: 'Ano Safra', routerLink: ['/safras'] },
        { label: 'Culturas', routerLink: ['/culturas'] },
      ],
    },
    {
      label: 'Produtos',
      items: [
        { label: 'Produtos', routerLink: ['/produtos'] },
      ],
    },
    {
      label: 'Referências',
      icon: 'pi pi-database',
      items: [
        { label: 'Categorias', routerLink: ['/referencias/categorias'], icon: 'pi pi-sitemap' },
        { label: 'Unidades de Medida', routerLink: ['/referencias/unidades-medida'], icon: 'pi pi-calculator' },
        { label: 'Moedas', routerLink: ['/referencias/moedas'], icon: 'pi pi-dollar' },
        { label: 'Países', routerLink: ['/referencias/paises'], icon: 'pi pi-globe' },
        { label: 'Estados', routerLink: ['/referencias/ufs'], icon: 'pi pi-map' },
        { label: 'Municípios', routerLink: ['/referencias/municipios'], icon: 'pi pi-map-marker' },
        { label: 'Atividades Agropecuárias', routerLink: ['/referencias/atividades-agropecuarias'], icon: 'pi pi-briefcase' },
        { label: 'Embalagens', routerLink: ['/referencias/embalagens'], icon: 'pi pi-box' },
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
    this.mainMenuItems.forEach(item => this.setMenuItemState(item, currentUrl));
  }

  private setMenuItemState(item: MenuItem, currentUrl: string) {
    // Reset states
    item.expanded = false;
    item.styleClass = '';
    
    if (item.routerLink && item.routerLink.length > 0) {
      const routePath = item.routerLink[0];
      if (currentUrl === routePath || (routePath !== '/home' && currentUrl.startsWith(routePath))) {
        item.styleClass = 'active-menu-item';
      }
    }
    
    if (item.items) {
      let hasActiveChild = false;
      item.items.forEach(subItem => {
        this.setMenuItemState(subItem, currentUrl);
        if (subItem.styleClass?.includes('active-menu-item')) {
          hasActiveChild = true;
        }
      });
      
      if (hasActiveChild) {
        item.expanded = true;
        item.styleClass = 'active-parent-menu-item';
      }
    }
  }
}
