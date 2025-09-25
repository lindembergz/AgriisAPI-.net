import { Component, Input, Output, EventEmitter, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';

// PrimeNG imports
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

// Models
import { PropriedadeForm } from '../../../shared/models/propriedade.model';
import { PropriedadeFormControls, CulturaFormControls } from '../../../shared/models/forms.model';

/**
 * Propriedade Form Component for Produtores
 * Handles rural property data with dynamic list management
 */
@Component({
  selector: 'app-propriedade-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    ConfirmDialogModule
  ],
  providers: [ConfirmationService],
  templateUrl: './propriedade-form.component.html',
  styleUrls: ['./propriedade-form.component.scss']
})
export class PropriedadeFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);

  @Input() propriedadesFormArray!: FormArray<FormGroup<PropriedadeFormControls>>;
  @Input() readonly = false;
  @Output() propriedadesChange = new EventEmitter<PropriedadeForm[]>();

  // Signals for reactive state
  loading = signal(false);

  // Validation messages
  validationMessages = {
    nome: {
      required: 'Nome da propriedade é obrigatório',
      minlength: 'Nome deve ter pelo menos 3 caracteres'
    },
    area: {
      required: 'Área é obrigatória',
      min: 'Área deve ser maior que zero'
    }
  };

  ngOnInit(): void {
    // Initialize with at least one propriedade if empty
    if (this.propriedadesFormArray.length === 0) {
      this.addPropriedade();
    }
  }

  /**
   * Create new propriedade form group
   */
  private createPropriedadeFormGroup(): FormGroup<PropriedadeFormControls> {
    return this.fb.group<PropriedadeFormControls>({
      nome: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(3)]
      }),
      area: this.fb.control(0, {
        nonNullable: true,
        validators: [Validators.required, Validators.min(0.01)]
      }),
      latitude: this.fb.control<number | null>(null),
      longitude: this.fb.control<number | null>(null),
      culturas: this.fb.array<FormGroup<CulturaFormControls>>([])
    });
  }

  /**
   * Add new propriedade to the form array
   */
  addPropriedade(): void {
    const propriedadeGroup = this.createPropriedadeFormGroup();
    this.propriedadesFormArray.push(propriedadeGroup);
    this.emitChange();
  }

  /**
   * Remove propriedade from the form array
   */
  removePropriedade(index: number): void {
    if (this.propriedadesFormArray.length <= 1) {
      return; // Keep at least one propriedade
    }

    this.confirmationService.confirm({
      message: 'Tem certeza que deseja remover esta propriedade? Todas as culturas associadas também serão removidas.',
      header: 'Confirmar Remoção',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.propriedadesFormArray.removeAt(index);
        this.emitChange();
      }
    });
  }

  /**
   * Get propriedade form group at index
   */
  getPropriedadeFormGroup(index: number): FormGroup<PropriedadeFormControls> {
    return this.propriedadesFormArray.at(index) as FormGroup<PropriedadeFormControls>;
  }

  /**
   * Get culturas form array for a specific propriedade
   */
  getCulturasFormArray(propriedadeIndex: number): FormArray {
    return this.getPropriedadeFormGroup(propriedadeIndex).get('culturas') as FormArray;
  }

  /**
   * Get form control for validation display
   */
  getFormControl(index: number, controlName: string) {
    return this.getPropriedadeFormGroup(index).get(controlName);
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
    const propriedades = this.propriedadesFormArray.value as PropriedadeForm[];
    this.propriedadesChange.emit(propriedades);
    // Mark the form array as dirty to trigger change detection
    this.propriedadesFormArray.markAsDirty();
  }

  /**
   * Get propriedade title for display
   */
  getPropriedadeTitle(index: number): string {
    const propriedade = this.getPropriedadeFormGroup(index).value;
    if (propriedade.nome) {
      return propriedade.nome;
    }
    return `Propriedade ${index + 1}`;
  }

  /**
   * Get propriedade summary for display
   */
  getPropriedadeSummary(index: number): string {
    const propriedade = this.getPropriedadeFormGroup(index).value;
    const culturas = this.getCulturasFormArray(index);
    
    let summary = '';
    
    if (propriedade.area && propriedade.area > 0) {
      summary += `Área: ${propriedade.area} ha`;
    }
    
    if (culturas.length > 0) {
      summary += summary ? ` • ${culturas.length} cultura(s)` : `${culturas.length} cultura(s)`;
    }
    
    return summary || 'Dados não preenchidos';
  }

  /**
   * Check if can remove propriedade (must have at least one)
   */
  canRemovePropriedade(): boolean {
    return this.propriedadesFormArray.length > 1 && !this.readonly;
  }

  /**
   * Format area display
   */
  formatArea(area: number | null): string {
    if (!area || area === 0) return '';
    return `${area.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} hectares`;
  }

  /**
   * Handle coordinates update (will be called by map component)
   */
  updateCoordinates(index: number, latitude: number, longitude: number): void {
    const propriedadeGroup = this.getPropriedadeFormGroup(index);
    propriedadeGroup.patchValue({
      latitude,
      longitude
    });
    this.emitChange();
  }

  /**
   * Clear coordinates
   */
  clearCoordinates(index: number): void {
    const propriedadeGroup = this.getPropriedadeFormGroup(index);
    propriedadeGroup.patchValue({
      latitude: null,
      longitude: null
    });
    this.emitChange();
  }

  /**
   * Check if propriedade has coordinates
   */
  hasCoordinates(index: number): boolean {
    const propriedade = this.getPropriedadeFormGroup(index).value;
    return !!(propriedade.latitude && propriedade.longitude);
  }

  /**
   * Get coordinates display text
   */
  getCoordinatesText(index: number): string {
    const propriedade = this.getPropriedadeFormGroup(index).value;
    if (propriedade.latitude && propriedade.longitude) {
      return `${propriedade.latitude.toFixed(6)}, ${propriedade.longitude.toFixed(6)}`;
    }
    return 'Não definidas';
  }
}