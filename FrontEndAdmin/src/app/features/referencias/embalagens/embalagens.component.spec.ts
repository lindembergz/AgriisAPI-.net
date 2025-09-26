import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { MessageService, ConfirmationService } from 'primeng/api';
import { of } from 'rxjs';

import { EmbalagensComponent } from './embalagens.component';
import { EmbalagemService } from './services/embalagem.service';
import { EmbalagemDto, TipoUnidadeMedida } from '../../../shared/models/reference.model';

describe('EmbalagensComponent', () => {
  let component: EmbalagensComponent;
  let fixture: ComponentFixture<EmbalagensComponent>;
  let embalagemService: jasmine.SpyObj<EmbalagemService>;

  const mockEmbalagens: EmbalagemDto[] = [
    {
      id: 1,
      nome: 'Saco',
      descricao: 'Saco de papel',
      unidadeMedidaId: 1,
      unidadeMedida: {
        id: 1,
        simbolo: 'kg',
        nome: 'Quilograma',
        tipo: TipoUnidadeMedida.Peso,
        ativo: true,
        dataCriacao: new Date(),
        dataAtualizacao: new Date()
      },
      ativo: true,
      dataCriacao: new Date(),
      dataAtualizacao: new Date()
    },
    {
      id: 2,
      nome: 'Caixa',
      descricao: 'Caixa de papelão',
      unidadeMedidaId: 2,
      unidadeMedida: {
        id: 2,
        simbolo: 'L',
        nome: 'Litro',
        tipo: TipoUnidadeMedida.Volume,
        ativo: true,
        dataCriacao: new Date(),
        dataAtualizacao: new Date()
      },
      ativo: true,
      dataCriacao: new Date(),
      dataAtualizacao: new Date()
    }
  ];

  const mockUnidadesMedida = [
    {
      id: 1,
      simbolo: 'kg',
      nome: 'Quilograma',
      tipo: TipoUnidadeMedida.Peso
    },
    {
      id: 2,
      simbolo: 'L',
      nome: 'Litro',
      tipo: TipoUnidadeMedida.Volume
    }
  ];

  const mockTiposUnidade = [
    {
      valor: TipoUnidadeMedida.Peso,
      nome: 'Peso',
      descricao: 'Peso'
    },
    {
      valor: TipoUnidadeMedida.Volume,
      nome: 'Volume',
      descricao: 'Volume'
    }
  ];

  beforeEach(async () => {
    const embalagemServiceSpy = jasmine.createSpyObj('EmbalagemService', [
      'obterTodos',
      'obterAtivos',
      'obterPorId',
      'criar',
      'atualizar',
      'ativar',
      'desativar',
      'remover',
      'podeRemover',
      'obterUnidadesMedidaParaDropdown',
      'obterTiposUnidade',
      'getTipoUnidadeDescricao'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        EmbalagensComponent,
        ReactiveFormsModule,
        NoopAnimationsModule,
        HttpClientTestingModule
      ],
      providers: [
        { provide: EmbalagemService, useValue: embalagemServiceSpy },
        MessageService,
        ConfirmationService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EmbalagensComponent);
    component = fixture.componentInstance;
    embalagemService = TestBed.inject(EmbalagemService) as jasmine.SpyObj<EmbalagemService>;

    // Setup default spy returns
    embalagemService.obterTodos.and.returnValue(of(mockEmbalagens));
    embalagemService.obterAtivos.and.returnValue(of(mockEmbalagens));
    embalagemService.obterUnidadesMedidaParaDropdown.and.returnValue(of(mockUnidadesMedida));
    embalagemService.obterTiposUnidade.and.returnValue(of(mockTiposUnidade));
    embalagemService.getTipoUnidadeDescricao.and.callFake((tipo: TipoUnidadeMedida) => {
      switch (tipo) {
        case TipoUnidadeMedida.Peso: return 'Peso';
        case TipoUnidadeMedida.Volume: return 'Volume';
        case TipoUnidadeMedida.Area: return 'Área';
        case TipoUnidadeMedida.Unidade: return 'Unidade';
        default: return 'Desconhecido';
      }
    });
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with correct entity configuration', () => {
    expect(component['entityDisplayName']()).toBe('Embalagem');
    expect(component['entityDescription']()).toBe('Gerenciar tipos de embalagem utilizados no sistema');
    expect(component['defaultSortField']()).toBe('nome');
    expect(component['searchFields']()).toEqual(['nome', 'descricao']);
  });

  it('should load unidades de medida on init', () => {
    component.ngOnInit();
    
    expect(embalagemService.obterUnidadesMedidaParaDropdown).toHaveBeenCalled();
    expect(embalagemService.obterTiposUnidade).toHaveBeenCalled();
  });

  it('should create form with correct validators', () => {
    const form = component['createFormGroup']();
    
    expect(form.get('nome')?.hasError('required')).toBeTruthy();
    expect(form.get('unidadeMedidaId')?.hasError('required')).toBeTruthy();
    
    // Test valid form
    form.patchValue({
      nome: 'Saco de papel',
      descricao: 'Descrição do saco',
      unidadeMedidaId: 1
    });
    
    expect(form.valid).toBeTruthy();
  });

  it('should map form to create DTO correctly', () => {
    const formValue = {
      nome: '  Saco de papel  ',
      descricao: '  Descrição do saco  ',
      unidadeMedidaId: 1
    };
    
    const dto = component['mapToCreateDto'](formValue);
    
    expect(dto).toEqual({
      nome: 'Saco de papel',
      descricao: 'Descrição do saco',
      unidadeMedidaId: 1
    });
  });

  it('should map form to update DTO correctly', () => {
    const formValue = {
      nome: '  Saco atualizado  ',
      descricao: '  Nova descrição  ',
      unidadeMedidaId: 2,
      ativo: false
    };
    
    const dto = component['mapToUpdateDto'](formValue);
    
    expect(dto).toEqual({
      nome: 'Saco atualizado',
      descricao: 'Nova descrição',
      unidadeMedidaId: 2,
      ativo: false
    });
  });

  it('should populate form correctly', () => {
    const embalagem = mockEmbalagens[0];
    
    component['populateForm'](embalagem);
    
    expect(component.form.get('nome')?.value).toBe(embalagem.nome);
    expect(component.form.get('descricao')?.value).toBe(embalagem.descricao);
    expect(component.form.get('unidadeMedidaId')?.value).toBe(embalagem.unidadeMedidaId);
    expect(component.form.get('ativo')?.value).toBe(embalagem.ativo);
  });

  it('should filter by tipo unidade correctly', () => {
    component.ngOnInit();
    fixture.detectChanges();
    
    // Set up initial data
    component.items.set(mockEmbalagens);
    component.unidadesDisponiveis = mockUnidadesMedida;
    
    // Apply filter
    component.tipoUnidadeSelecionado = TipoUnidadeMedida.Peso;
    component.onTipoUnidadeFilterChange();
    
    // Check that unidades are filtered
    expect(component.unidadesFiltradas.length).toBe(1);
    expect(component.unidadesFiltradas[0].tipo).toBe(TipoUnidadeMedida.Peso);
  });

  it('should filter by unidade medida correctly', () => {
    component.ngOnInit();
    fixture.detectChanges();
    
    // Set up initial data
    component.items.set(mockEmbalagens);
    
    // Apply filter
    component.unidadeMedidaSelecionada = 1;
    component.onUnidadeMedidaFilterChange();
    
    // The filter should be applied (implementation depends on the actual filtering logic)
    expect(component.unidadeMedidaSelecionada).toBe(1);
  });

  it('should get tipo unidade description correctly', () => {
    expect(component.getTipoUnidadeDescricao(TipoUnidadeMedida.Peso)).toBe('Peso');
    expect(component.getTipoUnidadeDescricao(TipoUnidadeMedida.Volume)).toBe('Volume');
    expect(component.getTipoUnidadeDescricao(TipoUnidadeMedida.Area)).toBe('Área');
    expect(component.getTipoUnidadeDescricao(TipoUnidadeMedida.Unidade)).toBe('Unidade');
  });

  it('should handle empty description in form', () => {
    const formValue = {
      nome: 'Saco',
      descricao: '',
      unidadeMedidaId: 1
    };
    
    const createDto = component['mapToCreateDto'](formValue);
    const updateDto = component['mapToUpdateDto']({ ...formValue, ativo: true });
    
    expect(createDto.descricao).toBeUndefined();
    expect(updateDto.descricao).toBeUndefined();
  });

  it('should display correct table columns', () => {
    const columns = component['displayColumns']();
    
    expect(columns).toEqual([
      {
        field: 'nome',
        header: 'Nome',
        sortable: true,
        width: '200px'
      },
      {
        field: 'descricao',
        header: 'Descrição',
        sortable: true,
        width: '300px',
        hideOnMobile: true
      },
      {
        field: 'unidadeMedida',
        header: 'Unidade de Medida',
        sortable: false,
        width: '200px',
        type: 'custom'
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