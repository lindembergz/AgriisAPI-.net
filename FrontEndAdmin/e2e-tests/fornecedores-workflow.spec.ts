import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { Component } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MessageService } from 'primeng/api';
import { ReactiveFormsModule } from '@angular/forms';

import { FornecedorListComponent } from '../features/fornecedores/components/fornecedor-list.component';
import { FornecedorDetailComponent } from '../features/fornecedores/components/fornecedor-detail.component';
import { FornecedorService } from '../features/fornecedores/services/fornecedor.service';
import { AuthService } from '../core/auth/auth.service';
import { StorageService } from '../core/services/storage.service';

@Component({
  template: '<router-outlet></router-outlet>',
  standalone: true,
  imports: []
})
class MockAppComponent { }

describe('Fornecedores CRUD Workflow Integration Tests', () => {
  let fixture: ComponentFixture<MockAppComponent>;
  let router: Router;
  let location: Location;
  let httpMock: HttpTestingController;
  let fornecedorService: FornecedorService;
  let authService: AuthService;

  const mockFornecedor = {
    id: 1,
    codigo: 'FORN001',
    nome: 'Agro Insumos Ltda',
    cpfCnpj: '12345678000195',
    tipoCliente: 'PJ' as const,
    telefone: '(11) 3333-3333',
    email: 'contato@agroinsumos.com',
    enderecos: [
      {
        id: 1,
        logradouro: 'Av. Industrial',
        numero: '1000',
        bairro: 'Distrito Industrial',
        cidade: 'São Paulo',
        uf: 'SP',
        cep: '05001-000'
      }
    ],
    pontosDistribuicao: [
      {
        id: 1,
        nome: 'Centro de Distribuição SP',
        endereco: {
          logradouro: 'Rua da Distribuição',
          numero: '500',
          bairro: 'Logística',
          cidade: 'São Paulo',
          uf: 'SP',
          cep: '05002-000'
        },
        latitude: -23.5505,
        longitude: -46.6333
      }
    ],
    usuarioMaster: {
      id: 1,
      nome: 'Carlos Manager',
      email: 'carlos@agroinsumos.com',
      telefone: '(11) 3333-3334'
    }
  };

  const mockFornecedoresList = [
    mockFornecedor,
    {
      id: 2,
      codigo: 'FORN002',
      nome: 'Sementes Brasil S.A.',
      cpfCnpj: '98765432000123',
      tipoCliente: 'PJ' as const,
      telefone: '(11) 4444-4444',
      email: 'vendas@sementesbrasil.com',
      enderecos: [],
      pontosDistribuicao: [],
      usuarioMaster: null
    }
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MockAppComponent,
        NoopAnimationsModule,
        ReactiveFormsModule
      ],
      providers: [
        provideRouter([
          { path: 'fornecedores', component: FornecedorListComponent },
          { path: 'fornecedores/novo', component: FornecedorDetailComponent },
          { path: 'fornecedores/:id', component: FornecedorDetailComponent },
          { path: '', redirectTo: '/fornecedores', pathMatch: 'full' }
        ]),
        provideHttpClient(),
        provideHttpClientTesting(),
        MessageService,
        FornecedorService,
        AuthService,
        StorageService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MockAppComponent);
    router = TestBed.inject(Router);
    location = TestBed.inject(Location);
    httpMock = TestBed.inject(HttpTestingController);
    fornecedorService = TestBed.inject(FornecedorService);
    authService = TestBed.inject(AuthService);

    // Mock authentication
    spyOn(authService, 'isAuthenticated').and.returnValue(true);
    spyOn(authService, 'getToken').and.returnValue('mock-token');

    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Fornecedores List Workflow', () => {
    it('should load and display fornecedores list', async () => {
      // Navigate to fornecedores list
      await router.navigate(['/fornecedores']);
      fixture.detectChanges();

      // Should make API call to load fornecedores
      const req = httpMock.expectOne('/api/fornecedores');
      expect(req.request.method).toBe('GET');
      expect(req.request.headers.get('Authorization')).toBe('Bearer mock-token');

      req.flush(mockFornecedoresList);
      fixture.detectChanges();

      // Verify location
      expect(location.path()).toBe('/fornecedores');
    });

    it('should handle empty fornecedores list', async () => {
      await router.navigate(['/fornecedores']);
      fixture.detectChanges();

      const req = httpMock.expectOne('/api/fornecedores');
      req.flush([]);
      fixture.detectChanges();

      expect(location.path()).toBe('/fornecedores');
    });

    it('should handle API error when loading fornecedores', async () => {
      await router.navigate(['/fornecedores']);
      fixture.detectChanges();

      const req = httpMock.expectOne('/api/fornecedores');
      req.flush({ message: 'Server error' }, { status: 500, statusText: 'Internal Server Error' });
      fixture.detectChanges();

      expect(location.path()).toBe('/fornecedores');
    });

    it('should navigate to create new fornecedor', async () => {
      await router.navigate(['/fornecedores']);
      fixture.detectChanges();

      // Mock the list load
      const listReq = httpMock.expectOne('/api/fornecedores');
      listReq.flush(mockFornecedoresList);

      // Navigate to new fornecedor
      await router.navigate(['/fornecedores/novo']);
      fixture.detectChanges();

      expect(location.path()).toBe('/fornecedores/novo');
    });

    it('should navigate to edit existing fornecedor', async () => {
      await router.navigate(['/fornecedores']);
      fixture.detectChanges();

      // Mock the list load
      const listReq = httpMock.expectOne('/api/fornecedores');
      listReq.flush(mockFornecedoresList);

      // Navigate to edit fornecedor
      await router.navigate(['/fornecedores/1']);
      fixture.detectChanges();

      // Should load the specific fornecedor
      const detailReq = httpMock.expectOne('/api/fornecedores/1');
      expect(detailReq.request.method).toBe('GET');
      detailReq.flush(mockFornecedor);

      expect(location.path()).toBe('/fornecedores/1');
    });
  });

  describe('Fornecedores Create Workflow', () => {
    it('should create new fornecedor with complete data', async () => {
      // Navigate to create form
      await router.navigate(['/fornecedores/novo']);
      fixture.detectChanges();

      expect(location.path()).toBe('/fornecedores/novo');

      // Simulate form submission
      const newFornecedor = {
        nome: 'Novo Fornecedor Ltda',
        cpfCnpj: '11111111000111',
        tipoCliente: 'PJ',
        telefone: '(11) 5555-5555',
        email: 'novo@fornecedor.com',
        enderecos: [
          {
            logradouro: 'Rua Nova Empresa',
            numero: '789',
            bairro: 'Empresarial',
            cidade: 'São Paulo',
            uf: 'SP',
            cep: '01234-567'
          }
        ],
        pontosDistribuicao: [
          {
            nome: 'Ponto Central',
            endereco: {
              logradouro: 'Av. Central',
              numero: '100',
              bairro: 'Centro',
              cidade: 'São Paulo',
              uf: 'SP',
              cep: '01000-000'
            },
            latitude: -23.0000,
            longitude: -46.0000
          }
        ],
        usuarioMaster: {
          nome: 'Gestor Novo',
          email: 'gestor@fornecedor.com',
          telefone: '(11) 5555-5556'
        }
      };

      const createPromise = fornecedorService.create(newFornecedor);

      const req = httpMock.expectOne('/api/fornecedores');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newFornecedor);

      const createdFornecedor = { ...newFornecedor, id: 3, codigo: 'FORN003' };
      req.flush(createdFornecedor);

      const result = await createPromise;
      expect(result.id).toBe(3);
      expect(result.codigo).toBe('FORN003');
    });

    it('should validate required fields when creating fornecedor', async () => {
      await router.navigate(['/fornecedores/novo']);
      fixture.detectChanges();

      // Try to create with invalid data
      const invalidFornecedor = {
        nome: '', // Required field empty
        cpfCnpj: 'invalid', // Invalid CNPJ format
        tipoCliente: 'PJ',
        telefone: '',
        email: 'invalid-email' // Invalid email format
      };

      try {
        await fornecedorService.create(invalidFornecedor);
      } catch (error) {
        // Should fail validation
        expect(error).toBeDefined();
      }

      // No HTTP request should be made for invalid data
      httpMock.expectNone('/api/fornecedores');
    });

    it('should handle server validation errors', async () => {
      await router.navigate(['/fornecedores/novo']);
      fixture.detectChanges();

      const newFornecedor = {
        nome: 'Fornecedor Duplicado',
        cpfCnpj: '12345678000195', // Duplicate CNPJ
        tipoCliente: 'PJ'
      };

      const createPromise = fornecedorService.create(newFornecedor);

      const req = httpMock.expectOne('/api/fornecedores');
      req.flush(
        { 
          message: 'CNPJ já cadastrado',
          errors: { cpfCnpj: 'Este CNPJ já está em uso' }
        }, 
        { status: 400, statusText: 'Bad Request' }
      );

      try {
        await createPromise;
      } catch (error) {
        expect(error.status).toBe(400);
      }
    });
  });

  describe('Fornecedores Update Workflow', () => {
    it('should update existing fornecedor', async () => {
      // Navigate to edit form
      await router.navigate(['/fornecedores/1']);
      fixture.detectChanges();

      // Load existing fornecedor
      const loadReq = httpMock.expectOne('/api/fornecedores/1');
      loadReq.flush(mockFornecedor);

      // Update fornecedor data
      const updatedFornecedor = {
        ...mockFornecedor,
        nome: 'Agro Insumos Ltda - Atualizada',
        telefone: '(11) 3333-9999'
      };

      const updatePromise = fornecedorService.update(1, updatedFornecedor);

      const updateReq = httpMock.expectOne('/api/fornecedores/1');
      expect(updateReq.request.method).toBe('PUT');
      expect(updateReq.request.body.nome).toBe('Agro Insumos Ltda - Atualizada');

      updateReq.flush(updatedFornecedor);

      const result = await updatePromise;
      expect(result.nome).toBe('Agro Insumos Ltda - Atualizada');
    });

    it('should handle update validation errors', async () => {
      await router.navigate(['/fornecedores/1']);
      fixture.detectChanges();

      const loadReq = httpMock.expectOne('/api/fornecedores/1');
      loadReq.flush(mockFornecedor);

      const invalidUpdate = {
        ...mockFornecedor,
        email: 'invalid-email-format'
      };

      const updatePromise = fornecedorService.update(1, invalidUpdate);

      const updateReq = httpMock.expectOne('/api/fornecedores/1');
      updateReq.flush(
        { 
          message: 'Dados inválidos',
          errors: { email: 'Formato de email inválido' }
        }, 
        { status: 400, statusText: 'Bad Request' }
      );

      try {
        await updatePromise;
      } catch (error) {
        expect(error.status).toBe(400);
      }
    });
  });

  describe('Fornecedores Delete Workflow', () => {
    it('should delete fornecedor', async () => {
      const deletePromise = fornecedorService.delete(1);

      const req = httpMock.expectOne('/api/fornecedores/1');
      expect(req.request.method).toBe('DELETE');

      req.flush({});

      await deletePromise;
      // Should complete without error
    });

    it('should handle delete errors', async () => {
      const deletePromise = fornecedorService.delete(1);

      const req = httpMock.expectOne('/api/fornecedores/1');
      req.flush(
        { message: 'Não é possível excluir fornecedor com contratos ativos' }, 
        { status: 400, statusText: 'Bad Request' }
      );

      try {
        await deletePromise;
      } catch (error) {
        expect(error.status).toBe(400);
      }
    });
  });

  describe('Pontos de Distribuição Workflow', () => {
    it('should manage pontos de distribuição within fornecedor', async () => {
      // Navigate to edit fornecedor
      await router.navigate(['/fornecedores/1']);
      fixture.detectChanges();

      const loadReq = httpMock.expectOne('/api/fornecedores/1');
      loadReq.flush(mockFornecedor);

      // Add new ponto de distribuição
      const updatedFornecedor = {
        ...mockFornecedor,
        pontosDistribuicao: [
          ...mockFornecedor.pontosDistribuicao,
          {
            id: 2,
            nome: 'Ponto Norte',
            endereco: {
              logradouro: 'Av. Norte',
              numero: '200',
              bairro: 'Norte',
              cidade: 'São Paulo',
              uf: 'SP',
              cep: '02000-000'
            },
            latitude: -23.4000,
            longitude: -46.5000
          }
        ]
      };

      const updatePromise = fornecedorService.update(1, updatedFornecedor);

      const updateReq = httpMock.expectOne('/api/fornecedores/1');
      expect(updateReq.request.body.pontosDistribuicao).toHaveLength(2);
      updateReq.flush(updatedFornecedor);

      const result = await updatePromise;
      expect(result.pontosDistribuicao).toHaveLength(2);
    });
  });

  describe('Complex Fornecedores Workflows', () => {
    it('should handle complete CRUD cycle', async () => {
      // 1. List fornecedores
      await router.navigate(['/fornecedores']);
      fixture.detectChanges();

      const listReq = httpMock.expectOne('/api/fornecedores');
      listReq.flush(mockFornecedoresList);

      // 2. Create new fornecedor
      await router.navigate(['/fornecedores/novo']);
      fixture.detectChanges();

      const newFornecedor = {
        nome: 'Ciclo Completo Ltda',
        cpfCnpj: '99999999000199',
        tipoCliente: 'PJ'
      };

      const createPromise = fornecedorService.create(newFornecedor);
      const createReq = httpMock.expectOne('/api/fornecedores');
      const createdFornecedor = { ...newFornecedor, id: 99, codigo: 'FORN099' };
      createReq.flush(createdFornecedor);

      await createPromise;

      // 3. Edit the created fornecedor
      await router.navigate(['/fornecedores/99']);
      fixture.detectChanges();

      const loadReq = httpMock.expectOne('/api/fornecedores/99');
      loadReq.flush(createdFornecedor);

      const updatedFornecedor = { ...createdFornecedor, nome: 'Ciclo Completo Ltda - Atualizada' };
      const updatePromise = fornecedorService.update(99, updatedFornecedor);
      const updateReq = httpMock.expectOne('/api/fornecedores/99');
      updateReq.flush(updatedFornecedor);

      await updatePromise;

      // 4. Delete the fornecedor
      const deletePromise = fornecedorService.delete(99);
      const deleteReq = httpMock.expectOne('/api/fornecedores/99');
      deleteReq.flush({});

      await deletePromise;

      // 5. Verify list is updated
      await router.navigate(['/fornecedores']);
      fixture.detectChanges();

      const finalListReq = httpMock.expectOne('/api/fornecedores');
      finalListReq.flush(mockFornecedoresList); // Original list without the deleted item
    });

    it('should handle navigation between list and detail views', async () => {
      // Start at list
      await router.navigate(['/fornecedores']);
      fixture.detectChanges();
      expect(location.path()).toBe('/fornecedores');

      const listReq = httpMock.expectOne('/api/fornecedores');
      listReq.flush(mockFornecedoresList);

      // Navigate to detail
      await router.navigate(['/fornecedores/1']);
      fixture.detectChanges();
      expect(location.path()).toBe('/fornecedores/1');

      const detailReq = httpMock.expectOne('/api/fornecedores/1');
      detailReq.flush(mockFornecedor);

      // Navigate back to list
      await router.navigate(['/fornecedores']);
      fixture.detectChanges();
      expect(location.path()).toBe('/fornecedores');

      const backListReq = httpMock.expectOne('/api/fornecedores');
      backListReq.flush(mockFornecedoresList);

      // Navigate to create new
      await router.navigate(['/fornecedores/novo']);
      fixture.detectChanges();
      expect(location.path()).toBe('/fornecedores/novo');
    });
  });
});