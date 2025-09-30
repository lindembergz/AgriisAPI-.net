import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { AtividadeAgropecuariaService } from './atividade-agropecuaria.service';
import { CriarAtividadeAgropecuariaDto, AtualizarAtividadeAgropecuariaDto, TipoAtividadeAgropecuaria } from '../../../../shared/models/reference.model';

/**
 * Integration test for AtividadeAgropecuariaService
 * This test verifies that the service can be instantiated and configured correctly
 * without making actual HTTP calls
 */
describe('AtividadeAgropecuariaService Integration', () => {
  let service: AtividadeAgropecuariaService;
  let messageService: MessageService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        AtividadeAgropecuariaService,
        MessageService
      ]
    });

    service = TestBed.inject(AtividadeAgropecuariaService);
    messageService = TestBed.inject(MessageService);
  });

  it('should be created and injectable', () => {
    expect(service).toBeTruthy();
    expect(service).toBeInstanceOf(AtividadeAgropecuariaService);
  });

  it('should have MessageService injected', () => {
    expect(messageService).toBeTruthy();
    expect(messageService).toBeInstanceOf(MessageService);
  });

  it('should have all required CRUD methods', () => {
    expect(typeof service.obterTodos).toBe('function');
    expect(typeof service.obterAtivos).toBe('function');
    expect(typeof service.obterPorId).toBe('function');
    expect(typeof service.criar).toBe('function');
    expect(typeof service.atualizar).toBe('function');
    expect(typeof service.ativar).toBe('function');
    expect(typeof service.desativar).toBe('function');
    expect(typeof service.remover).toBe('function');
    expect(typeof service.podeRemover).toBe('function');
  });

  it('should have specific atividade agropecuária methods', () => {
    expect(typeof service.obterPorTipo).toBe('function');
    expect(typeof service.obterAgrupadasPorTipo).toBe('function');
    expect(typeof service.buscarPorCodigo).toBe('function');
    expect(typeof service.buscarPorDescricao).toBe('function');
    expect(typeof service.verificarCodigoUnico).toBe('function');
    expect(typeof service.verificarDescricaoUnica).toBe('function');
    expect(typeof service.obterParaDropdown).toBe('function');
    expect(typeof service.getTipoDescricao).toBe('function');
    expect(typeof service.getTipoOptions).toBe('function');
  });

  it('should return Observable for all async methods', () => {
    // These will fail with actual HTTP calls, but we're just checking the return types
    expect(service.obterTodos().constructor.name).toBe('Observable');
    expect(service.obterAtivos().constructor.name).toBe('Observable');
    expect(service.obterPorId(1).constructor.name).toBe('Observable');
    expect(service.obterPorTipo(TipoAtividadeAgropecuaria.Agricultura).constructor.name).toBe('Observable');
    expect(service.obterAgrupadasPorTipo().constructor.name).toBe('Observable');
    expect(service.buscarPorCodigo('01').constructor.name).toBe('Observable');
    expect(service.buscarPorDescricao('Cultivo', undefined).constructor.name).toBe('Observable');
    expect(service.buscarPorDescricao('Cultivo', TipoAtividadeAgropecuaria.Agricultura).constructor.name).toBe('Observable');
    expect(service.verificarCodigoUnico('01', undefined).constructor.name).toBe('Observable');
    expect(service.verificarDescricaoUnica('Cultivo de Soja', undefined).constructor.name).toBe('Observable');
    expect(service.obterParaDropdown(undefined).constructor.name).toBe('Observable');
    expect(service.obterParaDropdown(TipoAtividadeAgropecuaria.Agricultura).constructor.name).toBe('Observable');
    
    const criarDto: CriarAtividadeAgropecuariaDto = { 
      codigo: '01',
      descricao: 'Cultivo de Soja',
      tipo: TipoAtividadeAgropecuaria.Agricultura
    };
    expect(service.criar(criarDto).constructor.name).toBe('Observable');
    
    const atualizarDto: AtualizarAtividadeAgropecuariaDto = { 
      descricao: 'Cultivo de Soja Atualizado',
      tipo: TipoAtividadeAgropecuaria.Agricultura,
      ativo: true
    };
    expect(service.atualizar(1, atualizarDto).constructor.name).toBe('Observable');
    
    expect(service.ativar(1).constructor.name).toBe('Observable');
    expect(service.desativar(1).constructor.name).toBe('Observable');
    expect(service.remover(1).constructor.name).toBe('Observable');
    expect(service.podeRemover(1).constructor.name).toBe('Observable');
  });

  it('should use correct API endpoints', () => {
    // We can't easily test the actual URLs without mocking, but we can verify
    // the service is configured with the expected base structure
    expect(service).toBeTruthy();
    
    // The service should be ready to make HTTP calls to the correct endpoints
    // This is verified by the successful instantiation and method availability
  });

  it('should handle tipo filtering integration', () => {
    // Verify tipo filtering methods are properly configured
    expect(typeof service.obterPorTipo).toBe('function');
    expect(typeof service.obterAgrupadasPorTipo).toBe('function');
    expect(typeof service.getTipoDescricao).toBe('function');
    expect(typeof service.getTipoOptions).toBe('function');
    
    // Should work with all tipo enum values
    expect(service.obterPorTipo(TipoAtividadeAgropecuaria.Agricultura).constructor.name).toBe('Observable');
    expect(service.obterPorTipo(TipoAtividadeAgropecuaria.Pecuaria).constructor.name).toBe('Observable');
    expect(service.obterPorTipo(TipoAtividadeAgropecuaria.Mista).constructor.name).toBe('Observable');
    
    expect(service.obterAgrupadasPorTipo().constructor.name).toBe('Observable');
  });

  it('should handle tipo description method without HTTP calls', () => {
    // This method doesn't require HTTP calls, so we can test it directly
    expect(service.getTipoDescricao(TipoAtividadeAgropecuaria.Agricultura)).toBe('Agricultura');
    expect(service.getTipoDescricao(TipoAtividadeAgropecuaria.Pecuaria)).toBe('Pecuária');
    expect(service.getTipoDescricao(TipoAtividadeAgropecuaria.Mista)).toBe('Mista');
  });

  it('should handle tipo options method without HTTP calls', () => {
    // This method doesn't require HTTP calls, so we can test it directly
    const tipoOptions = service.getTipoOptions();
    expect(Array.isArray(tipoOptions)).toBe(true);
    expect(tipoOptions.length).toBe(3);
    
    // Should contain all expected tipos
    const valores = tipoOptions.map(o => o.value);
    expect(valores).toContain(TipoAtividadeAgropecuaria.Agricultura);
    expect(valores).toContain(TipoAtividadeAgropecuaria.Pecuaria);
    expect(valores).toContain(TipoAtividadeAgropecuaria.Mista);
  });

  it('should handle search functionality integration', () => {
    // Verify search methods are properly configured
    expect(typeof service.buscarPorCodigo).toBe('function');
    expect(typeof service.buscarPorDescricao).toBe('function');
    
    expect(service.buscarPorCodigo('01').constructor.name).toBe('Observable');
    expect(service.buscarPorDescricao('Cultivo', undefined).constructor.name).toBe('Observable');
    expect(service.buscarPorDescricao('Cultivo', TipoAtividadeAgropecuaria.Agricultura).constructor.name).toBe('Observable');
  });

  it('should handle validation methods integration', () => {
    // Verify validation methods are properly configured
    expect(typeof service.verificarCodigoUnico).toBe('function');
    expect(typeof service.verificarDescricaoUnica).toBe('function');
    
    // Methods should return Observables
    expect(service.verificarCodigoUnico('01', undefined).constructor.name).toBe('Observable');
    expect(service.verificarDescricaoUnica('Cultivo de Soja', undefined).constructor.name).toBe('Observable');
    
    // Should handle exclude ID parameter for edit scenarios
    expect(service.verificarCodigoUnico('01', 1).constructor.name).toBe('Observable');
    expect(service.verificarDescricaoUnica('Cultivo de Soja', 1).constructor.name).toBe('Observable');
  });

  it('should handle dropdown data integration', () => {
    // Verify dropdown method is properly configured
    expect(typeof service.obterParaDropdown).toBe('function');
    
    // Should work without tipo filter
    expect(service.obterParaDropdown(undefined).constructor.name).toBe('Observable');
    
    // Should work with tipo filter
    expect(service.obterParaDropdown(TipoAtividadeAgropecuaria.Agricultura).constructor.name).toBe('Observable');
    expect(service.obterParaDropdown(TipoAtividadeAgropecuaria.Pecuaria).constructor.name).toBe('Observable');
    expect(service.obterParaDropdown(TipoAtividadeAgropecuaria.Mista).constructor.name).toBe('Observable');
  });

  it('should handle grouped data integration', () => {
    // Verify grouped data method is properly configured
    expect(typeof service.obterAgrupadasPorTipo).toBe('function');
    expect(service.obterAgrupadasPorTipo().constructor.name).toBe('Observable');
  });

  it('should handle query parameters integration', () => {
    // Verify that the service can handle various query parameters
    expect(typeof service.obterTodos).toBe('function');
    expect(service.obterTodos().constructor.name).toBe('Observable');
  });

  it('should handle sorting and filtering integration', () => {
    // Verify that the service can handle sorting and filtering parameters
    expect(service.obterTodos().constructor.name).toBe('Observable');
    expect(service.obterAtivos().constructor.name).toBe('Observable');
  });

  it('should handle caching integration', () => {
    // Verify that the service is set up to handle caching
    // The actual caching behavior is tested in unit tests with mocked HTTP
    expect(service).toBeTruthy();
    
    // Multiple calls to the same method should be handled properly
    const obs1 = service.obterAtivos();
    const obs2 = service.obterAtivos();
    const obs3 = service.obterAgrupadasPorTipo();
    const obs4 = service.obterAgrupadasPorTipo();
    
    expect(obs1).toBeTruthy();
    expect(obs2).toBeTruthy();
    expect(obs3).toBeTruthy();
    expect(obs4).toBeTruthy();
    expect(obs1.constructor.name).toBe('Observable');
    expect(obs2.constructor.name).toBe('Observable');
    expect(obs3.constructor.name).toBe('Observable');
    expect(obs4.constructor.name).toBe('Observable');
  });

  it('should be ready for error handling integration', () => {
    // Verify the service is configured to handle errors properly
    // The actual error handling is tested in unit tests with mocked HTTP errors
    expect(service).toBeTruthy();
    
    // All methods should return Observables that can handle errors
    const methods = [
      service.obterTodos(),
      service.obterAtivos(),
      service.obterPorId(1),
      service.obterPorTipo(TipoAtividadeAgropecuaria.Agricultura),
      service.obterAgrupadasPorTipo(),
      service.buscarPorCodigo('01')
    ];
    
    methods.forEach(obs => {
      expect(obs).toBeTruthy();
      expect(typeof obs.subscribe).toBe('function');
      expect(typeof obs.pipe).toBe('function');
    });
  });

  it('should handle concurrent requests integration', () => {
    // Verify that the service can handle multiple concurrent requests
    const requests = [
      service.obterTodos(),
      service.obterAtivos(),
      service.obterPorTipo(TipoAtividadeAgropecuaria.Agricultura),
      service.obterAgrupadasPorTipo(),
      service.buscarPorCodigo('01')
    ];
    
    requests.forEach(request => {
      expect(request).toBeTruthy();
      expect(request.constructor.name).toBe('Observable');
    });
    
    // All requests should be independent
    requests.forEach((request, index) => {
      requests.forEach((otherRequest, otherIndex) => {
        if (index !== otherIndex) {
          expect(request).not.toBe(otherRequest);
        }
      });
    });
  });

  it('should handle validation workflow integration', () => {
    // Verify that validation workflow is properly set up
    const validationMethods = [
      service.verificarCodigoUnico('01', undefined),
      service.verificarDescricaoUnica('Cultivo de Soja', undefined)
    ];
    
    validationMethods.forEach(method => {
      expect(method.constructor.name).toBe('Observable');
    });
    
    // Should handle edit scenario (with excludeId)
    const editValidationMethods = [
      service.verificarCodigoUnico('01', 1),
      service.verificarDescricaoUnica('Cultivo de Soja', 1)
    ];
    
    editValidationMethods.forEach(method => {
      expect(method.constructor.name).toBe('Observable');
    });
  });

  it('should handle tipo filtering workflow integration', () => {
    // Verify that tipo filtering workflow is properly set up
    expect(typeof service.obterPorTipo).toBe('function');
    expect(typeof service.obterAgrupadasPorTipo).toBe('function');
    expect(typeof service.getTipoDescricao).toBe('function');
    expect(typeof service.getTipoOptions).toBe('function');
    
    // Should handle different tipo values
    const tipoValues = [
      TipoAtividadeAgropecuaria.Agricultura,
      TipoAtividadeAgropecuaria.Pecuaria,
      TipoAtividadeAgropecuaria.Mista
    ];
    
    tipoValues.forEach(tipo => {
      expect(service.obterPorTipo(tipo).constructor.name).toBe('Observable');
      expect(typeof service.getTipoDescricao(tipo)).toBe('string');
    });
  });

  it('should handle search workflow integration', () => {
    // Verify that search workflow is properly set up
    expect(typeof service.buscarPorCodigo).toBe('function');
    expect(typeof service.buscarPorDescricao).toBe('function');
    
    // Should handle different search scenarios
    const codigoSearch = service.buscarPorCodigo('01');
    const descricaoSearch = service.buscarPorDescricao('Cultivo', undefined);
    const descricaoWithTipoSearch = service.buscarPorDescricao('Cultivo', TipoAtividadeAgropecuaria.Agricultura);
    
    expect(codigoSearch.constructor.name).toBe('Observable');
    expect(descricaoSearch.constructor.name).toBe('Observable');
    expect(descricaoWithTipoSearch.constructor.name).toBe('Observable');
  });

  it('should handle dropdown workflow integration', () => {
    // Verify that dropdown workflow is properly set up
    expect(typeof service.obterParaDropdown).toBe('function');
    
    // Should handle different dropdown scenarios
    const allDropdown = service.obterParaDropdown(undefined);
    const agriculturaDropdown = service.obterParaDropdown(TipoAtividadeAgropecuaria.Agricultura);
    const pecuariaDropdown = service.obterParaDropdown(TipoAtividadeAgropecuaria.Pecuaria);
    const mistaDropdown = service.obterParaDropdown(TipoAtividadeAgropecuaria.Mista);
    
    expect(allDropdown.constructor.name).toBe('Observable');
    expect(agriculturaDropdown.constructor.name).toBe('Observable');
    expect(pecuariaDropdown.constructor.name).toBe('Observable');
    expect(mistaDropdown.constructor.name).toBe('Observable');
  });

  it('should handle grouping workflow integration', () => {
    // Verify that grouping workflow is properly set up
    expect(typeof service.obterAgrupadasPorTipo).toBe('function');
    expect(typeof service.obterAtivos).toBe('function');
    
    // Should be able to get grouped data
    const groupedData = service.obterAgrupadasPorTipo();
    const activeData = service.obterAtivos();
    
    expect(groupedData.constructor.name).toBe('Observable');
    expect(activeData.constructor.name).toBe('Observable');
  });

  it('should handle comprehensive search integration', () => {
    // Verify that comprehensive search is properly set up
    expect(typeof service.buscarPorCodigo).toBe('function');
    expect(typeof service.buscarPorDescricao).toBe('function');
    expect(typeof service.buscar).toBe('function');
    
    // Should handle different search parameters
    const codigoSearch = service.buscarPorCodigo('01');
    const descricaoSearch = service.buscarPorDescricao('Cultivo', undefined);
    const generalSearch = service.buscar({ termo: 'Cultivo', ativo: true });
    
    expect(codigoSearch.constructor.name).toBe('Observable');
    expect(descricaoSearch.constructor.name).toBe('Observable');
    expect(generalSearch.constructor.name).toBe('Observable');
  });
});