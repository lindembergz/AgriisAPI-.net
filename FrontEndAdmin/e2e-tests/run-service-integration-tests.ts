/**
 * Service Integration Test Runner
 * 
 * This script provides utilities to run and validate all service integration tests
 * for the Referencias module. It includes test execution, reporting, and validation
 * utilities for comprehensive service integration testing.
 */

import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { Observable, forkJoin, of, throwError } from 'rxjs';
import { catchError, map, timeout } from 'rxjs/operators';

// Import all services
import { UnidadeMedidaService } from '../features/referencias/unidades-medida/services/unidade-medida.service';
import { MoedaService } from '../features/referencias/moedas/services/moeda.service';
import { PaisService } from '../features/referencias/paises/services/pais.service';
import { UfService } from '../features/referencias/ufs/services/uf.service';
import { EmbalagemService } from '../features/referencias/embalagens/services/embalagem.service';
import { CategoriaService } from '../features/referencias/categorias/services/categoria.service';
import { AtividadeAgropecuariaService } from '../features/referencias/atividades-agropecuarias/services/atividade-agropecuaria.service';

/**
 * Interface for test results
 */
export interface TestResult {
  serviceName: string;
  testName: string;
  success: boolean;
  error?: string;
  duration: number;
}

/**
 * Interface for service test configuration
 */
export interface ServiceTestConfig {
  service: any;
  serviceName: string;
  tests: TestFunction[];
}

/**
 * Type for test functions
 */
export type TestFunction = (service: any) => Observable<boolean>;

/**
 * Service Integration Test Runner Class
 */
export class ServiceIntegrationTestRunner {
  private testResults: TestResult[] = [];
  private services: ServiceTestConfig[] = [];

  constructor() {
    this.setupTestBed();
    this.initializeServices();
  }

  /**
   * Setup Angular TestBed for integration testing
   */
  private setupTestBed(): void {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        UnidadeMedidaService,
        MoedaService,
        PaisService,
        UfService,
        EmbalagemService,
        CategoriaService,
        AtividadeAgropecuariaService,
        MessageService
      ]
    });
  }

  /**
   * Initialize all services for testing
   */
  private initializeServices(): void {
    this.services = [
      {
        service: TestBed.inject(UnidadeMedidaService),
        serviceName: 'UnidadeMedidaService',
        tests: this.getUnidadeMedidaTests()
      },
      {
        service: TestBed.inject(MoedaService),
        serviceName: 'MoedaService',
        tests: this.getMoedaTests()
      },
      {
        service: TestBed.inject(PaisService),
        serviceName: 'PaisService',
        tests: this.getPaisTests()
      },
      {
        service: TestBed.inject(UfService),
        serviceName: 'UfService',
        tests: this.getUfTests()
      },
      {
        service: TestBed.inject(EmbalagemService),
        serviceName: 'EmbalagemService',
        tests: this.getEmbalagemTests()
      },
      {
        service: TestBed.inject(CategoriaService),
        serviceName: 'CategoriaService',
        tests: this.getCategoriaTests()
      },
      {
        service: TestBed.inject(AtividadeAgropecuariaService),
        serviceName: 'AtividadeAgropecuariaService',
        tests: this.getAtividadeAgropecuariaTests()
      }
    ];
  }

  /**
   * Run all integration tests
   */
  public runAllTests(): Observable<TestResult[]> {
    const allTests: Observable<TestResult>[] = [];

    this.services.forEach(serviceConfig => {
      serviceConfig.tests.forEach((testFn, index) => {
        const testName = `Test ${index + 1}`;
        allTests.push(this.runSingleTest(serviceConfig.service, serviceConfig.serviceName, testName, testFn));
      });
    });

    return forkJoin(allTests).pipe(
      map(results => {
        this.testResults = results;
        return results;
      })
    );
  }

  /**
   * Run tests for a specific service
   */
  public runServiceTests(serviceName: string): Observable<TestResult[]> {
    const serviceConfig = this.services.find(s => s.serviceName === serviceName);
    if (!serviceConfig) {
      return throwError(() => new Error(`Service ${serviceName} not found`));
    }

    const serviceTests: Observable<TestResult>[] = serviceConfig.tests.map((testFn, index) => {
      const testName = `Test ${index + 1}`;
      return this.runSingleTest(serviceConfig.service, serviceName, testName, testFn);
    });

    return forkJoin(serviceTests);
  }

  /**
   * Run a single test
   */
  private runSingleTest(service: any, serviceName: string, testName: string, testFn: TestFunction): Observable<TestResult> {
    const startTime = Date.now();

    return testFn(service).pipe(
      timeout(10000), // 10 second timeout
      map(success => ({
        serviceName,
        testName,
        success,
        duration: Date.now() - startTime
      })),
      catchError(error => of({
        serviceName,
        testName,
        success: false,
        error: error.message || 'Unknown error',
        duration: Date.now() - startTime
      }))
    );
  }

  /**
   * Get test results
   */
  public getTestResults(): TestResult[] {
    return this.testResults;
  }

  /**
   * Get test summary
   */
  public getTestSummary(): { total: number; passed: number; failed: number; passRate: number } {
    const total = this.testResults.length;
    const passed = this.testResults.filter(r => r.success).length;
    const failed = total - passed;
    const passRate = total > 0 ? (passed / total) * 100 : 0;

    return { total, passed, failed, passRate };
  }

  /**
   * Generate test report
   */
  public generateReport(): string {
    const summary = this.getTestSummary();
    let report = `
=== Service Integration Test Report ===
Total Tests: ${summary.total}
Passed: ${summary.passed}
Failed: ${summary.failed}
Pass Rate: ${summary.passRate.toFixed(2)}%

=== Detailed Results ===
`;

    this.services.forEach(serviceConfig => {
      const serviceResults = this.testResults.filter(r => r.serviceName === serviceConfig.serviceName);
      const servicePassed = serviceResults.filter(r => r.success).length;
      const serviceTotal = serviceResults.length;

      report += `
${serviceConfig.serviceName}: ${servicePassed}/${serviceTotal} passed
`;

      serviceResults.forEach(result => {
        const status = result.success ? '✓' : '✗';
        report += `  ${status} ${result.testName} (${result.duration}ms)`;
        if (result.error) {
          report += ` - Error: ${result.error}`;
        }
        report += '\n';
      });
    });

    return report;
  }

  // Test definitions for each service

  /**
   * UnidadeMedida service tests
   */
  private getUnidadeMedidaTests(): TestFunction[] {
    return [
      (service) => of(typeof service.obterTodos === 'function'),
      (service) => of(typeof service.obterAtivos === 'function'),
      (service) => of(typeof service.obterPorId === 'function'),
      (service) => of(typeof service.criar === 'function'),
      (service) => of(typeof service.atualizar === 'function'),
      (service) => of(typeof service.buscar === 'function')
    ];
  }

  /**
   * Moeda service tests
   */
  private getMoedaTests(): TestFunction[] {
    return [
      (service) => of(typeof service.obterTodos === 'function'),
      (service) => of(typeof service.obterAtivos === 'function'),
      (service) => of(typeof service.obterPorId === 'function'),
      (service) => of(typeof service.criar === 'function'),
      (service) => of(typeof service.atualizar === 'function'),
      (service) => of(typeof service.buscar === 'function')
    ];
  }

  /**
   * Pais service tests
   */
  private getPaisTests(): TestFunction[] {
    return [
      (service) => of(typeof service.obterTodos === 'function'),
      (service) => of(typeof service.obterAtivos === 'function'),
      (service) => of(typeof service.obterPorId === 'function'),
      (service) => of(typeof service.criar === 'function'),
      (service) => of(typeof service.atualizar === 'function'),
      (service) => of(typeof service.buscar === 'function')
    ];
  }

  /**
   * UF service tests
   */
  private getUfTests(): TestFunction[] {
    return [
      (service) => of(typeof service.obterTodos === 'function'),
      (service) => of(typeof service.obterAtivos === 'function'),
      (service) => of(typeof service.obterPorId === 'function'),
      (service) => of(typeof service.criar === 'function'),
      (service) => of(typeof service.atualizar === 'function'),
      (service) => of(typeof service.buscar === 'function')
    ];
  }

  /**
   * Embalagem service tests
   */
  private getEmbalagemTests(): TestFunction[] {
    return [
      (service) => of(typeof service.obterTodos === 'function'),
      (service) => of(typeof service.obterAtivos === 'function'),
      (service) => of(typeof service.obterPorId === 'function'),
      (service) => of(typeof service.criar === 'function'),
      (service) => of(typeof service.atualizar === 'function'),
      (service) => of(typeof service.buscar === 'function')
    ];
  }

  /**
   * Categoria service tests
   */
  private getCategoriaTests(): TestFunction[] {
    return [
      (service) => of(typeof service.obterTodos === 'function'),
      (service) => of(typeof service.obterAtivos === 'function'),
      (service) => of(typeof service.obterPorId === 'function'),
      (service) => of(typeof service.criar === 'function'),
      (service) => of(typeof service.atualizar === 'function'),
      (service) => of(typeof service.buscar === 'function'),
      (service) => of(typeof service.obterTiposDisponiveis === 'function')
    ];
  }

  /**
   * AtividadeAgropecuaria service tests
   */
  private getAtividadeAgropecuariaTests(): TestFunction[] {
    return [
      (service) => of(typeof service.obterTodos === 'function'),
      (service) => of(typeof service.obterAtivos === 'function'),
      (service) => of(typeof service.obterPorId === 'function'),
      (service) => of(typeof service.criar === 'function'),
      (service) => of(typeof service.atualizar === 'function'),
      (service) => of(typeof service.buscar === 'function'),
      (service) => of(typeof service.buscarPorCodigo === 'function'),
      (service) => of(typeof service.getTipoOptions === 'function')
    ];
  }
}

/**
 * Utility function to run all integration tests
 */
export function runAllServiceIntegrationTests(): Observable<TestResult[]> {
  const runner = new ServiceIntegrationTestRunner();
  return runner.runAllTests();
}

/**
 * Utility function to run tests for a specific service
 */
export function runServiceIntegrationTests(serviceName: string): Observable<TestResult[]> {
  const runner = new ServiceIntegrationTestRunner();
  return runner.runServiceTests(serviceName);
}

/**
 * Utility function to generate a test report
 */
export function generateIntegrationTestReport(): Observable<string> {
  const runner = new ServiceIntegrationTestRunner();
  return runner.runAllTests().pipe(
    map(() => runner.generateReport())
  );
}