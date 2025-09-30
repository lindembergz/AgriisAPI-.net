import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, of, forkJoin, timer } from 'rxjs';
import { map, catchError, timeout, retry, switchMap } from 'rxjs/operators';

/**
 * Service for validating API connectivity and performance
 */
@Injectable({
  providedIn: 'root'
})
export class ApiValidationService {
  private http = inject(HttpClient);
  
  // API endpoints to validate
  private readonly API_ENDPOINTS = {
    referencias: {
      'unidades-medida': '/api/referencias/unidades-medida',
      'moedas': '/api/referencias/moedas',
      'paises': '/api/referencias/paises',
      'ufs': '/api/referencias/ufs',
      'municipios': '/api/referencias/municipios',
      'atividades-agropecuarias': '/api/referencias/atividades-agropecuarias',
      'embalagens': '/api/referencias/embalagens',
      'categorias': '/api/referencias/categorias'
    },
    specific: {
      'categorias-hierarquia': '/api/referencias/categorias/hierarquia',
      'paises-com-contadores': '/api/referencias/paises/ativos/com-contadores',
      'ufs-com-pais': '/api/referencias/ufs?include=pais',
      'municipios-ativos': '/api/referencias/municipios/ativos',
      'unidades-medida-tipos': '/api/referencias/unidades-medida/tipos'
    }
  };
  
  /**
   * Validate all API endpoints
   */
  validateAllEndpoints(): Observable<ApiValidationReport> {
    const allEndpoints = {
      ...this.API_ENDPOINTS.referencias,
      ...this.API_ENDPOINTS.specific
    };
    
    const validationTests = Object.entries(allEndpoints).map(([name, endpoint]) =>
      this.validateEndpoint(name, endpoint)
    );
    
    return forkJoin(validationTests).pipe(
      map(results => this.generateApiValidationReport(results))
    );
  }
  
  /**
   * Validate single API endpoint
   */
  validateEndpoint(name: string, endpoint: string): Observable<EndpointValidationResult> {
    const startTime = performance.now();
    
    return this.http.get(endpoint).pipe(
      timeout(10000), // 10 second timeout
      retry(2), // Retry twice on failure
      map(response => {
        const endTime = performance.now();
        const responseTime = endTime - startTime;
        
        return {
          endpointName: name,
          url: endpoint,
          status: 'success' as const,
          statusCode: 200,
          responseTime,
          responseSize: this.estimateResponseSize(response),
          isHealthy: responseTime < 2000, // Healthy if < 2 seconds
          errorMessage: null,
          dataValidation: this.validateResponseData(response),
          performanceMetrics: {
            responseTime,
            throughput: this.calculateThroughput(response, responseTime),
            availability: 100 // 100% if successful
          }
        };
      }),
      catchError((error: HttpErrorResponse) => {
        const endTime = performance.now();
        const responseTime = endTime - startTime;
        
        return of({
          endpointName: name,
          url: endpoint,
          status: 'error' as const,
          statusCode: error.status || 0,
          responseTime,
          responseSize: 0,
          isHealthy: false,
          errorMessage: this.getErrorMessage(error),
          dataValidation: {
            isValid: false,
            hasRequiredFields: false,
            dataType: 'unknown',
            issues: [`HTTP ${error.status}: ${error.message}`]
          },
          performanceMetrics: {
            responseTime,
            throughput: 0,
            availability: 0
          }
        });
      })
    );
  }
  
  /**
   * Test API performance under load
   */
  testApiPerformanceUnderLoad(endpoint: string, concurrentRequests: number = 10): Observable<LoadTestResult> {
    const requests = Array(concurrentRequests).fill(null).map(() =>
      this.measureSingleRequest(endpoint)
    );
    
    const startTime = performance.now();
    
    return forkJoin(requests).pipe(
      map(results => {
        const endTime = performance.now();
        const totalTime = endTime - startTime;
        
        const successfulRequests = results.filter(r => r.success);
        const failedRequests = results.filter(r => !r.success);
        
        const avgResponseTime = successfulRequests.length > 0
          ? successfulRequests.reduce((sum, r) => sum + r.responseTime, 0) / successfulRequests.length
          : 0;
        
        const minResponseTime = successfulRequests.length > 0
          ? Math.min(...successfulRequests.map(r => r.responseTime))
          : 0;
        
        const maxResponseTime = successfulRequests.length > 0
          ? Math.max(...successfulRequests.map(r => r.responseTime))
          : 0;
        
        return {
          endpoint,
          concurrentRequests,
          totalTime,
          successfulRequests: successfulRequests.length,
          failedRequests: failedRequests.length,
          successRate: (successfulRequests.length / concurrentRequests) * 100,
          averageResponseTime: avgResponseTime,
          minResponseTime,
          maxResponseTime,
          throughput: (successfulRequests.length / totalTime) * 1000, // requests per second
          errors: failedRequests.map(r => r.error).filter(Boolean)
        };
      })
    );
  }
  
  /**
   * Measure single request performance
   */
  private measureSingleRequest(endpoint: string): Observable<RequestResult> {
    const startTime = performance.now();
    
    return this.http.get(endpoint).pipe(
      timeout(5000),
      map(() => {
        const endTime = performance.now();
        return {
          success: true,
          responseTime: endTime - startTime,
          error: null
        };
      }),
      catchError((error: HttpErrorResponse) => {
        const endTime = performance.now();
        return of({
          success: false,
          responseTime: endTime - startTime,
          error: this.getErrorMessage(error)
        });
      })
    );
  }
  
  /**
   * Test API availability over time
   */
  testApiAvailability(endpoint: string, duration: number = 60000, interval: number = 5000): Observable<AvailabilityTestResult> {
    const checks: AvailabilityCheck[] = [];
    const startTime = Date.now();
    
    return timer(0, interval).pipe(
      switchMap(() => this.checkEndpointHealth(endpoint)),
      timeout(duration),
      map(healthCheck => {
        checks.push({
          timestamp: Date.now(),
          isAvailable: healthCheck.isHealthy,
          responseTime: healthCheck.responseTime,
          statusCode: healthCheck.statusCode
        });
        
        return healthCheck;
      }),
      catchError(() => of(null)), // Continue on errors
      map(() => {
        const endTime = Date.now();
        const totalDuration = endTime - startTime;
        
        const availableChecks = checks.filter(c => c.isAvailable);
        const availabilityPercentage = (availableChecks.length / checks.length) * 100;
        
        const avgResponseTime = availableChecks.length > 0
          ? availableChecks.reduce((sum, c) => sum + c.responseTime, 0) / availableChecks.length
          : 0;
        
        return {
          endpoint,
          duration: totalDuration,
          totalChecks: checks.length,
          availableChecks: availableChecks.length,
          unavailableChecks: checks.length - availableChecks.length,
          availabilityPercentage,
          averageResponseTime: avgResponseTime,
          checks: checks.slice(-20) // Keep last 20 checks
        };
      })
    );
  }
  
  /**
   * Check endpoint health
   */
  private checkEndpointHealth(endpoint: string): Observable<{ isHealthy: boolean; responseTime: number; statusCode: number }> {
    const startTime = performance.now();
    
    return this.http.get(endpoint).pipe(
      timeout(3000),
      map(() => {
        const endTime = performance.now();
        return {
          isHealthy: true,
          responseTime: endTime - startTime,
          statusCode: 200
        };
      }),
      catchError((error: HttpErrorResponse) => {
        const endTime = performance.now();
        return of({
          isHealthy: false,
          responseTime: endTime - startTime,
          statusCode: error.status || 0
        });
      })
    );
  }
  
  /**
   * Validate response data structure
   */
  private validateResponseData(response: any): DataValidationResult {
    const issues: string[] = [];
    let isValid = true;
    let hasRequiredFields = true;
    let dataType = 'unknown';
    
    try {
      if (Array.isArray(response)) {
        dataType = 'array';
        
        if (response.length > 0) {
          const firstItem = response[0];
          
          // Check for common required fields
          if (!firstItem.hasOwnProperty('id')) {
            issues.push('Missing "id" field in array items');
            hasRequiredFields = false;
          }
          
          if (!firstItem.hasOwnProperty('nome') && !firstItem.hasOwnProperty('name')) {
            issues.push('Missing "nome" or "name" field in array items');
            hasRequiredFields = false;
          }
        }
      } else if (typeof response === 'object' && response !== null) {
        dataType = 'object';
        
        // Check for pagination structure
        if (response.hasOwnProperty('items') && response.hasOwnProperty('totalCount')) {
          dataType = 'paginated';
          
          if (!Array.isArray(response.items)) {
            issues.push('Paginated response "items" should be an array');
            isValid = false;
          }
          
          if (typeof response.totalCount !== 'number') {
            issues.push('Paginated response "totalCount" should be a number');
            isValid = false;
          }
        }
      } else {
        dataType = typeof response;
        issues.push(`Unexpected response type: ${dataType}`);
        isValid = false;
      }
      
    } catch (error) {
      issues.push(`Data validation error: ${error}`);
      isValid = false;
      hasRequiredFields = false;
    }
    
    return {
      isValid: isValid && issues.length === 0,
      hasRequiredFields,
      dataType,
      issues
    };
  }
  
  /**
   * Estimate response size
   */
  private estimateResponseSize(response: any): number {
    try {
      return JSON.stringify(response).length * 2; // Rough estimate in bytes
    } catch {
      return 0;
    }
  }
  
  /**
   * Calculate throughput
   */
  private calculateThroughput(response: any, responseTime: number): number {
    const sizeInBytes = this.estimateResponseSize(response);
    const sizeInKB = sizeInBytes / 1024;
    const timeInSeconds = responseTime / 1000;
    
    return timeInSeconds > 0 ? sizeInKB / timeInSeconds : 0; // KB/s
  }
  
  /**
   * Get user-friendly error message
   */
  private getErrorMessage(error: HttpErrorResponse): string {
    switch (error.status) {
      case 0:
        return 'Network connection error - server may be unreachable';
      case 400:
        return 'Bad request - invalid parameters or data';
      case 401:
        return 'Unauthorized - authentication required';
      case 403:
        return 'Forbidden - insufficient permissions';
      case 404:
        return 'Not found - endpoint does not exist';
      case 500:
        return 'Internal server error - server-side issue';
      case 502:
        return 'Bad gateway - server communication error';
      case 503:
        return 'Service unavailable - server temporarily down';
      case 504:
        return 'Gateway timeout - server response too slow';
      default:
        return `HTTP ${error.status}: ${error.message || 'Unknown error'}`;
    }
  }
  
  /**
   * Generate comprehensive API validation report
   */
  private generateApiValidationReport(results: EndpointValidationResult[]): ApiValidationReport {
    const totalEndpoints = results.length;
    const healthyEndpoints = results.filter(r => r.isHealthy).length;
    const errorEndpoints = results.filter(r => r.status === 'error').length;
    
    const avgResponseTime = results
      .filter(r => r.status === 'success')
      .reduce((sum, r) => sum + r.responseTime, 0) / (totalEndpoints - errorEndpoints || 1);
    
    const totalResponseSize = results.reduce((sum, r) => sum + r.responseSize, 0);
    
    const statusCodeDistribution = results.reduce((acc, r) => {
      acc[r.statusCode] = (acc[r.statusCode] || 0) + 1;
      return acc;
    }, {} as Record<number, number>);
    
    const dataValidationIssues = results
      .flatMap(r => r.dataValidation.issues)
      .filter(issue => issue.length > 0);
    
    const performanceSummary = {
      averageResponseTime: avgResponseTime,
      fastestEndpoint: results.reduce((fastest, current) => 
        current.responseTime < fastest.responseTime ? current : fastest
      ),
      slowestEndpoint: results.reduce((slowest, current) => 
        current.responseTime > slowest.responseTime ? current : slowest
      ),
      totalThroughput: results.reduce((sum, r) => sum + r.performanceMetrics.throughput, 0)
    };
    
    const healthScore = Math.round((healthyEndpoints / totalEndpoints) * 100);
    
    return {
      timestamp: new Date(),
      summary: {
        totalEndpoints,
        healthyEndpoints,
        unhealthyEndpoints: totalEndpoints - healthyEndpoints,
        errorEndpoints,
        healthScore,
        averageResponseTime: avgResponseTime,
        totalResponseSize
      },
      endpointResults: results,
      statusCodeDistribution,
      dataValidationSummary: {
        totalIssues: dataValidationIssues.length,
        commonIssues: this.getCommonIssues(dataValidationIssues),
        endpointsWithIssues: results.filter(r => r.dataValidation.issues.length > 0).length
      },
      performanceSummary,
      recommendations: this.generateApiRecommendations(results),
      criticalIssues: this.identifyCriticalIssues(results)
    };
  }
  
  /**
   * Get common data validation issues
   */
  private getCommonIssues(issues: string[]): Array<{ issue: string; count: number }> {
    const issueCounts = issues.reduce((acc, issue) => {
      acc[issue] = (acc[issue] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);
    
    return Object.entries(issueCounts)
      .map(([issue, count]) => ({ issue, count }))
      .sort((a, b) => b.count - a.count)
      .slice(0, 5); // Top 5 issues
  }
  
  /**
   * Generate API recommendations
   */
  private generateApiRecommendations(results: EndpointValidationResult[]): string[] {
    const recommendations: string[] = [];
    
    // Performance recommendations
    const slowEndpoints = results.filter(r => r.responseTime > 2000);
    if (slowEndpoints.length > 0) {
      recommendations.push(`Optimize performance for ${slowEndpoints.length} slow endpoints (>2s response time)`);
    }
    
    // Error recommendations
    const errorEndpoints = results.filter(r => r.status === 'error');
    if (errorEndpoints.length > 0) {
      recommendations.push(`Fix ${errorEndpoints.length} endpoints returning errors`);
    }
    
    // Data validation recommendations
    const dataIssues = results.filter(r => !r.dataValidation.isValid);
    if (dataIssues.length > 0) {
      recommendations.push(`Address data validation issues in ${dataIssues.length} endpoints`);
    }
    
    // Throughput recommendations
    const lowThroughput = results.filter(r => r.performanceMetrics.throughput < 10);
    if (lowThroughput.length > 0) {
      recommendations.push(`Improve throughput for ${lowThroughput.length} endpoints (<10 KB/s)`);
    }
    
    return recommendations;
  }
  
  /**
   * Identify critical issues
   */
  private identifyCriticalIssues(results: EndpointValidationResult[]): string[] {
    const criticalIssues: string[] = [];
    
    // Critical: Any 500 errors
    const serverErrors = results.filter(r => r.statusCode >= 500);
    if (serverErrors.length > 0) {
      criticalIssues.push(`${serverErrors.length} endpoints returning server errors (5xx)`);
    }
    
    // Critical: Very slow responses (>5s)
    const verySlowEndpoints = results.filter(r => r.responseTime > 5000);
    if (verySlowEndpoints.length > 0) {
      criticalIssues.push(`${verySlowEndpoints.length} endpoints with very slow response times (>5s)`);
    }
    
    // Critical: Missing required endpoints
    const missingEndpoints = results.filter(r => r.statusCode === 404);
    if (missingEndpoints.length > 0) {
      criticalIssues.push(`${missingEndpoints.length} required endpoints not found (404)`);
    }
    
    return criticalIssues;
  }
}

// Interfaces
export interface EndpointValidationResult {
  endpointName: string;
  url: string;
  status: 'success' | 'error';
  statusCode: number;
  responseTime: number;
  responseSize: number;
  isHealthy: boolean;
  errorMessage: string | null;
  dataValidation: DataValidationResult;
  performanceMetrics: {
    responseTime: number;
    throughput: number; // KB/s
    availability: number; // percentage
  };
}

export interface DataValidationResult {
  isValid: boolean;
  hasRequiredFields: boolean;
  dataType: string;
  issues: string[];
}

export interface LoadTestResult {
  endpoint: string;
  concurrentRequests: number;
  totalTime: number;
  successfulRequests: number;
  failedRequests: number;
  successRate: number;
  averageResponseTime: number;
  minResponseTime: number;
  maxResponseTime: number;
  throughput: number; // requests per second
  errors: string[];
}

export interface RequestResult {
  success: boolean;
  responseTime: number;
  error: string | null;
}

export interface AvailabilityCheck {
  timestamp: number;
  isAvailable: boolean;
  responseTime: number;
  statusCode: number;
}

export interface AvailabilityTestResult {
  endpoint: string;
  duration: number;
  totalChecks: number;
  availableChecks: number;
  unavailableChecks: number;
  availabilityPercentage: number;
  averageResponseTime: number;
  checks: AvailabilityCheck[];
}

export interface ApiValidationReport {
  timestamp: Date;
  summary: {
    totalEndpoints: number;
    healthyEndpoints: number;
    unhealthyEndpoints: number;
    errorEndpoints: number;
    healthScore: number;
    averageResponseTime: number;
    totalResponseSize: number;
  };
  endpointResults: EndpointValidationResult[];
  statusCodeDistribution: Record<number, number>;
  dataValidationSummary: {
    totalIssues: number;
    commonIssues: Array<{ issue: string; count: number }>;
    endpointsWithIssues: number;
  };
  performanceSummary: {
    averageResponseTime: number;
    fastestEndpoint: EndpointValidationResult;
    slowestEndpoint: EndpointValidationResult;
    totalThroughput: number;
  };
  recommendations: string[];
  criticalIssues: string[];
}