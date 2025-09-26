import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { SafrasListComponent } from './safras-list.component';
import { SafraService } from '../../services/safra.service';
import { SafraDto, SafraAtualDto } from '../../models/safra.interface';

describe('SafrasListComponent Integration', () => {
  let component: SafrasListComponent;
  let fixture: ComponentFixture<SafrasListComponent>;
  let httpMock: HttpTestingController;
  let mockRouter: jasmine.SpyObj<Router>;

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
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [
        SafrasListComponent,
        HttpClientTestingModule,
        BrowserAnimationsModule
      ],
      providers: [
        SafraService,
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SafrasListComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should load and display safras from API', () => {
    component.ngOnInit();
    
    // Expect HTTP requests
    const safrasReq = httpMock.expectOne('/api/safras');
    const safraAtualReq = httpMock.expectOne('/api/safras/atual');
    
    expect(safrasReq.request.method).toBe('GET');
    expect(safraAtualReq.request.method).toBe('GET');
    
    // Respond with mock data
    safrasReq.flush(mockSafras);
    safraAtualReq.flush(mockSafraAtual);
    
    fixture.detectChanges();

    // Verify data is displayed
    expect(component.safras()).toEqual(mockSafras);
    expect(component.safraAtual()).toEqual(mockSafraAtual);
    
    // Check if table rows are rendered
    const tableRows = fixture.debugElement.queryAll(By.css('tbody tr'));
    expect(tableRows.length).toBe(2);
  });

  it('should display safra data in table columns', () => {
    component.ngOnInit();
    
    const safrasReq = httpMock.expectOne('/api/safras');
    const safraAtualReq = httpMock.expectOne('/api/safras/atual');
    
    safrasReq.flush(mockSafras);
    safraAtualReq.flush(mockSafraAtual);
    
    fixture.detectChanges();

    const firstRow = fixture.debugElement.query(By.css('tbody tr:first-child'));
    const cells = firstRow.queryAll(By.css('td'));

    expect(cells[0].nativeElement.textContent.trim()).toBe('1'); // ID
    expect(cells[1].nativeElement.textContent.trim()).toBe('2024'); // Ano Colheita
    expect(cells[4].nativeElement.textContent.trim()).toBe('Soja 2024'); // Nome Plantio
    expect(cells[5].nativeElement.textContent.trim()).toBe('Safra de soja 2024'); // Descrição
    expect(cells[6].nativeElement.textContent.trim()).toBe('2024/2025 Soja 2024'); // Safra Formatada
  });

  it('should highlight current safra row', () => {
    component.ngOnInit();
    
    const safrasReq = httpMock.expectOne('/api/safras');
    const safraAtualReq = httpMock.expectOne('/api/safras/atual');
    
    safrasReq.flush(mockSafras);
    safraAtualReq.flush(mockSafraAtual);
    
    fixture.detectChanges();

    const firstRow = fixture.debugElement.query(By.css('tbody tr:first-child'));
    expect(firstRow.nativeElement.classList).toContain('safra-atual-row');
  });

  it('should show correct status badges', () => {
    component.ngOnInit();
    
    const safrasReq = httpMock.expectOne('/api/safras');
    const safraAtualReq = httpMock.expectOne('/api/safras/atual');
    
    safrasReq.flush(mockSafras);
    safraAtualReq.flush(mockSafraAtual);
    
    fixture.detectChanges();

    const statusBadges = fixture.debugElement.queryAll(By.css('.status-badge'));
    
    expect(statusBadges[0].nativeElement.textContent.trim()).toBe('Atual');
    expect(statusBadges[0].nativeElement.classList).toContain('status-atual');
    
    expect(statusBadges[1].nativeElement.textContent.trim()).toBe('Inativa');
    expect(statusBadges[1].nativeElement.classList).toContain('status-inativa');
  });

  it('should navigate to new safra when clicking "Nova Safra" button', () => {
    component.ngOnInit();
    
    const safrasReq = httpMock.expectOne('/api/safras');
    const safraAtualReq = httpMock.expectOne('/api/safras/atual');
    
    safrasReq.flush(mockSafras);
    safraAtualReq.flush(mockSafraAtual);
    
    fixture.detectChanges();

    const novaSafraButton = fixture.debugElement.query(By.css('p-button[label="Nova Safra"]'));
    novaSafraButton.nativeElement.click();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/safras/nova']);
  });

  it('should navigate to edit safra when clicking edit button', () => {
    component.ngOnInit();
    
    const safrasReq = httpMock.expectOne('/api/safras');
    const safraAtualReq = httpMock.expectOne('/api/safras/atual');
    
    safrasReq.flush(mockSafras);
    safraAtualReq.flush(mockSafraAtual);
    
    fixture.detectChanges();

    const editButton = fixture.debugElement.query(By.css('p-button[icon="pi pi-pencil"]'));
    editButton.nativeElement.click();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/safras', 1]);
  });

  it('should filter safras by year', () => {
    component.ngOnInit();
    
    const safrasReq = httpMock.expectOne('/api/safras');
    const safraAtualReq = httpMock.expectOne('/api/safras/atual');
    
    safrasReq.flush(mockSafras);
    safraAtualReq.flush(mockSafraAtual);
    
    fixture.detectChanges();

    // Change filter to specific year
    component.onFiltroAnoChange(2024);

    // Expect new request with year filter
    const filteredReq = httpMock.expectOne('/api/safras/ano-colheita/2024');
    expect(filteredReq.request.method).toBe('GET');
    
    filteredReq.flush([mockSafras[0]]); // Only 2024 safra
    
    fixture.detectChanges();

    expect(component.safras().length).toBe(1);
    expect(component.safras()[0].anoColheita).toBe(2024);
  });

  it('should show loading state', () => {
    component.ngOnInit();
    
    // Before HTTP requests complete, loading should be true
    expect(component.loading()).toBeTrue();
    
    const safrasReq = httpMock.expectOne('/api/safras');
    const safraAtualReq = httpMock.expectOne('/api/safras/atual');
    
    safrasReq.flush(mockSafras);
    safraAtualReq.flush(mockSafraAtual);
    
    fixture.detectChanges();

    // After requests complete, loading should be false
    expect(component.loading()).toBeFalse();
  });

  it('should show empty state when no safras exist', () => {
    component.ngOnInit();
    
    const safrasReq = httpMock.expectOne('/api/safras');
    const safraAtualReq = httpMock.expectOne('/api/safras/atual');
    
    safrasReq.flush([]); // Empty array
    safraAtualReq.flush(mockSafraAtual);
    
    fixture.detectChanges();

    const emptyState = fixture.debugElement.query(By.css('.empty-state'));
    expect(emptyState).toBeTruthy();
    expect(emptyState.nativeElement.textContent).toContain('Nenhuma safra encontrada');
  });

  it('should handle API errors gracefully', () => {
    spyOn(component['messageService'], 'add');
    
    component.ngOnInit();
    
    const safrasReq = httpMock.expectOne('/api/safras');
    const safraAtualReq = httpMock.expectOne('/api/safras/atual');
    
    safrasReq.error(new ErrorEvent('Network error'));
    safraAtualReq.error(new ErrorEvent('Network error'));
    
    fixture.detectChanges();

    expect(component.loading()).toBeFalse();
    expect(component['messageService'].add).toHaveBeenCalledWith({
      severity: 'error',
      summary: 'Erro',
      detail: 'Erro ao carregar safras'
    });
  });

  it('should delete safra via API', () => {
    component.ngOnInit();
    
    const safrasReq = httpMock.expectOne('/api/safras');
    const safraAtualReq = httpMock.expectOne('/api/safras/atual');
    
    safrasReq.flush(mockSafras);
    safraAtualReq.flush(mockSafraAtual);
    
    fixture.detectChanges();

    // Mock confirmation service to auto-accept
    spyOn(component['confirmationService'], 'confirm').and.callFake((confirmation) => {
      if (confirmation.accept) {
        confirmation.accept();
      }
      return component['confirmationService'];
    });
    
    spyOn(component['messageService'], 'add');

    component.excluirSafra(mockSafras[0]);

    // Expect DELETE request
    const deleteReq = httpMock.expectOne('/api/safras/1');
    expect(deleteReq.request.method).toBe('DELETE');
    
    deleteReq.flush({});

    // Expect reload request
    const reloadReq = httpMock.expectOne('/api/safras');
    reloadReq.flush(mockSafras.slice(1)); // Return remaining safras

    expect(component['messageService'].add).toHaveBeenCalledWith({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Safra excluída com sucesso'
    });
  });
});