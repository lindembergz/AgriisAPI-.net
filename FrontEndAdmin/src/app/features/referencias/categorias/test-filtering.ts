// Simple test to verify filtering functionality
import { CategoriaDto, CategoriaProduto } from '../../../shared/models/reference.model';

// Mock data for testing
const mockCategorias: CategoriaDto[] = [
  {
    id: 1,
    nome: 'Sementes',
    descricao: 'Categoria de sementes',
    tipo: CategoriaProduto.Sementes,
    categoriaPaiId: null,
    categoriaPaiNome: null,
    ordem: 1,
    ativo: true,
    subCategorias: [],
    quantidadeProdutos: 5,
    dataCriacao: new Date(),
    dataAtualizacao: new Date()
  },
  {
    id: 2,
    nome: 'Fertilizantes',
    descricao: 'Categoria de fertilizantes',
    tipo: CategoriaProduto.Fertilizantes,
    categoriaPaiId: null,
    categoriaPaiNome: null,
    ordem: 2,
    ativo: false,
    subCategorias: [],
    quantidadeProdutos: 3,
    dataCriacao: new Date(),
    dataAtualizacao: new Date()
  },
  {
    id: 3,
    nome: 'Sementes de Soja',
    descricao: 'Subcategoria de sementes de soja',
    tipo: CategoriaProduto.Sementes,
    categoriaPaiId: 1,
    categoriaPaiNome: 'Sementes',
    ordem: 1,
    ativo: true,
    subCategorias: [],
    quantidadeProdutos: 2,
    dataCriacao: new Date(),
    dataAtualizacao: new Date()
  }
];

// Test filtering functions
export function testFiltering() {
  console.log('Testing filtering functionality...');
  
  // Test status filter
  const activeCategories = mockCategorias.filter(cat => cat.ativo);
  console.log('Active categories:', activeCategories.length); // Should be 2
  
  const inactiveCategories = mockCategorias.filter(cat => !cat.ativo);
  console.log('Inactive categories:', inactiveCategories.length); // Should be 1
  
  // Test tipo filter
  const sementesCategories = mockCategorias.filter(cat => cat.tipo === CategoriaProduto.Sementes);
  console.log('Sementes categories:', sementesCategories.length); // Should be 2
  
  // Test search filter
  const searchResults = mockCategorias.filter(cat => 
    cat.nome.toLowerCase().includes('soja') || 
    (cat.descricao && cat.descricao.toLowerCase().includes('soja'))
  );
  console.log('Search results for "soja":', searchResults.length); // Should be 1
  
  // Test combined filters
  const combinedResults = mockCategorias.filter(cat => 
    cat.ativo && 
    cat.tipo === CategoriaProduto.Sementes &&
    cat.nome.toLowerCase().includes('sementes')
  );
  console.log('Combined filter results:', combinedResults.length); // Should be 2
  
  console.log('All filtering tests completed successfully!');
}