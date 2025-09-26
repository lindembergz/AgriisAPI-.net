import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MessageService } from 'primeng/api';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { CulturasDetailComponent } from './culturas-detail.component';
import { CulturaService } from '../../services/cultura.service';
import { CulturaDto, CriarCulturaDto, AtualizarCulturaDto } from '../../models';
import { environment } from '../../../../../environments/environment';

describe('CulturasDetailComponent Integration', () => {
  let component: CulturasDetailComponent;
  let fixture: ComponentFixture<CulturasDetailComponent>;
  let httpMock: HttpTestingController;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: any;
  let mockMessageService: jasmine.SpyObj<MessageService>;

  const mockCultura: CulturaDto = {
    id: 1,
    nome: 'Soja',
    descricao: 'Cultura de soja para grãos',
    ativo: true,
    dataCriacao: new Date('2024-01-01'),
    dataAtualizacao: new Date('2024-01-02')
  };

  beforeEach(async () => {
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
        ReactiveFormsModule,
        HttpClientTestingModule,
        NoopAnimationsModule
      ],
      providers: [
        CulturaService,
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: MessageService, useValue: messageServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CulturasDetailComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    mockMessageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Create Mode Integration', () => {
    beforeEach(() => {
      mockActivatedRoute.snapshot.paramMap.get.and.returnValue(null);
      fixture.detectChanges();
    });

    it('should create new cultura via API', () => {
      const criarDto: CriarCulturaDto = {
        nome: 'Milho',
        descricao: 'Cultura de milho'
      };

      component.culturaForm.patchValue({
        nome: criarDto.nome,
        descricao: criarDto.descricao
      });

      component.onSave();

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(criarDto);

      req.flush(mockCultura);

      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'success',
        summary: 'Sucesso',
        detail: 'Cultura criada com sucesso'
      });
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/culturas']);
    });

    it('should handle API error during creation', () => {
      component.culturaForm.patchValue({
        nome: 'Cultura Teste'
      });

      component.onSave();

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
      req.flush(
        { error_description: 'Nome já existe' },
        { status: 400, statusText: 'Bad Request' }
      );

      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Nome já existe',
        life: 5000
      });
    });
  });

  describe('Edit Mode Integration', () => {
    beforeEach(() => {
      mockActivatedRoute.snapshot.paramMap.get.and.returnValue('1');
      fixture.detectChanges();

      // Mock the initial load request
      const loadReq = httpMock.expectOne(`${environment.apiUrl}/culturas/1`);
      loadReq.flush(mockCultura);
    });

    it('should load cultura data and update via API', () => {
      const atualizarDto: AtualizarCulturaDto = {
        nome: 'Soja Atualizada',
        descricao: 'Nova descrição',
        ativo: false
      };

      component.culturaForm.patchValue({
        nome: atualizarDto.nome,
        descricao: atualizarDto.descricao,
        ativo: atualizarDto.ativo
      });

      component.onSave();

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas/1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(atualizarDto);

      req.flush({ ...mockCultura, ...atualizarDto });

      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'success',
        summary: 'Sucesso',
        detail: 'Cultura atualizada com sucesso'
      });
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/culturas']);
    });

    it('should handle API error during update', () => {
      component.culturaForm.patchValue({
        nome: 'Nome Inválido',
        ativo: true
      });

      component.onSave();

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas/1`);
      req.flush(
        { error_description: 'Dados inválidos' },
        { status: 422, statusText: 'Unprocessable Entity' }
      );

      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Dados inválidos',
        life: 5000
      });
    });

    it('should handle load error and redirect', () => {
      // Reset component for this test
      TestBed.resetTestingModule();
      
      const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
      const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);
      
      const mockRoute = {
        snapshot: {
          paramMap: {
            get: jasmine.createSpy('get').and.returnValue('999')
          }
        }
      };

      TestBed.configureTestingModule({
        imports: [
          CulturasDetailComponent,
          ReactiveFormsModule,
          HttpClientTestingModule,
          NoopAnimationsModule
        ],
        providers: [
          CulturaService,
          { provide: Router, useValue: routerSpy },
          { provide: ActivatedRoute, useValue: mockRoute },
          { provide: MessageService, useValue: messageServiceSpy }
        ]
      });

      const newFixture = TestBed.createComponent(CulturasDetailComponent);
      const newComponent = newFixture.componentInstance;
      const newHttpMock = TestBed.inject(HttpTestingController);
      const newRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;

      newFixture.detectChanges();

      const req = newHttpMock.expectOne(`${environment.apiUrl}/culturas/999`);
      req.flush(
        { error_description: 'Cultura não encontrada' },
        { status: 404, statusText: 'Not Found' }
      );

      expect(newRouter.navigate).toHaveBeenCalledWith(['/culturas']);
      
      newHttpMock.verify();
    });
  });

  describe('Form Validation Integration', () => {
    beforeEach(() => {
      mockActivatedRoute.snapshot.paramMap.get.and.returnValue(null);
      fixture.detectChanges();
    });

    it('should prevent submission with invalid form', () => {
      // Leave nome empty (required field)
      component.culturaForm.patchValue({
        nome: '',
        descricao: 'Test description'
      });

      component.onSave();

      // No HTTP request should be made
      httpMock.expectNone(`${environment.apiUrl}/culturas`);
      
      expect(component.culturaForm.invalid).toBeTruthy();
      expect(component.hasFieldError('nome')).toBeTruthy();
    });

    it('should show validation messages', () => {
      const nomeControl = component.culturaForm.get('nome');
      nomeControl?.setValue('');
      nomeControl?.markAsTouched();

      expect(component.hasFieldError('nome')).toBeTruthy();
      expect(component.getFieldErrorMessage('nome')).toBe('Nome é obrigatório');
    });
  });

  describe('Navigation Integration', () => {
    beforeEach(() => {
      mockActivatedRoute.snapshot.paramMap.get.and.returnValue(null);
      fixture.detectChanges();
    });

    it('should navigate back on cancel', () => {
      component.onCancel();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/culturas']);
    });
  });

  describe('Component State Integration', () => {
    it('should show correct titles and button text for create mode', () => {
      mockActivatedRoute.snapshot.paramMap.get.and.returnValue(null);
      fixture.detectChanges();

      expect(component.pageTitle).toBe('Nova Cultura');
      expect(component.saveButtonText).toBe('Salvar');
      expect(component.isEditMode()).toBeFalse();
    });

    it('should show correct titles and button text for edit mode', () => {
      mockActivatedRoute.snapshot.paramMap.get.and.returnValue('1');
      fixture.detectChanges();

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas/1`);
      req.flush(mockCultura);

      expect(component.pageTitle).toBe('Editar Cultura');
      expect(component.saveButtonText).toBe('Atualizar');
      expect(component.isEditMode()).toBeTruthy();
    });
  });
});