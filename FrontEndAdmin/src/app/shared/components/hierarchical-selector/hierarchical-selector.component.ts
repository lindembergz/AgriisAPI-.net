import { Component, OnInit, inject, signal, input, output, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TreeModule } from 'primeng/tree';
import { TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';
import { CheckboxModule } from 'primeng/checkbox';
import { RadioButtonModule } from 'primeng/radiobutton';

/**
 * Selection mode for hierarchical selector
 */
export type SelectionMode = 'single' | 'multiple' | 'checkbox';

/**
 * Hierarchical item interface
 */
export interface HierarchicalItem {
  id: number;
  nome: string;
  parentId?: number;
  level?: number;
  children?: HierarchicalItem[];
  ativo?: boolean;
  [key: string]: any;
}

/**
 * Selection change event data
 */
export interface SelectionChangeEvent {
  selectedItems: HierarchicalItem[];
  selectedIds: number[];
  selectedNode?: TreeNode;
}

/**
 * Hierarchical selector component for tree-view data selection
 * Supports single and multiple selection modes with search functionality
 */
@Component({
  selector: 'app-hierarchical-selector',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TreeModule,
    ButtonModule,
    InputTextModule,
    ProgressSpinnerModule,
    MessageModule,
    TooltipModule,
    CheckboxModule,
    RadioButtonModule
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => HierarchicalSelectorComponent),
      multi: true
    }
  ],
  template: `
    <div class="hierarchical-selector" [class]="containerClass()">
      <!-- Header with search and actions -->
      <div class="selector-header" *ngIf="showHeader()">
        <div class="search-container" *ngIf="showSearch()">
          <span class="p-input-icon-left">
            <i class="pi pi-search"></i>
            <input
              type="text"
              pInputText
              [(ngModel)]="searchTerm"
              (input)="onSearchChange($event)"
              placeholder="Buscar..."
              class="search-input"
              [disabled]="disabled() || loading()">
          </span>
          <p-button
            *ngIf="searchTerm"
            icon="pi pi-times"
            (onClick)="clearSearch()"
            class="p-button-text p-button-sm clear-search-btn"
            pTooltip="Limpar busca"
            tooltipPosition="top">
          </p-button>
        </div>
        
        <div class="header-actions" *ngIf="showActions()">
          <p-button
            label="Expandir Todos"
            icon="pi pi-plus"
            (onClick)="expandAll()"
            class="p-button-text p-button-sm"
            [disabled]="disabled() || loading()"
            pTooltip="Expandir todos os nós"
            tooltipPosition="top">
          </p-button>
          <p-button
            label="Recolher Todos"
            icon="pi pi-minus"
            (onClick)="collapseAll()"
            class="p-button-text p-button-sm"
            [disabled]="disabled() || loading()"
            pTooltip="Recolher todos os nós"
            tooltipPosition="top">
          </p-button>
          <p-button
            *ngIf="selectionMode() === 'multiple' || selectionMode() === 'checkbox'"
            label="Limpar Seleção"
            icon="pi pi-times"
            (onClick)="clearSelection()"
            class="p-button-text p-button-sm"
            [disabled]="disabled() || loading() || !hasSelection()"
            pTooltip="Limpar toda a seleção"
            tooltipPosition="top">
          </p-button>
        </div>
      </div>

      <!-- Loading indicator -->
      <div *ngIf="loading()" class="loading-container">
        <p-progressSpinner [style]="{ width: '30px', height: '30px' }"></p-progressSpinner>
        <span>Carregando dados...</span>
      </div>

      <!-- Error message -->
      <div *ngIf="error()" class="error-container">
        <p-message severity="error" [text]="error()"></p-message>
      </div>

      <!-- Tree view -->
      <div *ngIf="!loading() && !error()" class="tree-container">
        <p-tree
          [value]="filteredTreeNodes()"
          [selectionMode]="getTreeSelectionMode()"
          [(selection)]="treeSelection"
          (onNodeSelect)="onNodeSelect($event)"
          (onNodeUnselect)="onNodeUnselect($event)"
          (onNodeExpand)="onNodeExpand($event)"
          (onNodeCollapse)="onNodeCollapse($event)"
          [loading]="loading()"
          [filter]="false"
          [filterBy]="'label'"
          [scrollHeight]="scrollHeight()"
          styleClass="hierarchical-tree"
          [class.tree-disabled]="disabled()">
          
          <!-- Custom node template -->
          <ng-template let-node pTemplate="default">
            <div class="tree-node-content" 
                 [class.node-selected]="isNodeSelected(node)"
                 [class.node-disabled]="isNodeDisabled(node)"
                 [class.node-inactive]="!isNodeActive(node)">
              
              <!-- Selection control for checkbox mode -->
              <p-checkbox
                *ngIf="selectionMode() === 'checkbox'"
                [binary]="true"
                [ngModel]="isNodeSelected(node)"
                (onChange)="onCheckboxChange(node, $event)"
                [disabled]="disabled() || isNodeDisabled(node)"
                class="node-checkbox">
              </p-checkbox>
              
              <!-- Selection control for radio mode -->
              <p-radioButton
                *ngIf="selectionMode() === 'single' && showRadioButtons()"
                [value]="node.data.id"
                [(ngModel)]="singleSelectionId"
                (onChange)="onRadioChange(node)"
                [disabled]="disabled() || isNodeDisabled(node)"
                class="node-radio">
              </p-radioButton>
              
              <!-- Node icon -->
              <i [class]="getNodeIcon(node)" class="node-icon"></i>
              
              <!-- Node label -->
              <span class="node-label" [title]="node.label">
                {{ node.label }}
              </span>
              
              <!-- Node badges -->
              <div class="node-badges" *ngIf="showBadges()">
                <span *ngIf="!isNodeActive(node)" class="badge badge-inactive">Inativo</span>
                <span *ngIf="hasChildren(node)" class="badge badge-parent">
                  {{ getChildrenCount(node) }}
                </span>
              </div>
            </div>
          </ng-template>
          
          <!-- Empty template -->
          <ng-template pTemplate="empty">
            <div class="empty-state">
              <i class="pi pi-info-circle empty-icon"></i>
              <h4>{{ getEmptyMessage() }}</h4>
              <p *ngIf="searchTerm" class="empty-description">
                Tente ajustar os termos de busca ou limpar o filtro.
              </p>
            </div>
          </ng-template>
        </p-tree>
      </div>

      <!-- Selection summary -->
      <div *ngIf="showSelectionSummary() && hasSelection()" class="selection-summary">
        <div class="summary-content">
          <i class="pi pi-check-circle summary-icon"></i>
          <span class="summary-text">
            {{ getSelectionSummaryText() }}
          </span>
        </div>
        <p-button
          icon="pi pi-times"
          (onClick)="clearSelection()"
          class="p-button-text p-button-sm clear-summary-btn"
          pTooltip="Limpar seleção"
          tooltipPosition="top">
        </p-button>
      </div>
    </div>
  `,
  styleUrls: ['./hierarchical-selector.component.scss']
})
export class HierarchicalSelectorComponent implements OnInit, ControlValueAccessor {
  
  // Input properties
  items = input<HierarchicalItem[]>([]);
  selectionMode = input<SelectionMode>('single');
  disabled = input<boolean>(false);
  loading = input<boolean>(false);
  error = input<string | null>(null);
  
  // Display options
  showHeader = input<boolean>(true);
  showSearch = input<boolean>(true);
  showActions = input<boolean>(true);
  showBadges = input<boolean>(true);
  showRadioButtons = input<boolean>(false);
  showSelectionSummary = input<boolean>(true);
  scrollHeight = input<string>('400px');
  containerClass = input<string>('');
  
  // Behavior options
  expandOnSelect = input<boolean>(false);
  selectParentsAutomatically = input<boolean>(false);
  selectChildrenAutomatically = input<boolean>(false);
  
  // Output events
  selectionChange = output<SelectionChangeEvent>();
  nodeExpand = output<TreeNode>();
  nodeCollapse = output<TreeNode>();
  searchChange = output<string>();

  // Internal state
  treeNodes = signal<TreeNode[]>([]);
  filteredTreeNodes = signal<TreeNode[]>([]);
  treeSelection = signal<TreeNode | TreeNode[] | null>(null);
  selectedItems = signal<HierarchicalItem[]>([]);
  expandedNodes = signal<Set<string>>(new Set());
  
  searchTerm = '';
  singleSelectionId: number | null = null;

  // ControlValueAccessor implementation
  private onChange = (value: HierarchicalItem | HierarchicalItem[] | null) => {};
  private onTouched = () => {};

  ngOnInit(): void {
    // Watch for items changes
    this.buildTreeNodes();
  }

  /**
   * Build tree nodes from flat items array
   */
  private buildTreeNodes(): void {
    const items = this.items();
    if (!items || items.length === 0) {
      this.treeNodes.set([]);
      this.filteredTreeNodes.set([]);
      return;
    }

    const nodeMap = new Map<number, TreeNode>();
    const rootNodes: TreeNode[] = [];

    // First pass: create all nodes
    items.forEach(item => {
      const node: TreeNode = {
        key: item.id.toString(),
        label: item.nome,
        data: item,
        children: [],
        expanded: this.expandedNodes().has(item.id.toString()),
        selectable: item.ativo !== false
      };
      nodeMap.set(item.id, node);
    });

    // Second pass: build hierarchy
    items.forEach(item => {
      const node = nodeMap.get(item.id)!;
      
      if (item.parentId && nodeMap.has(item.parentId)) {
        const parent = nodeMap.get(item.parentId)!;
        parent.children!.push(node);
        node.parent = parent;
      } else {
        rootNodes.push(node);
      }
    });

    // Sort nodes by name
    const sortNodes = (nodes: TreeNode[]) => {
      nodes.sort((a, b) => a.label!.localeCompare(b.label!));
      nodes.forEach(node => {
        if (node.children && node.children.length > 0) {
          sortNodes(node.children);
        }
      });
    };

    sortNodes(rootNodes);
    
    this.treeNodes.set(rootNodes);
    this.applyFilter();
  }

  /**
   * Apply search filter to tree nodes
   */
  private applyFilter(): void {
    const nodes = this.treeNodes();
    
    if (!this.searchTerm.trim()) {
      this.filteredTreeNodes.set(nodes);
      return;
    }

    const filterNodes = (nodes: TreeNode[]): TreeNode[] => {
      return nodes.reduce((filtered: TreeNode[], node) => {
        const matchesSearch = node.label!.toLowerCase().includes(this.searchTerm.toLowerCase());
        const filteredChildren = node.children ? filterNodes(node.children) : [];
        
        if (matchesSearch || filteredChildren.length > 0) {
          const filteredNode: TreeNode = {
            ...node,
            children: filteredChildren,
            expanded: filteredChildren.length > 0 || node.expanded
          };
          filtered.push(filteredNode);
        }
        
        return filtered;
      }, []);
    };

    this.filteredTreeNodes.set(filterNodes(nodes));
  }

  /**
   * Handle search input change
   */
  onSearchChange(event: any): void {
    this.searchTerm = event.target.value;
    this.applyFilter();
    this.searchChange.emit(this.searchTerm);
  }

  /**
   * Clear search filter
   */
  clearSearch(): void {
    this.searchTerm = '';
    this.applyFilter();
    this.searchChange.emit('');
  }

  /**
   * Handle node selection
   */
  onNodeSelect(event: any): void {
    const node = event.node as TreeNode;
    this.updateSelection();
    
    if (this.expandOnSelect() && node.children && node.children.length > 0) {
      node.expanded = true;
      this.onNodeExpand({ node });
    }
    
    this.onTouched();
  }

  /**
   * Handle node unselection
   */
  onNodeUnselect(event: any): void {
    this.updateSelection();
    this.onTouched();
  }

  /**
   * Handle checkbox change
   */
  onCheckboxChange(node: TreeNode, event: any): void {
    const isChecked = event.checked;
    
    if (isChecked) {
      this.selectNode(node);
    } else {
      this.unselectNode(node);
    }
    
    // Handle automatic parent/children selection
    if (this.selectChildrenAutomatically()) {
      this.selectNodeChildren(node, isChecked);
    }
    
    if (this.selectParentsAutomatically()) {
      this.updateParentSelection(node);
    }
    
    this.updateSelection();
    this.onTouched();
  }

  /**
   * Handle radio button change
   */
  onRadioChange(node: TreeNode): void {
    this.treeSelection.set(node);
    this.updateSelection();
    this.onTouched();
  }

  /**
   * Select a node
   */
  private selectNode(node: TreeNode): void {
    const currentSelection = this.treeSelection();
    
    if (this.selectionMode() === 'single') {
      this.treeSelection.set(node);
    } else {
      const selection = Array.isArray(currentSelection) ? [...currentSelection] : [];
      if (!selection.includes(node)) {
        selection.push(node);
        this.treeSelection.set(selection);
      }
    }
  }

  /**
   * Unselect a node
   */
  private unselectNode(node: TreeNode): void {
    const currentSelection = this.treeSelection();
    
    if (this.selectionMode() === 'single') {
      this.treeSelection.set(null);
    } else if (Array.isArray(currentSelection)) {
      const selection = currentSelection.filter(n => n !== node);
      this.treeSelection.set(selection.length > 0 ? selection : null);
    }
  }

  /**
   * Select/unselect node children recursively
   */
  private selectNodeChildren(node: TreeNode, select: boolean): void {
    if (!node.children) return;
    
    node.children.forEach(child => {
      if (select) {
        this.selectNode(child);
      } else {
        this.unselectNode(child);
      }
      this.selectNodeChildren(child, select);
    });
  }

  /**
   * Update parent selection based on children
   */
  private updateParentSelection(node: TreeNode): void {
    if (!node.parent) return;
    
    const parent = node.parent;
    const siblings = parent.children || [];
    const selectedSiblings = siblings.filter(sibling => this.isNodeSelected(sibling));
    
    if (selectedSiblings.length === siblings.length) {
      // All siblings selected, select parent
      this.selectNode(parent);
    } else if (selectedSiblings.length === 0) {
      // No siblings selected, unselect parent
      this.unselectNode(parent);
    }
    
    // Recursively update parent's parent
    this.updateParentSelection(parent);
  }

  /**
   * Update internal selection state
   */
  private updateSelection(): void {
    const selection = this.treeSelection();
    let items: HierarchicalItem[] = [];
    
    if (selection) {
      if (Array.isArray(selection)) {
        items = selection.map(node => node.data);
      } else {
        items = [selection.data];
        this.singleSelectionId = selection.data.id;
      }
    } else {
      this.singleSelectionId = null;
    }
    
    this.selectedItems.set(items);
    
    // Emit change events
    const changeEvent: SelectionChangeEvent = {
      selectedItems: items,
      selectedIds: items.map(item => item.id),
      selectedNode: Array.isArray(selection) ? undefined : selection || undefined
    };
    
    this.selectionChange.emit(changeEvent);
    
    // Emit ControlValueAccessor change
    if (this.selectionMode() === 'single') {
      this.onChange(items.length > 0 ? items[0] : null);
    } else {
      this.onChange(items);
    }
  }

  /**
   * Handle node expand
   */
  onNodeExpand(event: any): void {
    const node = event.node as TreeNode;
    const expanded = new Set(this.expandedNodes());
    expanded.add(node.key!);
    this.expandedNodes.set(expanded);
    this.nodeExpand.emit(node);
  }

  /**
   * Handle node collapse
   */
  onNodeCollapse(event: any): void {
    const node = event.node as TreeNode;
    const expanded = new Set(this.expandedNodes());
    expanded.delete(node.key!);
    this.expandedNodes.set(expanded);
    this.nodeCollapse.emit(node);
  }

  /**
   * Expand all nodes
   */
  expandAll(): void {
    const expandAllNodes = (nodes: TreeNode[]) => {
      nodes.forEach(node => {
        node.expanded = true;
        if (node.children) {
          expandAllNodes(node.children);
        }
      });
    };
    
    const nodes = this.filteredTreeNodes();
    expandAllNodes(nodes);
    this.filteredTreeNodes.set([...nodes]);
  }

  /**
   * Collapse all nodes
   */
  collapseAll(): void {
    const collapseAllNodes = (nodes: TreeNode[]) => {
      nodes.forEach(node => {
        node.expanded = false;
        if (node.children) {
          collapseAllNodes(node.children);
        }
      });
    };
    
    const nodes = this.filteredTreeNodes();
    collapseAllNodes(nodes);
    this.filteredTreeNodes.set([...nodes]);
    this.expandedNodes.set(new Set());
  }

  /**
   * Clear all selection
   */
  clearSelection(): void {
    this.treeSelection.set(null);
    this.singleSelectionId = null;
    this.updateSelection();
  }

  /**
   * Check if node is selected
   */
  isNodeSelected(node: TreeNode): boolean {
    const selection = this.treeSelection();
    
    if (!selection) return false;
    
    if (Array.isArray(selection)) {
      return selection.includes(node);
    } else {
      return selection === node;
    }
  }

  /**
   * Check if node is disabled
   */
  isNodeDisabled(node: TreeNode): boolean {
    return !node.selectable || this.disabled();
  }

  /**
   * Check if node is active
   */
  isNodeActive(node: TreeNode): boolean {
    return node.data.ativo !== false;
  }

  /**
   * Check if node has children
   */
  hasChildren(node: TreeNode): boolean {
    return !!(node.children && node.children.length > 0);
  }

  /**
   * Get children count for node
   */
  getChildrenCount(node: TreeNode): number {
    return node.children ? node.children.length : 0;
  }

  /**
   * Get node icon class
   */
  getNodeIcon(node: TreeNode): string {
    if (this.hasChildren(node)) {
      return node.expanded ? 'pi pi-folder-open' : 'pi pi-folder';
    } else {
      return 'pi pi-file';
    }
  }

  /**
   * Get tree selection mode for PrimeNG
   */
  getTreeSelectionMode(): string {
    switch (this.selectionMode()) {
      case 'multiple':
        return 'multiple';
      case 'checkbox':
        return 'checkbox';
      default:
        return 'single';
    }
  }

  /**
   * Check if has any selection
   */
  hasSelection(): boolean {
    return this.selectedItems().length > 0;
  }

  /**
   * Get selection summary text
   */
  getSelectionSummaryText(): string {
    const count = this.selectedItems().length;
    
    if (count === 0) return 'Nenhum item selecionado';
    if (count === 1) return `1 item selecionado`;
    return `${count} itens selecionados`;
  }

  /**
   * Get empty state message
   */
  getEmptyMessage(): string {
    if (this.searchTerm) {
      return 'Nenhum item encontrado';
    }
    return 'Nenhum item disponível';
  }

  // ControlValueAccessor implementation
  writeValue(value: HierarchicalItem | HierarchicalItem[] | null): void {
    if (!value) {
      this.clearSelection();
      return;
    }

    // Find and select nodes based on value
    const findAndSelectNodes = (items: HierarchicalItem[]) => {
      const nodes = this.findNodesByItems(items);
      
      if (this.selectionMode() === 'single') {
        this.treeSelection.set(nodes[0] || null);
        this.singleSelectionId = nodes[0]?.data.id || null;
      } else {
        this.treeSelection.set(nodes.length > 0 ? nodes : null);
      }
      
      this.updateSelection();
    };

    if (Array.isArray(value)) {
      findAndSelectNodes(value);
    } else {
      findAndSelectNodes([value]);
    }
  }

  registerOnChange(fn: (value: HierarchicalItem | HierarchicalItem[] | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    // Handled by input signal
  }

  /**
   * Find tree nodes by items
   */
  private findNodesByItems(items: HierarchicalItem[]): TreeNode[] {
    const nodes: TreeNode[] = [];
    const itemIds = items.map(item => item.id);
    
    const searchNodes = (treeNodes: TreeNode[]) => {
      treeNodes.forEach(node => {
        if (itemIds.includes(node.data.id)) {
          nodes.push(node);
        }
        if (node.children) {
          searchNodes(node.children);
        }
      });
    };
    
    searchNodes(this.treeNodes());
    return nodes;
  }
}