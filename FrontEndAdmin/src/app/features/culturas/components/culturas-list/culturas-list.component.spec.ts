import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CulturasListComponent } from './culturas-list.component';
import { CulturaService } from '../../services/cultura.service';
import { CulturaDto } from '../../models/cultura.interface';
import { of, throwError } from 'rxjs';

describe('CulturasListComponent', () => {
  let component: CulturasListComponent;
  let fixture: ComponentFixture<CulturasListComponent>;
  let culturaService: jasmine.SpyObj<CulturaService>;
  let router: jasmine.SpyObj<Router>;
  let confirmationService: jasmine.SpyObj<ConfirmationService>;
  let messageService: jasmine.SpyObj<MessageService>;

  const mockCulturas: CulturaDto[] = [
    {
      id: 1,
      nome: 'Soja',
      descricao: 'Cultura de soja',
      ativo: true,
      dataCriacao: new Date('2024-01-01'),
      dataAtualizacao: new Date('2024-01-02')
    },
    {
      id: 2,
      nome: 'Milho',
      descricao: 'Cultura de milho',
      ativo: false,
      dataCriacao: new Date('2024-01-03'),
      dataAtualizacao: new Date('2024-01-04')
    }
  ];

  beforeEach(async () => {
    const culturaServiceSpy = jasmine.createSpyObj('CulturaService', [
      'obterTodas',
      'obterAtivas',
      'remover'
    ]);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    const confirmationServiceSpy = jasmine.createSpyObj('ConfirmationService', ['confirm']);
    const messageServiceSpy = jasmine.createSpyObj('MessageService', ['add']);

    await TestBed.configureTestingModule({
      imports: [
        CulturasListComponent,
        HttpClientTestingModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: CulturaService, useValue: culturaServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ConfirmationService, useValue: confirmationServiceSpy },
        { provide: MessageService, useValue: messageServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CulturasListComponent);
    component = fixture.componentInstance;
    culturaService = TestBed.inject(CulturaService) as jasmine.SpyObj<CulturaService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    confirmationService = TestBed.inject(ConfirmationService) as jasmine.SpyObj<ConfirmationService>;
    messageService = TestBed.inject(MessageService) as jasmine.SpyObj<MessageService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load all culturas on init', () => {
    culturaService.obterTodas.and.returnValue(of(mockCulturas));

    component.ngOnInit();

    expect(culturaService.obterTodas).toHaveBeenCalled();
    expect(component.culturas()).toEqual(mockCulturas);
    expect(component.loading()).toBeFalse();
  });

  it('should load only active culturas when filter is set to ativas', () => {
    const activeCulturas = mockCulturas.filter(c => c.ativo);
    culturaService.obterAtivas.and.returnValue(of(activeCulturas));
    
    component.selectedStatusFilter.set('ativas');
    component.carregarCulturas();

    expect(culturaService.obterAtivas).toHaveBeenCalled();
    expect(component.culturas()).toEqual(activeCulturas);
  });

  it('should filter inactive culturas when filter is set to inativas', () => {
    const inactiveCulturas = mockCulturas.filter(c => !c.ativo);
    culturaService.obterTodas.and.returnValue(of(mockCulturas));
    
    component.selectedStatusFilter.set('inativas');
    component.carregarCulturas();

    expect(culturaService.obterTodas).toHaveBeenCalled();
    expect(component.culturas()).toEqual(inactiveCulturas);
  });

  it('should navigate to create new cultura', () => {
    component.novaCultura();

    expect(router.navigate).toHaveBeenCalledWith(['/culturas/nova']);
  });

  it('should navigate to edit cultura', () => {
    const cultura = mockCulturas[0];
    
    component.editarCultura(cultura);

    expect(router.navigate).toHaveBeenCalledWith(['/culturas', cultura.id]);
  });

  it('should show confirmation dialog when deleting cultura', () => {
    const cultura = mockCulturas[0];
    
    component.excluirCultura(cultura);

    expect(confirmationService.confirm).toHaveBeenCalledWith(jasmine.objectContaining({
      message: `Tem certeza que deseja excluir a cultura "${cultura.nome}"?`,
      header: 'Confirmar Exclusão'
    }));
  });

  it('should handle error when loading culturas', () => {
    const error = new Error('API Error');
    culturaService.obterTodas.and.returnValue(throwError(() => error));
    spyOn(console, 'error');

    component.carregarCulturas();

    expect(console.error).toHaveBeenCalledWith('Erro ao carregar culturas:', error);
    expect(component.loading()).toBeFalse();
  });

  it('should format date correctly', () => {
    const date = new Date('2024-01-15');
    const formatted = component.formatarData(date);

    expect(formatted).toBe('15/01/2024');
  });

  it('should return correct status label', () => {
    expect(component.getStatusLabel(true)).toBe('Ativa');
    expect(component.getStatusLabel(false)).toBe('Inativa');
  });

  it('should return correct status severity', () => {
    expect(component.getStatusSeverity(true)).toBe('success');
    expect(component.getStatusSeverity(false)).toBe('danger');
  });

  it('should handle status filter change', () => {
    spyOn(component, 'carregarCulturas');
    const event = { value: 'ativas' };

    component.onStatusFilterChange(event);

    expect(component.selectedStatusFilter()).toBe('ativas');
    expect(component.carregarCulturas).toHaveBeenCalled();
  });
});