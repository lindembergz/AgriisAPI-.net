import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MessageService } from 'primeng/api';
import { of, throwError } from 'rxjs';

import { UnidadesMedidaComponent } from './unidades-medida.component';
import { UnidadeMedidaService } from './services/unidade-medida.service';
import { FieldValidatorsUtil } from '../../../shared/utils/field-validators.util';
import { UnidadeMedidaDto, TipoUnidadeMedida } from '../../../shared/models/reference.model';

describe('UnidadesMedidaComponent', () => {
  let component: UnidadesMedidaComponent;
  let fixture: ComponentFixture<UnidadesMedidaComponent>;
  let mockUnidadeMedidaService: jasmine.SpyObj<UnidadeMedidaService>;
  let mockMessageService: jasmine.SpyObj<MessageService>;

  const mockUnidadeMedida: UnidadeMedidaDto = {
    id: 1,
    simbolo: 'kg',
    nome: 'Quilograma',
    tipo: TipoUnidadeMedida.Peso,
    fatorConversao: 1,
    ativo: true,
    dataCriacao: new Date(),
    dataAtualizacao: null
  };

  const mockTipos = [
    { valor: 1, nome: 'Peso', descricao: 'Peso' },
    { valor: 2, nome: 'Volume', descricao: 'Volume' },
    { valor: 3, nome: 'Area', descricao: 'Área' },
    { valor: 4, nome: 'Unidade', descricao: 'Unidade' }
  ];

  const mockUnidadesDropdown = [
    { id: 1, simbolo: 'kg', nome: 'Quilograma' },
    { id: 2, simbolo: 'g', nome: 'Grama' }
  ];

  beforeEach(async () => {
    const unidadeMedidaServiceSpy = jasmine.createSpyObj('UnidadeMedidaService', [
      'obterTodos',
      'obterAtivos',
      'obterPorId',
      'criar',
      'atualizar',
      'remover',
      'ativar',
      'desativar',
      'obterTipos',
      'obterParaDropdown',
      'obterPorTipo',
      'converter',
      'verificarSimboloUnico',
      'verificarNomeUnico',
      'getTipoDescricao'
    ]);

    const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);

    await TestBed.configureTestingModule({
      imports: [
        UnidadesMedidaComponent,
        ReactiveFormsModule,
        FormsModule,
        HttpClientTestingModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: UnidadeMedidaService, useValue: unidadeMedidaServiceSpy },
        { provide: MessageService, useValue: messageServiceSpy },
        FieldValidatorsUtil
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UnidadesMedidaComponent);
    component = fixture.componentInstance;
    mockUnidadeMedidaService = TestBed.inject(UnidadeMedidaService) as jasmine.SpyObj<UnidadeMedidaService>;
    mockMessageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;

    // Setup default mock returns
    mockUnidadeMedidaService.obterTodos.and.returnValue(of({ items: [mockUnidadeMedida], total: 1 }));
    mockUnidadeMedidaService.obterAtivos.and.returnValue(of([mockUnidadeMedida]));
    mockUnidadeMedidaService.obterTipos.and.returnValue(of(mockTipos));
    mockUnidadeMedidaService.obterParaDropdown.and.returnValue(of(mockUnidadesDropdown));
    mockUnidadeMedidaService.getTipoDescricao.and.returnValue('Peso');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with correct entity configuration', () => {
    expect(component['entityDisplayName']()).toBe('Unidade de Medida');
    expect(component['entityDescription']()).toBe('Gerenciar unidades de medida utilizadas no sistema');
    expect(component['defaultSortField']()).toBe('simbolo');
    expect(component['searchFields']()).toEqual(['simbolo', 'nome']);
  });

  it('should load tipos on init', () => {
    component.ngOnInit();
    
    expect(mockUnidadeMedidaService.obterTipos).toHaveBeenCalled();
    expect(component.tiposDisponiveis).toEqual(mockTipos);
  });

  it('should load unidades for conversion on init', () => {
    component.ngOnInit();
    
    expect(mockUnidadeMedidaService.obterParaDropdown).toHaveBeenCalled();
    expect(component.unidadesParaConversao).toEqual(mockUnidadesDropdown);
  });

  it('should create form with correct validators', () => {
    const form = component['createFormGroup']();
    
    expect(form.get('simbolo')?.hasError('required')).toBeTruthy();
    expect(form.get('nome')?.hasError('required')).toBeTruthy();
    expect(form.get('tipo')?.hasError('required')).toBeTruthy();
    
    // Test valid values
    form.patchValue({
      simbolo: 'kg',
      nome: 'Quilograma',
      tipo: TipoUnidadeMedida.Peso,
      fatorConversao: 1
    });
    
    expect(form.valid).toBeTruthy();
  });

  it('should map form to create DTO correctly', () => {
    const formValue = {
      simbolo: ' kg ',
      nome: ' Quilograma ',
      tipo: TipoUnidadeMedida.Peso,
      fatorConversao: 1.5
    };
    
    const dto = component['mapToCreateDto'](formValue);
    
    expect(dto).toEqual({
      simbolo: 'kg',
      nome: 'Quilograma',
      tipo: TipoUnidadeMedida.Peso,
      fatorConversao: 1.5
    });
  });

  it('should map form to update DTO correctly', () => {
    const formValue = {
      nome: ' Quilograma Atualizado ',
      tipo: TipoUnidadeMedida.Volume,
      fatorConversao: 2,
      ativo: false
    };
    
    const dto = component['mapToUpdateDto'](formValue);
    
    expect(dto).toEqual({
      nome: 'Quilograma Atualizado',
      tipo: TipoUnidadeMedida.Volume,
      fatorConversao: 2,
      ativo: false
    });
  });

  it('should populate form correctly', () => {
    component['createFormGroup']();
    component['populateForm'](mockUnidadeMedida);
    
    expect(component.form.get('simbolo')?.value).toBe('kg');
    expect(component.form.get('nome')?.value).toBe('Quilograma');
    expect(component.form.get('tipo')?.value).toBe(TipoUnidadeMedida.Peso);
    expect(component.form.get('fatorConversao')?.value).toBe(1);
    expect(component.form.get('ativo')?.value).toBe(true);
  });

  describe('Type Filtering', () => {
    beforeEach(() => {
      component.items = [mockUnidadeMedida];
      component.filteredItems = [mockUnidadeMedida];
    });

    it('should filter by type when type is selected', () => {
      component.tipoSelecionado = TipoUnidadeMedida.Peso;
      component.onTipoFilterChange();
      
      expect(component.filteredItems).toEqual([mockUnidadeMedida]);
    });

    it('should clear filter when no type is selected', () => {
      component.tipoSelecionado = null;
      component.onTipoFilterChange();
      
      expect(component.filteredItems).toEqual([mockUnidadeMedida]);
    });
  });

  describe('Conversion Calculator', () => {
    beforeEach(() => {
      component.unidadesParaConversao = mockUnidadesDropdown;
    });

    it('should toggle conversion calculator visibility', () => {
      expect(component.showConversionCalculator).toBeFalsy();
      
      component.toggleConversionCalculator();
      expect(component.showConversionCalculator).toBeTruthy();
      
      component.toggleConversionCalculator();
      expect(component.showConversionCalculator).toBeFalsy();
    });

    it('should reset conversion form when showing calculator', () => {
      component.conversaoForm = {
        quantidade: 10,
        unidadeOrigemId: 1,
        unidadeDestinoId: 2
      };
      
      component.toggleConversionCalculator();
      
      expect(component.conversaoForm.quantidade).toBe(0);
      expect(component.conversaoForm.unidadeOrigemId).toBeNull();
      expect(component.conversaoForm.unidadeDestinoId).toBeNull();
    });

    it('should validate conversion form correctly', () => {
      // Invalid - missing data
      expect(component.podeCalcularConversao()).toBeFalsy();
      
      // Invalid - zero quantity
      component.conversaoForm = {
        quantidade: 0,
        unidadeOrigemId: 1,
        unidadeDestinoId: 2
      };
      expect(component.podeCalcularConversao()).toBeFalsy();
      
      // Valid
      component.conversaoForm = {
        quantidade: 10,
        unidadeOrigemId: 1,
        unidadeDestinoId: 2
      };
      expect(component.podeCalcularConversao()).toBeTruthy();
    });

    it('should calculate conversion successfully', () => {
      const mockResult = {
        quantidadeOriginal: 10,
        unidadeOrigemId: 1,
        unidadeDestinoId: 2,
        quantidadeConvertida: 10000
      };
      
      mockUnidadeMedidaService.converter.and.returnValue(of(mockResult));
      
      component.conversaoForm = {
        quantidade: 10,
        unidadeOrigemId: 1,
        unidadeDestinoId: 2
      };
      
      component.calcularConversao();
      
      expect(mockUnidadeMedidaService.converter).toHaveBeenCalledWith(10, 1, 2);
      expect(component.resultadoConversao).toEqual(mockResult);
      expect(component.calculandoConversao).toBeFalsy();
    });

    it('should handle conversion error', () => {
      const mockError = {
        error: { errorDescription: 'Conversion error' }
      };
      
      mockUnidadeMedidaService.converter.and.returnValue(throwError(() => mockError));
      
      component.conversaoForm = {
        quantidade: 10,
        unidadeOrigemId: 1,
        unidadeDestinoId: 2
      };
      
      component.calcularConversao();
      
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro na Conversão',
        detail: 'Conversion error'
      });
      expect(component.calculandoConversao).toBeFalsy();
    });

    it('should get unit by id correctly', () => {
      const unit = component.getUnidadeById(1);
      expect(unit).toEqual(mockUnidadesDropdown[0]);
      
      const notFound = component.getUnidadeById(999);
      expect(notFound).toBeUndefined();
    });
  });

  describe('Display Columns', () => {
    it('should return correct display columns', () => {
      const columns = component['displayColumns']();
      
      expect(columns).toEqual([
        {
          field: 'simbolo',
          header: 'Símbolo',
          sortable: true,
          width: '120px'
        },
        {
          field: 'nome',
          header: 'Nome',
          sortable: true,
          width: '250px'
        },
        {
          field: 'tipo',
          header: 'Tipo',
          sortable: true,
          width: '150px',
          type: 'custom'
        },
        {
          field: 'fatorConversao',
          header: 'Fator Conversão',
          sortable: true,
          width: '150px',
          hideOnMobile: true
        },
        {
          field: 'ativo',
          header: 'Status',
          sortable: true,
          width: '100px',
          type: 'boolean',
          hideOnMobile: true
        },
        {
          field: 'dataCriacao',
          header: 'Criado em',
          sortable: true,
          width: '150px',
          type: 'date',
          hideOnMobile: true,
          hideOnTablet: true
        }
      ]);
    });
  });

  describe('Error Handling', () => {
    it('should handle error when loading tipos', () => {
      mockUnidadeMedidaService.obterTipos.and.returnValue(throwError(() => new Error('Network error')));
      
      component.ngOnInit();
      
      expect(mockMessageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Erro ao carregar tipos de unidade de medida'
      });
    });
  });
});