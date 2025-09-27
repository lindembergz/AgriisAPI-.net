import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MessageService, ConfirmationService } from 'primeng/api';
import { of, throwError } from 'rxjs';

import { CategoriasComponent } from './categorias.component';
import { CategoriaService } from './services/categoria.service';
import { ValidationService } from '../../../shared/services/validation.service';
import { CategoriaDto, CategoriaProduto } from '../../../shared/models/reference.model';

describe('CategoriasComponent - CRUD Operations', () => {
  let component: CategoriasComponent;
  let fixture: ComponentFixture<CategoriasComponent>;
  let mockCategoriaService: jasmine.SpyObj<CategoriaService>;
  let mockMessageService: jasmine.SpyObj<MessageService>;
  let mockConfirmationService: jasmine.SpyObj<ConfirmationService>;

  const mockCategoria: CategoriaDto = {
    id: 1,
    nome: 'Sementes de Soja',
    descricao: 'Categoria para sementes de soja',
    tipo: CategoriaProduto.Sementes,
    categoriaPaiId: null,
    categoriaPaiNome: null,
    ordem: 1,
    ativo: true,
    subCategorias: [],
    quantidadeProdutos: 5,
    dataCriacao: new Date(),
    dataAtualizacao: new Date()
  };

  beforeEach(async () => {
    const categoriaServiceSpy = jasmine.createSpyObj('CategoriaService', [
      'obterTodos', 'obterAtivos', 'obterPorId', 'criar', 'atualizar', 
      'ativar', 'desativar', 'remover', 'podeRemover', 'obterParaDropdown',
      'obterTiposDisponiveis', 'existeComNome', 'validarReferenciaCircular',
      'transformToTreeNodes', 'obterLabelTipo'
    ]);
    const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);
    const confirmationServiceSpy = jasmine.createSpyObj('ConfirmationService', ['confirm']);

    await TestBed.configureTestingModule({
      imports: [
        CategoriasComponent,
        ReactiveFormsModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: CategoriaService, useValue: categoriaServiceSpy },
        { provide: MessageService, useValue: messageServiceSpy },
        { provide: ConfirmationService, useValue: confirmationServiceSpy },
        ValidationService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CategoriasComponent);
    component = fixture.componentInstance;
    mockCategoriaService = TestBed.inject(CategoriaService) as jasmine.SpyObj<CategoriaService>;
    mockMessageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;
    mockConfirmationService = TestBed.inject(ConfirmationService) as jasmine.SpyObj<ConfirmationService>;

    // Setup default mock returns
    mockCategoriaService.obterTodos.and.returnValue(of([mockCategoria]));
    mockCategoriaService.obterAtivos.and.returnValue(of([mockCategoria]));
    mockCategoriaService.obterParaDropdown.and.returnValue(of([]));
    mockCategoriaService.obterTiposDisponiveis.and.returnValue([
      { value: CategoriaProduto.Sementes, label: 'Sementes' }
    ]);
    mockCategoriaService.existeComNome.and.returnValue(of(false));
    mockCategoriaService.validarReferenciaCircular.and.returnValue(of(true));
  });

  describe('novoItem', () => {
    it('should open form dialog for creating new category', () => {
      // Act
      component.novoItem();

      // Assert
      expect(component.showForm()).toBe(true);
      expect(component.selectedItem()).toBeNull();
      expect(component.isEditMode()).toBe(false);
      expect(mockCategoriaService.obterParaDropdown).toHaveBeenCalled();
    });

    it('should reset form with default values', () => {
      // Act
      component.novoItem();

      // Assert
      expect(component.form.get('ordem')?.value).toBe(0);
      expect(component.form.get('ativo')?.value).toBe(true);
    });
  });

  describe('editarItem', () => {
    it('should load fresh data and open form dialog for editing', () => {
      // Arrange
      mockCategoriaService.obterPorId.and.returnValue(of(mockCategoria));

      // Act
      component.editarItem(mockCategoria);

      // Assert
      expect(mockCategoriaService.obterPorId).toHaveBeenCalledWith(mockCategoria.id);
      expect(component.showForm()).toBe(true);
      expect(component.selectedItem()).toEqual(mockCategoria);
      expect(component.isEditMode()).toBe(true);
    });

    it('should handle error when loading category for edit', () => {
      // Arrange
      mockCategoriaService.obterPorId.and.returnValue(throwError(() => new Error('Network error')));

      // Act
      component.editarItem(mockCategoria);

      // Assert
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'error',
        summary: 'Erro'
      }));
    });
  });

  describe('salvarItem', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should create new category when not in edit mode', () => {
      // Arrange
      const createDto = {
        nome: 'Nova Categoria',
        tipo: CategoriaProduto.Sementes,
        ordem: 0
      };
      mockCategoriaService.criar.and.returnValue(of(mockCategoria));
      
      component.form.patchValue(createDto);
      component.selectedItem.set(null);

      // Act
      component.salvarItem();

      // Assert
      expect(mockCategoriaService.criar).toHaveBeenCalled();
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'success',
        summary: 'Sucesso'
      }));
    });

    it('should update existing category when in edit mode', () => {
      // Arrange
      const updateDto = {
        nome: 'Categoria Atualizada',
        tipo: CategoriaProduto.Sementes,
        ordem: 1,
        ativo: true
      };
      mockCategoriaService.atualizar.and.returnValue(of(mockCategoria));
      
      component.form.patchValue(updateDto);
      component.selectedItem.set(mockCategoria);

      // Act
      component.salvarItem();

      // Assert
      expect(mockCategoriaService.atualizar).toHaveBeenCalled();
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'success',
        summary: 'Sucesso'
      }));
    });

    it('should not save when form is invalid', () => {
      // Arrange
      component.form.patchValue({ nome: '' }); // Invalid form

      // Act
      component.salvarItem();

      // Assert
      expect(mockCategoriaService.criar).not.toHaveBeenCalled();
      expect(mockCategoriaService.atualizar).not.toHaveBeenCalled();
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'warn'
      }));
    });
  });

  describe('ativarItem', () => {
    it('should activate category successfully', () => {
      // Arrange
      mockCategoriaService.ativar.and.returnValue(of(void 0));

      // Act
      component.ativarItem(mockCategoria);

      // Assert
      expect(mockCategoriaService.ativar).toHaveBeenCalledWith(mockCategoria.id);
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'success',
        summary: 'Sucesso'
      }));
    });

    it('should handle activation error', () => {
      // Arrange
      mockCategoriaService.ativar.and.returnValue(throwError(() => new Error('Network error')));

      // Act
      component.ativarItem(mockCategoria);

      // Assert
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'error',
        summary: 'Erro'
      }));
    });
  });

  describe('desativarItem', () => {
    it('should show confirmation dialog before deactivating', () => {
      // Act
      component.desativarItem(mockCategoria);

      // Assert
      expect(mockConfirmationService.confirm).toHaveBeenCalledWith(jasmine.objectContaining({
        message: jasmine.stringContaining(mockCategoria.nome),
        header: 'Confirmar Desativação'
      }));
    });
  });

  describe('excluirItem', () => {
    it('should check if category can be removed before showing confirmation', () => {
      // Arrange
      mockCategoriaService.podeRemover.and.returnValue(of(true));

      // Act
      component.excluirItem(mockCategoria);

      // Assert
      expect(mockCategoriaService.podeRemover).toHaveBeenCalledWith(mockCategoria.id);
      expect(mockConfirmationService.confirm).toHaveBeenCalled();
    });

    it('should show warning when category cannot be removed', () => {
      // Arrange
      mockCategoriaService.podeRemover.and.returnValue(of(false));

      // Act
      component.excluirItem(mockCategoria);

      // Assert
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'warn',
        summary: 'Não é possível excluir'
      }));
      expect(mockConfirmationService.confirm).not.toHaveBeenCalled();
    });

    it('should handle error when checking if category can be removed', () => {
      // Arrange
      mockCategoriaService.podeRemover.and.returnValue(throwError(() => new Error('Network error')));

      // Act
      component.excluirItem(mockCategoria);

      // Assert
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'error',
        summary: 'Erro'
      }));
    });
  });

  describe('Filtering and Search', () => {
    const mockCategorias: CategoriaDto[] = [
      {
        id: 1,
        nome: 'Sementes',
        descricao: 'Categoria de sementes',
        tipo: CategoriaProduto.Sementes,
        categoriaPaiId: null,
        categoriaPaiNome: null,
        ordem: 1,
        ativo: true,
        subCategorias: [],
        quantidadeProdutos: 5,
        dataCriacao: new Date(),
        dataAtualizacao: new Date()
      },
      {
        id: 2,
        nome: 'Fertilizantes',
        descricao: 'Categoria de fertilizantes',
        tipo: CategoriaProduto.Fertilizantes,
        categoriaPaiId: null,
        categoriaPaiNome: null,
        ordem: 2,
        ativo: false,
        subCategorias: [],
        quantidadeProdutos: 3,
        dataCriacao: new Date(),
        dataAtualizacao: new Date()
      },
      {
        id: 3,
        nome: 'Sementes de Soja',
        descricao: 'Subcategoria de sementes de soja',
        tipo: CategoriaProduto.Sementes,
        categoriaPaiId: 1,
        categoriaPaiNome: 'Sementes',
        ordem: 1,
        ativo: true,
        subCategorias: [],
        quantidadeProdutos: 2,
        dataCriacao: new Date(),
        dataAtualizacao: new Date()
      }
    ];

    beforeEach(() => {
      mockCategoriaService.obterTodos.and.returnValue(of(mockCategorias));
      mockCategoriaService.transformToTreeNodes.and.returnValue([]);
      mockCategoriaService.obterTiposDisponiveis.and.returnValue([
        { label: 'Sementes', value: CategoriaProduto.Sementes },
        { label: 'Fertilizantes', value: CategoriaProduto.Fertilizantes }
      ]);
      component.ngOnInit();
    });

    describe('onSearchTextChange', () => {
      it('should filter categories by name', () => {
        // Act
        component.onSearchTextChange({ target: { value: 'Soja' } });

        // Assert
        expect(component.searchText()).toBe('Soja');
        // The filtered results should include the matching category and its parent
        const filteredItems = component.items();
        expect(filteredItems.some(item => item.nome.includes('Soja'))).toBe(true);
      });

      it('should filter categories by description', () => {
        // Act
        component.onSearchTextChange({ target: { value: 'fertilizantes' } });

        // Assert
        expect(component.searchText()).toBe('fertilizantes');
        const filteredItems = component.items();
        expect(filteredItems.some(item => item.descricao?.toLowerCase().includes('fertilizantes'))).toBe(true);
      });

      it('should be case insensitive', () => {
        // Act
        component.onSearchTextChange({ target: { value: 'SEMENTES' } });

        // Assert
        expect(component.searchText()).toBe('SEMENTES');
        const filteredItems = component.items();
        expect(filteredItems.some(item => item.nome.toLowerCase().includes('sementes'))).toBe(true);
      });
    });

    describe('onTipoFilterChange', () => {
      it('should filter categories by tipo', () => {
        // Act
        component.onTipoFilterChange({ value: CategoriaProduto.Sementes });

        // Assert
        expect(component.selectedTipoFilter()).toBe(CategoriaProduto.Sementes);
        const filteredItems = component.items();
        expect(filteredItems.every(item => item.tipo === CategoriaProduto.Sementes)).toBe(true);
      });

      it('should show all categories when tipo filter is null', () => {
        // Arrange
        component.selectedTipoFilter.set(CategoriaProduto.Sementes);

        // Act
        component.onTipoFilterChange({ value: null });

        // Assert
        expect(component.selectedTipoFilter()).toBe(null);
        const filteredItems = component.items();
        expect(filteredItems.length).toBeGreaterThan(1);
      });
    });

    describe('onStatusFilterChange', () => {
      it('should filter active categories', () => {
        // Act
        component.onStatusFilterChange({ value: 'ativas' });

        // Assert
        expect(component.selectedStatusFilter()).toBe('ativas');
        const filteredItems = component.items();
        expect(filteredItems.every(item => item.ativo === true)).toBe(true);
      });

      it('should filter inactive categories', () => {
        // Act
        component.onStatusFilterChange({ value: 'inativas' });

        // Assert
        expect(component.selectedStatusFilter()).toBe('inativas');
        const filteredItems = component.items();
        expect(filteredItems.every(item => item.ativo === false)).toBe(true);
      });
    });

    describe('limparFiltros', () => {
      it('should clear all filters', () => {
        // Arrange
        component.searchText.set('test');
        component.selectedTipoFilter.set(CategoriaProduto.Sementes);
        component.selectedStatusFilter.set('ativas');

        // Act
        component.limparFiltros();

        // Assert
        expect(component.searchText()).toBe('');
        expect(component.selectedTipoFilter()).toBe(null);
        expect(component.selectedStatusFilter()).toBe('todas');
      });
    });

    describe('temFiltrosAtivos', () => {
      it('should return true when search text is active', () => {
        // Arrange
        component.searchText.set('test');

        // Act & Assert
        expect(component.temFiltrosAtivos()).toBe(true);
      });

      it('should return true when tipo filter is active', () => {
        // Arrange
        component.selectedTipoFilter.set(CategoriaProduto.Sementes);

        // Act & Assert
        expect(component.temFiltrosAtivos()).toBe(true);
      });

      it('should return true when status filter is not "todas"', () => {
        // Arrange
        component.selectedStatusFilter.set('ativas');

        // Act & Assert
        expect(component.temFiltrosAtivos()).toBe(true);
      });

      it('should return false when no filters are active', () => {
        // Arrange
        component.searchText.set('');
        component.selectedTipoFilter.set(null);
        component.selectedStatusFilter.set('todas');

        // Act & Assert
        expect(component.temFiltrosAtivos()).toBe(false);
      });
    });

    describe('Combined Filters', () => {
      it('should apply multiple filters with AND logic', () => {
        // Act
        component.onSearchTextChange({ target: { value: 'Sementes' } });
        component.onTipoFilterChange({ value: CategoriaProduto.Sementes });
        component.onStatusFilterChange({ value: 'ativas' });

        // Assert
        const filteredItems = component.items();
        expect(filteredItems.every(item => 
          item.nome.toLowerCase().includes('sementes') &&
          item.tipo === CategoriaProduto.Sementes &&
          item.ativo === true
        )).toBe(true);
      });
    });
  });
});