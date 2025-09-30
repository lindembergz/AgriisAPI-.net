import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MessageService, ConfirmationService } from 'primeng/api';
import { MoedasComponent } from './moedas.component';
import { MoedaService } from './services/moeda.service';
import { of } from 'rxjs';

describe('MoedasComponent', () => {
  let component: MoedasComponent;
  let fixture: ComponentFixture<MoedasComponent>;
  let service: jasmine.SpyObj<MoedaService>;

  beforeEach(async () => {
    const serviceSpy = jasmine.createSpyObj('MoedaService', [
      'obterTodos', 'obterAtivos', 'obterPorId', 'criar', 'atualizar', 'remover', 'ativar', 'desativar', 'podeRemover'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        MoedasComponent,
        HttpClientTestingModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: MoedaService, useValue: serviceSpy },
        MessageService,
        ConfirmationService
      ]
    }).compileComponents();

    service = TestBed.inject(MoedaService) as jasmine.SpyObj<MoedaService>;
    fixture = TestBed.createComponent(MoedasComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    service.obterTodos.and.returnValue(of([]));
    expect(component).toBeTruthy();
  });

  it('should load items on init', () => {
    const mockItems = [
      { id: 1, codigo: 'BRL', nome: 'Real Brasileiro', simbolo: 'R$', ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() }
    ];
    service.obterTodos.and.returnValue(of(mockItems));

    component.ngOnInit();

    expect(service.obterTodos).toHaveBeenCalled();
    expect(component.items()).toEqual(mockItems);
  });

  it('should create form with proper validation', () => {
    service.obterTodos.and.returnValue(of([]));
    component.ngOnInit();

    expect(component.form.get('codigo')).toBeTruthy();
    expect(component.form.get('nome')).toBeTruthy();
    expect(component.form.get('simbolo')).toBeTruthy();
    expect(component.form.get('ativo')).toBeTruthy();
  });

  it('should map form to create DTO correctly', () => {
    const formValue = {
      codigo: 'usd',
      nome: 'Dólar Americano',
      simbolo: 'US$',
      ativo: true
    };

    const dto = component['mapToCreateDto'](formValue);

    expect(dto.codigo).toBe('USD');
    expect(dto.nome).toBe('Dólar Americano');
    expect(dto.simbolo).toBe('US$');
  });

  it('should map form to update DTO correctly', () => {
    const formValue = {
      codigo: 'USD',
      nome: 'Dólar Americano',
      simbolo: 'US$',
      ativo: false
    };

    const dto = component['mapToUpdateDto'](formValue);

    expect(dto.nome).toBe('Dólar Americano');
    expect(dto.simbolo).toBe('US$');
    expect(dto.ativo).toBe(false);
  });
});