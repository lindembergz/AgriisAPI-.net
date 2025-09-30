import { Injectable, signal } from '@angular/core';
import { Observable, BehaviorSubject, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';

/**
 * Loading state types for different operations
 */
export type LoadingStateType = 
  | 'table-data'
  | 'form-submit'
  | 'item-action'
  | 'validation'
  | 'search'
  | 'filter'
  | 'export'
  | 'import';

/**
 * Loading state entry
 */
interface LoadingStateEntry {
  type: LoadingStateType;
  key: string;
  message?: string;
  progress?: number;
  timestamp: number;
}

/**
 * Service for managing loading states across the application
 * Provides centralized loading state management with progress tracking
 */
@Injectable({
  providedIn: 'root'
})
export class LoadingStateService {
  
  private loadingStates = new Map<string, LoadingStateEntry>();
  private loadingStatesSubject = new BehaviorSubject<Map<string, LoadingStateEntry>>(new Map());
  
  // Signals for reactive state management
  private globalLoadingSignal = signal<boolean>(false);
  private loadingCountSignal = signal<number>(0);
  private loadingMessagesSignal = signal<string[]>([]);

  /**
   * Get global loading state as signal
   */
  get isGlobalLoading() {
    return this.globalLoadingSignal.asReadonly();
  }

  /**
   * Get loading count as signal
   */
  get loadingCount() {
    return this.loadingCountSignal.asReadonly();
  }

  /**
   * Get loading messages as signal
   */
  get loadingMessages() {
    return this.loadingMessagesSignal.asReadonly();
  }

  /**
   * Get loading states as observable
   */
  get loadingStates$(): Observable<Map<string, LoadingStateEntry>> {
    return this.loadingStatesSubject.asObservable();
  }

  /**
   * Set loading state for a specific operation
   */
  setLoading(
    type: LoadingStateType,
    key: string,
    loading: boolean,
    message?: string,
    progress?: number
  ): void {
    const stateKey = `${type}-${key}`;
    
    if (loading) {
      const entry: LoadingStateEntry = {
        type,
        key,
        message,
        progress,
        timestamp: Date.now()
      };
      
      this.loadingStates.set(stateKey, entry);
    } else {
      this.loadingStates.delete(stateKey);
    }
    
    this.updateSignals();
    this.loadingStatesSubject.next(new Map(this.loadingStates));
  }

  /**
   * Check if specific operation is loading
   */
  isLoading(type: LoadingStateType, key: string): boolean {
    const stateKey = `${type}-${key}`;
    return this.loadingStates.has(stateKey);
  }

  /**
   * Check if any operation of specific type is loading
   */
  isTypeLoading(type: LoadingStateType): boolean {
    for (const [key, entry] of this.loadingStates) {
      if (entry.type === type) {
        return true;
      }
    }
    return false;
  }

  /**
   * Get loading state for specific operation
   */
  getLoadingState(type: LoadingStateType, key: string): LoadingStateEntry | null {
    const stateKey = `${type}-${key}`;
    return this.loadingStates.get(stateKey) || null;
  }

  /**
   * Get all loading states of specific type
   */
  getLoadingStatesByType(type: LoadingStateType): LoadingStateEntry[] {
    const states: LoadingStateEntry[] = [];
    for (const [key, entry] of this.loadingStates) {
      if (entry.type === type) {
        states.push(entry);
      }
    }
    return states;
  }

  /**
   * Clear all loading states
   */
  clearAll(): void {
    this.loadingStates.clear();
    this.updateSignals();
    this.loadingStatesSubject.next(new Map());
  }

  /**
   * Clear loading states of specific type
   */
  clearType(type: LoadingStateType): void {
    const keysToDelete: string[] = [];
    
    for (const [key, entry] of this.loadingStates) {
      if (entry.type === type) {
        keysToDelete.push(key);
      }
    }
    
    keysToDelete.forEach(key => this.loadingStates.delete(key));
    this.updateSignals();
    this.loadingStatesSubject.next(new Map(this.loadingStates));
  }

  /**
   * Update progress for specific loading state
   */
  updateProgress(type: LoadingStateType, key: string, progress: number): void {
    const stateKey = `${type}-${key}`;
    const entry = this.loadingStates.get(stateKey);
    
    if (entry) {
      entry.progress = Math.max(0, Math.min(100, progress));
      this.loadingStates.set(stateKey, entry);
      this.loadingStatesSubject.next(new Map(this.loadingStates));
    }
  }

  /**
   * Update message for specific loading state
   */
  updateMessage(type: LoadingStateType, key: string, message: string): void {
    const stateKey = `${type}-${key}`;
    const entry = this.loadingStates.get(stateKey);
    
    if (entry) {
      entry.message = message;
      this.loadingStates.set(stateKey, entry);
      this.loadingStatesSubject.next(new Map(this.loadingStates));
    }
  }

  /**
   * Get observable for specific loading state
   */
  getLoadingState$(type: LoadingStateType, key: string): Observable<boolean> {
    return this.loadingStates$.pipe(
      map(states => {
        const stateKey = `${type}-${key}`;
        return states.has(stateKey);
      })
    );
  }

  /**
   * Get observable for specific type loading state
   */
  getTypeLoading$(type: LoadingStateType): Observable<boolean> {
    return this.loadingStates$.pipe(
      map(states => {
        for (const [key, entry] of states) {
          if (entry.type === type) {
            return true;
          }
        }
        return false;
      })
    );
  }

  /**
   * Get combined loading state for multiple operations
   */
  getCombinedLoading$(operations: Array<{ type: LoadingStateType; key: string }>): Observable<boolean> {
    const observables = operations.map(op => this.getLoadingState$(op.type, op.key));
    
    return combineLatest(observables).pipe(
      map(states => states.some(state => state))
    );
  }

  /**
   * Update reactive signals
   */
  private updateSignals(): void {
    const hasLoading = this.loadingStates.size > 0;
    const newMessages = Array.from(this.loadingStates.values())
      .map(entry => entry.message)
      .filter(message => message !== undefined) as string[];
    
    // Only update if the content of the messages array has changed
    const currentMessages = this.loadingMessagesSignal();
    if (newMessages.length !== currentMessages.length || newMessages.some((msg, i) => msg !== currentMessages[i])) {
      this.loadingMessagesSignal.set(newMessages);
    }

    this.globalLoadingSignal.set(hasLoading);
    this.loadingCountSignal.set(this.loadingStates.size);
  }

  /**
   * Create loading wrapper for async operations
   */
  wrapAsync<T>(
    type: LoadingStateType,
    key: string,
    operation: () => Observable<T>,
    message?: string
  ): Observable<T> {
    return new Observable<T>(subscriber => {
      this.setLoading(type, key, true, message);
      
      const subscription = operation().subscribe({
        next: (value) => {
          subscriber.next(value);
        },
        error: (error) => {
          this.setLoading(type, key, false);
          subscriber.error(error);
        },
        complete: () => {
          this.setLoading(type, key, false);
          subscriber.complete();
        }
      });
      
      return () => {
        this.setLoading(type, key, false);
        subscription.unsubscribe();
      };
    });
  }

  /**
   * Create loading wrapper for promises
   */
  async wrapPromise<T>(
    type: LoadingStateType,
    key: string,
    operation: () => Promise<T>,
    message?: string
  ): Promise<T> {
    this.setLoading(type, key, true, message);
    
    try {
      const result = await operation();
      this.setLoading(type, key, false);
      return result;
    } catch (error) {
      this.setLoading(type, key, false);
      throw error;
    }
  }

  /**
   * Cleanup old loading states (older than 5 minutes)
   */
  cleanup(): void {
    const fiveMinutesAgo = Date.now() - (5 * 60 * 1000);
    const keysToDelete: string[] = [];
    
    for (const [key, entry] of this.loadingStates) {
      if (entry.timestamp < fiveMinutesAgo) {
        keysToDelete.push(key);
      }
    }
    
    if (keysToDelete.length > 0) {
      keysToDelete.forEach(key => this.loadingStates.delete(key));
      this.updateSignals();
      this.loadingStatesSubject.next(new Map(this.loadingStates));
    }
  }
}