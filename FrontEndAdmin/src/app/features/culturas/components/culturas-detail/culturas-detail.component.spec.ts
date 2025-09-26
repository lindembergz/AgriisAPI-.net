import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MessageService } from 'primeng/api';
import { of, throwError } from 'rxjs';

import { CulturasDetailComponent } from './culturas-detail.component';
import { CulturaService } from '../../services/cultura.service';
import { CulturaDto, CriarCulturaDto, AtualizarCulturaDto } from '../../models';

describe('CulturasDetailComponent', () => {
  let component: CulturasDetailComponent;
  let fixture: ComponentFixture<CulturasDetailComponent>;
  let mockCulturaService: jasmine.SpyObj<CulturaService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: any;
  let mockMessageService: jasmine.SpyObj<MessageService>;

  const mockCultura: CulturaDto = {
    id: 1,
    nome: 'Soja',
    descricao: 'Cultura de soja',
    ativo: true,
    dataCriacao: new Date('2024-01-01'),
    dataAtualizacao: new Date('2024-01-02')
  };

  beforeEach(async () => {
    const culturaServiceSpy = jasmine.createSpyObj('CulturaService', [
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
        CulturasDetailComponent,
        ReactiveFormsModule
      ],
      providers: [
        { provide: CulturaService, useValue: culturaServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: MessageService, useValue: messageServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CulturasDetailComponent);
    component = fixture.componentInstance;
    mockCulturaService = TestBed.inject(CulturaService) as jasmine.SpyObj<CulturaService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    mockMessageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with default values in create mode', () => {
    mockActivatedRoute.snapshot.paramMap.get.and.returnValue(null);
    
    fixture.detectChanges();
    
    expect(component.isEditMode()).toBeFalse();
    expect(component.culturaForm.get('nome')?.value).toBe('');
    expect(component.culturaForm.get('descricao')?.value).toBeNull();
    expect(component.culturaForm.get('ativo')?.value).toBe(true);
  });

  it('should load cultura data in edit mode', () => {
    mockActivatedRoute.snapshot.paramMap.get.and.returnValue('1');
    mockCulturaService.obterPorId.and.returnValue(of(mockCultura));
    
    fixture.detectChanges();
    
    expect(component.isEditMode()).toBeTrue();
    expect(component.culturaId()).toBe(1);
    expect(mockCulturaService.obterPorId).toHaveBeenCalledWith(1);
  });

  it('should validate required nome field', () => {
    fixture.detectChanges();
    
    const nomeControl = component.culturaForm.get('nome');
    nomeControl?.setValue('');
    nomeControl?.markAsTouched();
    
    expect(component.hasFieldError('nome')).toBeTruthy();
    expect(component.getFieldErrorMessage('nome')).toBe('Nome é obrigatório');
  });

  it('should create cultura successfully', () => {
    const criarDto: CriarCulturaDto = {
      nome: 'Nova Cultura',
      descricao: 'Descrição da cultura'
    };
    
    mockCulturaService.criar.and.returnValue(of(mockCultura));
    fixture.detectChanges();
    
    component.culturaForm.patchValue({
      nome: criarDto.nome,
      descricao: criarDto.descricao
    });
    
    component.onSave();
    
    expect(mockCulturaService.criar).toHaveBeenCalledWith(criarDto);
    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Cultura criada com sucesso'
    });
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/culturas']);
  });

  it('should update cultura successfully', () => {
    const atualizarDto: AtualizarCulturaDto = {
      nome: 'Cultura Atualizada',
      descricao: 'Nova descrição',
      ativo: false
    };
    
    mockActivatedRoute.snapshot.paramMap.get.and.returnValue('1');
    mockCulturaService.obterPorId.and.returnValue(of(mockCultura));
    mockCulturaService.atualizar.and.returnValue(of(mockCultura));
    
    fixture.detectChanges();
    
    component.culturaForm.patchValue({
      nome: atualizarDto.nome,
      descricao: atualizarDto.descricao,
      ativo: atualizarDto.ativo
    });
    
    component.onSave();
    
    expect(mockCulturaService.atualizar).toHaveBeenCalledWith(1, atualizarDto);
    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Cultura atualizada com sucesso'
    });
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/culturas']);
  });

  it('should handle cancel action', () => {
    fixture.detectChanges();
    
    component.onCancel();
    
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/culturas']);
  });

  it('should handle invalid route parameter', () => {
    mockActivatedRoute.snapshot.paramMap.get.and.returnValue('invalid');
    
    fixture.detectChanges();
    
    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'error',
      summary: 'Erro',
      detail: 'ID da cultura inválido'
    });
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/culturas']);
  });

  it('should handle load cultura error', () => {
    mockActivatedRoute.snapshot.paramMap.get.and.returnValue('1');
    mockCulturaService.obterPorId.and.returnValue(throwError(() => new Error('API Error')));
    
    fixture.detectChanges();
    
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/culturas']);
  });

  it('should not submit form when invalid', () => {
    fixture.detectChanges();
    
    // Leave nome empty (required field)
    component.culturaForm.patchValue({
      nome: '',
      descricao: 'Test'
    });
    
    component.onSave();
    
    expect(mockCulturaService.criar).not.toHaveBeenCalled();
    expect(mockCulturaService.atualizar).not.toHaveBeenCalled();
  });

  it('should return correct page title', () => {
    // Create mode
    fixture.detectChanges();
    expect(component.pageTitle).toBe('Nova Cultura');
    
    // Edit mode
    mockActivatedRoute.snapshot.paramMap.get.and.returnValue('1');
    mockCulturaService.obterPorId.and.returnValue(of(mockCultura));
    component.ngOnInit();
    expect(component.pageTitle).toBe('Editar Cultura');
  });

  it('should return correct save button text', () => {
    // Create mode
    fixture.detectChanges();
    expect(component.saveButtonText).toBe('Salvar');
    
    // Edit mode
    mockActivatedRoute.snapshot.paramMap.get.and.returnValue('1');
    mockCulturaService.obterPorId.and.returnValue(of(mockCultura));
    component.ngOnInit();
    expect(component.saveButtonText).toBe('Atualizar');
  });
});