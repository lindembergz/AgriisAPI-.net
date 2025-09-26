import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SafrasListComponent } from './safras-list.component';
import { SafraService } from '../../services/safra.service';
import { SafraDto, SafraAtualDto } from '../../models/safra.interface';

describe('SafrasListComponent', () => {
  let component: SafrasListComponent;
  let fixture: ComponentFixture<SafrasListComponent>;
  let mockSafraService: jasmine.SpyObj<SafraService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockConfirmationService: jasmine.SpyObj<ConfirmationService>;
  let mockMessageService: jasmine.SpyObj<MessageService>;

  const mockSafras: SafraDto[] = [
    {
      id: 1,
      plantioInicial: new Date('2024-01-15'),
      plantioFinal: new Date('2024-03-15'),
      plantioNome: 'Soja 2024',
      descricao: 'Safra de soja 2024',
      anoColheita: 2024,
      safraFormatada: '2024/2025 Soja 2024',
      atual: true,
      dataCriacao: new Date('2024-01-01'),
      dataAtualizacao: new Date('2024-01-02')
    },
    {
      id: 2,
      plantioInicial: new Date('2023-01-15'),
      plantioFinal: new Date('2023-03-15'),
      plantioNome: 'Milho 2023',
      descricao: 'Safra de milho 2023',
      anoColheita: 2023,
      safraFormatada: '2023/2024 Milho 2023',
      atual: false,
      dataCriacao: new Date('2023-01-01')
    }
  ];

  const mockSafraAtual: SafraAtualDto = {
    id: 1,
    descricao: 'Safra de soja 2024',
    safra: '2024/2025 Soja 2024'
  };

  beforeEach(async () => {
    const safraServiceSpy = jasmine.createSpyObj('SafraService', [
      'obterTodas',
      'obterAtual',
      'obterPorAnoColheita',
      'remover'
    ]);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    const confirmationServiceSpy = jasmine.createSpyObj('ConfirmationService', ['confirm']);
    const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);

    await TestBed.configureTestingModule({
      imports: [SafrasListComponent],
      providers: [
        { provide: SafraService, useValue: safraServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ConfirmationService, useValue: confirmationServiceSpy },
        { provide: MessageService, useValue: messageServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SafrasListComponent);
    component = fixture.componentInstance;
    mockSafraService = TestBed.inject(SafraService) as jasmine.SpyObj<SafraService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    mockConfirmationService = TestBed.inject(ConfirmationService) as jasmine.SpyObj<ConfirmationService>;
    mockMessageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;

    // Setup default mocks
    mockSafraService.obterTodas.and.returnValue(of(mockSafras));
    mockSafraService.obterAtual.and.returnValue(of(mockSafraAtual));
    mockSafraService.obterPorAnoColheita.and.returnValue(of(mockSafras));
    mockSafraService.remover.and.returnValue(of(void 0));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load safras on init', () => {
    component.ngOnInit();

    expect(mockSafraService.obterTodas).toHaveBeenCalled();
    expect(component.safras()).toEqual(mockSafras);
    expect(component.loading()).toBeFalse();
  });

  it('should load current safra on init', () => {
    component.ngOnInit();

    expect(mockSafraService.obterAtual).toHaveBeenCalled();
    expect(component.safraAtual()).toEqual(mockSafraAtual);
  });

  it('should generate year filter options', () => {
    component.ngOnInit();

    const anosFiltro = component.anosFiltro();
    expect(anosFiltro.length).toBeGreaterThan(1);
    expect(anosFiltro[0]).toEqual({ label: 'Todos os anos', value: null });
  });

  it('should filter safras by year', () => {
    component.ngOnInit();
    
    component.onFiltroAnoChange(2024);

    expect(component.anoSelecionado()).toBe(2024);
    expect(mockSafraService.obterPorAnoColheita).toHaveBeenCalledWith(2024);
  });

  it('should navigate to new safra page', () => {
    component.novaSafra();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/safras/nova']);
  });

  it('should navigate to edit safra page', () => {
    const safra = mockSafras[0];
    
    component.editarSafra(safra);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/safras', safra.id]);
  });

  it('should show confirmation dialog when deleting safra', () => {
    const safra = mockSafras[0];
    
    component.excluirSafra(safra);

    expect(mockConfirmationService.confirm).toHaveBeenCalledWith(
      jasmine.objectContaining({
        message: `Tem certeza que deseja excluir a safra "${safra.safraFormatada}"?`,
        header: 'Confirmar Exclusão'
      })
    );
  });

  it('should delete safra when confirmed', () => {
    const safra = mockSafras[0];
    mockConfirmationService.confirm.and.callFake((confirmation) => {
      if (confirmation.accept) {
        confirmation.accept();
      }
      return mockConfirmationService;
    });
    
    component.excluirSafra(safra);

    expect(mockSafraService.remover).toHaveBeenCalledWith(safra.id);
    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Safra excluída com sucesso'
    });
  });

  it('should identify current safra correctly', () => {
    component.ngOnInit();
    
    const isAtual = component.isSafraAtual(mockSafras[0]);
    const isNotAtual = component.isSafraAtual(mockSafras[1]);

    expect(isAtual).toBeTrue();
    expect(isNotAtual).toBeFalse();
  });

  it('should format date correctly', () => {
    const date = new Date('2024-01-15');
    
    const formatted = component.formatarData(date);

    expect(formatted).toBe('15/01/2024');
  });

  it('should return correct status text', () => {
    component.ngOnInit();
    
    const statusAtual = component.getStatusSafra(mockSafras[0]);
    const statusInativa = component.getStatusSafra(mockSafras[1]);

    expect(statusAtual).toBe('Atual');
    expect(statusInativa).toBe('Inativa');
  });

  it('should return correct status class', () => {
    component.ngOnInit();
    
    const classAtual = component.getStatusClass(mockSafras[0]);
    const classInativa = component.getStatusClass(mockSafras[1]);

    expect(classAtual).toBe('status-atual');
    expect(classInativa).toBe('status-inativa');
  });

  it('should handle error when loading safras', () => {
    mockSafraService.obterTodas.and.returnValue(throwError(() => new Error('API Error')));
    
    component.ngOnInit();

    expect(component.loading()).toBeFalse();
    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'error',
      summary: 'Erro',
      detail: 'Erro ao carregar safras'
    });
  });

  it('should handle error when deleting safra', () => {
    const safra = mockSafras[0];
    mockSafraService.remover.and.returnValue(throwError(() => new Error('API Error')));
    mockConfirmationService.confirm.and.callFake((confirmation) => {
      if (confirmation.accept) {
        confirmation.accept();
      }
      return mockConfirmationService;
    });
    
    component.excluirSafra(safra);

    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'error',
      summary: 'Erro',
      detail: 'Erro ao excluir safra'
    });
  });

  it('should handle missing current safra gracefully', () => {
    mockSafraService.obterAtual.and.returnValue(throwError(() => new Error('No current safra')));
    spyOn(console, 'warn');
    
    component.ngOnInit();

    expect(console.warn).toHaveBeenCalledWith('Nenhuma safra atual encontrada');
    expect(component.safraAtual()).toBeNull();
  });
});