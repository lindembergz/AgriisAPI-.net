import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { UnidadeMedidaService } from './unidade-medida.service';
import { CriarUnidadeMedidaDto, AtualizarUnidadeMedidaDto, TipoUnidadeMedida } from '../../../../shared/models/reference.model';

/**
 * Integration test for UnidadeMedidaService
 * This test verifies that the service can be instantiated and configured correctly
 * without making actual HTTP calls
 */
describe('UnidadeMedidaService Integration', () => {
  let service: UnidadeMedidaService;
  let messageService: MessageService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        UnidadeMedidaService,
        MessageService
      ]
    });

    service = TestBed.inject(UnidadeMedidaService);
    messageService = TestBed.inject(MessageService);
  });

  it('should be created and injectable', () => {
    expect(service).toBeTruthy();
    expect(service).toBeInstanceOf(UnidadeMedidaService);
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

  it('should have basic search methods', () => {
    expect(typeof service.buscar).toBe('function');
  });

  it('should return Observable for all async methods', () => {
    expect(service.obterTodos().constructor.name).toBe('Observable');
    expect(service.obterAtivos().constructor.name).toBe('Observable');
    expect(service.obterPorId(1).constructor.name).toBe('Observable');
    expect(service.buscar({ termo: 'kg' }).constructor.name).toBe('Observable');
  });
});