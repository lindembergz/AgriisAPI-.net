import { TableColumn } from '../interfaces/component-template.interface';

/**
 * Standard column configurations for reference components
 */
export const STANDARD_COLUMNS = {
  unidadesMedida: [
    { 
      field: 'simbolo', 
      header: 'Símbolo', 
      sortable: true, 
      width: '120px' 
    },
    { 
      field: 'nome', 
      header: 'Nome', 
      sortable: true, 
      width: '250px' 
    },
    { 
      field: 'tipo', 
      header: 'Tipo', 
      sortable: true, 
      width: '150px', 
      type: 'custom' as const
    },
    { 
      field: 'fatorConversao', 
      header: 'Fator Conversão', 
      sortable: true, 
      width: '150px', 
      type: 'number' as const, 
      hideOnMobile: true 
    },
    { 
      field: 'ativo', 
      header: 'Status', 
      sortable: true, 
      width: '100px', 
      type: 'boolean' as const, 
      hideOnMobile: true 
    },
    { 
      field: 'dataCriacao', 
      header: 'Criado em', 
      sortable: true, 
      width: '150px', 
      type: 'date' as const, 
      hideOnMobile: true, 
      hideOnTablet: true 
    }
  ] as TableColumn[],
  
  ufs: [
    { 
      field: 'codigo', 
      header: 'Código', 
      sortable: true, 
      width: '100px' 
    },
    { 
      field: 'nome', 
      header: 'Nome', 
      sortable: true, 
      width: '250px' 
    },
    { 
      field: 'pais.nome', 
      header: 'País', 
      sortable: true, 
      width: '200px', 
      hideOnMobile: true 
    },
    { 
      field: 'municipiosCount', 
      header: 'Municípios', 
      sortable: false, 
      width: '120px', 
      type: 'custom' as const, 
      hideOnMobile: true 
    },
    { 
      field: 'ativo', 
      header: 'Status', 
      sortable: true, 
      width: '100px', 
      type: 'boolean' as const, 
      hideOnMobile: true 
    },
    { 
      field: 'dataCriacao', 
      header: 'Criado em', 
      sortable: true, 
      width: '150px', 
      type: 'date' as const, 
      hideOnMobile: true, 
      hideOnTablet: true 
    }
  ] as TableColumn[],
  
  moedas: [
    { 
      field: 'codigo', 
      header: 'Código', 
      sortable: true, 
      width: '120px' 
    },
    { 
      field: 'nome', 
      header: 'Nome', 
      sortable: true, 
      width: '300px' 
    },
    { 
      field: 'simbolo', 
      header: 'Símbolo', 
      sortable: true, 
      width: '100px' 
    },
    { 
      field: 'ativo', 
      header: 'Status', 
      sortable: true, 
      width: '100px', 
      type: 'boolean' as const, 
      hideOnMobile: true 
    },
    { 
      field: 'dataCriacao', 
      header: 'Criado em', 
      sortable: true, 
      width: '150px', 
      type: 'date' as const, 
      hideOnMobile: true, 
      hideOnTablet: true 
    }
  ] as TableColumn[],
  
  paises: [
    { 
      field: 'codigo', 
      header: 'Código', 
      sortable: true, 
      width: '120px' 
    },
    { 
      field: 'nome', 
      header: 'Nome', 
      sortable: true, 
      width: '300px' 
    },
    { 
      field: 'quantidadeUfs', 
      header: 'UFs', 
      sortable: false, 
      width: '100px', 
      type: 'custom' as const, 
      hideOnMobile: true 
    },
    { 
      field: 'ativo', 
      header: 'Status', 
      sortable: true, 
      width: '100px', 
      type: 'boolean' as const, 
      hideOnMobile: true 
    },
    { 
      field: 'dataCriacao', 
      header: 'Criado em', 
      sortable: true, 
      width: '150px', 
      type: 'date' as const, 
      hideOnMobile: true, 
      hideOnTablet: true 
    }
  ] as TableColumn[],
  
  categorias: [
    { 
      field: 'nome', 
      header: 'Nome', 
      sortable: true, 
      width: '40%' 
    },
    { 
      field: 'tipo', 
      header: 'Tipo', 
      sortable: true, 
      width: '20%', 
      type: 'custom' as const, 
      hideOnMobile: true 
    },
    { 
      field: 'ativo', 
      header: 'Status', 
      sortable: true, 
      width: '15%', 
      type: 'boolean' as const 
    },
    { 
      field: 'ordem', 
      header: 'Ordem', 
      sortable: true, 
      width: '10%', 
      hideOnMobile: true 
    },
    { 
      field: 'acoes', 
      header: 'Ações', 
      sortable: false, 
      width: '15%', 
      type: 'custom' as const 
    }
  ] as TableColumn[],
  
  municipios: [
    { 
      field: 'nome', 
      header: 'Nome', 
      sortable: true, 
      width: '250px' 
    },
    { 
      field: 'uf.nome', 
      header: 'UF', 
      sortable: true, 
      width: '150px', 
      hideOnMobile: true 
    },
    { 
      field: 'cep', 
      header: 'CEP', 
      sortable: true, 
      width: '120px', 
      hideOnMobile: true 
    },
    { 
      field: 'ativo', 
      header: 'Status', 
      sortable: true, 
      width: '100px', 
      type: 'boolean' as const, 
      hideOnMobile: true 
    },
    { 
      field: 'dataCriacao', 
      header: 'Criado em', 
      sortable: true, 
      width: '150px', 
      type: 'date' as const, 
      hideOnMobile: true, 
      hideOnTablet: true 
    }
  ] as TableColumn[],
  
  atividadesAgropecuarias: [
    { 
      field: 'codigo', 
      header: 'Código', 
      sortable: true, 
      width: '120px' 
    },
    { 
      field: 'descricao', 
      header: 'Descrição', 
      sortable: true, 
      width: '300px' 
    },
    { 
      field: 'tipoDescricao', 
      header: 'Tipo', 
      sortable: true, 
      width: '120px' 
    },
    { 
      field: 'ativo', 
      header: 'Status', 
      sortable: true, 
      width: '100px', 
      type: 'boolean' as const, 
      hideOnMobile: true 
    },
    { 
      field: 'dataCriacao', 
      header: 'Criado em', 
      sortable: true, 
      width: '150px', 
      type: 'date' as const, 
      hideOnMobile: true, 
      hideOnTablet: true 
    }
  ] as TableColumn[],
  
  embalagens: [
    { 
      field: 'nome', 
      header: 'Nome', 
      sortable: true, 
      width: '250px' 
    },
    { 
      field: 'descricao', 
      header: 'Descrição', 
      sortable: true, 
      width: '300px', 
      hideOnMobile: true 
    },
    { 
      field: 'ativo', 
      header: 'Status', 
      sortable: true, 
      width: '100px', 
      type: 'boolean' as const, 
      hideOnMobile: true 
    },
    { 
      field: 'dataCriacao', 
      header: 'Criado em', 
      sortable: true, 
      width: '150px', 
      type: 'date' as const, 
      hideOnMobile: true, 
      hideOnTablet: true 
    }
  ] as TableColumn[]
};

/**
 * Get standard columns for a component
 */
export function getStandardColumns(componentName: string): TableColumn[] {
  return STANDARD_COLUMNS[componentName as keyof typeof STANDARD_COLUMNS] || [];
}

/**
 * Common column definitions that can be reused
 */
export const COMMON_COLUMNS = {
  id: { 
    field: 'id', 
    header: 'ID', 
    sortable: true, 
    width: '80px', 
    hideOnMobile: true, 
    hideOnTablet: true 
  } as TableColumn,
  
  ativo: { 
    field: 'ativo', 
    header: 'Status', 
    sortable: true, 
    width: '100px', 
    type: 'boolean' as const, 
    hideOnMobile: true 
  } as TableColumn,
  
  dataCriacao: { 
    field: 'dataCriacao', 
    header: 'Criado em', 
    sortable: true, 
    width: '150px', 
    type: 'date' as const, 
    hideOnMobile: true, 
    hideOnTablet: true 
  } as TableColumn,
  
  dataAtualizacao: { 
    field: 'dataAtualizacao', 
    header: 'Atualizado em', 
    sortable: true, 
    width: '150px', 
    type: 'date' as const, 
    hideOnMobile: true, 
    hideOnTablet: true 
  } as TableColumn,
  
  acoes: { 
    field: 'acoes', 
    header: 'Ações', 
    sortable: false, 
    width: '150px', 
    type: 'custom' as const 
  } as TableColumn
};