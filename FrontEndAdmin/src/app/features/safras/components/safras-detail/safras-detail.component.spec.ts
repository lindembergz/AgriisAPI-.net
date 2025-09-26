import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { MessageService } from 'primeng/api';
import { of, throwError } from 'rxjs';

import { SafrasDetailComponent } from './safras-detail.component';
import { SafraService } from '../../services/safra.service';
import { SafraDto } from '../../models';

describe('SafrasDetailComponent', () => {
  let component: SafrasDetailComponent;
  let fixture: ComponentFixture<SafrasDetailComponent>;
  let mockSafraService: jasmine.SpyObj<SafraService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: any;
  let mockMessageService: jasmine.SpyObj<MessageService>;

  const mockSafra: SafraDto = {
    id: 1,
    plantioInicial: new Date('2024-09-01'),
    plantioFinal: new Date('2025-02-28'),
    plantioNome: 'Safra 2024/2025',
    descricao: 'Safra de soja e milho',
    anoColheita: 2025,
    safraFormatada: '2024/2025 Safra de soja e milho',
    atual: true,
    dataCriacao: new Date('2024-01-01'),
    dataAtualizacao: new Date('2024-01-15')
  };

  beforeEach(async () => {
    const safraServiceSpy = jasmine.createSpyObj('SafraService', [
      'obterPorId', 'criar', 'atualizar'
    ]);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);

    mockActivatedRoute = {
      snapshot: {
        paramMap: {
          get: jasmine.createSpy('get').and.returnValue(null)
        }
      }
    };

    await TestBed.configureTestingModule({
      imports: [
        SafrasDetailComponent,
        ReactiveFormsModule
      ],
      providers: [
        { provide: SafraService, useValue: safraServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: MessageService, useValue: messageServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SafrasDetailComponent);
    component = fixture.componentInstance;
    mockSafraService = TestBed.inject(SafraService) as jasmine.SpyObj<SafraService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    mockMessageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with default values in create mode', () => {
    mockActivatedRoute.snapshot.paramMap.get.and.returnValue(null);
    
    component.ngOnInit();
    
    expect(component.isEditMode()).toBeFalse();
    expect(component.safraForm.get('plantioNome')?.value).toBe('');
    expect(component.safraForm.get('descricao')?.value).toBe('');
  });

  it('should load safra data in edit mode', () => {
    mockActivatedRoute.snapshot.paramMap.get.and.returnValue('1');
    mockSafraService.obterPorId.and.returnValue(of(mockSafra));
    
    component.ngOnInit();
    
    expect(component.isEditMode()).toBeTrue();
    expect(mockSafraService.obterPorId).toHaveBeenCalledWith(1);
  });

  it('should validate required fields', () => {
    component.ngOnInit();
    
    // Test empty form
    expect(component.safraForm.invalid).toBeTrue();
    
    // Fill required fields
    component.safraForm.patchValue({
      plantioInicial: new Date('2024-09-01'),
      plantioFinal: new Date('2025-02-28'),
      plantioNome: 'Test Safra',
      descricao: 'Test Description'
    });
    
    expect(component.safraForm.valid).toBeTrue();
  });

  it('should validate date range (plantioFinal > plantioInicial)', () => {
    component.ngOnInit();
    
    // Set invalid date range
    component.safraForm.patchValue({
      plantioInicial: new Date('2025-02-28'),
      plantioFinal: new Date('2024-09-01'),
      plantioNome: 'Test Safra',
      descricao: 'Test Description'
    });
    
    expect(component.safraForm.invalid).toBeTrue();
    expect(component.safraForm.get('plantioFinal')?.errors?.['dateRange']).toBeTrue();
  });

  it('should validate minimum date (1900)', () => {
    component.ngOnInit();
    
    // Set date before 1900
    component.safraForm.patchValue({
      plantioInicial: new Date('1899-12-31'),
      plantioFinal: new Date('2024-02-28'),
      plantioNome: 'Test Safra',
      descricao: 'Test Description'
    });
    
    expect(component.safraForm.get('plantioInicial')?.errors?.['min']).toBeTrue();
  });

  it('should validate maximum length for plantioNome', () => {
    component.ngOnInit();
    
    const longName = 'a'.repeat(257); // 257 characters
    component.safraForm.patchValue({
      plantioNome: longName
    });
    
    expect(component.safraForm.get('plantioNome')?.errors?.['maxlength']).toBeTrue();
  });

  it('should validate maximum length for descricao', () => {
    component.ngOnInit();
    
    const longDescription = 'a'.repeat(65); // 65 characters
    component.safraForm.patchValue({
      descricao: longDescription
    });
    
    expect(component.safraForm.get('descricao')?.errors?.['maxlength']).toBeTrue();
  });

  it('should create safra when form is valid', () => {
    mockSafraService.criar.and.returnValue(of(mockSafra));
    component.ngOnInit();
    
    component.safraForm.patchValue({
      plantioInicial: new Date('2024-09-01'),
      plantioFinal: new Date('2025-02-28'),
      plantioNome: 'Test Safra',
      descricao: 'Test Description'
    });
    
    component.onSave();
    
    expect(mockSafraService.criar).toHaveBeenCalled();
    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Safra criada com sucesso'
    });
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/safras']);
  });

  it('should update safra when in edit mode', () => {
    mockActivatedRoute.snapshot.paramMap.get.and.returnValue('1');
    mockSafraService.obterPorId.and.returnValue(of(mockSafra));
    mockSafraService.atualizar.and.returnValue(of(mockSafra));
    
    component.ngOnInit();
    component.onSave();
    
    expect(mockSafraService.atualizar).toHaveBeenCalledWith(1, jasmine.any(Object));
    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Safra atualizada com sucesso'
    });
  });

  it('should handle save errors', () => {
    mockSafraService.criar.and.returnValue(throwError(() => new Error('API Error')));
    component.ngOnInit();
    
    component.safraForm.patchValue({
      plantioInicial: new Date('2024-09-01'),
      plantioFinal: new Date('2025-02-28'),
      plantioNome: 'Test Safra',
      descricao: 'Test Description'
    });
    
    component.onSave();
    
    expect(component.saving()).toBeFalse();
  });

  it('should navigate back on cancel', () => {
    component.onCancel();
    
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/safras']);
  });

  it('should return correct page title', () => {
    // Create mode
    component.isEditMode.set(false);
    expect(component.pageTitle).toBe('Nova Safra');
    
    // Edit mode
    component.isEditMode.set(true);
    expect(component.pageTitle).toBe('Editar Safra');
  });

  it('should return correct save button text', () => {
    // Create mode
    component.isEditMode.set(false);
    expect(component.saveButtonText).toBe('Salvar');
    
    // Edit mode
    component.isEditMode.set(true);
    expect(component.saveButtonText).toBe('Atualizar');
  });

  it('should detect field errors correctly', () => {
    component.ngOnInit();
    
    const plantioNomeControl = component.safraForm.get('plantioNome');
    plantioNomeControl?.markAsTouched();
    
    expect(component.hasFieldError('plantioNome')).toBeTrue();
    
    plantioNomeControl?.setValue('Valid Name');
    expect(component.hasFieldError('plantioNome')).toBeFalse();
  });

  it('should return appropriate error messages', () => {
    component.ngOnInit();
    
    const plantioNomeControl = component.safraForm.get('plantioNome');
    plantioNomeControl?.markAsTouched();
    
    expect(component.getFieldErrorMessage('plantioNome')).toBe('Nome do plantio é obrigatório');
  });
});