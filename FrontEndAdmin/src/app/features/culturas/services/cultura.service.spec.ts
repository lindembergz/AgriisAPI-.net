import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MessageService } from 'primeng/api';
import { CulturaService } from './cultura.service';
import { CulturaDto, CriarCulturaDto, AtualizarCulturaDto } from '../models';
import { environment } from '../../../../environments/environment';

describe('CulturaService', () => {
  let service: CulturaService;
  let httpMock: HttpTestingController;
  let messageService: jasmine.SpyObj<MessageService>;

  const mockCultura: CulturaDto = {
    id: 1,
    nome: 'Soja',
    descricao: 'Cultura de soja',
    ativo: true,
    dataCriacao: new Date('2024-01-01'),
    dataAtualizacao: new Date('2024-01-02')
  };

  const mockCriarCultura: CriarCulturaDto = {
    nome: 'Milho',
    descricao: 'Cultura de milho'
  };

  const mockAtualizarCultura: AtualizarCulturaDto = {
    nome: 'Soja Atualizada',
    descricao: 'Descrição atualizada',
    ativo: false
  };

  beforeEach(() => {
    const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        CulturaService,
        { provide: MessageService, useValue: messageServiceSpy }
      ]
    });

    service = TestBed.inject(CulturaService);
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
    it('should return all culturas', () => {
      const mockCulturas: CulturaDto[] = [mockCultura];

      service.obterTodas().subscribe(culturas => {
        expect(culturas).toEqual(mockCulturas);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
      expect(req.request.method).toBe('GET');
      req.flush(mockCulturas);
    });

    it('should handle error when getting all culturas', () => {
      service.obterTodas().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Erro interno do servidor');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
      req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Erro interno do servidor',
        life: 5000
      });
    });
  });

  describe('obterAtivas', () => {
    it('should return active culturas', () => {
      const mockCulturasAtivas: CulturaDto[] = [mockCultura];

      service.obterAtivas().subscribe(culturas => {
        expect(culturas).toEqual(mockCulturasAtivas);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas/ativas`);
      expect(req.request.method).toBe('GET');
      req.flush(mockCulturasAtivas);
    });
  });

  describe('obterPorId', () => {
    it('should return cultura by id', () => {
      const culturaId = 1;

      service.obterPorId(culturaId).subscribe(cultura => {
        expect(cultura).toEqual(mockCultura);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas/${culturaId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockCultura);
    });

    it('should handle 404 error when cultura not found', () => {
      const culturaId = 999;

      service.obterPorId(culturaId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Cultura não encontrada');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas/${culturaId}`);
      req.flush('Not found', { status: 404, statusText: 'Not Found' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Cultura não encontrada',
        life: 5000
      });
    });
  });

  describe('criar', () => {
    it('should create new cultura', () => {
      service.criar(mockCriarCultura).subscribe(cultura => {
        expect(cultura).toEqual(mockCultura);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockCriarCultura);
      req.flush(mockCultura);
    });

    it('should handle validation error when creating cultura', () => {
      service.criar(mockCriarCultura).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Dados inválidos fornecidos');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
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
    it('should update existing cultura', () => {
      const culturaId = 1;

      service.atualizar(culturaId, mockAtualizarCultura).subscribe(cultura => {
        expect(cultura).toEqual(mockCultura);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas/${culturaId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(mockAtualizarCultura);
      req.flush(mockCultura);
    });

    it('should handle conflict error when updating cultura', () => {
      const culturaId = 1;

      service.atualizar(culturaId, mockAtualizarCultura).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Já existe uma cultura com este nome');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas/${culturaId}`);
      req.flush('Conflict', { status: 409, statusText: 'Conflict' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Já existe uma cultura com este nome',
        life: 5000
      });
    });
  });

  describe('remover', () => {
    it('should remove cultura', () => {
      const culturaId = 1;

      service.remover(culturaId).subscribe(result => {
        expect(result).toBeUndefined();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas/${culturaId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should handle error when removing cultura', () => {
      const culturaId = 1;

      service.remover(culturaId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toContain('Cultura não encontrada');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas/${culturaId}`);
      req.flush('Not found', { status: 404, statusText: 'Not Found' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Cultura não encontrada',
        life: 5000
      });
    });
  });

  describe('error handling', () => {
    it('should handle API error with error_description', () => {
      const errorResponse = {
        error_description: 'Nome da cultura é obrigatório'
      };

      service.obterTodas().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toBe('Nome da cultura é obrigatório');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
      req.flush(errorResponse, { status: 400, statusText: 'Bad Request' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Nome da cultura é obrigatório',
        life: 5000
      });
    });

    it('should handle API error with message', () => {
      const errorResponse = {
        message: 'Validation failed'
      };

      service.obterTodas().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.message).toBe('Validation failed');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
      req.flush(errorResponse, { status: 422, statusText: 'Unprocessable Entity' });

      expect(messageService.add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Erro',
        detail: 'Validation failed',
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

      const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
      req.error(new ErrorEvent('Network error'));

      expect(messageService.add).toHaveBeenCalled();
    });
  });
});