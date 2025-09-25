import {
  BaseEntity,
  Endereco,
  Usuario,
  Cultura,
  TipoCultura,
  Propriedade,
  Produtor,
  TipoCliente,
  PontoDistribuicao,
  Fornecedor,
  LoginForm,
  AuthResponse,
  ApiResponse,
  ValidationError
} from './index';

describe('Models', () => {
  it('should import all models without errors', () => {
    // Test that all interfaces can be used for type checking
    const baseEntity: BaseEntity = {
      id: 1,
      dataCriacao: new Date(),
      ativo: true
    };

    const endereco: Endereco = {
      ...baseEntity,
      logradouro: 'Rua Test',
      numero: '123',
      bairro: 'Centro',
      cidade: 'São Paulo',
      uf: 'SP',
      cep: '01000-000'
    };

    const usuario: Usuario = {
      ...baseEntity,
      nome: 'Test User',
      email: 'test@example.com',
      senha: 'password123'
    };

    const cultura: Cultura = {
      ...baseEntity,
      tipo: TipoCultura.SOJA,
      anoSafra: 2024,
      areaCultivada: 100.5,
      propriedadeId: 1
    };

    const propriedade: Propriedade = {
      ...baseEntity,
      nome: 'Fazenda Test',
      area: 1000,
      produtorId: 1,
      culturas: [cultura]
    };

    const produtor: Produtor = {
      ...baseEntity,
      codigo: 'PROD001',
      nome: 'Produtor Test',
      cpfCnpj: '12345678901',
      tipoCliente: TipoCliente.PF,
      enderecos: [endereco],
      propriedades: [propriedade]
    };

    const pontoDistribuicao: PontoDistribuicao = {
      ...baseEntity,
      nome: 'Ponto Test',
      endereco: endereco,
      fornecedorId: 1
    };

    const fornecedor: Fornecedor = {
      ...baseEntity,
      codigo: 'FORN001',
      nome: 'Fornecedor Test',
      cpfCnpj: '12345678000195',
      tipoCliente: TipoCliente.PJ,
      enderecos: [endereco],
      pontosDistribuicao: [pontoDistribuicao]
    };

    const loginForm: LoginForm = {
      username: 'test@example.com',
      password: 'password123',
      rememberMe: false
    };

    const authResponse: AuthResponse = {
      token: 'jwt-token',
      refreshToken: 'refresh-token',
      user: usuario,
      expiresIn: 3600
    };

    const apiResponse: ApiResponse<Produtor> = {
      data: produtor,
      success: true,
      message: 'Success'
    };

    const validationError: ValidationError = {
      field: 'email',
      message: 'Invalid email format',
      code: 'INVALID_EMAIL'
    };

    // Verify all objects are properly typed
    expect(baseEntity.id).toBe(1);
    expect(endereco.logradouro).toBe('Rua Test');
    expect(usuario.nome).toBe('Test User');
    expect(cultura.tipo).toBe(TipoCultura.SOJA);
    expect(propriedade.nome).toBe('Fazenda Test');
    expect(produtor.codigo).toBe('PROD001');
    expect(pontoDistribuicao.nome).toBe('Ponto Test');
    expect(fornecedor.codigo).toBe('FORN001');
    expect(loginForm.username).toBe('test@example.com');
    expect(authResponse.token).toBe('jwt-token');
    expect(apiResponse.success).toBe(true);
    expect(validationError.field).toBe('email');
  });

  it('should support enum values', () => {
    expect(TipoCultura.SOJA).toBe('SOJA');
    expect(TipoCultura.MILHO).toBe('MILHO');
    expect(TipoCultura.ALGODAO).toBe('ALGODAO');
    expect(TipoCultura.OUTROS).toBe('OUTROS');

    expect(TipoCliente.PF).toBe('PF');
    expect(TipoCliente.PJ).toBe('PJ');
  });
});