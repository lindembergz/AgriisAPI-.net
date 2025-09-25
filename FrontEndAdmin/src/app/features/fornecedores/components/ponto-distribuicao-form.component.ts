import { Component, Input, Output, EventEmitter, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';

// PrimeNG imports
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { AccordionModule } from 'primeng/accordion';

// Models
import { PontoDistribuicaoForm } from '../../../shared/models/ponto-distribuicao.model';
import { PontoDistribuicaoFormControls, EnderecoFormControls } from '../../../shared/models/forms.model';

// Shared components
import { EnderecoFormComponent } from '../../../shared/components/endereco-form.component';

/**
 * Ponto Distribuicao Form Component for Fornecedores
 * Handles distribution point data with dynamic list management
 */
@Component({
  selector: 'app-ponto-distribuicao-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    ConfirmDialogModule,
    AccordionModule,
    EnderecoFormComponent
  ],
  providers: [ConfirmationService],
  templateUrl: './ponto-distribuicao-form.component.html',
  styleUrls: ['./ponto-distribuicao-form.component.scss']
})
export class PontoDistribuicaoFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);

  @Input() pontosDistribuicaoFormArray!: FormArray<FormGroup<PontoDistribuicaoFormControls>>;
  @Input() readonly = false;
  @Output() pontosDistribuicaoChange = new EventEmitter<PontoDistribuicaoForm[]>();

  // Signals for reactive state
  loading = signal(false);

  // Validation messages
  validationMessages = {
    nome: {
      required: 'Nome do ponto de distribuição é obrigatório',
      minlength: 'Nome deve ter pelo menos 3 caracteres'
    }
  };

  ngOnInit(): void {
    // Component initialization - no default ponto needed as it's optional
  }

  /**
   * Create new ponto distribuicao form group
   */
  private createPontoDistribuicaoFormGroup(): FormGroup<PontoDistribuicaoFormControls> {
    return this.fb.group<PontoDistribuicaoFormControls>({
      nome: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(3)]
      }),
      latitude: this.fb.control<number | null>(null),
      longitude: this.fb.control<number | null>(null),
      endereco: this.createEnderecoFormGroup()
    });
  }

  /**
   * Create endereco form group for ponto distribuicao
   */
  private createEnderecoFormGroup(): FormGroup<EnderecoFormControls> {
    return this.fb.group<EnderecoFormControls>({
      logradouro: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(3)]
      }),
      numero: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required]
      }),
      complemento: this.fb.control('', { nonNullable: true }),
      bairro: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(2)]
      }),
      cidade: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(2)]
      }),
      uf: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required]
      }),
      cep: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required]
      }),
      latitude: this.fb.control<number | null>(null),
      longitude: this.fb.control<number | null>(null)
    });
  }

  /**
   * Add new ponto distribuicao to the form array
   */
  addPontoDistribuicao(): void {
    const pontoGroup = this.createPontoDistribuicaoFormGroup();
    this.pontosDistribuicaoFormArray.push(pontoGroup);
    this.emitChange();
  }

  /**
   * Remove ponto distribuicao from the form array
   */
  removePontoDistribuicao(index: number): void {
    this.confirmationService.confirm({
      message: 'Tem certeza que deseja remover este ponto de distribuição?',
      header: 'Confirmar Remoção',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.pontosDistribuicaoFormArray.removeAt(index);
        this.emitChange();
      }
    });
  }

  /**
   * Get ponto distribuicao form group at index
   */
  getPontoDistribuicaoFormGroup(index: number): FormGroup<PontoDistribuicaoFormControls> {
    return this.pontosDistribuicaoFormArray.at(index) as FormGroup<PontoDistribuicaoFormControls>;
  }

  /**
   * Get endereco form group for a specific ponto distribuicao
   */
  getEnderecoFormGroup(pontoIndex: number): FormGroup<EnderecoFormControls> {
    return this.getPontoDistribuicaoFormGroup(pontoIndex).get('endereco') as FormGroup<EnderecoFormControls>;
  }

  /**
   * Get form control for validation display
   */
  getFormControl(index: number, controlName: string) {
    return this.getPontoDistribuicaoFormGroup(index).get(controlName);
  }

  /**
   * Check if field has error
   */
  hasFieldError(index: number, controlName: string, errorType?: string): boolean {
    const control = this.getFormControl(index, controlName);
    if (!control) return false;
    
    if (errorType) {
      return control.hasError(errorType) && (control.dirty || control.touched);
    }
    
    return control.invalid && (control.dirty || control.touched);
  }

  /**
   * Get field error message
   */
  getFieldErrorMessage(index: number, controlName: string): string {
    const control = this.getFormControl(index, controlName);
    if (!control || !control.errors) return '';
    
    const fieldMessages = this.validationMessages[controlName as keyof typeof this.validationMessages];
    if (!fieldMessages) return 'Campo inválido';
    
    const errorType = Object.keys(control.errors)[0] as keyof typeof fieldMessages;
    return fieldMessages[errorType] || 'Campo inválido';
  }

  /**
   * Emit changes to parent component
   */
  private emitChange(): void {
    const pontos = this.pontosDistribuicaoFormArray.value as PontoDistribuicaoForm[];
    this.pontosDistribuicaoChange.emit(pontos);
    // Mark the form array as dirty to trigger change detection
    this.pontosDistribuicaoFormArray.markAsDirty();
  }

  /**
   * Get ponto distribuicao title for display
   */
  getPontoDistribuicaoTitle(index: number): string {
    const ponto = this.getPontoDistribuicaoFormGroup(index).value;
    if (ponto.nome) {
      return ponto.nome;
    }
    return `Ponto de Distribuição ${index + 1}`;
  }

  /**
   * Get ponto distribuicao summary for display
   */
  getPontoDistribuicaoSummary(index: number): string {
    const ponto = this.getPontoDistribuicaoFormGroup(index).value;
    const endereco = ponto.endereco;
    
    let summary = '';
    
    if (endereco?.cidade && endereco?.uf) {
      summary = `${endereco.cidade} - ${endereco.uf}`;
    } else if (endereco?.logradouro) {
      summary = endereco.logradouro;
    }
    
    if (ponto.latitude && ponto.longitude) {
      summary += summary ? ' • Com coordenadas' : 'Com coordenadas';
    }
    
    return summary || 'Dados não preenchidos';
  }

  /**
   * Handle endereco changes for a specific ponto
   */
  onEnderecoChange(pontoIndex: number, endereco: any): void {
    const pontoGroup = this.getPontoDistribuicaoFormGroup(pontoIndex);
    const enderecoGroup = pontoGroup.get('endereco') as FormGroup;
    
    // Update endereco data
    enderecoGroup.patchValue(endereco);
    
    // If endereco has coordinates, update ponto coordinates
    if (endereco.latitude && endereco.longitude) {
      pontoGroup.patchValue({
        latitude: endereco.latitude,
        longitude: endereco.longitude
      });
    }
    
    this.emitChange();
  }

  /**
   * Handle coordinates update (will be called by map component)
   */
  updateCoordinates(index: number, latitude: number, longitude: number): void {
    const pontoGroup = this.getPontoDistribuicaoFormGroup(index);
    const enderecoGroup = pontoGroup.get('endereco') as FormGroup;
    
    // Update both ponto and endereco coordinates
    pontoGroup.patchValue({
      latitude,
      longitude
    });
    
    enderecoGroup.patchValue({
      latitude,
      longitude
    });
    
    this.emitChange();
  }

  /**
   * Clear coordinates
   */
  clearCoordinates(index: number): void {
    const pontoGroup = this.getPontoDistribuicaoFormGroup(index);
    const enderecoGroup = pontoGroup.get('endereco') as FormGroup;
    
    // Clear both ponto and endereco coordinates
    pontoGroup.patchValue({
      latitude: null,
      longitude: null
    });
    
    enderecoGroup.patchValue({
      latitude: null,
      longitude: null
    });
    
    this.emitChange();
  }

  /**
   * Check if ponto distribuicao has coordinates
   */
  hasCoordinates(index: number): boolean {
    const ponto = this.getPontoDistribuicaoFormGroup(index).value;
    return !!(ponto.latitude && ponto.longitude);
  }

  /**
   * Get coordinates display text
   */
  getCoordinatesText(index: number): string {
    const ponto = this.getPontoDistribuicaoFormGroup(index).value;
    if (ponto.latitude && ponto.longitude) {
      return `${ponto.latitude.toFixed(6)}, ${ponto.longitude.toFixed(6)}`;
    }
    return 'Não definidas';
  }

  /**
   * Check if has any pontos distribuicao
   */
  get hasPontosDistribuicao(): boolean {
    return this.pontosDistribuicaoFormArray.length > 0;
  }

  /**
   * Get empty state message
   */
  get emptyStateMessage(): string {
    return 'Nenhum ponto de distribuição cadastrado. Clique em "Adicionar Ponto de Distribuição" para começar.';
  }
}