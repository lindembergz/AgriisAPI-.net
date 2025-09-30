import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PaisService } from './pais.service';
import { PaisDto, CriarPaisDto, AtualizarPaisDto } from '../../../../shared/models/reference.model';

describe('PaisService', () => {
  let service: PaisService;
  let httpMock: HttpTestingController;

  const mockPais: PaisDto = {
    id: 1,
    codigo: 'BR',
    nome: 'Brasil',
    ativo: true,
    dataCriacao: new Date(),
    dataAtualizacao: new Date()
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PaisService]
    });

    service = TestBed.inject(PaisService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('CRUD Operations', () => {
    it('should fetch all países', () => {
      service.obterTodos().subscribe(paises => {
        expect(paises).toEqual([mockPais]);
      });

      const req = httpMock.expectOne('/api/referencias/paises');
      expect(req.request.method).toBe('GET');
      req.flush([mockPais]);
    });

    it('should fetch active países', () => {
      service.obterAtivos().subscribe(paises => {
        expect(paises).toEqual([mockPais]);
      });

      const req = httpMock.expectOne('/api/referencias/paises/ativos');
      expect(req.request.method).toBe('GET');
      req.flush([mockPais]);
    });

    it('should fetch países with UF counters', () => {
      const mockPaisComContador = { ...mockPais, quantidadeUfs: 27 };

      service.obterAtivosComContadores().subscribe(paises => {
        expect(paises).toEqual([mockPaisComContador]);
      });

      const req = httpMock.expectOne('/api/referencias/paises/ativos/com-contadores');
      expect(req.request.method).toBe('GET');
      req.flush([mockPaisComContador]);
    });

    it('should fetch país by id', () => {
      service.obterPorId(1).subscribe(pais => {
        expect(pais).toEqual(mockPais);
      });

      const req = httpMock.expectOne('/api/referencias/paises/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockPais);
    });

    it('should create new país', () => {
      const createDto: CriarPaisDto = {
        codigo: 'US',
        nome: 'Estados Unidos'
      };

      const createdPais = { ...mockPais, id: 2, ...createDto };

      service.criar(createDto).subscribe(pais => {
        expect(pais).toEqual(createdPais);
      });

      const req = httpMock.expectOne('/api/referencias/paises');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(createdPais);
    });

    it('should update país', () => {
      const updateDto: AtualizarPaisDto = {
        nome: 'Brasil Atualizado',
        ativo: false
      };

      const updatedPais = { ...mockPais, ...updateDto };

      service.atualizar(1, updateDto).subscribe(pais => {
        expect(pais).toEqual(updatedPais);
      });

      const req = httpMock.expectOne('/api/referencias/paises/1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateDto);
      req.flush(updatedPais);
    });

    it('should activate país', () => {
      service.ativar(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/paises/1/ativar');
      expect(req.request.method).toBe('PATCH');
      req.flush({});
    });

    it('should deactivate país', () => {
      service.desativar(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/paises/1/desativar');
      expect(req.request.method).toBe('PATCH');
      req.flush({});
    });

    it('should remove país', () => {
      service.remover(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/paises/1');
      expect(req.request.method).toBe('DELETE');
      req.flush({});
    });

    it('should check if can remove', () => {
      service.podeRemover(1).subscribe(canRemove => {
        expect(canRemove).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/paises/1/pode-remover');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });
  });

  describe('Specific Methods', () => {
    it('should verify UF dependencies', () => {
      service.verificarDependenciasUf(1).subscribe(hasDependencies => {
        expect(hasDependencies).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/paises/1/ufs/dependencias');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });

    it('should get UFs count', () => {
      service.obterContagemUfs(1).subscribe(count => {
        expect(count).toBe(27);
      });

      const req = httpMock.expectOne('/api/referencias/paises/1/ufs/contagem');
      expect(req.request.method).toBe('GET');
      req.flush(27);
    });

    it('should verify unique código', () => {
      service.verificarCodigoUnico('US', undefined).subscribe(isUnique => {
        expect(isUnique).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/paises/verificar-codigo?codigo=US');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });

    it('should verify unique código for edit', () => {
      service.verificarCodigoUnico('BR', 1).subscribe(isUnique => {
        expect(isUnique).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/paises/verificar-codigo?codigo=BR&excludeId=1');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });

    it('should verify unique nome', () => {
      service.verificarNomeUnico('Estados Unidos', undefined).subscribe(isUnique => {
        expect(isUnique).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/paises/verificar-nome?nome=Estados%20Unidos');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });
  });

  describe('Geographic Data', () => {
    it('should get país by código ISO', () => {
      service.obterPorCodigoIso('BR').subscribe(pais => {
        expect(pais).toEqual(mockPais);
      });

      const req = httpMock.expectOne('/api/referencias/paises/codigo/BR');
      expect(req.request.method).toBe('GET');
      req.flush(mockPais);
    });

    it('should search países by name', () => {
      service.buscarPorNome('Bras').subscribe(paises => {
        expect(paises).toEqual([mockPais]);
      });

      const req = httpMock.expectOne('/api/referencias/paises/buscar?nome=Bras');
      expect(req.request.method).toBe('GET');
      req.flush([mockPais]);
    });

    it('should get países by continent', () => {
      service.obterPorContinente('América do Sul').subscribe(paises => {
        expect(paises).toEqual([mockPais]);
      });

      const req = httpMock.expectOne('/api/referencias/paises/continente/Am%C3%A9rica%20do%20Sul');
      expect(req.request.method).toBe('GET');
      req.flush([mockPais]);
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

      const req = httpMock.expectOne('/api/referencias/paises/999');
      req.flush('Not found', { status: 404, statusText: 'Not Found' });
    });

    it('should handle validation error on create', () => {
      const createDto: CriarPaisDto = {
        codigo: '',
        nome: ''
      };

      service.criar(createDto).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne('/api/referencias/paises');
      req.flush('Validation error', { status: 400, statusText: 'Bad Request' });
    });

    it('should handle dependency check error', () => {
      service.verificarDependenciasUf(1).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne('/api/referencias/paises/1/ufs/dependencias');
      req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
    });

    it('should handle invalid ISO code error', () => {
      service.obterPorCodigoIso('INVALID').subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(404);
        }
      });

      const req = httpMock.expectOne('/api/referencias/paises/codigo/INVALID');
      req.flush('Invalid ISO code', { status: 404, statusText: 'Not Found' });
    });
  });

  describe('Query Parameters', () => {
    it('should handle search parameters', () => {
      const params = { search: 'Brasil', ativo: true };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/paises?search=Brasil&ativo=true');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should handle pagination parameters', () => {
      const params = { page: 1, pageSize: 50 };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/paises?page=1&pageSize=50');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should handle continent filter', () => {
      const params = { continente: 'América do Sul' };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/paises?continente=Am%C3%A9rica%20do%20Sul');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });

  describe('Caching', () => {
    it('should cache active países request', () => {
      // First call
      service.obterAtivos().subscribe();
      const req1 = httpMock.expectOne('/api/referencias/paises/ativos');
      req1.flush([mockPais]);

      // Second call should use cache (no HTTP request)
      service.obterAtivos().subscribe(paises => {
        expect(paises).toEqual([mockPais]);
      });

      httpMock.expectNone('/api/referencias/paises/ativos');
    });

    it('should cache países with counters request', () => {
      const mockPaisComContador = { ...mockPais, quantidadeUfs: 27 };

      // First call
      service.obterAtivosComContadores().subscribe();
      const req1 = httpMock.expectOne('/api/referencias/paises/ativos/com-contadores');
      req1.flush([mockPaisComContador]);

      // Second call should use cache
      service.obterAtivosComContadores().subscribe(paises => {
        expect(paises).toEqual([mockPaisComContador]);
      });

      httpMock.expectNone('/api/referencias/paises/ativos/com-contadores');
    });
  });
});