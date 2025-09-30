/**
 * API optimization configuration
 * Defines strategies for optimizing API calls and reducing unnecessary requests
 */

/**
 * Debounce configuration for different types of operations
 */
export const DEBOUNCE_CONFIG = {
  // Search operations
  search: {
    delay: 300, // 300ms delay for search
    minLength: 2 // Minimum characters before search
  },
  
  // Form validation
  validation: {
    delay: 500, // 500ms delay for validation
    minLength: 1
  },
  
  // Auto-save operations
  autoSave: {
    delay: 1000, // 1 second delay for auto-save
    minLength: 0
  },
  
  // Filter operations
  filter: {
    delay: 200, // 200ms delay for filters
    minLength: 0
  }
};

/**
 * Pagination configuration for optimal performance
 */
export const PAGINATION_CONFIG = {
  // Default page sizes for different data types
  defaultPageSizes: {
    referencias: 20, // Reference data - smaller pages
    usuarios: 15, // User data - medium pages
    transacoes: 10, // Transaction data - smaller pages for complex data
    relatorios: 25 // Reports - larger pages for simple data
  },
  
  // Maximum page sizes to prevent performance issues
  maxPageSizes: {
    referencias: 100,
    usuarios: 50,
    transacoes: 25,
    relatorios: 100
  },
  
  // Preload configuration
  preload: {
    enabled: true,
    nextPages: 1, // Preload next 1 page
    threshold: 0.8 // Start preloading when 80% through current page
  }
};

/**
 * Request batching configuration
 */
export const BATCHING_CONFIG = {
  // Batch similar requests together
  enabled: true,
  
  // Maximum batch size
  maxBatchSize: 10,
  
  // Batch timeout (ms) - send batch even if not full
  batchTimeout: 100,
  
  // Operations that can be batched
  batchableOperations: [
    'verificar-codigo-unico',
    'verificar-nome-unico',
    'obter-por-ids',
    'validar-campos'
  ]
};

/**
 * Request deduplication configuration
 */
export const DEDUPLICATION_CONFIG = {
  // Enable request deduplication
  enabled: true,
  
  // Time window for deduplication (ms)
  timeWindow: 1000,
  
  // Operations to deduplicate
  deduplicateOperations: [
    'obter-todos',
    'obter-ativos',
    'obter-por-id',
    'buscar'
  ]
};

/**
 * Connection optimization configuration
 */
export const CONNECTION_CONFIG = {
  // HTTP/2 server push hints
  serverPushHints: [
    '/api/referencias/paises/ativos',
    '/api/referencias/ufs',
    '/api/referencias/moedas/ativos'
  ],
  
  // Connection pooling
  connectionPool: {
    maxConnections: 6, // Browser default
    keepAlive: true,
    timeout: 30000 // 30 seconds
  },
  
  // Request prioritization
  priorities: {
    critical: ['login', 'logout', 'refresh-token'],
    high: ['obter-ativos', 'buscar'],
    normal: ['obter-todos', 'obter-por-id'],
    low: ['estatisticas', 'relatorios']
  }
};

/**
 * Retry configuration for failed requests
 */
export const RETRY_CONFIG = {
  // Enable automatic retries
  enabled: true,
  
  // Maximum retry attempts
  maxRetries: 3,
  
  // Retry delay strategy
  delayStrategy: 'exponential', // 'fixed' | 'exponential' | 'linear'
  
  // Base delay (ms)
  baseDelay: 1000,
  
  // Maximum delay (ms)
  maxDelay: 10000,
  
  // HTTP status codes to retry
  retryableStatusCodes: [408, 429, 500, 502, 503, 504],
  
  // Operations that should not be retried
  nonRetryableOperations: [
    'criar',
    'atualizar', 
    'remover',
    'ativar',
    'desativar'
  ]
};

/**
 * Compression configuration
 */
export const COMPRESSION_CONFIG = {
  // Enable request/response compression
  enabled: true,
  
  // Compression algorithms in order of preference
  algorithms: ['gzip', 'deflate', 'br'],
  
  // Minimum response size to compress (bytes)
  minSize: 1024,
  
  // Content types to compress
  compressibleTypes: [
    'application/json',
    'text/plain',
    'text/html',
    'text/css',
    'application/javascript'
  ]
};

/**
 * Prefetching configuration
 */
export const PREFETCH_CONFIG = {
  // Enable intelligent prefetching
  enabled: true,
  
  // Prefetch strategies
  strategies: {
    // Prefetch on hover (for navigation)
    hover: {
      enabled: true,
      delay: 100 // ms
    },
    
    // Prefetch based on user behavior
    behavioral: {
      enabled: true,
      confidence: 0.7 // 70% confidence threshold
    },
    
    // Prefetch during idle time
    idle: {
      enabled: true,
      idleTime: 2000 // 2 seconds of idle time
    }
  },
  
  // Resources to prefetch
  resources: {
    // Critical reference data
    critical: [
      '/api/referencias/paises/ativos',
      '/api/referencias/ufs',
      '/api/referencias/moedas/ativos'
    ],
    
    // Likely next pages
    likely: [
      '/api/referencias/unidades-medida',
      '/api/referencias/categorias'
    ]
  }
};

/**
 * Monitoring configuration for API performance
 */
export const MONITORING_CONFIG = {
  // Enable performance monitoring
  enabled: true,
  
  // Metrics to collect
  metrics: [
    'requestDuration',
    'responseSize',
    'errorRate',
    'cacheHitRate',
    'retryCount'
  ],
  
  // Performance thresholds
  thresholds: {
    slowRequest: 2000, // 2 seconds
    largeResponse: 1024 * 1024, // 1MB
    highErrorRate: 0.05, // 5%
    lowCacheHitRate: 0.7 // 70%
  },
  
  // Sampling rate (0-1)
  samplingRate: 0.1, // 10% of requests
  
  // Reporting configuration
  reporting: {
    console: true,
    analytics: false, // Set to true for production
    interval: 60000 // Report every minute
  }
};

/**
 * Offline support configuration
 */
export const OFFLINE_CONFIG = {
  // Enable offline support
  enabled: true,
  
  // Cache strategies for offline
  cacheStrategies: {
    referencias: 'CacheFirst', // Reference data - cache first
    usuarios: 'NetworkFirst', // User data - network first
    transacoes: 'NetworkOnly' // Transactions - network only
  },
  
  // Offline queue for failed requests
  offlineQueue: {
    enabled: true,
    maxSize: 100,
    retryInterval: 30000 // 30 seconds
  },
  
  // Background sync
  backgroundSync: {
    enabled: true,
    syncInterval: 300000 // 5 minutes
  }
};

/**
 * Bundle splitting configuration for API modules
 */
export const API_BUNDLE_CONFIG = {
  // Split API services into separate chunks
  splitByFeature: true,
  
  // Chunk configuration
  chunks: {
    // Core API services (always loaded)
    core: [
      'auth.service',
      'error.service',
      'cache.service'
    ],
    
    // Reference services (lazy loaded)
    referencias: [
      'moeda.service',
      'pais.service',
      'uf.service',
      'unidade-medida.service'
    ],
    
    // Business services (lazy loaded)
    business: [
      'produtor.service',
      'fornecedor.service',
      'produto.service'
    ]
  }
};

/**
 * Performance budget for API operations
 */
export const API_PERFORMANCE_BUDGET = {
  // Response time budgets (ms)
  responseTimes: {
    critical: 500, // Critical operations (login, etc.)
    interactive: 1000, // Interactive operations (search, etc.)
    background: 3000 // Background operations (reports, etc.)
  },
  
  // Payload size budgets (bytes)
  payloadSizes: {
    small: 10 * 1024, // 10KB
    medium: 100 * 1024, // 100KB
    large: 1024 * 1024 // 1MB
  },
  
  // Concurrent request limits
  concurrency: {
    max: 6, // Maximum concurrent requests
    perDomain: 6, // Per domain limit
    critical: 2 // Reserved for critical operations
  }
};