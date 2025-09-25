/**
 * Bundle optimization configuration
 * Defines strategies for optimizing application bundle size and loading performance
 */

/**
 * Lazy loading configuration for feature modules
 */
export const LAZY_LOADING_CONFIG = {
  // Feature modules that should be lazy loaded
  lazyModules: [
    {
      path: 'produtores',
      loadChildren: () => import('../../features/produtores/produtores.routes').then(m => m.produtoresRoutes),
      preload: false // Don't preload unless user navigates
    },
    {
      path: 'fornecedores', 
      loadChildren: () => import('../../features/fornecedores/fornecedores.routes').then(m => m.fornecedoresRoutes),
      preload: false
    },

  ],

  // Modules that should be preloaded for better UX
  preloadModules: [
    'dashboard' // Dashboard is commonly accessed
  ]
};

/**
 * PrimeNG module optimization
 * Only import specific components to reduce bundle size
 */
export const PRIMENG_OPTIMIZATION = {
  // Core modules (always needed)
  coreModules: [
    'ButtonModule',
    'InputTextModule',
    'MessageModule',
    'ToastModule',
    'ConfirmDialogModule'
  ],

  // Feature-specific modules (lazy loaded with features)
  featureModules: {
    table: ['TableModule', 'PaginatorModule'],
    forms: ['DropdownModule', 'CalendarModule', 'InputMaskModule'],
    layout: ['TabViewModule', 'PanelModule', 'CardModule'],
    maps: ['GMapModule']
  },

  // Optional modules (loaded on demand)
  optionalModules: [
    'ChartModule',
    'FileUploadModule',
    'ProgressBarModule'
  ]
};

/**
 * Tree shaking configuration
 */
export const TREE_SHAKING_CONFIG = {
  // Libraries that support tree shaking
  treeShakableLibraries: [
    'lodash-es', // Use lodash-es instead of lodash
    'date-fns', // Use specific date-fns functions
    'rxjs/operators' // Import specific operators
  ],

  // Import patterns to avoid (these prevent tree shaking)
  avoidPatterns: [
    'import * from "lodash"', // Import entire library
    'import "rxjs"', // Import entire RxJS
    'import { } from "primeng/primeng"' // Import entire PrimeNG
  ],

  // Recommended import patterns
  recommendedPatterns: [
    'import { debounce } from "lodash-es"',
    'import { map, filter } from "rxjs/operators"',
    'import { ButtonModule } from "primeng/button"'
  ]
};

/**
 * Code splitting strategies
 */
export const CODE_SPLITTING_CONFIG = {
  // Vendor chunks configuration
  vendorChunks: {
    angular: ['@angular/core', '@angular/common', '@angular/forms'],
    primeng: ['primeng'],
    rxjs: ['rxjs'],
    maps: ['@angular/google-maps']
  },

  // Dynamic imports for large components
  dynamicImports: [
    {
      component: 'GoogleMapsComponent',
      condition: 'when maps are needed',
      import: () => import('../../shared/components/coordenadas-map.component')
    },

  ]
};

/**
 * Asset optimization configuration
 */
export const ASSET_OPTIMIZATION_CONFIG = {
  // Image optimization settings
  images: {
    formats: ['webp', 'jpg', 'png'],
    maxWidth: 1920,
    quality: 80,
    lazyLoading: true
  },

  // Font optimization
  fonts: {
    preload: ['primary-font.woff2'],
    display: 'swap',
    fallbacks: ['Arial', 'sans-serif']
  },

  // CSS optimization
  css: {
    purgeUnused: true,
    minify: true,
    criticalCSS: true
  }
};

/**
 * Performance budgets
 */
export const PERFORMANCE_BUDGETS = {
  // Bundle size limits
  bundles: {
    initial: {
      warning: '2MB',
      error: '5MB'
    },
    lazy: {
      warning: '1MB',
      error: '2MB'
    }
  },

  // Asset size limits
  assets: {
    images: {
      warning: '500KB',
      error: '1MB'
    },
    fonts: {
      warning: '100KB',
      error: '200KB'
    }
  },

  // Performance metrics
  metrics: {
    firstContentfulPaint: 2000, // 2 seconds
    largestContentfulPaint: 4000, // 4 seconds
    firstInputDelay: 100, // 100ms
    cumulativeLayoutShift: 0.1
  }
};

/**
 * Preloading strategies
 */
export const PRELOADING_STRATEGIES = {
  // Network-aware preloading
  networkAware: {
    enabled: true,
    conditions: {
      connectionType: ['4g', 'wifi'],
      saveData: false
    }
  },

  // User behavior based preloading
  behaviorBased: {
    enabled: true,
    triggers: [
      'hover', // Preload on hover
      'viewport', // Preload when in viewport
      'idle' // Preload during idle time
    ]
  },

  // Time-based preloading
  timeBased: {
    enabled: true,
    delay: 2000, // Wait 2 seconds after initial load
    priority: ['dashboard', 'produtores', 'fornecedores']
  }
};

/**
 * Service worker configuration for caching
 */
export const SERVICE_WORKER_CONFIG = {
  // Cache strategies
  cacheStrategies: {
    static: 'CacheFirst', // HTML, CSS, JS
    api: 'NetworkFirst', // API calls
    images: 'CacheFirst', // Images
    fonts: 'CacheFirst' // Fonts
  },

  // Cache durations
  cacheDurations: {
    static: '1y', // 1 year for static assets
    api: '5m', // 5 minutes for API responses
    images: '30d', // 30 days for images
    fonts: '1y' // 1 year for fonts
  },

  // Update strategies
  updateStrategies: {
    checkInterval: '1h', // Check for updates every hour
    forceUpdate: false, // Don't force immediate updates
    notifyUser: true // Notify user of available updates
  }
};

/**
 * Bundle analysis configuration
 */
export const BUNDLE_ANALYSIS_CONFIG = {
  // Tools for bundle analysis
  tools: [
    'webpack-bundle-analyzer',
    'source-map-explorer',
    'bundlephobia'
  ],

  // Metrics to track
  metrics: [
    'bundleSize',
    'chunkCount',
    'duplicateModules',
    'unusedExports'
  ],

  // Thresholds for alerts
  thresholds: {
    bundleSize: 5 * 1024 * 1024, // 5MB
    chunkCount: 20,
    duplicateModules: 5
  }
};

/**
 * Runtime optimization configuration
 */
export const RUNTIME_OPTIMIZATION_CONFIG = {
  // Change detection optimization
  changeDetection: {
    strategy: 'OnPush', // Use OnPush where possible
    trackByFunctions: true, // Use trackBy for ngFor
    pureComponents: true // Prefer pure components
  },

  // Memory management
  memoryManagement: {
    subscriptionCleanup: true,
    eventListenerCleanup: true,
    timerCleanup: true,
    googleMapsCleanup: true
  },

  // Performance monitoring
  monitoring: {
    enabled: true,
    metrics: ['navigation', 'resource', 'paint', 'layout-shift'],
    reporting: {
      console: true,
      analytics: false // Set to true for production analytics
    }
  }
};