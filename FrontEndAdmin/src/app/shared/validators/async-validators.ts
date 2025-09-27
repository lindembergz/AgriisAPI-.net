import { AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { Observable, of, timer } from 'rxjs';
import { map, switchMap, catchError, take } from 'rxjs/operators';
import { inject } from '@angular/core';
import { ApiValidationService } from '../services/api-validation.service';

/**
 * Async validator for produto codigo uniqueness
 */
export function produtoCodigoUniqueValidator(
  apiValidationService: ApiValidationService,
  excludeId?: number
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value || control.value.length < 2) {
      return of(null);
    }

    return timer(300).pipe(
      switchMap(() => apiValidationService.validateProdutoCodigoUnico(control.value, excludeId)),
      map(isUnique => isUnique ? null : { codigoJaExiste: { value: control.value } }),
      catchError(() => of(null))
    );
  };
}

/**
 * Async validator for produto nome uniqueness
 */
export function produtoNomeUniqueValidator(
  apiValidationService: ApiValidationService,
  excludeId?: number
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value || control.value.length < 3) {
      return of(null);
    }

    return timer(300).pipe(
      switchMap(() => apiValidationService.validateProdutoNomeUnico(control.value, excludeId)),
      map(isUnique => isUnique ? null : { nomeJaExiste: { value: control.value } }),
      catchError(() => of(null))
    );
  };
}

/**
 * Async validator for fornecedor CNPJ uniqueness
 */
export function fornecedorCnpjUniqueValidator(
  apiValidationService: ApiValidationService,
  excludeId?: number
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value || control.value.length < 14) {
      return of(null);
    }

    return timer(300).pipe(
      switchMap(() => apiValidationService.validateFornecedorCnpjDisponivel(control.value, excludeId)),
      map(isAvailable => isAvailable ? null : { cnpjJaExiste: { value: control.value } }),
      catchError(() => of(null))
    );
  };
}

/**
 * Async validator for produtor CPF uniqueness
 */
export function produtorCpfUniqueValidator(
  apiValidationService: ApiValidationService,
  excludeId?: number
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value || control.value.length < 11) {
      return of(null);
    }

    return timer(300).pipe(
      switchMap(() => apiValidationService.validateProdutorCpfDisponivel(control.value, excludeId)),
      map(isAvailable => isAvailable ? null : { cpfJaExiste: { value: control.value } }),
      catchError(() => of(null))
    );
  };
}

/**
 * Async validator for produtor CNPJ uniqueness
 */
export function produtorCnpjUniqueValidator(
  apiValidationService: ApiValidationService,
  excludeId?: number
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value || control.value.length < 14) {
      return of(null);
    }

    return timer(300).pipe(
      switchMap(() => apiValidationService.validateProdutorCnpjDisponivel(control.value, excludeId)),
      map(isAvailable => isAvailable ? null : { cnpjJaExiste: { value: control.value } }),
      catchError(() => of(null))
    );
  };
}

/**
 * Async validator for geographic relationship (UF-Município)
 */
export function geographicRelationshipValidator(
  apiValidationService: ApiValidationService,
  ufIdControlName: string = 'ufId'
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    const parent = control.parent;
    if (!parent || !control.value) {
      return of(null);
    }

    const ufId = parent.get(ufIdControlName)?.value;
    if (!ufId) {
      return of(null);
    }

    return timer(300).pipe(
      switchMap(() => apiValidationService.validateGeographicRelationship(ufId, control.value)),
      map(isValid => isValid ? null : { municipioNaoPertenceUf: { municipioId: control.value, ufId } }),
      catchError(() => of(null))
    );
  };
}

/**
 * Async validator for reference entity existence and active status
 */
export function referenceEntityValidator(
  apiValidationService: ApiValidationService,
  entityType: string
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value) {
      return of(null);
    }

    return timer(300).pipe(
      switchMap(() => apiValidationService.validateReferenceEntity(entityType, control.value)),
      map(isValid => isValid ? null : { referenciaInvalida: { entityType, id: control.value } }),
      catchError(() => of(null))
    );
  };
}

/**
 * Cross-field validator for produto references
 */
export function produtoReferencesValidator(
  apiValidationService: ApiValidationService
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value) {
      return of(null);
    }

    const dto = {
      unidadeMedidaId: control.get('unidadeMedidaId')?.value,
      embalagemId: control.get('embalagemId')?.value,
      atividadeAgropecuariaId: control.get('atividadeAgropecuariaId')?.value,
      categoriaId: control.get('categoriaId')?.value,
      fornecedorId: control.get('fornecedorId')?.value
    };

    // Only validate if we have the required fields
    if (!dto.unidadeMedidaId || !dto.categoriaId || !dto.fornecedorId) {
      return of(null);
    }

    return timer(500).pipe(
      switchMap(() => apiValidationService.validateProdutoReferencias(dto)),
      map(result => result.isValid ? null : { referenciasInvalidas: { errors: result.errors } }),
      catchError(() => of(null))
    );
  };
}