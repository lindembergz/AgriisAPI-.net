import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MessageService, ConfirmationService } from 'primeng/api';
import { PaisesComponent } from './paises.component';
import { PaisService } from './services/pais.service';
import { of } from 'rxjs';

describe('PaisesComponent', () => {
  let component: PaisesComponent;
  let fixture: ComponentFixture<PaisesComponent>;
  let service: jasmine.SpyObj<PaisService>;

  beforeEach(async () => {
    const serviceSpy = jasmine.createSpyObj('PaisService', [
      'obterTodos', 'obterAtivos', 'obterPorId', 'criar', 'atualizar', 'remover', 
      'ativar', 'desativar', 'podeRemover', 'verificarDependenciasUf'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        PaisesComponent,
        HttpClientTestingModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: PaisService, useValue: serviceSpy },
        MessageService,
        ConfirmationService
      ]
    }).compileComponents();

    service = TestBed.inject(PaisService) as jasmine.SpyObj<PaisService>;
    fixture = TestBed.createComponent(PaisesComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    service.obterTodos.and.returnValue(of([]));
    expect(component).toBeTruthy();
  });

  it('should load items on init', () => {
    const mockItems = [
      { id: 1, codigo: 'BR', nome: 'Brasil', ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() }
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
    expect(component.form.get('ativo')).toBeTruthy();
  });

  it('should map form to create DTO correctly', () => {
    const formValue = {
      codigo: 'us',
      nome: 'Estados Unidos',
      ativo: true
    };

    const dto = component['mapToCreateDto'](formValue);

    expect(dto.codigo).toBe('US');
    expect(dto.nome).toBe('Estados Unidos');
  });

  it('should map form to update DTO correctly', () => {
    const formValue = {
      codigo: 'US',
      nome: 'Estados Unidos',
      ativo: false
    };

    const dto = component['mapToUpdateDto'](formValue);

    expect(dto.nome).toBe('Estados Unidos');
    expect(dto.ativo).toBe(false);
  });

  it('should check UF dependencies before deletion', () => {
    const mockPais = { id: 1, codigo: 'BR', nome: 'Brasil', ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() };
    service.verificarDependenciasUf.and.returnValue(of(true));

    component.excluirItem(mockPais);

    expect(service.verificarDependenciasUf).toHaveBeenCalledWith(1);
  });
});