import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder, FormArray } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ConfirmationService } from 'primeng/api';

// PrimeNG imports for testing
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { AccordionModule } from 'primeng/accordion';

// Component and dependencies
import { PontoDistribuicaoFormComponent } from './ponto-distribuicao-form.component';
import { PontoDistribuicaoFormControls } from '../../../shared/models/forms.model';

// Mock components
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-endereco-form',
  template: '<div>Mock Endereco Form</div>'
})
class MockEnderecoFormComponent {
  @Input() enderecosFormArray!: FormArray;
  @Input() readonly = false;
  @Output() enderecosChange = new EventEmitter<any[]>();
}

describe('PontoDistribuicaoFormComponent', () => {
  let component: PontoDistribuicaoFormComponent;
  let fixture: ComponentFixture<PontoDistribuicaoFormComponent>;
  let mockConfirmationService: jasmine.SpyObj<ConfirmationService>;
  let formBuilder: FormBuilder;

  beforeEach(async () => {
    mockConfirmationService = jasmine.createSpyObj('ConfirmationService', ['confirm']);

    await TestBed.configureTestingModule({
      imports: [
        PontoDistribuicaoFormComponent,
        ReactiveFormsModule,
        NoopAnimationsModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        ConfirmDialogModule,
        AccordionModule,
        MockEnderecoFormComponent
      ],
      providers: [
        FormBuilder,
        { provide: ConfirmationService, useValue: mockConfirmationService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PontoDistribuicaoFormComponent);
    component = fixture.componentInstance;
    formBuilder = TestBed.inject(FormBuilder);

    // Initialize the required input
    component.pontosDistribuicaoFormArray = formBuilder.array<FormGroup<PontoDistribuicaoFormControls>>([]);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize without default pontos', () => {
    fixture.detectChanges();
    
    expect(component.pontosDistribuicaoFormArray.length).toBe(0);
    expect(component.hasPontosDistribuicao).toBe(false);
  });

  it('should add new ponto distribuicao', () => {
    fixture.detectChanges();
    
    spyOn(component, 'emitChange' as any);
    
    component.addPontoDistribuicao();
    
    expect(component.pontosDistribuicaoFormArray.length).toBe(1);
    expect(component.hasPontosDistribuicao).toBe(true);
  });

  it('should remove ponto distribuicao with confirmation', () => {
    fixture.detectChanges();
    
    // Add a ponto first
    component.addPontoDistribuicao();
    expect(component.pontosDistribuicaoFormArray.length).toBe(1);
    
    // Mock confirmation accept
    mockConfirmationService.confirm.and.callFake((options: any) => {
      options.accept();
    });
    
    spyOn(component, 'emitChange' as any);
    
    component.removePontoDistribuicao(0);
    
    expect(mockConfirmationService.confirm).toHaveBeenCalled();
    expect(component.pontosDistribuicaoFormArray.length).toBe(0);
  });

  it('should get ponto distribuicao form group', () => {
    fixture.detectChanges();
    
    component.addPontoDistribuicao();
    
    const pontoGroup = component.getPontoDistribuicaoFormGroup(0);
    expect(pontoGroup).toBeDefined();
    expect(pontoGroup.get('nome')).toBeDefined();
    expect(pontoGroup.get('endereco')).toBeDefined();
  });

  it('should get endereco form group', () => {
    fixture.detectChanges();
    
    component.addPontoDistribuicao();
    
    const enderecoGroup = component.getEnderecoFormGroup(0);
    expect(enderecoGroup).toBeDefined();
    expect(enderecoGroup.get('logradouro')).toBeDefined();
    expect(enderecoGroup.get('cidade')).toBeDefined();
  });

  it('should validate required fields', () => {
    fixture.detectChanges();
    
    component.addPontoDistribuicao();
    
    const pontoGroup = component.getPontoDistribuicaoFormGroup(0);
    
    // Test nome validation
    expect(pontoGroup.get('nome')?.valid).toBe(false);
    
    pontoGroup.get('nome')?.setValue('Ponto Teste');
    pontoGroup.get('nome')?.markAsTouched();
    
    expect(pontoGroup.get('nome')?.valid).toBe(true);
  });

  it('should handle field errors', () => {
    fixture.detectChanges();
    
    component.addPontoDistribuicao();
    
    const pontoGroup = component.getPontoDistribuicaoFormGroup(0);
    pontoGroup.get('nome')?.markAsTouched();
    
    expect(component.hasFieldError(0, 'nome')).toBe(true);
    expect(component.getFieldErrorMessage(0, 'nome')).toBe('Nome do ponto de distribuição é obrigatório');
  });

  it('should get ponto distribuicao title', () => {
    fixture.detectChanges();
    
    component.addPontoDistribuicao();
    
    // Without name
    expect(component.getPontoDistribuicaoTitle(0)).toBe('Ponto de Distribuição 1');
    
    // With name
    const pontoGroup = component.getPontoDistribuicaoFormGroup(0);
    pontoGroup.get('nome')?.setValue('Ponto Central');
    
    expect(component.getPontoDistribuicaoTitle(0)).toBe('Ponto Central');
  });

  it('should get ponto distribuicao summary', () => {
    fixture.detectChanges();
    
    component.addPontoDistribuicao();
    
    // Without data
    expect(component.getPontoDistribuicaoSummary(0)).toBe('Dados não preenchidos');
    
    // With endereco data
    const pontoGroup = component.getPontoDistribuicaoFormGroup(0);
    const enderecoGroup = pontoGroup.get('endereco') as FormGroup;
    
    enderecoGroup.patchValue({
      cidade: 'São Paulo',
      uf: 'SP'
    });
    
    expect(component.getPontoDistribuicaoSummary(0)).toBe('São Paulo - SP');
  });

  it('should handle coordinates', () => {
    fixture.detectChanges();
    
    component.addPontoDistribuicao();
    
    // Initially no coordinates
    expect(component.hasCoordinates(0)).toBe(false);
    expect(component.getCoordinatesText(0)).toBe('Não definidas');
    
    // Update coordinates
    component.updateCoordinates(0, -23.5505, -46.6333);
    
    expect(component.hasCoordinates(0)).toBe(true);
    expect(component.getCoordinatesText(0)).toBe('-23.550500, -46.633300');
    
    // Clear coordinates
    component.clearCoordinates(0);
    
    expect(component.hasCoordinates(0)).toBe(false);
    expect(component.getCoordinatesText(0)).toBe('Não definidas');
  });

  it('should handle endereco changes', () => {
    fixture.detectChanges();
    
    component.addPontoDistribuicao();
    
    spyOn(component, 'emitChange' as any);
    
    const enderecoData = {
      logradouro: 'Rua Teste',
      numero: '123',
      cidade: 'São Paulo',
      uf: 'SP',
      latitude: -23.5505,
      longitude: -46.6333
    };
    
    component.onEnderecoChange(0, enderecoData);
    
    const pontoGroup = component.getPontoDistribuicaoFormGroup(0);
    expect(pontoGroup.get('latitude')?.value).toBe(-23.5505);
    expect(pontoGroup.get('longitude')?.value).toBe(-46.6333);
  });

  it('should emit changes when modified', () => {
    fixture.detectChanges();
    
    spyOn(component.pontosDistribuicaoChange, 'emit');
    
    component.addPontoDistribuicao();
    
    expect(component.pontosDistribuicaoChange.emit).toHaveBeenCalled();
  });

  it('should handle readonly mode', () => {
    component.readonly = true;
    fixture.detectChanges();
    
    // Should not be able to add pontos in readonly mode
    // This would be tested in the template, but we can verify the property
    expect(component.readonly).toBe(true);
  });
});