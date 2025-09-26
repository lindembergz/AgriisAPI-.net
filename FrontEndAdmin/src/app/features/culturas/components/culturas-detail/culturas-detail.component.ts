import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MessageService } from 'primeng/api';

// PrimeNG Components
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { CulturaService } from '../../services/cultura.service';
import { CulturaDto, CriarCulturaDto, AtualizarCulturaDto, CulturaForm } from '../../models';
import { 
  culturaNomeValidator, 
  maxLengthValidator, 
  getFormValidationErrorMessage 
} from '../../../../shared/utils/field-validators.util';

/**
 * Componente para criação e edição de culturas
 * Implementa formulário reativo com validações conforme regras da entidade Cultura
 */
@Component({
  selector: 'app-culturas-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    CheckboxModule,
    CardModule,
    ProgressSpinnerModule
  ],
  templateUrl: './culturas-detail.component.html',
  styleUrls: ['./culturas-detail.component.scss']
})
export class CulturasDetailComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private culturaService = inject(CulturaService);
  private messageService = inject(MessageService);

  // Signals for component state
  loading = signal(false);
  saving = signal(false);
  isEditMode = signal(false);
  culturaId = signal<number | null>(null);

  // Reactive form
  culturaForm!: FormGroup<CulturaForm>;



  ngOnInit(): void {
    this.initializeForm();
    this.checkRouteParams();
  }

  /**
   * Initialize reactive form with validation rules
   */
  private initializeForm(): void {
    this.culturaForm = this.fb.group<CulturaForm>({
      nome: this.fb.control('', { 
        nonNullable: true, 
        validators: [culturaNomeValidator()] 
      }),
      descricao: this.fb.control<string | null>(null, {
        validators: [maxLengthValidator(500, 'Descrição')]
      }),
      ativo: this.fb.control(true, { nonNullable: true })
    });
  }

  /**
   * Check route parameters to determine if it's create or edit mode
   */
  private checkRouteParams(): void {
    const id = this.route.snapshot.paramMap.get('id');
    
    if (id && id !== 'nova') {
      const culturaId = parseInt(id, 10);
      if (!isNaN(culturaId)) {
        this.isEditMode.set(true);
        this.culturaId.set(culturaId);
        this.loadCultura(culturaId);
      } else {
        this.handleInvalidRoute();
      }
    } else {
      // Create mode - set default values
      this.isEditMode.set(false);
      this.culturaId.set(null);
      this.culturaForm.patchValue({
        ativo: true
      });
    }
  }

  /**
   * Load cultura data for edit mode
   */
  private loadCultura(id: number): void {
    this.loading.set(true);
    
    this.culturaService.obterPorId(id).subscribe({
      next: (cultura: CulturaDto) => {
        this.culturaForm.patchValue({
          nome: cultura.nome,
          descricao: cultura.descricao || null,
          ativo: cultura.ativo
        });
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar cultura:', error);
        this.loading.set(false);
        this.router.navigate(['/culturas']);
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
      detail: 'ID da cultura inválido'
    });
    this.router.navigate(['/culturas']);
  }

  /**
   * Save cultura (create or update)
   */
  onSave(): void {
    if (this.culturaForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.saving.set(true);

    if (this.isEditMode()) {
      this.updateCultura();
    } else {
      this.createCultura();
    }
  }

  /**
   * Create new cultura
   */
  private createCultura(): void {
    const formValue = this.culturaForm.value;
    const criarCulturaDto: CriarCulturaDto = {
      nome: formValue.nome!,
      descricao: formValue.descricao || undefined
    };

    this.culturaService.criar(criarCulturaDto).subscribe({
      next: (cultura: CulturaDto) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Cultura criada com sucesso'
        });
        this.saving.set(false);
        this.router.navigate(['/culturas']);
      },
      error: (error) => {
        console.error('Erro ao criar cultura:', error);
        this.saving.set(false);
      }
    });
  }

  /**
   * Update existing cultura
   */
  private updateCultura(): void {
    const culturaId = this.culturaId();
    if (!culturaId) return;

    const formValue = this.culturaForm.value;
    const atualizarCulturaDto: AtualizarCulturaDto = {
      nome: formValue.nome!,
      descricao: formValue.descricao || undefined,
      ativo: formValue.ativo!
    };

    this.culturaService.atualizar(culturaId, atualizarCulturaDto).subscribe({
      next: (cultura: CulturaDto) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Cultura atualizada com sucesso'
        });
        this.saving.set(false);
        this.router.navigate(['/culturas']);
      },
      error: (error) => {
        console.error('Erro ao atualizar cultura:', error);
        this.saving.set(false);
      }
    });
  }

  /**
   * Cancel and return to list
   */
  onCancel(): void {
    this.router.navigate(['/culturas']);
  }

  /**
   * Mark all form controls as touched to show validation errors
   */
  private markFormGroupTouched(): void {
    Object.keys(this.culturaForm.controls).forEach(key => {
      const control = this.culturaForm.get(key);
      control?.markAsTouched();
    });
  }

  /**
   * Check if a field has validation errors and is touched
   */
  hasFieldError(fieldName: string): boolean {
    const field = this.culturaForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  /**
   * Get validation error message for a field
   */
  getFieldErrorMessage(fieldName: string): string {
    const control = this.culturaForm.get(fieldName);
    if (!control) return '';
    return getFormValidationErrorMessage(control, 'cultura', fieldName);
  }

  /**
   * Check if a field is valid and touched (for success styling)
   */
  isFieldValid(fieldName: string): boolean {
    const field = this.culturaForm.get(fieldName);
    return !!(field && field.valid && field.touched && field.value);
  }

  /**
   * Get page title based on mode
   */
  get pageTitle(): string {
    return this.isEditMode() ? 'Editar Cultura' : 'Nova Cultura';
  }

  /**
   * Get save button text based on mode
   */
  get saveButtonText(): string {
    return this.isEditMode() ? 'Atualizar' : 'Salvar';
  }
}