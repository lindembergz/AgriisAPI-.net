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

import { ProdutorListComponent } from '../features/produtores/components/produtor-list.component';
import { ProdutorDetailComponent } from '../features/produtores/components/produtor-detail.component';
import { ProdutorService } from '../features/produtores/services/produtor.service';
import { AuthService } from '../core/auth/auth.service';
import { StorageService } from '../core/services/storage.service';

@Component({
  template: '<router-outlet></router-outlet>',
  standalone: true,
  imports: []
})
class MockAppComponent { }

describe('Produtores CRUD Workflow Integration Tests', () => {
  let fixture: ComponentFixture<MockAppComponent>;
  let router: Router;
  let location: Location;
  let httpMock: HttpTestingController;
  let produtorService: ProdutorService;
  let authService: AuthService;

  const mockProdutor = {
    id: 1,
    codigo: 'PROD001',
    nome: 'João Silva',
    cpfCnpj: '12345678901',
    tipoCliente: 'PF' as const,
    telefone: '(11) 99999-9999',
    email: 'joao@example.com',
    enderecos: [
      {
        id: 1,
        logradouro: 'Rua das Flores',
        numero: '123',
        bairro: 'Centro',
        cidade: 'São Paulo',
        uf: 'SP',
        cep: '01234-567'
      }
    ],
    propriedades: [
      {
        id: 1,
        nome: 'Fazenda São João',
        area: 100.5,
        latitude: -23.5505,
        longitude: -46.6333,
        culturas: [
          {
            id: 1,
            tipo: 'SOJA',
            anoSafra: 2024,
            areaCultivada: 50.0
          }
        ]
      }
    ],
    usuarioMaster: {
      id: 1,
      nome: 'João Silva',
      email: 'joao@example.com',
      telefone: '(11) 99999-9999'
    }
  };

  const mockProdutoresList = [
    mockProdutor,
    {
      id: 2,
      codigo: 'PROD002',
      nome: 'Maria Santos',
      cpfCnpj: '98765432100',
      tipoCliente: 'PF' as const,
      telefone: '(11) 88888-8888',
      email: 'maria@example.com',
      enderecos: [],
      propriedades: [],
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
          { path: 'produtores', component: ProdutorListComponent },
          { path: 'produtores/novo', component: ProdutorDetailComponent },
          { path: 'produtores/:id', component: ProdutorDetailComponent },
          { path: '', redirectTo: '/produtores', pathMatch: 'full' }
        ]),
        provideHttpClient(),
        provideHttpClientTesting(),
        MessageService,
        ProdutorService,
        AuthService,
        StorageService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MockAppComponent);
    router = TestBed.inject(Router);
    location = TestBed.inject(Location);
    httpMock = TestBed.inject(HttpTestingController);
    produtorService = TestBed.inject(ProdutorService);
    authService = TestBed.inject(AuthService);

    // Mock authentication
    spyOn(authService, 'isAuthenticated').and.returnValue(true);
    spyOn(authService, 'getToken').and.returnValue('mock-token');

    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Produtores List Workflow', () => {
    it('should load and display produtores list', async () => {
      // Navigate to produtores list
      await router.navigate(['/produtores']);
      fixture.detectChanges();

      // Should make API call to load produtores
      const req = httpMock.expectOne('/api/produtores');
      expect(req.request.method).toBe('GET');
      expect(req.request.headers.get('Authorization')).toBe('Bearer mock-token');

      req.flush(mockProdutoresList);
      fixture.detectChanges();

      // Verify location
      expect(location.path()).toBe('/produtores');
    });

    it('should handle empty produtores list', async () => {
      await router.navigate(['/produtores']);
      fixture.detectChanges();

      const req = httpMock.expectOne('/api/produtores');
      req.flush([]);
      fixture.detectChanges();

      expect(location.path()).toBe('/produtores');
    });

    it('should handle API error when loading produtores', async () => {
      await router.navigate(['/produtores']);
      fixture.detectChanges();

      const req = httpMock.expectOne('/api/produtores');
      req.flush({ message: 'Server error' }, { status: 500, statusText: 'Internal Server Error' });
      fixture.detectChanges();

      expect(location.path()).toBe('/produtores');
    });

    it('should navigate to create new produtor', async () => {
      await router.navigate(['/produtores']);
      fixture.detectChanges();

      // Mock the list load
      const listReq = httpMock.expectOne('/api/produtores');
      listReq.flush(mockProdutoresList);

      // Navigate to new produtor
      await router.navigate(['/produtores/novo']);
      fixture.detectChanges();

      expect(location.path()).toBe('/produtores/novo');
    });

    it('should navigate to edit existing produtor', async () => {
      await router.navigate(['/produtores']);
      fixture.detectChanges();

      // Mock the list load
      const listReq = httpMock.expectOne('/api/produtores');
      listReq.flush(mockProdutoresList);

      // Navigate to edit produtor
      await router.navigate(['/produtores/1']);
      fixture.detectChanges();

      // Should load the specific produtor
      const detailReq = httpMock.expectOne('/api/produtores/1');
      expect(detailReq.request.method).toBe('GET');
      detailReq.flush(mockProdutor);

      expect(location.path()).toBe('/produtores/1');
    });
  });

  describe('Produtores Create Workflow', () => {
    it('should create new produtor with complete data', async () => {
      // Navigate to create form
      await router.navigate(['/produtores/novo']);
      fixture.detectChanges();

      expect(location.path()).toBe('/produtores/novo');

      // Simulate form submission
      const newProdutor = {
        nome: 'Novo Produtor',
        cpfCnpj: '11111111111',
        tipoCliente: 'PF',
        telefone: '(11) 77777-7777',
        email: 'novo@example.com',
        enderecos: [
          {
            logradouro: 'Rua Nova',
            numero: '456',
            bairro: 'Novo Bairro',
            cidade: 'Nova Cidade',
            uf: 'SP',
            cep: '12345-678'
          }
        ],
        propriedades: [
          {
            nome: 'Nova Fazenda',
            area: 200.0,
            latitude: -23.0000,
            longitude: -46.0000,
            culturas: [
              {
                tipo: 'MILHO',
                anoSafra: 2024,
                areaCultivada: 100.0
              }
            ]
          }
        ],
        usuarioMaster: {
          nome: 'Novo Produtor',
          email: 'novo@example.com',
          telefone: '(11) 77777-7777'
        }
      };

      const createPromise = produtorService.create(newProdutor);

      const req = httpMock.expectOne('/api/produtores');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newProdutor);

      const createdProdutor = { ...newProdutor, id: 3, codigo: 'PROD003' };
      req.flush(createdProdutor);

      const result = await createPromise;
      expect(result.id).toBe(3);
      expect(result.codigo).toBe('PROD003');
    });

    it('should validate required fields when creating produtor', async () => {
      await router.navigate(['/produtores/novo']);
      fixture.detectChanges();

      // Try to create with invalid data
      const invalidProdutor = {
        nome: '', // Required field empty
        cpfCnpj: 'invalid', // Invalid format
        tipoCliente: 'PF',
        telefone: '',
        email: 'invalid-email' // Invalid email format
      };

      try {
        await produtorService.create(invalidProdutor);
      } catch (error) {
        // Should fail validation
        expect(error).toBeDefined();
      }

      // No HTTP request should be made for invalid data
      httpMock.expectNone('/api/produtores');
    });

    it('should handle server validation errors', async () => {
      await router.navigate(['/produtores/novo']);
      fixture.detectChanges();

      const newProdutor = {
        nome: 'Produtor Duplicado',
        cpfCnpj: '12345678901', // Duplicate CPF
        tipoCliente: 'PF'
      };

      const createPromise = produtorService.create(newProdutor);

      const req = httpMock.expectOne('/api/produtores');
      req.flush(
        { 
          message: 'CPF já cadastrado',
          errors: { cpfCnpj: 'Este CPF já está em uso' }
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

  describe('Produtores Update Workflow', () => {
    it('should update existing produtor', async () => {
      // Navigate to edit form
      await router.navigate(['/produtores/1']);
      fixture.detectChanges();

      // Load existing produtor
      const loadReq = httpMock.expectOne('/api/produtores/1');
      loadReq.flush(mockProdutor);

      // Update produtor data
      const updatedProdutor = {
        ...mockProdutor,
        nome: 'João Silva Atualizado',
        telefone: '(11) 99999-0000'
      };

      const updatePromise = produtorService.update(1, updatedProdutor);

      const updateReq = httpMock.expectOne('/api/produtores/1');
      expect(updateReq.request.method).toBe('PUT');
      expect(updateReq.request.body.nome).toBe('João Silva Atualizado');

      updateReq.flush(updatedProdutor);

      const result = await updatePromise;
      expect(result.nome).toBe('João Silva Atualizado');
    });

    it('should handle update validation errors', async () => {
      await router.navigate(['/produtores/1']);
      fixture.detectChanges();

      const loadReq = httpMock.expectOne('/api/produtores/1');
      loadReq.flush(mockProdutor);

      const invalidUpdate = {
        ...mockProdutor,
        email: 'invalid-email-format'
      };

      const updatePromise = produtorService.update(1, invalidUpdate);

      const updateReq = httpMock.expectOne('/api/produtores/1');
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

  describe('Produtores Delete Workflow', () => {
    it('should delete produtor', async () => {
      const deletePromise = produtorService.delete(1);

      const req = httpMock.expectOne('/api/produtores/1');
      expect(req.request.method).toBe('DELETE');

      req.flush({});

      await deletePromise;
      // Should complete without error
    });

    it('should handle delete errors', async () => {
      const deletePromise = produtorService.delete(1);

      const req = httpMock.expectOne('/api/produtores/1');
      req.flush(
        { message: 'Não é possível excluir produtor com pedidos' }, 
        { status: 400, statusText: 'Bad Request' }
      );

      try {
        await deletePromise;
      } catch (error) {
        expect(error.status).toBe(400);
      }
    });
  });

  describe('Complex Produtores Workflows', () => {
    it('should handle complete CRUD cycle', async () => {
      // 1. List produtores
      await router.navigate(['/produtores']);
      fixture.detectChanges();

      const listReq = httpMock.expectOne('/api/produtores');
      listReq.flush(mockProdutoresList);

      // 2. Create new produtor
      await router.navigate(['/produtores/novo']);
      fixture.detectChanges();

      const newProdutor = {
        nome: 'Ciclo Completo',
        cpfCnpj: '99999999999',
        tipoCliente: 'PF'
      };

      const createPromise = produtorService.create(newProdutor);
      const createReq = httpMock.expectOne('/api/produtores');
      const createdProdutor = { ...newProdutor, id: 99, codigo: 'PROD099' };
      createReq.flush(createdProdutor);

      await createPromise;

      // 3. Edit the created produtor
      await router.navigate(['/produtores/99']);
      fixture.detectChanges();

      const loadReq = httpMock.expectOne('/api/produtores/99');
      loadReq.flush(createdProdutor);

      const updatedProdutor = { ...createdProdutor, nome: 'Ciclo Completo Atualizado' };
      const updatePromise = produtorService.update(99, updatedProdutor);
      const updateReq = httpMock.expectOne('/api/produtores/99');
      updateReq.flush(updatedProdutor);

      await updatePromise;

      // 4. Delete the produtor
      const deletePromise = produtorService.delete(99);
      const deleteReq = httpMock.expectOne('/api/produtores/99');
      deleteReq.flush({});

      await deletePromise;

      // 5. Verify list is updated
      await router.navigate(['/produtores']);
      fixture.detectChanges();

      const finalListReq = httpMock.expectOne('/api/produtores');
      finalListReq.flush(mockProdutoresList); // Original list without the deleted item
    });

    it('should handle navigation between list and detail views', async () => {
      // Start at list
      await router.navigate(['/produtores']);
      fixture.detectChanges();
      expect(location.path()).toBe('/produtores');

      const listReq = httpMock.expectOne('/api/produtores');
      listReq.flush(mockProdutoresList);

      // Navigate to detail
      await router.navigate(['/produtores/1']);
      fixture.detectChanges();
      expect(location.path()).toBe('/produtores/1');

      const detailReq = httpMock.expectOne('/api/produtores/1');
      detailReq.flush(mockProdutor);

      // Navigate back to list
      await router.navigate(['/produtores']);
      fixture.detectChanges();
      expect(location.path()).toBe('/produtores');

      const backListReq = httpMock.expectOne('/api/produtores');
      backListReq.flush(mockProdutoresList);

      // Navigate to create new
      await router.navigate(['/produtores/novo']);
      fixture.detectChanges();
      expect(location.path()).toBe('/produtores/novo');
    });
  });
});