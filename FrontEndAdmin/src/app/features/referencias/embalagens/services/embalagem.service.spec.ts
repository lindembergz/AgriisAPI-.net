import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { EmbalagemService } from './embalagem.service';
import { EmbalagemDto, CriarEmbalagemDto, AtualizarEmbalagemDto, TipoUnidadeMedida } from '../../../../shared/models/reference.model';

describe('EmbalagemService', () => {
  let service: EmbalagemService;
  let httpMock: HttpTestingController;

  const mockUnidadeMedida = {
    id: 1,
    simbolo: 'kg',
    nome: 'Quilograma',
    tipo: TipoUnidadeMedida.Peso,
    ativo: true,
    dataCriacao: new Date(),
    dataAtualizacao: new Date()
  };

  const mockEmbalagem: EmbalagemDto = {
    id: 1,
    nome: 'Saco',
    descricao: 'Saco de papel',
    unidadeMedidaId: 1,
    unidadeMedida: mockUnidadeMedida,
    ativo: true,
    dataCriacao: new Date(),
    dataAtualizacao: new Date()
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [EmbalagemService]
    });

    service = TestBed.inject(EmbalagemService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('CRUD Operations', () => {
    it('should fetch all embalagens', () => {
      service.obterTodos().subscribe(embalagens => {
        expect(embalagens).toEqual([mockEmbalagem]);
      });

      const req = httpMock.expectOne('/api/referencias/embalagens');
      expect(req.request.method).toBe('GET');
      req.flush([mockEmbalagem]);
    });

    it('should fetch active embalagens', () => {
      service.obterAtivos().subscribe(embalagens => {
        expect(embalagens).toEqual([mockEmbalagem]);
      });

      const req = httpMock.expectOne('/api/referencias/embalagens/ativos');
      expect(req.request.method).toBe('GET');
      req.flush([mockEmbalagem]);
    });

    it('should fetch embalagem by id', () => {
      service.obterPorId(1).subscribe(embalagem => {
        expect(embalagem).toEqual(mockEmbalagem);
      });

      const req = httpMock.expectOne('/api/referencias/embalagens/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockEmbalagem);
    });

    it('should create new embalagem', () => {
      const createDto: CriarEmbalagemDto = {
        nome: 'Caixa',
        descricao: 'Caixa de papelão',
        unidadeMedidaId: 2
      };

      const createdEmbalagem = { ...mockEmbalagem, id: 2, ...createDto };

      service.criar(createDto).subscribe(embalagem => {
        expect(embalagem).toEqual(createdEmbalagem);
      });

      const req = httpMock.expectOne('/api/referencias/embalagens');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(createdEmbalagem);
    });

    it('should update embalagem', () => {
      const updateDto: AtualizarEmbalagemDto = {
        nome: 'Saco Atualizado',
        descricao: 'Saco de papel kraft',
        unidadeMedidaId: 1,
        ativo: false
      };

      const updatedEmbalagem = { ...mockEmbalagem, ...updateDto };

      service.atualizar(1, updateDto).subscribe(embalagem => {
        expect(embalagem).toEqual(updatedEmbalagem);
      });

      const req = httpMock.expectOne('/api/referencias/embalagens/1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateDto);
      req.flush(updatedEmbalagem);
    });

    it('should activate embalagem', () => {
      service.ativar(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/embalagens/1/ativar');
      expect(req.request.method).toBe('PATCH');
      req.flush({});
    });

    it('should deactivate embalagem', () => {
      service.desativar(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/embalagens/1/desativar');
      expect(req.request.method).toBe('PATCH');
      req.flush({});
    });

    it('should remove embalagem', () => {
      service.remover(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/embalagens/1');
      expect(req.request.method).toBe('DELETE');
      req.flush({});
    });

    it('should check if can remove', () => {
      service.podeRemover(1).subscribe(canRemove => {
        expect(canRemove).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/embalagens/1/pode-remover');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });
  });

  describe('Specific Methods', () => {
    it('should fetch unidades de medida for dropdown', () => {
      const mockUnidades = [
        { id: 1, simbolo: 'kg', nome: 'Quilograma', tipo: TipoUnidadeMedida.Peso },
        { id: 2, simbolo: 'L', nome: 'Litro', tipo: TipoUnidadeMedida.Volume }
      ];

      service.obterUnidadesMedidaParaDropdown().subscribe(unidades => {
        expect(unidades).toEqual(mockUnidades);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/dropdown');
      expect(req.request.method).toBe('GET');
      req.flush(mockUnidades);
    });

    it('should fetch tipos de unidade', () => {
      const mockTipos = [
        { valor: TipoUnidadeMedida.Peso, nome: 'Peso', descricao: 'Peso' },
        { valor: TipoUnidadeMedida.Volume, nome: 'Volume', descricao: 'Volume' }
      ];

      service.obterTiposUnidade().subscribe(tipos => {
        expect(tipos).toEqual(mockTipos);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/tipos');
      expect(req.request.method).toBe('GET');
      req.flush(mockTipos);
    });

    it('should get tipo unidade description', () => {
      expect(service.getTipoUnidadeDescricao(TipoUnidadeMedida.Peso)).toBe('Peso');
      expect(service.getTipoUnidadeDescricao(TipoUnidadeMedida.Volume)).toBe('Volume');
      expect(service.getTipoUnidadeDescricao(TipoUnidadeMedida.Area)).toBe('Área');
      expect(service.getTipoUnidadeDescricao(TipoUnidadeMedida.Unidade)).toBe('Unidade');
    });

    it('should verify unique nome', () => {
      service.verificarNomeUnico('Caixa', undefined).subscribe(isUnique => {
        expect(isUnique).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/embalagens/verificar-nome?nome=Caixa');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });

    it('should verify unique nome for edit', () => {
      service.verificarNomeUnico('Saco', 1).subscribe(isUnique => {
        expect(isUnique).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/embalagens/verificar-nome?nome=Saco&excludeId=1');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });
  });

  describe('Filtering Methods', () => {
    it('should fetch embalagens by tipo unidade', () => {
      service.obterPorTipoUnidade(TipoUnidadeMedida.Peso).subscribe(embalagens => {
        expect(embalagens).toEqual([mockEmbalagem]);
      });

      const req = httpMock.expectOne('/api/referencias/embalagens/tipo-unidade/1');
      expect(req.request.method).toBe('GET');
      req.flush([mockEmbalagem]);
    });

    it('should fetch embalagens by unidade medida', () => {
      service.obterPorUnidadeMedida(1).subscribe(embalagens => {
        expect(embalagens).toEqual([mockEmbalagem]);
      });

      const req = httpMock.expectOne('/api/referencias/embalagens/unidade-medida/1');
      expect(req.request.method).toBe('GET');
      req.flush([mockEmbalagem]);
    });

    it('should search embalagens by name', () => {
      service.buscarPorNome('Saco').subscribe(embalagens => {
        expect(embalagens).toEqual([mockEmbalagem]);
      });

      const req = httpMock.expectOne('/api/referencias/embalagens/buscar?nome=Saco');
      expect(req.request.method).toBe('GET');
      req.flush([mockEmbalagem]);
    });
  });

  describe('Error Handling', () => {
    it('should handle 404 error when fetching by id', () => {
      service.obterPorId(999).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(404);
        }
      });

      const req = httpMock.expectOne('/api/referencias/embalagens/999');
      req.flush('Not found', { status: 404, statusText: 'Not Found' });
    });

    it('should handle validation error on create', () => {
      const createDto: CriarEmbalagemDto = {
        nome: '',
        unidadeMedidaId: 0
      };

      service.criar(createDto).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne('/api/referencias/embalagens');
      req.flush('Validation error', { status: 400, statusText: 'Bad Request' });
    });

    it('should handle foreign key constraint error', () => {
      const createDto: CriarEmbalagemDto = {
        nome: 'Test Embalagem',
        unidadeMedidaId: 999 // Non-existent unidade medida
      };

      service.criar(createDto).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne('/api/referencias/embalagens');
      req.flush('Foreign key constraint', { status: 400, statusText: 'Bad Request' });
    });

    it('should handle server error when fetching unidades medida', () => {
      service.obterUnidadesMedidaParaDropdown().subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/dropdown');
      req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
    });
  });

  describe('Query Parameters', () => {
    it('should handle search parameters', () => {
      const params = { search: 'Saco', ativo: true };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/embalagens?search=Saco&ativo=true');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should handle filtering by tipo unidade', () => {
      const params = { tipoUnidade: TipoUnidadeMedida.Peso };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/embalagens?tipoUnidade=1');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should handle filtering by unidade medida', () => {
      const params = { unidadeMedidaId: 1 };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/embalagens?unidadeMedidaId=1');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should handle pagination parameters', () => {
      const params = { page: 2, pageSize: 15 };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/embalagens?page=2&pageSize=15');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });

  describe('Caching', () => {
    it('should cache unidades medida dropdown request', () => {
      const mockUnidades = [
        { id: 1, simbolo: 'kg', nome: 'Quilograma', tipo: TipoUnidadeMedida.Peso }
      ];

      // First call
      service.obterUnidadesMedidaParaDropdown().subscribe();
      const req1 = httpMock.expectOne('/api/referencias/unidades-medida/dropdown');
      req1.flush(mockUnidades);

      // Second call should use cache (no HTTP request)
      service.obterUnidadesMedidaParaDropdown().subscribe(unidades => {
        expect(unidades).toEqual(mockUnidades);
      });

      httpMock.expectNone('/api/referencias/unidades-medida/dropdown');
    });

    it('should cache tipos unidade request', () => {
      const mockTipos = [
        { valor: TipoUnidadeMedida.Peso, nome: 'Peso', descricao: 'Peso' }
      ];

      // First call
      service.obterTiposUnidade().subscribe();
      const req1 = httpMock.expectOne('/api/referencias/unidades-medida/tipos');
      req1.flush(mockTipos);

      // Second call should use cache
      service.obterTiposUnidade().subscribe(tipos => {
        expect(tipos).toEqual(mockTipos);
      });

      httpMock.expectNone('/api/referencias/unidades-medida/tipos');
    });
  });
});