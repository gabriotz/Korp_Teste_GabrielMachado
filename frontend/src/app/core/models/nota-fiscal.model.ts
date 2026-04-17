export interface ItemNota {
  id: number;
  produtoId: number;
  produtoDescricao: string;
  quantidade: number;
}

export interface NotaFiscal {
  id: number;
  numero: number;
  status: string;
  criadaEm: string;
  atualizadaEm: string;
  itens: ItemNota[];
}

export interface CriarItemRequest {
  produtoId: number;
  quantidade: number;
}

export interface CriarNotaRequest {
  itens: CriarItemRequest[];
}

export interface ProdutoDisponivel {
  id: number;
  codigo: string;
  descricao: string;
  saldo: number;
}