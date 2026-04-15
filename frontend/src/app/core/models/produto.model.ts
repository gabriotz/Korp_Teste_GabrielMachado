export interface Produto {
  id: number;
  codigo: string;
  descricao: string;
  saldo: number;
  criadoEm: string;
  atualizadoEm: string;
}

export interface CriarProdutoRequest {
  codigo: string;
  descricao: string;
  saldo: number;
}

export interface AtualizarSaldoRequest {
  quantidade: number;
}