import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

// PrimeNG imports for testing
import { TabsModule } from 'primeng/tabs';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { InputMaskModule } from 'primeng/inputmask';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

// Component and dependencies
import { FornecedorDetailComponent } from './fornecedor-detail.component';
import { FornecedorService } from '../services/fornecedor.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Fornecedor } from '../../../shared/models/fornecedor.model';
import { TipoCliente } from '../../../shared/models/produtor.model';

// Mock components
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormArray, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-endereco-form',
  template: '<div>Mock Endereco Form</div>'
})
class MockEnderecoFormComponent {
  @Input() enderecosFormArray!: FormArray;
  @Input() readonly = false;
  @Output() enderecosChange = new EventEmitter<any[]>();
}

@Component({
  selector: 'app-coordenadas-map',
  template: '<div>Mock Coordenadas Map</div>'
})
class MockCoordenadasMapComponent {
  @Input() readonly = false;
  @Input() height = '400px';
  @Output() coordinatesSelected = new EventEmitter<{ latitude: number | null; longitude: number | null }>();
}

@Component({
  selector: 'app-usuario-master-form',
  template: '<div>Mock Usuario Master Form</div>'
})
class MockUsuarioMasterFormComponent {
  @Input() usuarioFormGroup!: FormGroup;
  @Input() readonly = false;
  @Input() showCard = true;
  @Output() usuarioChange = new EventEmitter<any>();
}

@Component({
  selector: 'app-ponto-distribuicao-form',
  template: '<div>Mock Ponto Distribuicao Form</div>'
})
class MockPontoDistribuicaoFormComponent {
  @Input() pontosDistribuicaoFormArray!: FormArray;
  @Input() readonly = false;
  @Output() pontosDistribuicaoChange = new EventEmitter<any[]>();
}

describe('FornecedorDetailComponent', () => {
  let component: FornecedorDetailComponent;
  let fixture: ComponentFixture<FornecedorDetailComponent>;
  let mockFornecedorService: jasmine.SpyObj<FornecedorService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: any;

  const mockFornecedor: Fornecedor = {
    id: 1,
    codigo: 'FORN001',
    nome: 'Fornecedor Teste',
    cpfCnpj: '12345678901234',
    tipoCliente: TipoCliente.PJ,
    telefone: '11999999999',
    email: 'teste@fornecedor.com',
    enderecos: [
      {
        id: 1,
        logradouro: 'Rua Teste',
        numero: '123',
        complemento: '',
        bairro: 'Centro',
        cidade: 'São Paulo',
        uf: 'SP',
        cep: '01234567',
        latitude: -23.5505,
        longitude: -46.6333,
        dataCriacao: new Date(),
        ativo: true
      }
    ],
    pontosDistribuicao: [
      {
        id: 1,
        nome: 'Ponto Central',
        endereco: {
          id: 2,
          logradouro: 'Av. Principal',
          numero: '456',
          complemento: 'Galpão A',
          bairro: 'Industrial',
          cidade: 'São Paulo',
          uf: 'SP',
          cep: '01234567',
          latitude: -23.5505,
          longitude: -46.6333,
          dataCriacao: new Date(),
          ativo: true
        },
        latitude: -23.5505,
        longitude: -46.6333,
        fornecedorId: 1,
        dataCriacao: new Date(),
        ativo: true
      }
    ],
    usuarioMaster: {
      id: 1,
      nome: 'Usuario Master',
      email: 'master@fornecedor.com',
      senha: 'hashedpassword',
      telefone: '11888888888',
      dataCriacao: new Date(),
      ativo: true
    },
    dataCriacao: new Date(),
    ativo: true
  };

  beforeEach(async () => {
    // Create spies
    mockFornecedorService = jasmine.createSpyObj('FornecedorService', ['getById', 'create', 'update']);
    mockNotificationService = jasmine.createSpyObj('NotificationService', [
      'showCustomSuccess', 
      'showCustomError', 
      'showValidationWarning'
    ]);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockActivatedRoute = {
      params: of({ id: '1' })
    };

    await TestBed.configureTestingModule({
      imports: [
        FornecedorDetailComponent,
        ReactiveFormsModule,
        NoopAnimationsModule,
        TabsModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        SelectModule,
        InputMaskModule,
        ToastModule,
        ProgressSpinnerModule,
        ConfirmDialogModule,
        MockEnderecoFormComponent,
        MockCoordenadasMapComponent,
        MockUsuarioMasterFormComponent,
        MockPontoDistribuicaoFormComponent
      ],
      providers: [
        FormBuilder,
        { provide: FornecedorService, useValue: mockFornecedorService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FornecedorDetailComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with default values', () => {
    fixture.detectChanges();
    
    expect(component.fornecedorForm).toBeDefined();
    expect(component.fornecedorForm.get('dadosGerais')).toBeDefined();
    expect(component.fornecedorForm.get('enderecos')).toBeDefined();
    expect(component.fornecedorForm.get('pontosDistribuicao')).toBeDefined();
    expect(component.fornecedorForm.get('usuarioMaster')).toBeDefined();
  });

  it('should load fornecedor data when editing', () => {
    mockFornecedorService.getById.and.returnValue(of(mockFornecedor));
    
    fixture.detectChanges();
    
    expect(mockFornecedorService.getById).toHaveBeenCalledWith(1);
    expect(component.isEditMode()).toBe(true);
    expect(component.fornecedorId()).toBe(1);
  });

  it('should handle load error', () => {
    const error = new Error('Load failed');
    mockFornecedorService.getById.and.returnValue(throwError(() => error));
    
    fixture.detectChanges();
    
    expect(mockNotificationService.showCustomError).toHaveBeenCalledWith('Erro ao carregar fornecedor: Load failed');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/fornecedores']);
  });

  it('should be in create mode for new fornecedor', () => {
    mockActivatedRoute.params = of({ id: 'novo' });
    
    fixture.detectChanges();
    
    expect(component.isEditMode()).toBe(false);
    expect(component.fornecedorId()).toBe(null);
  });

  it('should validate required fields', () => {
    fixture.detectChanges();
    
    // Try to save without filling required fields
    component.onSave();
    
    expect(mockNotificationService.showValidationWarning).toHaveBeenCalledWith(
      'Por favor, corrija os erros no formulário antes de salvar'
    );
  });

  it('should create fornecedor successfully', () => {
    mockFornecedorService.create.and.returnValue(of(mockFornecedor));
    
    fixture.detectChanges();
    
    // Fill required fields
    component.fornecedorForm.patchValue({
      dadosGerais: {
        nome: 'Novo Fornecedor',
        cpfCnpj: '12345678901234',
        tipoCliente: TipoCliente.PJ,
        telefone: '11999999999',
        email: 'novo@fornecedor.com'
      }
    });
    
    // Add required endereco
    component.enderecosFormArray.push(component['createEnderecoFormGroup']());
    component.enderecosFormArray.at(0).patchValue({
      logradouro: 'Rua Nova',
      numero: '123',
      bairro: 'Centro',
      cidade: 'São Paulo',
      uf: 'SP',
      cep: '01234567'
    });
    
    // Add required usuario master
    component.usuarioMasterFormGroup.patchValue({
      nome: 'Usuario Master',
      email: 'master@novo.com',
      senha: 'password123',
      telefone: '11888888888'
    });
    
    component.onSave();
    
    expect(mockFornecedorService.create).toHaveBeenCalled();
    expect(mockNotificationService.showCustomSuccess).toHaveBeenCalledWith('Fornecedor criado com sucesso');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/fornecedores']);
  });

  it('should update fornecedor successfully', () => {
    mockFornecedorService.getById.and.returnValue(of(mockFornecedor));
    mockFornecedorService.update.and.returnValue(of(mockFornecedor));
    
    fixture.detectChanges();
    
    component.onSave();
    
    expect(mockFornecedorService.update).toHaveBeenCalledWith(1, jasmine.any(Object));
    expect(mockNotificationService.showCustomSuccess).toHaveBeenCalledWith('Fornecedor atualizado com sucesso');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/fornecedores']);
  });

  it('should handle save error', () => {
    const error = new Error('Save failed');
    mockFornecedorService.create.and.returnValue(throwError(() => error));
    
    fixture.detectChanges();
    
    // Fill required fields
    component.fornecedorForm.patchValue({
      dadosGerais: {
        nome: 'Novo Fornecedor',
        cpfCnpj: '12345678901234',
        tipoCliente: TipoCliente.PJ,
        telefone: '11999999999',
        email: 'novo@fornecedor.com'
      }
    });
    
    // Add required endereco
    component.enderecosFormArray.push(component['createEnderecoFormGroup']());
    component.enderecosFormArray.at(0).patchValue({
      logradouro: 'Rua Nova',
      numero: '123',
      bairro: 'Centro',
      cidade: 'São Paulo',
      uf: 'SP',
      cep: '01234567'
    });
    
    // Add required usuario master
    component.usuarioMasterFormGroup.patchValue({
      nome: 'Usuario Master',
      email: 'master@novo.com',
      senha: 'password123',
      telefone: '11888888888'
    });
    
    component.onSave();
    
    expect(mockNotificationService.showCustomError).toHaveBeenCalledWith('Erro ao salvar fornecedor: Save failed');
  });

  it('should cancel and navigate back', () => {
    fixture.detectChanges();
    
    component.onCancel();
    
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/fornecedores']);
  });

  it('should validate CPF/CNPJ based on tipo cliente', () => {
    fixture.detectChanges();
    
    const dadosGerais = component.fornecedorForm.get('dadosGerais');
    
    // Test PF (CPF)
    dadosGerais?.patchValue({
      tipoCliente: TipoCliente.PF,
      cpfCnpj: '12345678901' // 11 digits for CPF
    });
    
    expect(dadosGerais?.get('cpfCnpj')?.valid).toBe(true);
    
    // Test PJ (CNPJ)
    dadosGerais?.patchValue({
      tipoCliente: TipoCliente.PJ,
      cpfCnpj: '12345678901234' // 14 digits for CNPJ
    });
    
    expect(dadosGerais?.get('cpfCnpj')?.valid).toBe(true);
  });

  it('should get correct CPF/CNPJ mask', () => {
    fixture.detectChanges();
    
    // Test PF mask
    component.fornecedorForm.get('dadosGerais.tipoCliente')?.setValue(TipoCliente.PF);
    expect(component.getCpfCnpjMask()).toBe('999.999.999-99');
    
    // Test PJ mask
    component.fornecedorForm.get('dadosGerais.tipoCliente')?.setValue(TipoCliente.PJ);
    expect(component.getCpfCnpjMask()).toBe('99.999.999/9999-99');
  });

  it('should handle tab changes', () => {
    fixture.detectChanges();
    
    component.onTabChange(2);
    expect(component.activeTabIndex()).toBe(2);
    
    component.onTabChange('3');
    expect(component.activeTabIndex()).toBe(3);
  });

  it('should handle coordinates selection', () => {
    fixture.detectChanges();
    
    // Add a ponto distribuicao first
    component.pontosDistribuicaoFormArray.push(component['createPontoDistribuicaoFormGroup']());
    
    const coordinates = { latitude: -23.5505, longitude: -46.6333 };
    component.onCoordinatesSelected(coordinates);
    
    const firstPonto = component.pontosDistribuicaoFormArray.at(0);
    expect(firstPonto.get('latitude')?.value).toBe(coordinates.latitude);
    expect(firstPonto.get('longitude')?.value).toBe(coordinates.longitude);
  });

  it('should emit changes when form arrays are modified', () => {
    fixture.detectChanges();
    
    spyOn(component, 'onEnderecosChange');
    spyOn(component, 'onPontosDistribuicaoChange');
    spyOn(component, 'onUsuarioMasterChange');
    
    component.onEnderecosChange([]);
    component.onPontosDistribuicaoChange([]);
    component.onUsuarioMasterChange({});
    
    expect(component.onEnderecosChange).toHaveBeenCalledWith([]);
    expect(component.onPontosDistribuicaoChange).toHaveBeenCalledWith([]);
    expect(component.onUsuarioMasterChange).toHaveBeenCalledWith({});
  });
});