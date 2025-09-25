import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';

import { ProdutorDetailComponent } from './produtor-detail.component';
import { ProdutorService } from '../services/produtor.service';
import { NotificationService } from '../../../core/services/notification.service';

// Mock services
const mockProdutorService = {
  getById: jasmine.createSpy('getById').and.returnValue(of({})),
  create: jasmine.createSpy('create').and.returnValue(of({})),
  update: jasmine.createSpy('update').and.returnValue(of({}))
};

const mockNotificationService = {
  showCustomSuccess: jasmine.createSpy('showCustomSuccess'),
  showCustomError: jasmine.createSpy('showCustomError'),
  showValidationWarning: jasmine.createSpy('showValidationWarning')
};

const mockRouter = {
  navigate: jasmine.createSpy('navigate')
};

const mockActivatedRoute = {
  params: of({ id: 'novo' })
};

describe('ProdutorDetailComponent Integration', () => {
  let component: ProdutorDetailComponent;
  let fixture: ComponentFixture<ProdutorDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ReactiveFormsModule,
        ProdutorDetailComponent // Standalone component
      ],
      providers: [
        { provide: ProdutorService, useValue: mockProdutorService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProdutorDetailComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with all required sections', () => {
    fixture.detectChanges();
    
    expect(component.produtorForm).toBeDefined();
    expect(component.produtorForm.get('dadosGerais')).toBeDefined();
    expect(component.enderecosFormArray).toBeDefined();
    expect(component.propriedadesFormArray).toBeDefined();
    expect(component.usuarioMasterFormGroup).toBeDefined();
  });

  it('should handle endereco changes', () => {
    fixture.detectChanges();
    
    const initialDirty = component.enderecosFormArray.dirty;
    component.onEnderecosChange();
    
    expect(component.enderecosFormArray.dirty).toBe(true);
  });

  it('should handle propriedade changes', () => {
    fixture.detectChanges();
    
    const initialDirty = component.propriedadesFormArray.dirty;
    component.onPropriedadesChange();
    
    expect(component.propriedadesFormArray.dirty).toBe(true);
  });

  it('should handle usuario master changes', () => {
    fixture.detectChanges();
    
    const initialDirty = component.usuarioMasterFormGroup.dirty;
    component.onUsuarioMasterChange();
    
    expect(component.usuarioMasterFormGroup.dirty).toBe(true);
  });

  it('should handle coordinates selection', () => {
    fixture.detectChanges();
    
    // Add a propriedade first by calling the private method
    const propriedadeGroup = (component as any).createPropriedadeFormGroup();
    component.propriedadesFormArray.push(propriedadeGroup);
    
    const coordinates = { latitude: -15.123456, longitude: -47.654321 };
    component.onCoordinatesSelected(coordinates);
    
    const firstProperty = component.propriedadesFormArray.at(0);
    expect(firstProperty.get('latitude')?.value).toBe(coordinates.latitude);
    expect(firstProperty.get('longitude')?.value).toBe(coordinates.longitude);
  });

  it('should validate all sections before saving', () => {
    fixture.detectChanges();
    
    // Try to save with empty form
    component.onSave();
    
    expect(mockNotificationService.showValidationWarning).toHaveBeenCalled();
    expect(mockProdutorService.create).not.toHaveBeenCalled();
  });

  it('should navigate to first error tab on validation failure', () => {
    fixture.detectChanges();
    
    // Set active tab to something other than 0
    component.activeTabIndex.set(3);
    
    // Try to save with invalid dados gerais
    component.onSave();
    
    // Should navigate back to first tab (dados gerais)
    expect(component.activeTabIndex()).toBe(0);
  });
});