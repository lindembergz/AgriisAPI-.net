import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';

import { AtividadesAgropecuariasComponent } from './atividades-agropecuarias.component';
import { AtividadeAgropecuariaService } from './services/atividade-agropecuaria.service';
import { 
  AtividadeAgropecuariaDto, 
  TipoAtividadeAgropecuaria 
} from '../../../shared/models/reference.model';

describe('AtividadesAgropecuariasComponent', () => {
  let component: AtividadesAgropecuariasComponent;
  let fixture: ComponentFixture<AtividadesAgropecuariasComponent>;
  let service: jasmine.SpyObj<AtividadeAgropecuariaService>;

  const mockAtividade: AtividadeAgropecuariaDto = {
    id: 1,
    codigo: 'AGR001',
    descricao: 'Cultivo de soja para produção de grãos',
    tipo: TipoAtividadeAgropecuaria.Agricultura,
    tipoDescricao: 'Agricultura',
    ativo: true,
    dataCriacao: new Date('2024-01-01'),
    dataAtualizacao: null,
    rowVersion: new Uint8Array()
  };

  beforeEach(async () => {
    const serviceSpy = jasmine.createSpyObj('AtividadeAgropecuariaService', [
      'obterTodos',
      'obterAtivos',
      'obterPorId',
      'criar',
      'atualizar',
      'remover',
      'obterPorTipo',
      'obterAgrupadasPorTipo',
      'getTipoOptions',
      'getTipoDescricao'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        AtividadesAgropecuariasComponent,
        ReactiveFormsModule,
        NoopAnimationsModule,
        HttpClientTestingModule
      ],
      providers: [
        { provide: AtividadeAgropecuariaService, useValue: serviceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AtividadesAgropecuariasComponent);
    component = fixture.componentInstance;
    service = TestBed.inject(AtividadeAgropecuariaService) as jasmine.SpyObj<AtividadeAgropecuariaService>;

    // Setup default service responses
    service.obterAtivos.and.returnValue(of([mockAtividade]));
    service.obterAgrupadasPorTipo.and.returnValue(of({
      [TipoAtividadeAgropecuaria.Agricultura]: [mockAtividade],
      [TipoAtividadeAgropecuaria.Pecuaria]: [],
      [TipoAtividadeAgropecuaria.Mista]: []
    }));
    service.getTipoOptions.and.returnValue([
      { value: TipoAtividadeAgropecuaria.Agricultura, label: 'Agricultura' },
      { value: TipoAtividadeAgropecuaria.Pecuaria, label: 'Pecuária' },
      { value: TipoAtividadeAgropecuaria.Mista, label: 'Mista' }
    ]);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with correct entity configuration', () => {
    expect(component['entityDisplayName']()).toBe('Atividade Agropecuária');
    expect(component['entityDescription']()).toBe('Gerenciar atividades agropecuárias do sistema');
    expect(component['defaultSortField']()).toBe('codigo');
    expect(component['searchFields']()).toEqual(['codigo', 'descricao']);
  });

  it('should create form with correct validators', () => {
    const form = component['createFormGroup']();
    
    expect(form.get('codigo')?.hasError('required')).toBeTruthy();
    expect(form.get('descricao')?.hasError('required')).toBeTruthy();
    expect(form.get('tipo')?.hasError('required')).toBeTruthy();
    expect(form.get('ativo')?.value).toBe(true);
  });

  it('should validate codigo field correctly', () => {
    const form = component['createFormGroup']();
    const codigoControl = form.get('codigo');

    // Test required validation
    codigoControl?.setValue('');
    expect(codigoControl?.hasError('required')).toBeTruthy();

    // Test minlength validation
    codigoControl?.setValue('A');
    expect(codigoControl?.hasError('minlength')).toBeTruthy();

    // Test maxlength validation
    codigoControl?.setValue('VERYLONGCODE');
    expect(codigoControl?.hasError('maxlength')).toBeTruthy();

    // Test valid value
    codigoControl?.setValue('AGR001');
    expect(codigoControl?.valid).toBeTruthy();
  });

  it('should validate descricao field correctly', () => {
    const form = component['createFormGroup']();
    const descricaoControl = form.get('descricao');

    // Test required validation
    descricaoControl?.setValue('');
    expect(descricaoControl?.hasError('required')).toBeTruthy();

    // Test minlength validation
    descricaoControl?.setValue('Abc');
    expect(descricaoControl?.hasError('minlength')).toBeTruthy();

    // Test valid value
    descricaoControl?.setValue('Cultivo de soja para produção');
    expect(descricaoControl?.valid).toBeTruthy();
  });

  it('should map form values to create DTO correctly', () => {
    const formValue = {
      codigo: '  agr001  ',
      descricao: '  Cultivo de Soja  ',
      tipo: TipoAtividadeAgropecuaria.Agricultura,
      ativo: true
    };

    const dto = component['mapToCreateDto'](formValue);

    expect(dto.codigo).toBe('AGR001');
    expect(dto.descricao).toBe('Cultivo de Soja');
    expect(dto.tipo).toBe(TipoAtividadeAgropecuaria.Agricultura);
  });

  it('should map form values to update DTO correctly', () => {
    const formValue = {
      codigo: 'AGR001',
      descricao: '  Cultivo de Soja Atualizado  ',
      tipo: TipoAtividadeAgropecuaria.Mista,
      ativo: false
    };

    const dto = component['mapToUpdateDto'](formValue);

    expect(dto.descricao).toBe('Cultivo de Soja Atualizado');
    expect(dto.tipo).toBe(TipoAtividadeAgropecuaria.Mista);
    expect(dto.ativo).toBe(false);
  });

  it('should populate form correctly when editing', () => {
    component['populateForm'](mockAtividade);

    expect(component.form.get('codigo')?.value).toBe('AGR001');
    expect(component.form.get('descricao')?.value).toBe('Cultivo de soja para produção de grãos');
    expect(component.form.get('tipo')?.value).toBe(TipoAtividadeAgropecuaria.Agricultura);
    expect(component.form.get('ativo')?.value).toBe(true);
  });

  it('should filter activities by type', () => {
    const filteredActivities = [mockAtividade];
    service.obterPorTipo.and.returnValue(of(filteredActivities));

    component.selectedTipoFilter = TipoAtividadeAgropecuaria.Agricultura;
    component.onTipoFilterChange();

    expect(service.obterPorTipo).toHaveBeenCalledWith(TipoAtividadeAgropecuaria.Agricultura);
    expect(component.items).toEqual(filteredActividades);
    expect(component.showGroupedView).toBe(false);
  });

  it('should clear filter and show grouped view', () => {
    spyOn(component, 'loadItems');
    
    component.selectedTipoFilter = null;
    component.onTipoFilterChange();

    expect(component.loadItems).toHaveBeenCalled();
    expect(service.obterAgrupadasPorTipo).toHaveBeenCalled();
  });

  it('should get correct type icon', () => {
    expect(component.getTypeIcon(TipoAtividadeAgropecuaria.Agricultura)).toBe('pi pi-sun');
    expect(component.getTypeIcon(TipoAtividadeAgropecuaria.Pecuaria)).toBe('pi pi-heart');
    expect(component.getTypeIcon(TipoAtividadeAgropecuaria.Mista)).toBe('pi pi-star');
  });

  it('should toggle between grouped and list view', () => {
    spyOn(component, 'loadItems');
    
    component.showGroupedView = false;
    component.toggleView();

    expect(component.showGroupedView).toBe(true);
    expect(component.selectedTipoFilter).toBeNull();
    expect(service.obterAgrupadasPorTipo).toHaveBeenCalled();
  });

  it('should handle service errors gracefully', () => {
    service.obterAtivos.and.returnValue(throwError(() => new Error('Service error')));
    spyOn(component, 'handleError');

    component.ngOnInit();

    expect(component.handleError).toHaveBeenCalled();
  });

  it('should display correct table columns', () => {
    const columns = component['displayColumns']();

    expect(columns).toEqual([
      { field: 'codigo', header: 'Código', sortable: true, width: '120px' },
      { field: 'descricao', header: 'Descrição', sortable: true, width: '300px' },
      { field: 'tipoDescricao', header: 'Tipo', sortable: true, width: '120px' },
      { field: 'ativo', header: 'Status', sortable: true, width: '100px', type: 'boolean', hideOnMobile: true },
      { field: 'dataCriacao', header: 'Criado em', sortable: true, width: '150px', type: 'date', hideOnMobile: true, hideOnTablet: true }
    ]);
  });

  it('should load grouped activities on init', () => {
    fixture.detectChanges();

    expect(service.obterAgrupadasPorTipo).toHaveBeenCalled();
    expect(component.groupedActivities.length).toBeGreaterThan(0);
  });
});