import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { MunicipiosComponent } from './municipios.component';
import { MunicipioService } from './services/municipio.service';
import { UfService } from '../ufs/services/uf.service';
import { PaisService } from '../paises/services/pais.service';
import { MunicipioDto, CriarMunicipioDto, AtualizarMunicipioDto, UfDto, PaisDto } from '../../../shared/models/reference.model';

// Mock data
const mockPaises: PaisDto[] = [
  { id: 1, nome: 'Brasil', codigo: 'BR', ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() }
];

const mockUfs: UfDto[] = [
  { id: 1, nome: 'São Paulo', codigo: 'SP', paisId: 1, ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() },
  { id: 2, nome: 'Rio de Janeiro', codigo: 'RJ', paisId: 1, ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() }
];

const mockMunicipios: MunicipioDto[] = [
  {
    id: 1,
    nome: 'São Paulo',
    codigoIbge: '3550308',
    ufId: 1,
    ativo: true,
    dataCriacao: new Date(),
    dataAtualizacao: new Date(),
    uf: mockUfs[0]
  },
  {
    id: 2,
    nome: 'Campinas',
    codigoIbge: '3509502',
    ufId: 1,
    ativo: true,
    dataCriacao: new Date(),
    dataAtualizacao: new Date(),
    uf: mockUfs[0]
  }
];

describe('MunicipiosComponent', () => {
  let component: MunicipiosComponent;
  let fixture: ComponentFixture<MunicipiosComponent>;
  let municipioService: jasmine.SpyObj<MunicipioService>;
  let ufService: jasmine.SpyObj<UfService>;
  let paisService: jasmine.SpyObj<PaisService>;

  beforeEach(async () => {
    const municipioServiceSpy = jasmine.createSpyObj('MunicipioService', [
      'obterTodos',
      'obterAtivos',
      'obterPorUf',
      'obterComUf',
      'obterAtivosPorUf',
      'buscarPorNome',
      'criar',
      'atualizar',
      'excluir',
      'ativar',
      'desativar',
      'validarCodigoIbgeUnico',
      'validarNomeUnico'
    ]);

    const ufServiceSpy = jasmine.createSpyObj('UfService', [
      'obterAtivos',
      'obterAtivosPorPais'
    ]);

    const paisServiceSpy = jasmine.createSpyObj('PaisService', [
      'obterAtivos'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        MunicipiosComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: MunicipioService, useValue: municipioServiceSpy },
        { provide: UfService, useValue: ufServiceSpy },
        { provide: PaisService, useValue: paisServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MunicipiosComponent);
    component = fixture.componentInstance;
    municipioService = TestBed.inject(MunicipioService) as jasmine.SpyObj<MunicipioService>;
    ufService = TestBed.inject(UfService) as jasmine.SpyObj<UfService>;
    paisService = TestBed.inject(PaisService) as jasmine.SpyObj<PaisService>;

    // Setup default spy returns
    paisService.obterAtivos.and.returnValue(of(mockPaises));
    ufService.obterAtivosPorPais.and.returnValue(of(mockUfs));
    municipioService.obterComUf.and.returnValue(of(mockMunicipios));
    municipioService.obterAtivos.and.returnValue(of(mockMunicipios.filter(m => m.ativo)));
    municipioService.validarCodigoIbgeUnico.and.returnValue(of(true));
    municipioService.validarNomeUnico.and.returnValue(of(true));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with correct entity configuration', () => {
      expect(component.entityDisplayName()).toBe('Município');
      expect(component.entityDescription()).toBe('Gerenciar Municípios com seleção cascateada de UF');
      expect(component.defaultSortField()).toBe('nome');
      expect(component.searchFields()).toEqual(['nome', 'codigoIbge', 'uf.nome', 'uf.codigo']);
    });

    it('should load países on init', () => {
      component.ngOnInit();
      expect(paisService.obterAtivos).toHaveBeenCalled();
      expect(component.paisesOptions()).toEqual(mockPaises);
    });

    it('should auto-select Brasil if available', () => {
      component.ngOnInit();
      expect(component.selectedPaisFilter()).toBe(1);
      expect(ufService.obterAtivosPorPais).toHaveBeenCalledWith(1);
    });
  });

  describe('Form Creation and Validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should create form with correct validators', () => {
      const form = component.createFormGroup();
      
      expect(form.get('nome')?.hasError('required')).toBeTruthy();
      expect(form.get('codigoIbge')?.hasError('required')).toBeTruthy();
      expect(form.get('paisId')?.hasError('required')).toBeTruthy();
      expect(form.get('ufId')?.hasError('required')).toBeTruthy();
    });

    it('should validate IBGE code format', () => {
      const form = component.createFormGroup();
      const codigoIbgeControl = form.get('codigoIbge');
      
      codigoIbgeControl?.setValue('123');
      expect(codigoIbgeControl?.hasError('pattern')).toBeTruthy();
      
      codigoIbgeControl?.setValue('12345678');
      expect(codigoIbgeControl?.hasError('pattern')).toBeTruthy();
      
      codigoIbgeControl?.setValue('1234567');
      expect(codigoIbgeControl?.hasError('pattern')).toBeFalsy();
    });

    it('should validate nome length', () => {
      const form = component.createFormGroup();
      const nomeControl = form.get('nome');
      
      nomeControl?.setValue('A');
      expect(nomeControl?.hasError('minlength')).toBeTruthy();
      
      nomeControl?.setValue('A'.repeat(101));
      expect(nomeControl?.hasError('maxlength')).toBeTruthy();
      
      nomeControl?.setValue('São Paulo');
      expect(nomeControl?.valid).toBeTruthy();
    });

    it('should setup cascading dropdown behavior', () => {
      const form = component.createFormGroup();
      
      form.get('paisId')?.setValue('1');
      expect(ufService.obterAtivosPorPais).toHaveBeenCalledWith(1);
      expect(form.get('ufId')?.value).toBe('');
    });
  });

  describe('Cascading Dropdowns', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should load UFs when país is selected', () => {
      component.onPaisFilterChange(1);
      
      expect(component.selectedPaisFilter()).toBe(1);
      expect(ufService.obterAtivosPorPais).toHaveBeenCalledWith(1);
      expect(component.selectedUfFilter()).toBeNull();
    });

    it('should clear UFs when país is deselected', () => {
      component.onPaisFilterChange(null);
      
      expect(component.selectedPaisFilter()).toBeNull();
      expect(component.ufsOptions()).toEqual([]);
      expect(component.selectedUfFilter()).toBeNull();
    });

    it('should filter municípios when UF is selected', () => {
      municipioService.obterAtivosPorUf.and.returnValue(of(mockMunicipios));
      
      component.onUfFilterChange(1);
      
      expect(component.selectedUfFilter()).toBe(1);
      expect(municipioService.obterAtivosPorUf).toHaveBeenCalledWith(1);
    });
  });

  describe('Search Functionality', () => {
    beforeEach(() => {
      component.ngOnInit();
      municipioService.buscarPorNome.and.returnValue(of(mockMunicipios));
    });

    it('should perform search with debounce', (done) => {
      component.onSearchChange('São Paulo');
      
      setTimeout(() => {
        expect(component.searchTerm()).toBe('São Paulo');
        expect(municipioService.buscarPorNome).toHaveBeenCalledWith('São Paulo', undefined);
        done();
      }, 350);
    });

    it('should search with UF filter when UF is selected', (done) => {
      component.selectedUfFilter.set(1);
      component.onSearchChange('Campinas');
      
      setTimeout(() => {
        expect(municipioService.buscarPorNome).toHaveBeenCalledWith('Campinas', 1);
        done();
      }, 350);
    });

    it('should not search with less than 2 characters', () => {
      component.onSearchChange('A');
      expect(municipioService.buscarPorNome).not.toHaveBeenCalled();
    });
  });

  describe('Data Loading', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should load all municípios with UF info by default', () => {
      component.carregarItens();
      expect(municipioService.obterComUf).toHaveBeenCalled();
    });

    it('should load active municípios when filter is set to active', () => {
      component.selectedStatusFilter.set('ativas');
      component.carregarItens();
      expect(municipioService.obterAtivos).toHaveBeenCalled();
    });

    it('should filter by UF when UF is selected', () => {
      component.selectedUfFilter.set(1);
      component.carregarItens();
      expect(municipioService.obterComUf).toHaveBeenCalled();
    });

    it('should use search when search term is provided', () => {
      component.searchTerm.set('São Paulo');
      component.carregarItens();
      expect(municipioService.buscarPorNome).toHaveBeenCalledWith('São Paulo', undefined);
    });
  });

  describe('Form Population', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should populate form with município data', (done) => {
      const municipio = mockMunicipios[0];
      
      component.populateForm(municipio);
      
      // Wait for async UF loading
      setTimeout(() => {
        expect(component.form.get('nome')?.value).toBe(municipio.nome);
        expect(component.form.get('codigoIbge')?.value).toBe(municipio.codigoIbge);
        expect(component.form.get('ufId')?.value).toBe(municipio.ufId);
        expect(component.form.get('ativo')?.value).toBe(municipio.ativo);
        done();
      }, 150);
    });
  });

  describe('DTO Mapping', () => {
    it('should map form values to create DTO correctly', () => {
      const formValue = {
        nome: '  São Paulo  ',
        codigoIbge: '  3550308  ',
        ufId: '1'
      };
      
      const dto = component.mapToCreateDto(formValue);
      
      expect(dto).toEqual({
        nome: 'São Paulo',
        codigoIbge: '3550308',
        ufId: 1
      });
    });

    it('should map form values to update DTO correctly', () => {
      const formValue = {
        nome: '  São Paulo Atualizado  ',
        ativo: false
      };
      
      const dto = component.mapToUpdateDto(formValue);
      
      expect(dto).toEqual({
        nome: 'São Paulo Atualizado',
        ativo: false
      });
    });
  });

  describe('Display Methods', () => {
    it('should format UF display correctly', () => {
      const municipio = mockMunicipios[0];
      const display = component.getUfDisplay(municipio);
      expect(display).toBe('SP - São Paulo');
    });

    it('should handle missing UF in display', () => {
      const municipio = { ...mockMunicipios[0], uf: undefined };
      const display = component.getUfDisplay(municipio);
      expect(display).toBe('N/A');
    });

    it('should format País display correctly', () => {
      const municipio = {
        ...mockMunicipios[0],
        uf: { ...mockUfs[0], pais: mockPaises[0] }
      };
      const display = component.getPaisDisplay(municipio);
      expect(display).toBe('Brasil');
    });
  });

  describe('Filter Management', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should clear all filters', () => {
      component.selectedPaisFilter.set(1);
      component.selectedUfFilter.set(1);
      component.searchTerm.set('test');
      
      component.limparFiltros();
      
      expect(component.selectedPaisFilter()).toBeNull();
      expect(component.selectedUfFilter()).toBeNull();
      expect(component.searchTerm()).toBe('');
      expect(component.ufsOptions()).toEqual([]);
    });

    it('should generate filter summary correctly', () => {
      component.selectedUfFilter.set(1);
      component.ufsOptions.set(mockUfs);
      component.searchTerm.set('São Paulo');
      
      const summary = component.getFilterSummary();
      expect(summary).toContain('UF: SP');
      expect(summary).toContain('Busca: "São Paulo"');
    });

    it('should return empty filter summary when no filters', () => {
      const summary = component.getFilterSummary();
      expect(summary).toBe('');
    });
  });

  describe('Error Handling', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should handle error when loading países', () => {
      paisService.obterAtivos.and.returnValue(throwError('Error loading países'));
      spyOn(console, 'error');
      
      component.ngOnInit();
      
      expect(console.error).toHaveBeenCalledWith('Erro ao carregar países:', 'Error loading países');
      expect(component.loadingPaises()).toBeFalsy();
    });

    it('should handle error when loading UFs', () => {
      ufService.obterAtivosPorPais.and.returnValue(throwError('Error loading UFs'));
      spyOn(console, 'error');
      
      component.carregarUfsPorPais(1);
      
      expect(console.error).toHaveBeenCalledWith('Erro ao carregar UFs:', 'Error loading UFs');
      expect(component.loadingUfs()).toBeFalsy();
    });

    it('should handle error when loading municípios', () => {
      municipioService.obterComUf.and.returnValue(throwError('Error loading municípios'));
      spyOn(console, 'error');
      
      component.carregarItens();
      
      expect(console.error).toHaveBeenCalledWith('Erro ao carregar municípios:', 'Error loading municípios');
      expect(component.loading()).toBeFalsy();
      expect(component.tableLoading()).toBeFalsy();
    });
  });

  describe('Async Validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should validate IBGE code uniqueness', (done) => {
      municipioService.validarCodigoIbgeUnico.and.returnValue(of(false));
      
      const validator = (component as any).validarCodigoIbgeUnico({ value: '1234567' });
      
      validator.subscribe((result: any) => {
        expect(result).toEqual({ codigoIbgeNaoUnico: true });
        expect(municipioService.validarCodigoIbgeUnico).toHaveBeenCalledWith('1234567', undefined);
        done();
      });
    });

    it('should validate nome uniqueness within UF', (done) => {
      municipioService.validarNomeUnico.and.returnValue(of(false));
      component.form.get('ufId')?.setValue('1');
      
      const validator = (component as any).validarNomeUnico({ value: 'São Paulo' });
      
      validator.subscribe((result: any) => {
        expect(result).toEqual({ nomeNaoUnico: true });
        expect(municipioService.validarNomeUnico).toHaveBeenCalledWith('São Paulo', 1, undefined);
        done();
      });
    });

    it('should return null for valid IBGE code', (done) => {
      municipioService.validarCodigoIbgeUnico.and.returnValue(of(true));
      
      const validator = (component as any).validarCodigoIbgeUnico({ value: '1234567' });
      
      validator.subscribe((result: any) => {
        expect(result).toBeNull();
        done();
      });
    });
  });
});