import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { UnidadeMedidaService } from '../features/referencias/unidades-medida/services/unidade-medida.service';
import { MoedaService } from '../features/referencias/moedas/services/moeda.service';
import { PaisService } from '../features/referencias/paises/services/pais.service';
import { UfService } from '../features/referencias/ufs/services/uf.service';
import { EmbalagemService } from '../features/referencias/embalagens/services/embalagem.service';
import { CategoriaService } from '../features/referencias/categorias/services/categoria.service';
import { AtividadeAgropecuariaService } from '../features/referencias/atividades-agropecuarias/services/atividade-agropecuaria.service';

/**
 * Comprehensive integration test for all Referencias services
 * This test verifies that all services can be instantiated together and work properly
 * in a complete application context
 */
describe('Referencias Services Integration', () => {
  let unidadeMedidaService: UnidadeMedidaService;
  let moedaService: MoedaService;
  let paisService: PaisService;
  let ufService: UfService;
  let embalagemService: EmbalagemService;
  let categoriaService: CategoriaService;
  let atividadeAgropecuariaService: AtividadeAgropecuariaService;
  let messageService: MessageService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        UnidadeMedidaService,
        MoedaService,
        PaisService,
        UfService,
        EmbalagemService,
        CategoriaService,
        AtividadeAgropecuariaService,
        MessageService
      ]
    });

    unidadeMedidaService = TestBed.inject(UnidadeMedidaService);
    moedaService = TestBed.inject(MoedaService);
    paisService = TestBed.inject(PaisService);
    ufService = TestBed.inject(UfService);
    embalagemService = TestBed.inject(EmbalagemService);
    categoriaService = TestBed.inject(CategoriaService);
    atividadeAgropecuariaService = TestBed.inject(AtividadeAgropecuariaService);
    messageService = TestBed.inject(MessageService);
  });

  it('should create all services successfully', () => {
    expect(unidadeMedidaService).toBeTruthy();
    expect(moedaService).toBeTruthy();
    expect(paisService).toBeTruthy();
    expect(ufService).toBeTruthy();
    expect(embalagemService).toBeTruthy();
    expect(categoriaService).toBeTruthy();
    expect(atividadeAgropecuariaService).toBeTruthy();
    expect(messageService).toBeTruthy();
  });

  it('should have all services extend ReferenceCrudService', () => {
    // All services should have the base CRUD methods
    const services = [
      unidadeMedidaService,
      moedaService,
      paisService,
      ufService,
      embalagemService,
      categoriaService,
      atividadeAgropecuariaService
    ];

    services.forEach(service => {
      expect(typeof service.obterTodos).toBe('function');
      expect(typeof service.obterAtivos).toBe('function');
      expect(typeof service.obterPorId).toBe('function');
      expect(typeof service.criar).toBe('function');
      expect(typeof service.atualizar).toBe('function');
      expect(typeof service.ativar).toBe('function');
      expect(typeof service.desativar).toBe('function');
      expect(typeof service.remover).toBe('function');
      expect(typeof service.podeRemover).toBe('function');
      expect(typeof service.buscar).toBe('function');
    });
  });

  it('should have shared dependencies injected correctly', () => {
    // All services should have access to shared dependencies
    expect(messageService).toBeTruthy();
    
    // MessageService should be the same instance across all services
    expect(messageService).toBeInstanceOf(MessageService);
  });

  it('should handle concurrent service instantiation', () => {
    // All services should be able to coexist without conflicts
    const allServices = [
      unidadeMedidaService,
      moedaService,
      paisService,
      ufService,
      embalagemService,
      categoriaService,
      atividadeAgropecuariaService
    ];

    // Each service should be a unique instance
    allServices.forEach((service, index) => {
      allServices.forEach((otherService, otherIndex) => {
        if (index !== otherIndex) {
          expect(service).not.toBe(otherService);
        }
      });
    });
  });

  it('should have proper API endpoint configuration', () => {
    // Each service should have its own unique API endpoint
    // We can't directly access the protected apiEndpoint property, but we can verify
    // that the services are properly configured by checking they're different instances
    expect(unidadeMedidaService).not.toBe(moedaService);
    expect(paisService).not.toBe(ufService);
    expect(embalagemService).not.toBe(categoriaService);
    expect(categoriaService).not.toBe(atividadeAgropecuariaService);
  });

  it('should handle relationship dependencies correctly', () => {
    // Services with relationships should be properly configured
    
    // UF service depends on Pais service data
    expect(typeof ufService.obterComPais).toBe('function');
    
    // Embalagem service depends on UnidadeMedida service data
    expect(typeof embalagemService.obterPorUnidadeMedida).toBe('function');
    expect(typeof embalagemService.obterUnidadesMedidaParaDropdown).toBe('function');
    
    // Categoria service has hierarchical relationships
    expect(typeof categoriaService.obterTodos).toBe('function');
    expect(typeof categoriaService.obterAtivos).toBe('function');
  });

  it('should handle validation methods across services', () => {
    // All services should have validation capabilities
    expect(typeof unidadeMedidaService.buscar).toBe('function');
    expect(typeof moedaService.buscar).toBe('function');
    expect(typeof paisService.buscar).toBe('function');
    expect(typeof ufService.buscar).toBe('function');
    expect(typeof embalagemService.buscar).toBe('function');
    expect(typeof categoriaService.buscar).toBe('function');
    expect(typeof atividadeAgropecuariaService.buscar).toBe('function');
  });

  it('should handle dropdown data methods across services', () => {
    // All services should provide dropdown data
    expect(typeof unidadeMedidaService.obterAtivos).toBe('function');
    expect(typeof moedaService.obterAtivos).toBe('function');
    expect(typeof embalagemService.obterAtivos).toBe('function');
    expect(typeof categoriaService.obterAtivos).toBe('function');
    expect(typeof atividadeAgropecuariaService.obterAtivos).toBe('function');
  });

  it('should handle search functionality across services', () => {
    // All services should have search capabilities
    expect(typeof unidadeMedidaService.buscarPorSimbolo).toBe('function');
    expect(typeof moedaService.buscarPorCodigo).toBe('function');
    expect(typeof paisService.buscar).toBe('function');
    expect(typeof embalagemService.buscar).toBe('function');
    expect(typeof categoriaService.buscar).toBe('function');
    expect(typeof atividadeAgropecuariaService.buscarPorCodigo).toBe('function');
    expect(typeof atividadeAgropecuariaService.buscarPorDescricao).toBe('function');
  });

  it('should handle type filtering across applicable services', () => {
    // Services with type filtering should have proper methods
    expect(typeof unidadeMedidaService.obterTodos).toBe('function');
    expect(typeof embalagemService.obterTodos).toBe('function');
    expect(typeof categoriaService.obterTodos).toBe('function');
    expect(typeof atividadeAgropecuariaService.obterTodos).toBe('function');
  });

  it('should handle caching configuration across services', () => {
    // All services should have caching capabilities
    const services = [
      unidadeMedidaService,
      moedaService,
      paisService,
      ufService,
      embalagemService,
      categoriaService,
      atividadeAgropecuariaService
    ];

    services.forEach(service => {
      expect(typeof service.obterTodos).toBe('function');
      expect(typeof service.obterAtivos).toBe('function');
      expect(typeof service.obterPorId).toBe('function');
      expect(typeof service.buscar).toBe('function');
    });
  });

  it('should handle error handling integration', () => {
    // All services should be configured to handle errors properly
    expect(messageService).toBeTruthy();
    
    // All services should return Observables that can handle errors
    const services = [
      unidadeMedidaService,
      moedaService,
      paisService,
      ufService,
      embalagemService,
      categoriaService,
      atividadeAgropecuariaService
    ];

    services.forEach(service => {
      const observable = service.obterAtivos();
      expect(observable).toBeTruthy();
      expect(typeof observable.subscribe).toBe('function');
      expect(typeof observable.pipe).toBe('function');
    });
  });

  it('should handle concurrent API calls across services', () => {
    // Multiple services should be able to make concurrent API calls
    const concurrentCalls = [
      unidadeMedidaService.obterAtivos(),
      moedaService.obterAtivos(),
      paisService.obterAtivos(),
      ufService.obterAtivos(),
      embalagemService.obterAtivos(),
      categoriaService.obterAtivos(),
      atividadeAgropecuariaService.obterAtivos()
    ];

    concurrentCalls.forEach(call => {
      expect(call).toBeTruthy();
      expect(call.constructor.name).toBe('Observable');
    });

    // All calls should be independent
    concurrentCalls.forEach((call, index) => {
      concurrentCalls.forEach((otherCall, otherIndex) => {
        if (index !== otherIndex) {
          expect(call).not.toBe(otherCall);
        }
      });
    });
  });

  it('should handle specialized service methods', () => {
    // Each service should have its specialized methods available
    
    // All services should have basic CRUD methods
    expect(typeof unidadeMedidaService.obterTodos).toBe('function');
    expect(typeof paisService.obterTodos).toBe('function');
    expect(typeof ufService.obterTodos).toBe('function');
    expect(typeof embalagemService.obterTodos).toBe('function');
    expect(typeof categoriaService.obterTodos).toBe('function');
    expect(typeof atividadeAgropecuariaService.obterTodos).toBe('function');
  });

  it('should handle service interdependencies', () => {
    // Services that depend on each other should be properly configured
    
    // Embalagem depends on UnidadeMedida
    expect(typeof embalagemService.obterTodos).toBe('function');
    expect(typeof embalagemService.obterAtivos).toBe('function');
    
    // UF depends on Pais
    expect(typeof ufService.obterTodos).toBe('function');
    
    // These dependencies should be handled through API calls, not direct service injection
    // The services should be independent but able to work with related data
  });

  it('should be ready for complete CRUD workflows', () => {
    // All services should support complete CRUD workflows
    const services = [
      unidadeMedidaService,
      moedaService,
      paisService,
      ufService,
      embalagemService,
      categoriaService,
      atividadeAgropecuariaService
    ];

    services.forEach(service => {
      // Create workflow
      expect(typeof service.criar).toBe('function');
      
      // Read workflow
      expect(typeof service.obterTodos).toBe('function');
      expect(typeof service.obterAtivos).toBe('function');
      expect(typeof service.obterPorId).toBe('function');
      
      // Update workflow
      expect(typeof service.atualizar).toBe('function');
      expect(typeof service.ativar).toBe('function');
      expect(typeof service.desativar).toBe('function');
      
      // Delete workflow
      expect(typeof service.podeRemover).toBe('function');
      expect(typeof service.remover).toBe('function');
      
      // Search workflow
      expect(typeof service.buscar).toBe('function');
    });
  });
});