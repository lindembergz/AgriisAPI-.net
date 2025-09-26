import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MessageService, ConfirmationService } from 'primeng/api';
import { of } from 'rxjs';

import { UfsComponent } from './ufs.component';
import { UfService } from './services/uf.service';
import { PaisService } from '../paises/services/pais.service';
import { UfDto, PaisDto } from '../../../shared/models/reference.model';

describe('UfsComponent', () => {
  let component: UfsComponent;
  let fixture: ComponentFixture<UfsComponent>;
  let ufService: jasmine.SpyObj<UfService>;
  let paisService: jasmine.SpyObj<PaisService>;

  const mockPaises: PaisDto[] = [
    { id: 1, codigo: 'BR', nome: 'Brasil', ativo: true, dataCriacao: new Date(), dataAtualizacao: null }
  ];

  const mockUfs: UfDto[] = [
    { 
      id: 1, 
      codigo: 'SP', 
      nome: 'São Paulo', 
      paisId: 1, 
      pais: mockPaises[0],
      ativo: true, 
      dataCriacao: new Date(), 
      dataAtualizacao: null 
    },
    { 
      id: 2, 
      codigo: 'RJ', 
      nome: 'Rio de Janeiro', 
      paisId: 1, 
      pais: mockPaises[0],
      ativo: true, 
      dataCriacao: new Date(), 
      dataAtualizacao: null 
    }
  ];

  beforeEach(async () => {
    const ufServiceSpy = jasmine.createSpyObj('UfService', [
      'obterTodos',
      'obterAtivos',
      'obterComPais',
      'obterPorId',
      'criar',
      'atualizar',
      'ativar',
      'desativar',
      'remover',
      'podeRemover',
      'obterContagemMunicipios',
      'verificarDependenciasMunicipio',
      'validarCodigoUnico'
    ]);

    const paisServiceSpy = jasmine.createSpyObj('PaisService', [
      'obterAtivos'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        UfsComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: UfService, useValue: ufServiceSpy },
        { provide: PaisService, useValue: paisServiceSpy },
        MessageService,
        ConfirmationService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UfsComponent);
    component = fixture.componentInstance;
    ufService = TestBed.inject(UfService) as jasmine.SpyObj<UfService>;
    paisService = TestBed.inject(PaisService) as jasmine.SpyObj<PaisService>;

    // Setup default spy returns
    ufService.obterComPais.and.returnValue(of(mockUfs));
    ufService.obterAtivos.and.returnValue(of(mockUfs));
    ufService.obterContagemMunicipios.and.returnValue(of(5));
    paisService.obterAtivos.and.returnValue(of(mockPaises));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load países on init', () => {
    component.ngOnInit();
    
    expect(paisService.obterAtivos).toHaveBeenCalled();
    expect(component.paisesOptions()).toEqual(mockPaises);
  });

  it('should load UFs with país information', () => {
    component.carregarItens();
    
    expect(ufService.obterComPais).toHaveBeenCalled();
    expect(component.items()).toEqual(mockUfs);
  });

  it('should create form with required validators', () => {
    const form = component['createFormGroup']();
    
    expect(form.get('codigo')?.hasError('required')).toBeTruthy();
    expect(form.get('nome')?.hasError('required')).toBeTruthy();
    expect(form.get('paisId')?.hasError('required')).toBeTruthy();
  });

  it('should validate código format', () => {
    const form = component['createFormGroup']();
    const codigoControl = form.get('codigo');
    
    codigoControl?.setValue('SP');
    expect(codigoControl?.valid).toBeTruthy();
    
    codigoControl?.setValue('SPP');
    expect(codigoControl?.hasError('pattern')).toBeTruthy();
    
    codigoControl?.setValue('sp');
    expect(codigoControl?.hasError('pattern')).toBeTruthy();
  });

  it('should map form to create DTO correctly', () => {
    const formValue = {
      codigo: 'sp',
      nome: '  São Paulo  ',
      paisId: '1'
    };
    
    const dto = component['mapToCreateDto'](formValue);
    
    expect(dto).toEqual({
      codigo: 'SP',
      nome: 'São Paulo',
      paisId: 1
    });
  });

  it('should map form to update DTO correctly', () => {
    const formValue = {
      nome: '  São Paulo Atualizado  ',
      ativo: false
    };
    
    const dto = component['mapToUpdateDto'](formValue);
    
    expect(dto).toEqual({
      nome: 'São Paulo Atualizado',
      ativo: false
    });
  });

  it('should populate form correctly', () => {
    const uf = mockUfs[0];
    component['populateForm'](uf);
    
    expect(component.form.get('codigo')?.value).toBe('SP');
    expect(component.form.get('nome')?.value).toBe('São Paulo');
    expect(component.form.get('paisId')?.value).toBe(1);
    expect(component.form.get('ativo')?.value).toBe(true);
  });

  it('should get municípios count display correctly', () => {
    expect(component.getMunicipiosCountDisplay({ municipiosCount: 0 })).toBe('Nenhum');
    expect(component.getMunicipiosCountDisplay({ municipiosCount: 5 })).toBe('5');
    expect(component.getMunicipiosCountDisplay({})).toBe('Nenhum');
  });

  it('should get municípios count severity correctly', () => {
    expect(component.getMunicipiosCountSeverity(0)).toBe('secondary');
    expect(component.getMunicipiosCountSeverity(5)).toBe('info');
    expect(component.getMunicipiosCountSeverity(25)).toBe('success');
    expect(component.getMunicipiosCountSeverity(75)).toBe('warning');
    expect(component.getMunicipiosCountSeverity(150)).toBe('danger');
  });

  it('should get municípios tooltip correctly', () => {
    expect(component.getMunicipiosTooltip(0)).toBe('Nenhum município cadastrado');
    expect(component.getMunicipiosTooltip(1)).toBe('1 município cadastrado');
    expect(component.getMunicipiosTooltip(5)).toBe('5 municípios cadastrados');
  });

  it('should check dependencies before deletion', () => {
    const uf = mockUfs[0];
    ufService.verificarDependenciasMunicipio.and.returnValue(of(true));
    
    component.excluirItem(uf);
    
    expect(ufService.verificarDependenciasMunicipio).toHaveBeenCalledWith(uf.id);
  });

  it('should prevent deletion when UF has municípios', () => {
    const uf = mockUfs[0];
    ufService.verificarDependenciasMunicipio.and.returnValue(of(true));
    spyOn(component['messageService'], 'add');
    
    component.excluirItem(uf);
    
    expect(component['messageService'].add).toHaveBeenCalledWith({
      severity: 'warn',
      summary: 'Não é possível excluir',
      detail: 'Esta UF possui municípios cadastrados. Remova os municípios primeiro.',
      life: 5000
    });
  });

  it('should allow deletion when UF has no municípios', () => {
    const uf = mockUfs[0];
    ufService.verificarDependenciasMunicipio.and.returnValue(of(false));
    spyOn(component, 'excluirItem').and.callThrough();
    
    // Mock the parent method
    spyOn(Object.getPrototypeOf(Object.getPrototypeOf(component)), 'excluirItem');
    
    component.excluirItem(uf);
    
    expect(ufService.verificarDependenciasMunicipio).toHaveBeenCalledWith(uf.id);
  });

  it('should validate código uniqueness', () => {
    ufService.validarCodigoUnico.and.returnValue(of(true));
    
    const form = component['createFormGroup']();
    form.get('paisId')?.setValue(1);
    form.get('codigo')?.setValue('SP');
    
    // Trigger async validation
    form.get('codigo')?.updateValueAndValidity();
    
    expect(ufService.validarCodigoUnico).toHaveBeenCalledWith('SP', 1, undefined);
  });

  it('should handle entity display configuration', () => {
    expect(component['entityDisplayName']()).toBe('UF');
    expect(component['entityDescription']()).toBe('Gerenciar Unidades Federativas (Estados)');
    expect(component['defaultSortField']()).toBe('codigo');
    expect(component['searchFields']()).toEqual(['codigo', 'nome', 'pais.nome']);
  });

  it('should configure display columns correctly', () => {
    const columns = component['displayColumns']();
    
    expect(columns).toEqual([
      { field: 'codigo', header: 'Código', sortable: true, width: '100px' },
      { field: 'nome', header: 'Nome', sortable: true, width: '250px' },
      { field: 'pais', header: 'País', sortable: true, width: '200px', type: 'custom', hideOnMobile: true },
      { field: 'municipiosCount', header: 'Municípios', sortable: true, width: '120px', type: 'custom', hideOnMobile: true },
      { field: 'ativo', header: 'Status', sortable: true, width: '100px', type: 'boolean', hideOnMobile: true },
      { field: 'dataCriacao', header: 'Criado em', sortable: true, width: '150px', type: 'date', hideOnMobile: true, hideOnTablet: true }
    ]);
  });
});