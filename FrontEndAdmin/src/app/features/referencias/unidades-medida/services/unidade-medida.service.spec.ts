import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { UnidadeMedidaService } from './unidade-medida.service';
import { UnidadeMedidaDto, CriarUnidadeMedidaDto, AtualizarUnidadeMedidaDto, TipoUnidadeMedida } from '../../../../shared/models/reference.model';

describe('UnidadeMedidaService', () => {
  let service: UnidadeMedidaService;
  let httpMock: HttpTestingController;

  const mockUnidadeMedida: UnidadeMedidaDto = {
    id: 1,
    simbolo: 'kg',
    nome: 'Quilograma',
    tipo: TipoUnidadeMedida.Peso,
    fatorConversao: 1,
    ativo: true,
    dataCriacao: new Date(),
    dataAtualizacao: new Date()
  };

  const mockTipos = [
    { valor: TipoUnidadeMedida.Peso, nome: 'Peso', descricao: 'Peso' },
    { valor: TipoUnidadeMedida.Volume, nome: 'Volume', descricao: 'Volume' }
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UnidadeMedidaService]
    });

    service = TestBed.inject(UnidadeMedidaService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('CRUD Operations', () => {
    it('should fetch all unidades de medida', () => {
      const mockResponse = { items: [mockUnidadeMedida], total: 1 };

      service.obterTodos().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida');
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should fetch active unidades de medida', () => {
      service.obterAtivos().subscribe(items => {
        expect(items).toEqual([mockUnidadeMedida]);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/ativos');
      expect(req.request.method).toBe('GET');
      req.flush([mockUnidadeMedida]);
    });

    it('should fetch unidade de medida by id', () => {
      service.obterPorId(1).subscribe(item => {
        expect(item).toEqual(mockUnidadeMedida);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockUnidadeMedida);
    });

    it('should create new unidade de medida', () => {
      const createDto: CriarUnidadeMedidaDto = {
        simbolo: 'g',
        nome: 'Grama',
        tipo: TipoUnidadeMedida.Peso,
        fatorConversao: 0.001
      };

      const createdItem = { ...mockUnidadeMedida, id: 2, ...createDto };

      service.criar(createDto).subscribe(item => {
        expect(item).toEqual(createdItem);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(createdItem);
    });

    it('should update unidade de medida', () => {
      const updateDto: AtualizarUnidadeMedidaDto = {
        nome: 'Quilograma Atualizado',
        tipo: TipoUnidadeMedida.Peso,
        fatorConversao: 1.5,
        ativo: false
      };

      const updatedItem = { ...mockUnidadeMedida, ...updateDto };

      service.atualizar(1, updateDto).subscribe(item => {
        expect(item).toEqual(updatedItem);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateDto);
      req.flush(updatedItem);
    });

    it('should activate unidade de medida', () => {
      service.ativar(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/unidades-medida/1/ativar');
      expect(req.request.method).toBe('PATCH');
      req.flush({});
    });

    it('should deactivate unidade de medida', () => {
      service.desativar(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/unidades-medida/1/desativar');
      expect(req.request.method).toBe('PATCH');
      req.flush({});
    });

    it('should remove unidade de medida', () => {
      service.remover(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/unidades-medida/1');
      expect(req.request.method).toBe('DELETE');
      req.flush({});
    });

    it('should check if can remove', () => {
      service.podeRemover(1).subscribe(canRemove => {
        expect(canRemove).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/1/pode-remover');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });
  });

  describe('Specific Methods', () => {
    it('should fetch tipos', () => {
      service.obterTipos().subscribe(tipos => {
        expect(tipos).toEqual(mockTipos);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/tipos');
      expect(req.request.method).toBe('GET');
      req.flush(mockTipos);
    });

    it('should fetch unidades for dropdown', () => {
      const mockDropdown = [
        { id: 1, simbolo: 'kg', nome: 'Quilograma' }
      ];

      service.obterParaDropdown().subscribe(items => {
        expect(items).toEqual(mockDropdown);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/dropdown');
      expect(req.request.method).toBe('GET');
      req.flush(mockDropdown);
    });

    it('should fetch unidades by tipo', () => {
      service.obterPorTipo(TipoUnidadeMedida.Peso).subscribe(items => {
        expect(items).toEqual([mockUnidadeMedida]);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/tipo/1');
      expect(req.request.method).toBe('GET');
      req.flush([mockUnidadeMedida]);
    });

    it('should convert between units', () => {
      const mockResult = {
        quantidadeOriginal: 1,
        unidadeOrigemId: 1,
        unidadeDestinoId: 2,
        quantidadeConvertida: 1000
      };

      service.converter(1, 1, 2).subscribe(result => {
        expect(result).toEqual(mockResult);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/converter');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        quantidade: 1,
        unidadeOrigemId: 1,
        unidadeDestinoId: 2
      });
      req.flush(mockResult);
    });

    it('should verify unique symbol', () => {
      service.verificarSimboloUnico('kg', undefined).subscribe(isUnique => {
        expect(isUnique).toBe(false);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/verificar-simbolo?simbolo=kg');
      expect(req.request.method).toBe('GET');
      req.flush(false);
    });

    it('should verify unique name', () => {
      service.verificarNomeUnico('Quilograma', undefined).subscribe(isUnique => {
        expect(isUnique).toBe(false);
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida/verificar-nome?nome=Quilograma');
      expect(req.request.method).toBe('GET');
      req.flush(false);
    });

    it('should get tipo description', () => {
      expect(service.getTipoDescricao(TipoUnidadeMedida.Peso)).toBe('Peso');
      expect(service.getTipoDescricao(TipoUnidadeMedida.Volume)).toBe('Volume');
      expect(service.getTipoDescricao(TipoUnidadeMedida.Area)).toBe('Área');
      expect(service.getTipoDescricao(TipoUnidadeMedida.Unidade)).toBe('Unidade');
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

      const req = httpMock.expectOne('/api/referencias/unidades-medida/999');
      req.flush('Not found', { status: 404, statusText: 'Not Found' });
    });

    it('should handle validation error on create', () => {
      const createDto: CriarUnidadeMedidaDto = {
        simbolo: '',
        nome: '',
        tipo: TipoUnidadeMedida.Peso
      };

      service.criar(createDto).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida');
      req.flush('Validation error', { status: 400, statusText: 'Bad Request' });
    });

    it('should handle server error', () => {
      service.obterTodos().subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne('/api/referencias/unidades-medida');
      req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
    });
  });

  describe('Caching', () => {
    it('should cache tipos request', () => {
      // First call
      service.obterTipos().subscribe();
      const req1 = httpMock.expectOne('/api/referencias/unidades-medida/tipos');
      req1.flush(mockTipos);

      // Second call should use cache (no HTTP request)
      service.obterTipos().subscribe(tipos => {
        expect(tipos).toEqual(mockTipos);
      });

      httpMock.expectNone('/api/referencias/unidades-medida/tipos');
    });

    it('should cache dropdown request', () => {
      const mockDropdown = [{ id: 1, simbolo: 'kg', nome: 'Quilograma' }];

      // First call
      service.obterParaDropdown().subscribe();
      const req1 = httpMock.expectOne('/api/referencias/unidades-medida/dropdown');
      req1.flush(mockDropdown);

      // Second call should use cache
      service.obterParaDropdown().subscribe(items => {
        expect(items).toEqual(mockDropdown);
      });

      httpMock.expectNone('/api/referencias/unidades-medida/dropdown');
    });
  });
});