import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { ReferenceCrudBaseComponent } from '../../../shared/components/reference-crud-base/reference-crud-base.component';
import { UfDto, CriarUfDto, AtualizarUfDto, PaisDto } from '../../../shared/models/reference.model';
import { UfService } from './services/uf.service';
import { PaisService } from '../paises/services/pais.service';
import { map } from 'rxjs';

interface TableColumn {
  field: string;
  header: string;
  sortable?: boolean;
  width?: string;
  type?: 'text' | 'boolean' | 'date' | 'custom';
  hideOnMobile?: boolean;
  hideOnTablet?: boolean;
}

/**
 * Component for managing UFs (Unidades Federativas) with País selection and Município dependency
 * Shows País relationship and Município count, prevents deletion when Municípios exist
 */
@Component({
  selector: 'app-ufs',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
    SelectModule,
    TagModule,
    TooltipModule,
    TableModule,
    ButtonModule,
    SelectModule,
    ConfirmDialogModule,
    ToastModule,
    ProgressSpinnerModule,
    DialogModule,
    CheckboxModule
  ],
  templateUrl: './ufs.component.html',
  styleUrls: ['./ufs.component.scss']
})
export class UfsComponent extends ReferenceCrudBaseComponent<
  UfDto,
  CriarUfDto,
  AtualizarUfDto
> implements OnInit {
  
  protected service = inject(UfService);
  private paisService = inject(PaisService);

  // Signals for reactive data
  paisesOptions = signal<PaisDto[]>([]);
  loadingPaises = signal<boolean>(false);

  // Entity configuration
  protected entityDisplayName = () => 'UF';
  protected entityDescription = () => 'Gerenciar Unidades Federativas (Estados)';
  protected defaultSortField = () => 'codigo';
  protected searchFields = () => ['codigo', 'nome', 'pais.nome'];

  // Table columns configuration
  protected displayColumns = (): TableColumn[] => [
    {
      field: 'codigo',
      header: 'Código',
      sortable: true,
      width: '100px'
    },
    {
      field: 'nome',
      header: 'Nome',
      sortable: true,
      width: '250px'
    },
    {
      field: 'pais',
      header: 'País',
      sortable: true,
      width: '200px',
      type: 'custom',
      hideOnMobile: true
    },
    {
      field: 'municipiosCount',
      header: 'Municípios',
      sortable: true,
      width: '120px',
      type: 'custom',
      hideOnMobile: true
    },
    {
      field: 'ativo',
      header: 'Status',
      sortable: true,
      width: '100px',
      type: 'boolean',
      hideOnMobile: true
    },
    {
      field: 'dataCriacao',
      header: 'Criado em',
      sortable: true,
      width: '150px',
      type: 'date',
      hideOnMobile: true,
      hideOnTablet: true
    }
  ];

  ngOnInit(): void {
    super.ngOnInit();
    this.carregarPaises();
  }

  /**
   * Load países for dropdown
   */
  private carregarPaises(): void {
    this.loadingPaises.set(true);
    
    this.paisService.obterAtivos().subscribe({
      next: (paises) => {
        this.paisesOptions.set(paises);
        this.loadingPaises.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar países:', error);
        this.loadingPaises.set(false);
      }
    });
  }

  /**
   * Override carregarItens to include País information
   */
  override carregarItens(): void {
    this.loading.set(true);
    this.tableLoading.set(true);
    
    const filter = this.selectedStatusFilter();
    let request;

    if (filter === 'ativas') {
      request = this.service.obterAtivos();
    } else {
      request = this.service.obterComPais();
    }

    request.subscribe({
      next: (items) => {
        if (filter === 'inativas') {
          // Filter inactive items when showing only inactive ones
          this.items.set(items.filter(item => !item.ativo));
        } else {
          this.items.set(items);
        }
        
        // Load municípios count for each UF
        this.carregarContagemMunicipios(items);
        
        this.loading.set(false);
        this.tableLoading.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar UFs:', error);
        this.loading.set(false);
        this.tableLoading.set(false);
      }
    });
  }

  /**
   * Load municípios count for UFs
   */
  private carregarContagemMunicipios(ufs: UfDto[]): void {
    ufs.forEach(uf => {
      this.service.obterContagemMunicipios(uf.id).subscribe({
        next: (count) => {
          // Update the UF object with municípios count
          (uf as any).municipiosCount = count;
        },
        error: (error) => {
          console.error(`Erro ao carregar contagem de municípios para UF ${uf.codigo}:`, error);
          (uf as any).municipiosCount = 0;
        }
      });
    });
  }



  /**
   * Map form values to create DTO
   */
  protected mapToCreateDto(formValue: any): CriarUfDto {
    return {
      codigo: formValue.codigo?.toUpperCase().trim(),
      nome: formValue.nome?.trim(),
      paisId: parseInt(formValue.paisId)
    };
  }

  /**
   * Map form values to update DTO
   */
  protected mapToUpdateDto(formValue: any): AtualizarUfDto {
    return {
      nome: formValue.nome?.trim(),
      ativo: formValue.ativo
    };
  }

  /**
   * Populate form with entity data for editing
   */
  protected populateForm(item: UfDto): void {
    this.form.patchValue({
      codigo: item.codigo,
      nome: item.nome,
      paisId: item.paisId,
      ativo: item.ativo
    });
  }

  /**
   * Get Municípios count display text
   */
  getMunicipiosCountDisplay(item: any): string {
    const count = item.municipiosCount || 0;
    return count === 0 ? 'Nenhum' : `${count}`;
  }

  /**
   * Get Municípios count severity for styling
   */
  getMunicipiosCountSeverity(count: number): string {
    if (count === 0) return 'secondary';
    if (count <= 10) return 'info';
    if (count <= 50) return 'success';
    if (count <= 100) return 'warning';
    return 'danger';
  }

  /**
   * Get tooltip text for municípios count
   */
  getMunicipiosTooltip(count: number): string {
    if (count === 0) return 'Nenhum município cadastrado';
    return `${count} município${count > 1 ? 's' : ''} cadastrado${count > 1 ? 's' : ''}`;
  }

  /**
   * Override excluir to check Município dependencies
   */
  override excluirItem(item: UfDto): void {
    // Check if UF has Municípios before allowing deletion
    this.service.verificarDependenciasMunicipio(item.id).subscribe({
      next: (temMunicipios) => {
        if (temMunicipios) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Não é possível excluir',
            detail: 'Esta UF possui municípios cadastrados. Remova os municípios primeiro.',
            life: 5000
          });
          return;
        }
        
        // Proceed with normal deletion flow
        super.excluirItem(item);
      },
      error: (error) => {
        console.error('Erro ao verificar dependências:', error);
      }
    });
  }

  /**
   * Custom validation for código uniqueness within país
   */
  private validarCodigoUnico = (control: any) => {
    if (!control.value || !this.form?.get('paisId')?.value) {
      return null;
    }

    const codigo = control.value.toUpperCase();
    const paisId = parseInt(this.form.get('paisId')?.value);
    const ufId = this.selectedItem()?.id;

    return this.service.validarCodigoUnico(codigo, paisId, ufId).pipe(
      map(isUnique => isUnique ? null : { codigoNaoUnico: true })
    );
  };

  /**
   * Override form creation to add custom validators
   */
  protected override createFormGroup(): FormGroup {
    const form = this.fb.group({
      codigo: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(2),
        Validators.pattern(/^[A-Z]{2}$/)
      ]],
      nome: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100)
      ]],
      paisId: ['', [
        Validators.required
      ]],
      ativo: [true]
    });

    // Add async validator for código uniqueness
    form.get('codigo')?.setAsyncValidators([this.validarCodigoUnico]);

    return form;
  }
}