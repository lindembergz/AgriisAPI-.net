import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService, ConfirmationService } from 'primeng/api';
import { EmbalagemDto, CriarEmbalagemDto, AtualizarEmbalagemDto, TipoUnidadeMedida } from '../../../shared/models/reference.model';
import { EmbalagemService, UnidadeDropdownOption, TipoUnidadeOption } from './services/embalagem.service';

/**
 * Component for managing Embalagens (Packaging) with CRUD operations
 * Includes UnidadeMedida relationship and type filtering functionality
 */
@Component({
  selector: 'app-embalagens',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    InputTextModule,
    SelectModule,
    ButtonModule,
    CardModule,
    TableModule,
    DialogModule,
    ToastModule,
    ConfirmDialogModule,
    TagModule,
    CheckboxModule
  ],
  providers: [MessageService, ConfirmationService],
  template: `
    <div class="embalagens-container">
      <!-- Header -->
      <div class="header-section">
        <div class="title-section">
          <h2>Embalagens</h2>
          <p class="subtitle">Gerenciar tipos de embalagem utilizados no sistema</p>
        </div>
        
        <div class="actions-section">
          <!-- Filters -->
          <div class="filters">
            <!-- Type filter -->
            <p-select
              [options]="tiposUnidadeDisponiveis"
              [(ngModel)]="tipoUnidadeSelecionado"
              (onChange)="onTipoUnidadeFilterChange()"
              optionLabel="descricao"
              optionValue="valor"
              placeholder="Filtrar por tipo"
              [showClear]="true"
              class="filter-dropdown">
            </p-select>
            
            <!-- Unit filter -->
            <p-select
              [options]="unidadesFiltradas"
              [(ngModel)]="unidadeMedidaSelecionada"
              (onChange)="onUnidadeMedidaFilterChange()"
              optionLabel="nome"
              optionValue="id"
              placeholder="Filtrar por unidade"
              [showClear]="true"
              [filter]="true"
              filterBy="nome,simbolo"
              class="filter-dropdown">
              <ng-template pTemplate="item" let-unidade>
                <div class="dropdown-item">
                  <strong>{{ unidade.simbolo }}</strong> - {{ unidade.nome }}
                </div>
              </ng-template>
            </p-select>
          </div>
          
          <!-- New button -->
          <p-button
            label="Nova Embalagem"
            icon="pi pi-plus"
            (onClick)="novoItem()"
            class="p-button-primary">
          </p-button>
        </div>
      </div>

      <!-- Loading -->
      <div *ngIf="loading()" class="loading-container">
        <p>Carregando embalagens...</p>
      </div>

      <!-- Table -->
      <div *ngIf="!loading()" class="table-container">
        <p-table
          [value]="items()"
          [paginator]="true"
          [rows]="10"
          [rowsPerPageOptions]="[5, 10, 20, 50]"
          responsiveLayout="scroll"
          styleClass="p-datatable-gridlines p-datatable-striped">
          
          <ng-template pTemplate="header">
            <tr>
              <th>Nome</th>
              <th>Descrição</th>
              <th>Unidade de Medida</th>
              <th>Status</th>
              <th>Ações</th>
            </tr>
          </ng-template>

          <ng-template pTemplate="body" let-item>
            <tr>
              <td>{{ item.nome }}</td>
              <td>{{ item.descricao || '-' }}</td>
              <td>
                <div *ngIf="item.unidadeMedida" class="unidade-info">
                  <strong>{{ item.unidadeMedida.simbolo }}</strong> - {{ item.unidadeMedida.nome }}
                  <br>
                  <small>{{ getTipoUnidadeDescricao(item.unidadeMedida.tipo) }}</small>
                </div>
                <span *ngIf="!item.unidadeMedida">-</span>
              </td>
              <td>
                <p-tag
                  [value]="item.ativo ? 'Ativo' : 'Inativo'"
                  [severity]="item.ativo ? 'success' : 'danger'">
                </p-tag>
              </td>
              <td>
                <div class="action-buttons">
                  <p-button
                    icon="pi pi-pencil"
                    (onClick)="editarItem(item)"
                    class="p-button-rounded p-button-text p-button-info"
                    pTooltip="Editar">
                  </p-button>
                  <p-button
                    *ngIf="item.ativo"
                    icon="pi pi-times"
                    (onClick)="desativarItem(item)"
                    class="p-button-rounded p-button-text p-button-warning"
                    pTooltip="Desativar">
                  </p-button>
                  <p-button
                    *ngIf="!item.ativo"
                    icon="pi pi-check"
                    (onClick)="ativarItem(item)"
                    class="p-button-rounded p-button-text p-button-success"
                    pTooltip="Ativar">
                  </p-button>
                  <p-button
                    icon="pi pi-trash"
                    (onClick)="excluirItem(item)"
                    class="p-button-rounded p-button-text p-button-danger"
                    pTooltip="Excluir">
                  </p-button>
                </div>
              </td>
            </tr>
          </ng-template>

          <ng-template pTemplate="emptymessage">
            <tr>
              <td colspan="5" class="empty-state">
                <div class="empty-content">
                  <i class="pi pi-info-circle"></i>
                  <h3>Nenhuma embalagem encontrada</h3>
                  <p>Não há embalagens cadastradas no sistema.</p>
                  <p-button
                    label="Cadastrar Nova Embalagem"
                    icon="pi pi-plus"
                    (onClick)="novoItem()"
                    class="p-button-primary">
                  </p-button>
                </div>
              </td>
            </tr>
          </ng-template>
        </p-table>
      </div>

      <!-- Form Dialog -->
      <p-dialog
        [header]="dialogTitle()"
[visible]="showForm()"
        (onHide)="cancelarEdicao()"
        [modal]="true"
        [style]="{ width: '50vw' }"
        [draggable]="false"
        [resizable]="false">
        
        <form [formGroup]="form" (ngSubmit)="salvarItem()" class="form-container">
          <!-- Nome field -->
          <div class="field">
            <label for="nome" class="required">Nome</label>
            <input
              pInputText
              id="nome"
              formControlName="nome"
              placeholder="Ex: Saco, Caixa, Tambor, Fardo"
              maxlength="100"
              class="w-full" />
            <small class="field-help">Nome da embalagem</small>
          </div>

          <!-- Descrição field -->
          <div class="field">
            <label for="descricao">Descrição</label>
            <textarea
              id="descricao"
              formControlName="descricao"
              placeholder="Descrição detalhada da embalagem (opcional)"
              maxlength="500"
              rows="3"
              class="w-full">
            </textarea>
            <small class="field-help">Descrição opcional da embalagem</small>
          </div>

          <!-- UnidadeMedida field -->
          <div class="field">
            <label for="unidadeMedidaId" class="required">Unidade de Medida</label>
            <p-select
              id="unidadeMedidaId"
              formControlName="unidadeMedidaId"
              [options]="unidadesDisponiveis"
              optionLabel="nome"
              optionValue="id"
              placeholder="Selecione a unidade de medida"
              [filter]="true"
              filterBy="nome,simbolo"
              class="w-full">
              <ng-template pTemplate="item" let-unidade>
                <div class="dropdown-item">
                  <strong>{{ unidade.simbolo }}</strong> - {{ unidade.nome }}
                  <small class="tipo-badge">{{ getTipoUnidadeDescricao(unidade.tipo) }}</small>
                </div>
              </ng-template>
            </p-select>
            <small class="field-help">Unidade de medida associada à embalagem</small>
          </div>

          <!-- Status field (only for edit) -->
          <div *ngIf="isEditMode()" class="field">
            <label for="ativo">Status</label>
            <p-checkbox
              formControlName="ativo"
              binary="true"
              label="Ativo">
            </p-checkbox>
          </div>
        </form>

        <ng-template pTemplate="footer">
          <div class="dialog-footer">
            <p-button
              label="Cancelar"
              icon="pi pi-times"
              (onClick)="cancelarEdicao()"
              class="p-button-text">
            </p-button>
            <p-button
              label="Salvar"
              icon="pi pi-check"
              (onClick)="salvarItem()"
              class="p-button-primary"
              [disabled]="form.invalid">
            </p-button>
          </div>
        </ng-template>
      </p-dialog>

      <!-- Toast Messages -->
      <p-toast position="top-right"></p-toast>
      
      <!-- Confirmation Dialog -->
      <p-confirmDialog></p-confirmDialog>
    </div>
  `,
  styleUrls: ['./embalagens.component.scss']
})
export class EmbalagensComponent implements OnInit {
  
  private service = inject(EmbalagemService);
  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  // Signals for reactive state management
  items = signal<EmbalagemDto[]>([]);
  loading = signal<boolean>(false);
  showForm = signal<boolean>(false);
  selectedItem = signal<EmbalagemDto | null>(null);

  // Form
  form!: FormGroup;

  // UnidadeMedida filtering
  unidadesDisponiveis: UnidadeDropdownOption[] = [];
  unidadesFiltradas: UnidadeDropdownOption[] = [];
  tiposUnidadeDisponiveis: TipoUnidadeOption[] = [];
  tipoUnidadeSelecionado: TipoUnidadeMedida | null = null;
  unidadeMedidaSelecionada: number | null = null;

  ngOnInit(): void {
    this.createForm();
    this.carregarItens();
    this.carregarUnidadesMedida();
    this.carregarTiposUnidade();
  }

  /**
   * Create reactive form
   */
  private createForm(): void {
    this.form = this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      descricao: ['', [Validators.maxLength(500)]],
      unidadeMedidaId: [null, [Validators.required]],
      ativo: [true]
    });
  }

  /**
   * Load items
   */
  carregarItens(): void {
    this.loading.set(true);
    
    this.service.obterTodos().subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar embalagens:', error);
        this.loading.set(false);
      }
    });
  }

  /**
   * Load unidades de medida
   */
  private carregarUnidadesMedida(): void {
    this.service.obterUnidadesMedidaParaDropdown().subscribe({
      next: (unidades) => {
        this.unidadesDisponiveis = unidades;
        this.unidadesFiltradas = [...unidades];
      },
      error: (error) => {
        console.error('Erro ao carregar unidades de medida:', error);
      }
    });
  }

  /**
   * Load tipos de unidade
   */
  private carregarTiposUnidade(): void {
    this.service.obterTiposUnidade().subscribe({
      next: (tipos) => {
        this.tiposUnidadeDisponiveis = tipos;
      },
      error: (error) => {
        console.error('Erro ao carregar tipos de unidade:', error);
      }
    });
  }

  /**
   * Handle tipo unidade filter change
   */
  onTipoUnidadeFilterChange(): void {
    if (this.tipoUnidadeSelecionado) {
      this.unidadesFiltradas = this.unidadesDisponiveis.filter(
        unidade => unidade.tipo === this.tipoUnidadeSelecionado
      );
    } else {
      this.unidadesFiltradas = [...this.unidadesDisponiveis];
    }
    
    // Clear unidade selection if it doesn't match the type filter
    if (this.unidadeMedidaSelecionada && this.tipoUnidadeSelecionado) {
      const selectedUnidade = this.unidadesDisponiveis.find(u => u.id === this.unidadeMedidaSelecionada);
      if (selectedUnidade && selectedUnidade.tipo !== this.tipoUnidadeSelecionado) {
        this.unidadeMedidaSelecionada = null;
      }
    }
    
    this.applyFilters();
  }

  /**
   * Handle unidade medida filter change
   */
  onUnidadeMedidaFilterChange(): void {
    this.applyFilters();
  }

  /**
   * Apply filters to items
   */
  private applyFilters(): void {
    // For now, just reload items - in a real implementation, you'd filter the items array
    this.carregarItens();
  }

  /**
   * Get tipo unidade description
   */
  getTipoUnidadeDescricao(tipo: TipoUnidadeMedida): string {
    return this.service.getTipoUnidadeDescricao(tipo);
  }

  /**
   * Open form for new item
   */
  novoItem(): void {
    this.selectedItem.set(null);
    this.form.reset({ ativo: true });
    this.showForm.set(true);
  }

  /**
   * Open form for editing item
   */
  editarItem(item: EmbalagemDto): void {
    this.selectedItem.set(item);
    this.form.patchValue({
      nome: item.nome,
      descricao: item.descricao || '',
      unidadeMedidaId: item.unidadeMedidaId,
      ativo: item.ativo
    });
    this.showForm.set(true);
  }

  /**
   * Save item
   */
  salvarItem(): void {
    if (this.form.invalid) {
      return;
    }

    const formValue = this.form.value;
    
    if (this.isEditMode()) {
      // Update
      const updateDto: AtualizarEmbalagemDto = {
        nome: formValue.nome?.trim(),
        descricao: formValue.descricao?.trim() || undefined,
        unidadeMedidaId: formValue.unidadeMedidaId,
        ativo: formValue.ativo
      };
      
      this.service.atualizar(this.selectedItem()!.id, updateDto).subscribe({
        next: () => {
          this.showForm.set(false);
          this.messageService.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: 'Embalagem atualizada com sucesso'
          });
          this.carregarItens();
        },
        error: (error) => {
          console.error('Erro ao atualizar embalagem:', error);
        }
      });
    } else {
      // Create
      const createDto: CriarEmbalagemDto = {
        nome: formValue.nome?.trim(),
        descricao: formValue.descricao?.trim() || undefined,
        unidadeMedidaId: formValue.unidadeMedidaId
      };
      
      this.service.criar(createDto).subscribe({
        next: () => {
          this.showForm.set(false);
          this.messageService.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: 'Embalagem criada com sucesso'
          });
          this.carregarItens();
        },
        error: (error) => {
          console.error('Erro ao criar embalagem:', error);
        }
      });
    }
  }

  /**
   * Cancel editing
   */
  cancelarEdicao(): void {
    this.showForm.set(false);
    this.selectedItem.set(null);
    this.form.reset();
  }

  /**
   * Activate item
   */
  ativarItem(item: EmbalagemDto): void {
    this.service.ativar(item.id).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Embalagem ativada com sucesso'
        });
        this.carregarItens();
      },
      error: (error) => {
        console.error('Erro ao ativar embalagem:', error);
      }
    });
  }

  /**
   * Deactivate item
   */
  desativarItem(item: EmbalagemDto): void {
    this.confirmationService.confirm({
      message: 'Tem certeza que deseja desativar esta embalagem?',
      header: 'Confirmar Desativação',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.service.desativar(item.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: 'Embalagem desativada com sucesso'
            });
            this.carregarItens();
          },
          error: (error) => {
            console.error('Erro ao desativar embalagem:', error);
          }
        });
      }
    });
  }

  /**
   * Delete item
   */
  excluirItem(item: EmbalagemDto): void {
    this.confirmationService.confirm({
      message: 'Tem certeza que deseja excluir esta embalagem?',
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.service.remover(item.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: 'Embalagem excluída com sucesso'
            });
            this.carregarItens();
          },
          error: (error) => {
            console.error('Erro ao excluir embalagem:', error);
          }
        });
      }
    });
  }

  /**
   * Check if in edit mode
   */
  isEditMode(): boolean {
    return this.selectedItem() !== null;
  }

  /**
   * Get dialog title
   */
  dialogTitle(): string {
    return this.isEditMode() ? 'Editar Embalagem' : 'Nova Embalagem';
  }
}