import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MoedaService } from './moeda.service';
import { MoedaDto, CriarMoedaDto, AtualizarMoedaDto } from '../../../../shared/models/reference.model';

describe('MoedaService', () => {
  let service: MoedaService;
  let httpMock: HttpTestingController;

  const mockMoeda: MoedaDto = {
    id: 1,
    codigo: 'BRL',
    nome: 'Real Brasileiro',
    simbolo: 'R$',
    ativo: true,
    dataCriacao: new Date(),
    dataAtualizacao: new Date()
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [MoedaService]
    });

    service = TestBed.inject(MoedaService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('CRUD Operations', () => {
    it('should fetch all moedas', () => {
      service.obterTodos().subscribe(moedas => {
        expect(moedas).toEqual([mockMoeda]);
      });

      const req = httpMock.expectOne('/api/referencias/moedas');
      expect(req.request.method).toBe('GET');
      req.flush([mockMoeda]);
    });

    it('should fetch active moedas', () => {
      service.obterAtivos().subscribe(moedas => {
        expect(moedas).toEqual([mockMoeda]);
      });

      const req = httpMock.expectOne('/api/referencias/moedas/ativos');
      expect(req.request.method).toBe('GET');
      req.flush([mockMoeda]);
    });

    it('should fetch moeda by id', () => {
      service.obterPorId(1).subscribe(moeda => {
        expect(moeda).toEqual(mockMoeda);
      });

      const req = httpMock.expectOne('/api/referencias/moedas/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockMoeda);
    });

    it('should create new moeda', () => {
      const createDto: CriarMoedaDto = {
        codigo: 'USD',
        nome: 'Dólar Americano',
        simbolo: 'US$'
      };

      const createdMoeda = { ...mockMoeda, id: 2, ...createDto };

      service.criar(createDto).subscribe(moeda => {
        expect(moeda).toEqual(createdMoeda);
      });

      const req = httpMock.expectOne('/api/referencias/moedas');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(createdMoeda);
    });

    it('should update moeda', () => {
      const updateDto: AtualizarMoedaDto = {
        nome: 'Real Brasileiro Atualizado',
        simbolo: 'R$ ',
        ativo: false
      };

      const updatedMoeda = { ...mockMoeda, ...updateDto };

      service.atualizar(1, updateDto).subscribe(moeda => {
        expect(moeda).toEqual(updatedMoeda);
      });

      const req = httpMock.expectOne('/api/referencias/moedas/1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateDto);
      req.flush(updatedMoeda);
    });

    it('should activate moeda', () => {
      service.ativar(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/moedas/1/ativar');
      expect(req.request.method).toBe('PATCH');
      req.flush({});
    });

    it('should deactivate moeda', () => {
      service.desativar(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/moedas/1/desativar');
      expect(req.request.method).toBe('PATCH');
      req.flush({});
    });

    it('should remove moeda', () => {
      service.remover(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/moedas/1');
      expect(req.request.method).toBe('DELETE');
      req.flush({});
    });

    it('should check if can remove', () => {
      service.podeRemover(1).subscribe(canRemove => {
        expect(canRemove).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/moedas/1/pode-remover');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });
  });

  describe('Validation Methods', () => {
    it('should verify unique código', () => {
      service.verificarCodigoUnico('USD', undefined).subscribe(isUnique => {
        expect(isUnique).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/moedas/verificar-codigo?codigo=USD');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });

    it('should verify unique código for edit', () => {
      service.verificarCodigoUnico('BRL', 1).subscribe(isUnique => {
        expect(isUnique).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/moedas/verificar-codigo?codigo=BRL&excludeId=1');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });

    it('should verify unique nome', () => {
      service.verificarNomeUnico('Dólar Americano', undefined).subscribe(isUnique => {
        expect(isUnique).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/moedas/verificar-nome?nome=D%C3%B3lar%20Americano');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });

    it('should verify unique símbolo', () => {
      service.verificarSimboloUnico('US$', undefined).subscribe(isUnique => {
        expect(isUnique).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/moedas/verificar-simbolo?simbolo=US%24');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });
  });

  describe('Currency Conversion', () => {
    it('should get exchange rate', () => {
      const mockRate = { taxa: 5.25, dataAtualizacao: new Date() };

      service.obterTaxaCambio('USD', 'BRL').subscribe(rate => {
        expect(rate).toEqual(mockRate);
      });

      const req = httpMock.expectOne('/api/referencias/moedas/taxa-cambio?origem=USD&destino=BRL');
      expect(req.request.method).toBe('GET');
      req.flush(mockRate);
    });

    it('should convert currency', () => {
      const mockConversion = {
        valor: 100,
        moedaOrigem: 'USD',
        moedaDestino: 'BRL',
        valorConvertido: 525,
        taxa: 5.25,
        dataConversao: new Date()
      };

      service.converterMoeda(100, 'USD', 'BRL').subscribe(conversion => {
        expect(conversion).toEqual(mockConversion);
      });

      const req = httpMock.expectOne('/api/referencias/moedas/converter');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        valor: 100,
        moedaOrigem: 'USD',
        moedaDestino: 'BRL'
      });
      req.flush(mockConversion);
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

      const req = httpMock.expectOne('/api/referencias/moedas/999');
      req.flush('Not found', { status: 404, statusText: 'Not Found' });
    });

    it('should handle validation error on create', () => {
      const createDto: CriarMoedaDto = {
        codigo: '',
        nome: '',
        simbolo: ''
      };

      service.criar(createDto).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne('/api/referencias/moedas');
      req.flush('Validation error', { status: 400, statusText: 'Bad Request' });
    });

    it('should handle currency conversion error', () => {
      service.converterMoeda(100, 'INVALID', 'BRL').subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne('/api/referencias/moedas/converter');
      req.flush('Invalid currency', { status: 400, statusText: 'Bad Request' });
    });

    it('should handle exchange rate service unavailable', () => {
      service.obterTaxaCambio('USD', 'BRL').subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(503);
        }
      });

      const req = httpMock.expectOne('/api/referencias/moedas/taxa-cambio?origem=USD&destino=BRL');
      req.flush('Service unavailable', { status: 503, statusText: 'Service Unavailable' });
    });
  });

  describe('Query Parameters', () => {
    it('should handle search parameters', () => {
      const params = { search: 'Real', ativo: true };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/moedas?search=Real&ativo=true');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should handle pagination parameters', () => {
      const params = { page: 2, pageSize: 10 };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/moedas?page=2&pageSize=10');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should handle sorting parameters', () => {
      const params = { sortField: 'codigo', sortOrder: 'asc' };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/moedas?sortField=codigo&sortOrder=asc');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });

  describe('Caching', () => {
    it('should cache active moedas request', () => {
      // First call
      service.obterAtivos().subscribe();
      const req1 = httpMock.expectOne('/api/referencias/moedas/ativos');
      req1.flush([mockMoeda]);

      // Second call should use cache (no HTTP request)
      service.obterAtivos().subscribe(moedas => {
        expect(moedas).toEqual([mockMoeda]);
      });

      httpMock.expectNone('/api/referencias/moedas/ativos');
    });
  });
});