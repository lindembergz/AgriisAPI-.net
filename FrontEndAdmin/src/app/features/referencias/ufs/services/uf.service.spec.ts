import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { UfService } from './uf.service';
import { UfDto, CriarUfDto, AtualizarUfDto, PaisDto } from '../../../../shared/models/reference.model';

describe('UfService', () => {
  let service: UfService;
  let httpMock: HttpTestingController;

  const mockPais: PaisDto = {
    id: 1,
    codigo: 'BR',
    nome: 'Brasil',
    ativo: true,
    dataCriacao: new Date(),
    dataAtualizacao: new Date()
  };

  const mockUf: UfDto = {
    id: 1,
    codigo: 'SP',
    nome: 'São Paulo',
    paisId: 1,
    pais: mockPais,
    ativo: true,
    dataCriacao: new Date(),
    dataAtualizacao: new Date()
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UfService]
    });

    service = TestBed.inject(UfService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('CRUD Operations', () => {
    it('should fetch all UFs', () => {
      service.obterTodos().subscribe(ufs => {
        expect(ufs).toEqual([mockUf]);
      });

      const req = httpMock.expectOne('/api/referencias/ufs');
      expect(req.request.method).toBe('GET');
      req.flush([mockUf]);
    });

    it('should fetch active UFs', () => {
      service.obterAtivos().subscribe(ufs => {
        expect(ufs).toEqual([mockUf]);
      });

      const req = httpMock.expectOne('/api/referencias/ufs/ativos');
      expect(req.request.method).toBe('GET');
      req.flush([mockUf]);
    });

    it('should fetch UFs with país information', () => {
      service.obterComPais().subscribe(ufs => {
        expect(ufs).toEqual([mockUf]);
        expect(ufs[0].pais).toBeDefined();
      });

      const req = httpMock.expectOne('/api/referencias/ufs?include=pais');
      expect(req.request.method).toBe('GET');
      req.flush([mockUf]);
    });

    it('should fetch UF by id', () => {
      service.obterPorId(1).subscribe(uf => {
        expect(uf).toEqual(mockUf);
      });

      const req = httpMock.expectOne('/api/referencias/ufs/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockUf);
    });

    it('should create new UF', () => {
      const createDto: CriarUfDto = {
        codigo: 'RJ',
        nome: 'Rio de Janeiro',
        paisId: 1
      };

      const createdUf = { ...mockUf, id: 2, ...createDto };

      service.criar(createDto).subscribe(uf => {
        expect(uf).toEqual(createdUf);
      });

      const req = httpMock.expectOne('/api/referencias/ufs');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(createdUf);
    });

    it('should update UF', () => {
      const updateDto: AtualizarUfDto = {
        nome: 'São Paulo Atualizado',
        ativo: false
      };

      const updatedUf = { ...mockUf, ...updateDto };

      service.atualizar(1, updateDto).subscribe(uf => {
        expect(uf).toEqual(updatedUf);
      });

      const req = httpMock.expectOne('/api/referencias/ufs/1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateDto);
      req.flush(updatedUf);
    });

    it('should activate UF', () => {
      service.ativar(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/ufs/1/ativar');
      expect(req.request.method).toBe('PATCH');
      req.flush({});
    });

    it('should deactivate UF', () => {
      service.desativar(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/ufs/1/desativar');
      expect(req.request.method).toBe('PATCH');
      req.flush({});
    });

    it('should remove UF', () => {
      service.remover(1).subscribe();

      const req = httpMock.expectOne('/api/referencias/ufs/1');
      expect(req.request.method).toBe('DELETE');
      req.flush({});
    });

    it('should check if can remove', () => {
      service.podeRemover(1).subscribe(canRemove => {
        expect(canRemove).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/ufs/1/pode-remover');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });
  });

  describe('Specific Methods', () => {
    it('should get municípios count', () => {
      service.obterContagemMunicipios(1).subscribe(count => {
        expect(count).toBe(5);
      });

      const req = httpMock.expectOne('/api/referencias/ufs/1/municipios/contagem');
      expect(req.request.method).toBe('GET');
      req.flush(5);
    });

    it('should verify município dependencies', () => {
      service.verificarDependenciasMunicipio(1).subscribe(hasDependencies => {
        expect(hasDependencies).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/ufs/1/municipios/dependencias');
      expect(req.request.method).toBe('GET');
      req.flush(true);
    });

    it('should validate unique código', () => {
      service.validarCodigoUnico('SP', 1, undefined).subscribe(isUnique => {
        expect(isUnique).toBe(false);
      });

      const req = httpMock.expectOne('/api/referencias/ufs/validar-codigo?codigo=SP&paisId=1');
      expect(req.request.method).toBe('GET');
      req.flush(false);
    });

    it('should validate unique código for edit', () => {
      service.validarCodigoUnico('SP', 1, 1).subscribe(isUnique => {
        expect(isUnique).toBe(true);
      });

      const req = httpMock.expectOne('/api/referencias/ufs/validar-codigo?codigo=SP&paisId=1&excludeId=1');
      expect(req.request.method).toBe('GET');
      req.flush(true);
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

      const req = httpMock.expectOne('/api/referencias/ufs/999');
      req.flush('Not found', { status: 404, statusText: 'Not Found' });
    });

    it('should handle validation error on create', () => {
      const createDto: CriarUfDto = {
        codigo: '',
        nome: '',
        paisId: 0
      };

      service.criar(createDto).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne('/api/referencias/ufs');
      req.flush('Validation error', { status: 400, statusText: 'Bad Request' });
    });

    it('should handle server error when fetching with país', () => {
      service.obterComPais().subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne('/api/referencias/ufs?include=pais');
      req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
    });

    it('should handle dependency check error', () => {
      service.verificarDependenciasMunicipio(1).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne('/api/referencias/ufs/1/municipios/dependencias');
      req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
    });
  });

  describe('Query Parameters', () => {
    it('should handle pagination parameters', () => {
      const params = { page: 2, pageSize: 20, search: 'São' };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/ufs?page=2&pageSize=20&search=S%C3%A3o');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should handle sorting parameters', () => {
      const params = { sortField: 'nome', sortOrder: 'desc' };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/ufs?sortField=nome&sortOrder=desc');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should handle filtering parameters', () => {
      const params = { ativo: true, paisId: 1 };

      service.obterTodos(params).subscribe();

      const req = httpMock.expectOne('/api/referencias/ufs?ativo=true&paisId=1');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });
});