import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CulturasListComponent } from './culturas-list.component';
import { CulturaDto } from '../../models/cultura.interface';
import { environment } from '../../../../../environments/environment';

describe('CulturasListComponent Integration', () => {
  let component: CulturasListComponent;
  let fixture: ComponentFixture<CulturasListComponent>;
  let httpMock: HttpTestingController;
  let router: jasmine.SpyObj<Router>;

  const mockCulturas: CulturaDto[] = [
    {
      id: 1,
      nome: 'Soja',
      descricao: 'Cultura de soja para grãos',
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
    },
    {
      id: 3,
      nome: 'Algodão',
      descricao: 'Cultura de algodão',
      ativo: true,
      dataCriacao: new Date('2024-01-05')
    }
  ];

  beforeEach(async () => {
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [
        CulturasListComponent,
        HttpClientTestingModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: Router, useValue: routerSpy },
        ConfirmationService,
        MessageService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CulturasListComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create and load culturas on init', () => {
    expect(component).toBeTruthy();
    
    // Component should start loading
    expect(component.loading()).toBe(false); // Initial state
    
    // Trigger ngOnInit
    component.ngOnInit();
    expect(component.loading()).toBe(true);

    // Mock HTTP response
    const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
    expect(req.request.method).toBe('GET');
    req.flush(mockCulturas);

    // Verify component state after loading
    expect(component.loading()).toBe(false);
    expect(component.culturas()).toEqual(mockCulturas);
  });

  it('should load only active culturas when filter is set to ativas', () => {
    const activeCulturas = mockCulturas.filter(c => c.ativo);
    
    component.selectedStatusFilter.set('ativas');
    component.carregarCulturas();

    const req = httpMock.expectOne(`${environment.apiUrl}/culturas/ativas`);
    expect(req.request.method).toBe('GET');
    req.flush(activeCulturas);

    expect(component.culturas()).toEqual(activeCulturas);
  });

  it('should filter inactive culturas when filter is set to inativas', () => {
    const inactiveCulturas = mockCulturas.filter(c => !c.ativo);
    
    component.selectedStatusFilter.set('inativas');
    component.carregarCulturas();

    const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
    expect(req.request.method).toBe('GET');
    req.flush(mockCulturas);

    expect(component.culturas()).toEqual(inactiveCulturas);
  });

  it('should handle HTTP error gracefully', () => {
    spyOn(console, 'error');
    
    component.carregarCulturas();

    const req = httpMock.expectOne(`${environment.apiUrl}/culturas`);
    req.error(new ProgressEvent('Network error'), { status: 500, statusText: 'Server Error' });

    expect(console.error).toHaveBeenCalled();
    expect(component.loading()).toBe(false);
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

  it('should delete cultura successfully', () => {
    const cultura = mockCulturas[0];
    
    // Set initial data
    component.culturas.set(mockCulturas);
    
    // Call the private method directly for testing
    component['confirmarExclusao'](cultura);

    // Expect DELETE request
    const deleteReq = httpMock.expectOne(`${environment.apiUrl}/culturas/${cultura.id}`);
    expect(deleteReq.request.method).toBe('DELETE');
    deleteReq.flush(null);

    // Expect reload request
    const reloadReq = httpMock.expectOne(`${environment.apiUrl}/culturas`);
    expect(reloadReq.request.method).toBe('GET');
    reloadReq.flush(mockCulturas.filter(c => c.id !== cultura.id));
  });

  it('should format date correctly', () => {
    const date = new Date('2024-01-15T10:30:00');
    const formatted = component.formatarData(date);
    expect(formatted).toBe('15/01/2024');
  });

  it('should return correct status labels and severities', () => {
    expect(component.getStatusLabel(true)).toBe('Ativa');
    expect(component.getStatusLabel(false)).toBe('Inativa');
    expect(component.getStatusSeverity(true)).toBe('success');
    expect(component.getStatusSeverity(false)).toBe('danger');
  });

  it('should handle status filter change', () => {
    const event = { value: 'ativas' };
    
    component.onStatusFilterChange(event);
    
    expect(component.selectedStatusFilter()).toBe('ativas');
    
    // Should trigger HTTP request
    const req = httpMock.expectOne(`${environment.apiUrl}/culturas/ativas`);
    req.flush([]);
  });
});