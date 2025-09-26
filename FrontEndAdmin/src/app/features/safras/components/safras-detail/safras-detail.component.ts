import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MessageService } from 'primeng/api';

// PrimeNG Components
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { DatePickerModule } from 'primeng/datepicker';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { SafraService } from '../../services/safra.service';
import { SafraDto, CriarSafraDto, AtualizarSafraDto, SafraForm } from '../../models';
import { 
  safraDateValidator, 
  dateRangeValidator, 
  safraPlantioNomeValidator, 
  safraDescricaoValidator,
  getFormValidationErrorMessage 
} from '../../../../shared/utils/field-validators.util';

/**
 * Componente para criação e edição de safras
 * Implementa formulário reativo com validações complexas de datas conforme regras da entidade Safra
 */
@Component({
  selector: 'app-safras-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    DatePickerModule,
    CardModule,
    ProgressSpinnerModule
  ],
  templateUrl: './safras-detail.component.html',
  styleUrls: ['./safras-detail.component.scss']
})
export class SafrasDetailComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private safraService = inject(SafraService);
  private messageService = inject(MessageService);

  // Signals for component state
  loading = signal(false);
  saving = signal(false);
  isEditMode = signal(false);
  safraId = signal<number | null>(null);

  // Reactive form
  safraForm!: FormGroup<SafraForm>;

  // Date constraints
  readonly minDate = new Date(1900, 0, 1); // January 1, 1900
  readonly maxDate = new Date(new Date().getFullYear() + 10, 11, 31); // 10 years in the future

  /**
   * Get validation error message for a form field
   */
  getFieldErrorMessage(fieldName: string): string {
    const control = this.safraForm.get(fieldName);
    if (!control) return '';
    return getFormValidationErrorMessage(control, 'safra', fieldName);
  }

  ngOnInit(): void {
    this.initializeForm();
    this.checkRouteParams();
  }

  /**
   * Initialize reactive form with validation rules
   */
  private initializeForm(): void {
    this.safraForm = this.fb.group<SafraForm>({
      plantioInicial: this.fb.control<Date>(new Date(), { 
        nonNullable: true, 
        validators: [safraDateValidator()] 
      }),
      plantioFinal: this.fb.control<Date>(new Date(), { 
        nonNullable: true, 
        validators: [safraDateValidator(), dateRangeValidator('plantioInicial', 'plantioFinal')] 
      }),
      plantioNome: this.fb.control('', { 
        nonNullable: true, 
        validators: [safraPlantioNomeValidator()] 
      }),
      descricao: this.fb.control('', { 
        nonNullable: true, 
        validators: [safraDescricaoValidator()] 
      })
    });
  }



  /**
   * Check route parameters to determine if it's create or edit mode
   */
  private checkRouteParams(): void {
    const id = this.route.snapshot.paramMap.get('id');
    
    if (id && id !== 'nova') {
      const safraId = parseInt(id, 10);
      if (!isNaN(safraId)) {
        this.isEditMode.set(true);
        this.safraId.set(safraId);
        this.loadSafra(safraId);
      } else {
        this.handleInvalidRoute();
      }
    } else {
      // Create mode - set default values
      this.isEditMode.set(false);
      this.safraId.set(null);
      // Set default dates (current year planting season)
      const currentYear = new Date().getFullYear();
      this.safraForm.patchValue({
        plantioInicial: new Date(currentYear, 8, 1), // September 1st
        plantioFinal: new Date(currentYear + 1, 1, 28) // February 28th next year
      });
    }
  }

  /**
   * Load safra data for edit mode
   */
  private loadSafra(id: number): void {
    this.loading.set(true);
    
    this.safraService.obterPorId(id).subscribe({
      next: (safra: SafraDto) => {
        this.safraForm.patchValue({
          plantioInicial: new Date(safra.plantioInicial),
          plantioFinal: new Date(safra.plantioFinal),
          plantioNome: safra.plantioNome,
          descricao: safra.descricao
        });
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar safra:', error);
        this.loading.set(false);
        this.router.navigate(['/safras']);
      }
    });
  }

  /**
   * Handle invalid route parameters
   */
  private handleInvalidRoute(): void {
    this.messageService.add({
      severity: 'error',
      summary: 'Erro',
      detail: 'ID da safra inválido'
    });
    this.router.navigate(['/safras']);
  }

  /**
   * Save safra (create or update)
   */
  onSave(): void {
    if (this.safraForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.saving.set(true);

    if (this.isEditMode()) {
      this.updateSafra();
    } else {
      this.createSafra();
    }
  }

  /**
   * Create new safra
   */
  private createSafra(): void {
    const formValue = this.safraForm.value;
    const criarSafraDto: CriarSafraDto = {
      plantioInicial: formValue.plantioInicial!,
      plantioFinal: formValue.plantioFinal!,
      plantioNome: formValue.plantioNome!,
      descricao: formValue.descricao!
    };

    this.safraService.criar(criarSafraDto).subscribe({
      next: (safra: SafraDto) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Safra criada com sucesso'
        });
        this.saving.set(false);
        this.router.navigate(['/safras']);
      },
      error: (error) => {
        console.error('Erro ao criar safra:', error);
        this.saving.set(false);
      }
    });
  }

  /**
   * Update existing safra
   */
  private updateSafra(): void {
    const safraId = this.safraId();
    if (!safraId) return;

    const formValue = this.safraForm.value;
    const atualizarSafraDto: AtualizarSafraDto = {
      plantioInicial: formValue.plantioInicial!,
      plantioFinal: formValue.plantioFinal!,
      plantioNome: formValue.plantioNome!,
      descricao: formValue.descricao!
    };

    this.safraService.atualizar(safraId, atualizarSafraDto).subscribe({
      next: (safra: SafraDto) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Safra atualizada com sucesso'
        });
        this.saving.set(false);
        this.router.navigate(['/safras']);
      },
      error: (error) => {
        console.error('Erro ao atualizar safra:', error);
        this.saving.set(false);
      }
    });
  }

  /**
   * Cancel and return to list
   */
  onCancel(): void {
    this.router.navigate(['/safras']);
  }

  /**
   * Mark all form controls as touched to show validation errors
   */
  private markFormGroupTouched(): void {
    Object.keys(this.safraForm.controls).forEach(key => {
      const control = this.safraForm.get(key);
      control?.markAsTouched();
    });
  }

  /**
   * Check if a field has validation errors and is touched
   */
  hasFieldError(fieldName: string): boolean {
    const field = this.safraForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }



  /**
   * Check if a field is valid and touched (for success styling)
   */
  isFieldValid(fieldName: string): boolean {
    const field = this.safraForm.get(fieldName);
    return !!(field && field.valid && field.touched && field.value);
  }

  /**
   * Get page title based on mode
   */
  get pageTitle(): string {
    return this.isEditMode() ? 'Editar Safra' : 'Nova Safra';
  }

  /**
   * Get save button text based on mode
   */
  get saveButtonText(): string {
    return this.isEditMode() ? 'Atualizar' : 'Salvar';
  }
}
