import { Component, Input, Output, EventEmitter, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';

// PrimeNG imports
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { AccordionModule } from 'primeng/accordion';

// Models
import { CulturaForm, TipoCultura } from '../../../shared/models/cultura.model';
import { CulturaFormControls, PropriedadeFormControls } from '../../../shared/models/forms.model';

/**
 * Interface for propriedade with culturas
 */
interface PropriedadeWithCulturas {
  index: number;
  nome: string;
  area: number;
  culturas: FormArray<FormGroup<CulturaFormControls>>;
}

/**
 * Cultura Form Component for Produtores
 * Handles crop data with year and cultivated area linked to properties
 */
@Component({
  selector: 'app-cultura-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    SelectModule,
    InputNumberModule,
    ConfirmDialogModule,
    AccordionModule
  ],
  providers: [ConfirmationService],
  templateUrl: './cultura-form.component.html',
  styleUrls: ['./cultura-form.component.scss']
})
export class CulturaFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);

  @Input() propriedadesFormArray!: FormArray<FormGroup<PropriedadeFormControls>>;
  @Input() readonly = false;
  @Output() culturasChange = new EventEmitter<void>();

  // Signals for reactive state
  loading = signal(false);
  activeAccordionIndex = signal<number | null>(0);

  // Dropdown options for cultura types
  tipoCulturaOptions = [
    { label: 'Soja', value: TipoCultura.SOJA, icon: 'pi pi-circle-fill', color: '#4CAF50' },
    { label: 'Milho', value: TipoCultura.MILHO, icon: 'pi pi-circle-fill', color: '#FFC107' },
    { label: 'Algodão', value: TipoCultura.ALGODAO, icon: 'pi pi-circle-fill', color: '#F5F5F5' },
    { label: 'Outros', value: TipoCultura.OUTROS, icon: 'pi pi-circle-fill', color: '#9E9E9E' }
  ];

  // Current year for validation
  currentYear = new Date().getFullYear();

  // Make TipoCultura available in template
  TipoCultura = TipoCultura;

  // Validation messages
  validationMessages = {
    tipo: {
      required: 'Tipo de cultura é obrigatório'
    },
    anoSafra: {
      required: 'Ano da safra é obrigatório',
      min: 'Ano deve ser maior que 2000',
      max: `Ano não pode ser maior que ${this.currentYear + 5}`
    },
    areaCultivada: {
      required: 'Área cultivada é obrigatória',
      min: 'Área deve ser maior que zero'
    }
  };

  ngOnInit(): void {
    // Component initialization
  }

  /**
   * Get propriedades with their culturas for display
   */
  get propriedadesWithCulturas(): PropriedadeWithCulturas[] {
    return this.propriedadesFormArray.controls.map((propriedadeControl, index) => {
      const propriedade = propriedadeControl.value;
      const culturasArray = propriedadeControl.get('culturas') as FormArray<FormGroup<CulturaFormControls>>;
      
      return {
        index,
        nome: propriedade.nome || `Propriedade ${index + 1}`,
        area: propriedade.area || 0,
        culturas: culturasArray
      };
    });
  }

  /**
   * Create new cultura form group
   */
  private createCulturaFormGroup(): FormGroup<CulturaFormControls> {
    return this.fb.group<CulturaFormControls>({
      tipo: this.fb.control(TipoCultura.SOJA, {
        nonNullable: true,
        validators: [Validators.required]
      }),
      anoSafra: this.fb.control(this.currentYear, {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.min(2000),
          Validators.max(this.currentYear + 5)
        ]
      }),
      areaCultivada: this.fb.control(0, {
        nonNullable: true,
        validators: [Validators.required, Validators.min(0.01)]
      })
    });
  }

  /**
   * Add new cultura to a specific propriedade
   */
  addCultura(propriedadeIndex: number): void {
    const propriedadeControl = this.propriedadesFormArray.at(propriedadeIndex);
    const culturasArray = propriedadeControl.get('culturas') as FormArray<FormGroup<CulturaFormControls>>;
    
    const culturaGroup = this.createCulturaFormGroup();
    culturasArray.push(culturaGroup);
    this.emitChange();
  }

  /**
   * Remove cultura from a propriedade
   */
  removeCultura(propriedadeIndex: number, culturaIndex: number): void {
    const propriedadeControl = this.propriedadesFormArray.at(propriedadeIndex);
    const culturasArray = propriedadeControl.get('culturas') as FormArray<FormGroup<CulturaFormControls>>;
    
    this.confirmationService.confirm({
      message: 'Tem certeza que deseja remover esta cultura?',
      header: 'Confirmar Remoção',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        culturasArray.removeAt(culturaIndex);
        this.emitChange();
      }
    });
  }

  /**
   * Get cultura form group
   */
  getCulturaFormGroup(propriedadeIndex: number, culturaIndex: number): FormGroup<CulturaFormControls> {
    const propriedadeControl = this.propriedadesFormArray.at(propriedadeIndex);
    const culturasArray = propriedadeControl.get('culturas') as FormArray<FormGroup<CulturaFormControls>>;
    return culturasArray.at(culturaIndex) as FormGroup<CulturaFormControls>;
  }

  /**
   * Get form control for validation display
   */
  getFormControl(propriedadeIndex: number, culturaIndex: number, controlName: string) {
    return this.getCulturaFormGroup(propriedadeIndex, culturaIndex).get(controlName);
  }

  /**
   * Check if field has error
   */
  hasFieldError(propriedadeIndex: number, culturaIndex: number, controlName: string, errorType?: string): boolean {
    const control = this.getFormControl(propriedadeIndex, culturaIndex, controlName);
    if (!control) return false;
    
    if (errorType) {
      return control.hasError(errorType) && (control.dirty || control.touched);
    }
    
    return control.invalid && (control.dirty || control.touched);
  }

  /**
   * Get field error message
   */
  getFieldErrorMessage(propriedadeIndex: number, culturaIndex: number, controlName: string): string {
    const control = this.getFormControl(propriedadeIndex, culturaIndex, controlName);
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
    this.culturasChange.emit();
    // Mark the propriedades form array as dirty to trigger change detection
    this.propriedadesFormArray.markAsDirty();
  }

  /**
   * Get cultura display name
   */
  getCulturaDisplayName(propriedadeIndex: number, culturaIndex: number): string {
    const cultura = this.getCulturaFormGroup(propriedadeIndex, culturaIndex).value;
    const tipoOption = this.tipoCulturaOptions.find(opt => opt.value === cultura.tipo);
    return `${tipoOption?.label || cultura.tipo} - Safra ${cultura.anoSafra}`;
  }

  /**
   * Get total area cultivada for a propriedade
   */
  getTotalAreaCultivada(propriedadeIndex: number): number {
    const propriedadeControl = this.propriedadesFormArray.at(propriedadeIndex);
    const culturasArray = propriedadeControl.get('culturas') as FormArray<FormGroup<CulturaFormControls>>;
    
    return culturasArray.controls.reduce((total, culturaControl) => {
      const areaCultivada = culturaControl.get('areaCultivada')?.value || 0;
      return total + areaCultivada;
    }, 0);
  }

  /**
   * Get remaining area for a propriedade
   */
  getRemainingArea(propriedadeIndex: number): number {
    const propriedade = this.propriedadesFormArray.at(propriedadeIndex).value;
    const totalArea = propriedade.area || 0;
    const totalCultivada = this.getTotalAreaCultivada(propriedadeIndex);
    return Math.max(0, totalArea - totalCultivada);
  }

  /**
   * Check if propriedade has area exceeded
   */
  hasAreaExceeded(propriedadeIndex: number): boolean {
    return this.getRemainingArea(propriedadeIndex) < 0;
  }

  /**
   * Get area utilization percentage
   */
  getAreaUtilization(propriedadeIndex: number): number {
    const propriedade = this.propriedadesFormArray.at(propriedadeIndex).value;
    const totalArea = propriedade.area || 0;
    if (totalArea === 0) return 0;
    
    const totalCultivada = this.getTotalAreaCultivada(propriedadeIndex);
    return Math.min(100, (totalCultivada / totalArea) * 100);
  }

  /**
   * Get cultura type option by value
   */
  getCulturaTypeOption(tipo: TipoCultura) {
    return this.tipoCulturaOptions.find(opt => opt.value === tipo);
  }

  /**
   * Format area display
   */
  formatArea(area: number): string {
    return area.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  /**
   * Get propriedade summary
   */
  getPropriedadeSummary(propriedade: PropriedadeWithCulturas): string {
    const culturasCount = propriedade.culturas.length;
    const totalCultivada = this.getTotalAreaCultivada(propriedade.index);
    const utilization = this.getAreaUtilization(propriedade.index);
    
    if (culturasCount === 0) {
      return 'Nenhuma cultura cadastrada';
    }
    
    return `${culturasCount} cultura(s) • ${this.formatArea(totalCultivada)} ha (${utilization.toFixed(1)}%)`;
  }

  /**
   * Handle accordion tab change
   */
  onAccordionTabChange(value: number | null): void {
    this.activeAccordionIndex.set(value);
  }

  /**
   * Validate area against propriedade limit
   */
  validateAreaLimit = (propriedadeIndex: number) => {
    return (control: any) => {
      if (!control.value) return null;
      
      const propriedade = this.propriedadesFormArray.at(propriedadeIndex).value;
      const totalArea = propriedade.area || 0;
      
      if (control.value > totalArea) {
        return { areaExceeded: { max: totalArea, actual: control.value } };
      }
      
      return null;
    };
  };
}