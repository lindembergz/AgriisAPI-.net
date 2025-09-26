import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MessageService } from 'primeng/api';
import { SafraService } from './safra.service';
import { SafraDto, CriarSafraDto, AtualizarSafraDto, SafraAtualDto } from '../models';
import { environment } from '../../../../environments/environment';

describe('SafraService', () => {
  let service: SafraService;
  let httpMock: HttpTestingController;
  let messageService: jasmine.SpyObj<MessageService>;

  const mockSafra: SafraDto = {
    id: 1,
    plantioInicial: new Date('2024-01-01'),
    plantioFinal: new Date('2024-06-30'),
    plantioNome: 'Safra 2024/2025',
    descricao: 'Safra de soja 2024/2025',
    anoColheita: 2025,
    safraFormatada: '2024/2025 Safra de soja',
    atual: true,
    dataCriacao: new Date('2024-01-01'),
    dataAtualizacao: new Date('2024-01-02')
  };

  const mockSafraAtual: SafraAtualDto = {
    id: 1,
    descricao: 'Safra de soja 2024/2025',
    safra: '2024/2025'
  };

  const mockCriarSafra: CriarSafraDto = {
    plantioInicial: new Date('2024-07-01'),
    plantioFinal: new Date('2024-12-31'),
    plantioNome: 'Safra 2024/2025 Milho',
    descricao: 'Safra de milho 2024/2025'
  };

  const mockAtualizarSafra: AtualizarSafraDto = {
    plantioInicial: new Date('2024-01-15'),
    plantioFinal: new Date('2024-07-15'),
    plantioNome: 'Safra Atualizada',
    descricao: 'Descrição atualizada'
  };

  beforeEach(() => {
    const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        SafraService,
        { provide: MessageService, useValue: messageServiceSpy }
      ]
    });

    service = TestBed.inject(SafraService);
    httpMock = TestBed.inject(HttpTestingController);
    messageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('obterTodas', () => {
    it('should return all safras', () => {
      const mockSafras: SafraDto[] = [mockSafra];

      service.obterTodas().subscribe(safras => {
        expect(safras).toEqual(mockSafras);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras`);
      expect(req.request.method).toBe('GET');
      req.flush(mockSafras);
    });

    it('should handle error when getting all safras', () => {
      service.obterTodas().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Erro interno do servidor');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras`);
      req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Erro interno do servidor',
        life: 5000
      });
    });
  });

  describe('obterAtual', () => {
    it('should return current safra', () => {
      service.obterAtual().subscribe(safra => {
        expect(safra).toEqual(mockSafraAtual);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras/atual`);
      expect(req.request.method).toBe('GET');
      req.flush(mockSafraAtual);
    });

    it('should handle error when getting current safra', () => {
      service.obterAtual().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Safra não encontrada');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras/atual`);
      req.flush('Not found', { status: 404, statusText: 'Not Found' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Safra não encontrada',
        life: 5000
      });
    });
  });

  describe('obterPorId', () => {
    it('should return safra by id', () => {
      const safraId = 1;

      service.obterPorId(safraId).subscribe(safra => {
        expect(safra).toEqual(mockSafra);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras/${safraId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockSafra);
    });

    it('should handle 404 error when safra not found', () => {
      const safraId = 999;

      service.obterPorId(safraId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Safra não encontrada');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras/${safraId}`);
      req.flush('Not found', { status: 404, statusText: 'Not Found' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Safra não encontrada',
        life: 5000
      });
    });
  });

  describe('obterPorAnoColheita', () => {
    it('should return safras by harvest year', () => {
      const ano = 2025;
      const mockSafras: SafraDto[] = [mockSafra];

      service.obterPorAnoColheita(ano).subscribe(safras => {
        expect(safras).toEqual(mockSafras);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras/ano-colheita/${ano}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockSafras);
    });

    it('should handle error when getting safras by year', () => {
      const ano = 2025;

      service.obterPorAnoColheita(ano).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Dados inválidos fornecidos');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras/ano-colheita/${ano}`);
      req.flush('Bad request', { status: 400, statusText: 'Bad Request' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Dados inválidos fornecidos',
        life: 5000
      });
    });
  });

  describe('criar', () => {
    it('should create new safra', () => {
      service.criar(mockCriarSafra).subscribe(safra => {
        expect(safra).toEqual(mockSafra);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockCriarSafra);
      req.flush(mockSafra);
    });

    it('should handle validation error when creating safra', () => {
      service.criar(mockCriarSafra).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Dados inválidos fornecidos');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras`);
      req.flush('Bad request', { status: 400, statusText: 'Bad Request' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Dados inválidos fornecidos',
        life: 5000
      });
    });
  });

  describe('atualizar', () => {
    it('should update existing safra', () => {
      const safraId = 1;

      service.atualizar(safraId, mockAtualizarSafra).subscribe(safra => {
        expect(safra).toEqual(mockSafra);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras/${safraId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(mockAtualizarSafra);
      req.flush(mockSafra);
    });

    it('should handle conflict error when updating safra', () => {
      const safraId = 1;

      service.atualizar(safraId, mockAtualizarSafra).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Já existe uma safra com estas datas');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras/${safraId}`);
      req.flush('Conflict', { status: 409, statusText: 'Conflict' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Já existe uma safra com estas datas',
        life: 5000
      });
    });
  });

  describe('remover', () => {
    it('should remove safra', () => {
      const safraId = 1;

      service.remover(safraId).subscribe(result => {
        expect(result).toBeUndefined();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras/${safraId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should handle error when removing safra', () => {
      const safraId = 1;

      service.remover(safraId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Safra não encontrada');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras/${safraId}`);
      req.flush('Not found', { status: 404, statusText: 'Not Found' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Safra não encontrada',
        life: 5000
      });
    });
  });

  describe('error handling', () => {
    it('should handle API error with error_description', () => {
      const errorResponse = {
        error_description: 'Data de plantio inicial é obrigatória'
      };

      service.obterTodas().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toBe('Data de plantio inicial é obrigatória');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras`);
      req.flush(errorResponse, { status: 400, statusText: 'Bad Request' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Data de plantio inicial é obrigatória',
        life: 5000
      });
    });

    it('should handle API error with message', () => {
      const errorResponse = {
        message: 'Date validation failed'
      };

      service.obterTodas().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toBe('Date validation failed');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras`);
      req.flush(errorResponse, { status: 422, statusText: 'Unprocessable Entity' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Date validation failed',
        life: 5000
      });
    });

    it('should handle network error', () => {
      service.obterTodas().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Ocorreu um erro inesperado');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras`);
      req.error(new ErrorEvent('Network error'));

      expect(messageService.add).toHaveBeenCalled();
    });

    it('should handle service unavailable error', () => {
      service.obterTodas().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Serviço temporariamente indisponível');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/safras`);
      req.flush('Service unavailable', { status: 503, statusText: 'Service Unavailable' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Serviço temporariamente indisponível',
        life: 5000
      });
    });
  });
});