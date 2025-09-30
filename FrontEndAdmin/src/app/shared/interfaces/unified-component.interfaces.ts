export interface ComponentTemplate {
  displayMode: DisplayMode;
  hasCustomActions: boolean;
  hasCustomFilters: boolean;
  hasCustomFields: boolean;
  supportsBulkOperations: boolean;
  supportsExport: boolean;
}

export interface CustomFilter {
  key: string;
  label: string;
  placeholder: string;
  type: 'select' | 'multiselect' | 'datepicker';
  visible: boolean;
  options: any[];
  multiple?: boolean;
}

export interface CustomAction {
  key: string;
  label: string;
  icon: string;
  styleClass?: string;
  tooltip?: string;
  action: () => void;
}

export interface TableColumn {
  field: string;
  header: string;
  sortable: boolean;
  width?: string;
  type: 'text' | 'boolean' | 'date' | 'custom';
  align?: 'left' | 'center' | 'right';
  hideOnMobile?: boolean;
  hideOnTablet?: boolean;
}

export interface EmptyStateConfig {
  icon: string;
  title: string;
  description: string;
  primaryAction: {
    label: string;
    icon: string;
    action: () => void;
    styleClass?: string;
  };
  secondaryActions?: {
    label: string;
    icon: string;
    action: () => void;
    styleClass?: string;
  }[];
  helpText?: string;
}

export interface LoadingStateConfig {
  message: string;
  showProgress: boolean;
  subMessage?: string;
}

export interface ResponsiveConfig {
  mobile: {
    breakpoint: number;
    hiddenColumns: string[];
    compactMode: boolean;
    cardView: boolean;
  };
  tablet: {
    breakpoint: number;
    hiddenColumns: string[];
    compactMode: boolean;
  };
  desktop: {
    breakpoint: number;
    features: string[];
  };
}

export interface DialogConfig {
  width?: string;
  maxWidth?: string;
  height?: string;
  maxHeight?: string;
  resizable?: boolean;
  draggable?: boolean;
  modal?: boolean;
  closable?: boolean;
  blockScroll?: boolean;
}

export type DisplayMode = 'table' | 'tree' | 'cards';

export interface ActiveFilter {
  key: string;
  label: string;
  value: any;
  labelValue?: string;
}

export interface SortConfig {
  sortField: string;
  sortOrder: number;
}

export interface PaginationConfig {
  first: number;
  rows: number;
}
