import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MessageService, ConfirmationService } from 'primeng/api';
import { of, throwError } from 'rxjs';

import { CategoriasComponent } from './categorias.component';
import { CategoriaService } from './services/categoria.service';
import { ValidationService } from '../../../shared/services/validation.service';
import { CategoriaDto, CategoriaProduto, CriarCategoriaDto, AtualizarCategoriaDto } from '../../../shared/models/reference.model';

describe('CategoriasComponent - CRUD Integration', () => {
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
      'obterTiposDisponiveis', 'existeComNome', 'validarReferenciaCircular'
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

  describe('Complete CRUD Workflow', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should complete full create workflow', async () => {
      // Arrange
      const createDto: CriarCategoriaDto = {
        nome: 'Nova Categoria',
        descricao: 'Descrição da nova categoria',
        tipo: CategoriaProduto.Fertilizantes,
        ordem: 0
      };
      
      const createdCategoria: CategoriaDto = {
        ...mockCategoria,
        id: 2,
        nome: createDto.nome,
        descricao: createDto.descricao,
        tipo: createDto.tipo
      };

      mockCategoriaService.criar.and.returnValue(of(createdCategoria));

      // Act - Open new item form
      component.novoItem();
      
      // Assert - Form should be open and in create mode
      expect(component.showForm()).toBe(true);
      expect(component.isEditMode()).toBe(false);
      
      // Act - Fill form and save
      component.form.patchValue(createDto);
      component.salvarItem();

      // Assert - Service should be called and success message shown
      expect(mockCategoriaService.criar).toHaveBeenCalledWith(jasmine.objectContaining({
        nome: createDto.nome,
        tipo: createDto.tipo
      }));
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'success',
        summary: 'Sucesso'
      }));
    });

    it('should complete full edit workflow', async () => {
      // Arrange
      const updateDto: AtualizarCategoriaDto = {
        nome: 'Categoria Atualizada',
        descricao: 'Descrição atualizada',
        tipo: CategoriaProduto.Sementes,
        ordem: 1,
        ativo: true
      };

      const updatedCategoria: CategoriaDto = {
        ...mockCategoria,
        nome: updateDto.nome,
        descricao: updateDto.descricao
      };

      mockCategoriaService.obterPorId.and.returnValue(of(mockCategoria));
      mockCategoriaService.atualizar.and.returnValue(of(updatedCategoria));

      // Act - Open edit form
      component.editarItem(mockCategoria);

      // Assert - Fresh data should be loaded
      expect(mockCategoriaService.obterPorId).toHaveBeenCalledWith(mockCategoria.id);
      expect(component.showForm()).toBe(true);
      expect(component.isEditMode()).toBe(true);
      expect(component.selectedItem()).toEqual(mockCategoria);

      // Act - Update form and save
      component.form.patchValue(updateDto);
      component.salvarItem();

      // Assert - Update service should be called
      expect(mockCategoriaService.atualizar).toHaveBeenCalledWith(
        mockCategoria.id,
        jasmine.objectContaining({
          nome: updateDto.nome,
          ativo: updateDto.ativo
        }),
        undefined
      );
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'success',
        summary: 'Sucesso'
      }));
    });

    it('should complete activation workflow', () => {
      // Arrange
      mockCategoriaService.ativar.and.returnValue(of(void 0));

      // Act
      component.ativarItem(mockCategoria);

      // Assert
      expect(mockCategoriaService.ativar).toHaveBeenCalledWith(mockCategoria.id);
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'success',
        summary: 'Sucesso',
        detail: 'Categoria ativada com sucesso'
      }));
    });

    it('should complete deactivation workflow with confirmation', () => {
      // Arrange
      mockCategoriaService.desativar.and.returnValue(of(void 0));
      mockConfirmationService.confirm.and.callFake((options: any) => {
        options.accept();
      });

      // Act
      component.desativarItem(mockCategoria);

      // Assert
      expect(mockConfirmationService.confirm).toHaveBeenCalledWith(jasmine.objectContaining({
        message: jasmine.stringContaining(mockCategoria.nome),
        header: 'Confirmar Desativação'
      }));
      expect(mockCategoriaService.desativar).toHaveBeenCalledWith(mockCategoria.id);
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'success',
        summary: 'Sucesso'
      }));
    });

    it('should complete deletion workflow with validation', () => {
      // Arrange
      mockCategoriaService.podeRemover.and.returnValue(of(true));
      mockCategoriaService.remover.and.returnValue(of(void 0));
      mockConfirmationService.confirm.and.callFake((options: any) => {
        options.accept();
      });

      // Act
      component.excluirItem(mockCategoria);

      // Assert - Should check if can remove first
      expect(mockCategoriaService.podeRemover).toHaveBeenCalledWith(mockCategoria.id);
      expect(mockConfirmationService.confirm).toHaveBeenCalledWith(jasmine.objectContaining({
        message: jasmine.stringContaining(mockCategoria.nome),
        header: 'Confirmar Exclusão'
      }));
      expect(mockCategoriaService.remover).toHaveBeenCalledWith(mockCategoria.id);
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'success',
        summary: 'Sucesso'
      }));
    });

    it('should prevent deletion when category cannot be removed', () => {
      // Arrange
      mockCategoriaService.podeRemover.and.returnValue(of(false));

      // Act
      component.excluirItem(mockCategoria);

      // Assert
      expect(mockCategoriaService.podeRemover).toHaveBeenCalledWith(mockCategoria.id);
      expect(mockConfirmationService.confirm).not.toHaveBeenCalled();
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'warn',
        summary: 'Não é possível excluir'
      }));
    });

    it('should handle form validation errors', () => {
      // Act - Try to save with invalid form
      component.novoItem();
      component.form.patchValue({ nome: '' }); // Invalid - required field
      component.salvarItem();

      // Assert
      expect(mockCategoriaService.criar).not.toHaveBeenCalled();
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'warn',
        summary: 'Formulário Inválido'
      }));
    });

    it('should handle API errors gracefully', () => {
      // Arrange
      mockCategoriaService.criar.and.returnValue(throwError(() => ({ 
        originalError: { status: 400 },
        message: 'Validation error'
      })));

      // Act
      component.novoItem();
      component.form.patchValue({
        nome: 'Test Category',
        tipo: CategoriaProduto.Sementes,
        ordem: 0
      });
      component.salvarItem();

      // Assert
      expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        severity: 'error',
        summary: 'Erro de Validação'
      }));
    });
  });

  describe('Hierarchy Validation', () => {
    it('should validate circular references in edit mode', () => {
      // Arrange
      const parentCategory: CategoriaDto = {
        ...mockCategoria,
        id: 2,
        nome: 'Parent Category'
      };

      mockCategoriaService.obterPorId.and.returnValue(of(mockCategoria));
      mockCategoriaService.validarReferenciaCircular.and.returnValue(of(false));

      // Act
      component.editarItem(mockCategoria);
      component.form.patchValue({ categoriaPaiId: parentCategory.id });

      // Assert - Form should be invalid due to circular reference
      expect(component.form.get('categoriaPaiId')?.invalid).toBe(true);
    });

    it('should filter dropdown options to prevent circular references', () => {
      // Arrange
      component.selectedItem.set(mockCategoria);
      component.categoriasParaDropdown.set([
        { id: 1, nome: 'Current Category', nivel: 0, nomeCompleto: 'Current Category', ativo: true },
        { id: 2, nome: 'Valid Parent', nivel: 0, nomeCompleto: 'Valid Parent', ativo: true },
        { id: 3, nome: 'Child Category', nivel: 1, nomeCompleto: '— Child Category', ativo: true }
      ]);

      // Mock the items to simulate hierarchy
      component.items.set([
        mockCategoria,
        { ...mockCategoria, id: 3, categoriaPaiId: 1, nome: 'Child Category' }
      ]);

      // Act
      const filteredOptions = component.getCategoriasParaDropdownFiltradas();

      // Assert - Should exclude current category and its descendants
      expect(filteredOptions).not.toContain(jasmine.objectContaining({ id: 1 }));
      expect(filteredOptions).not.toContain(jasmine.objectContaining({ id: 3 }));
      expect(filteredOptions).toContain(jasmine.objectContaining({ id: 2 }));
    });
  });
});