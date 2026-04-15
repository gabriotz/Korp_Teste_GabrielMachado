import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Produto, CriarProdutoRequest, AtualizarSaldoRequest } from '../models/produto.model';

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private url = 'http://localhost:8081/produtos';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Produto[]> {
    return this.http.get<Produto[]>(this.url);
  }

  getById(id: number): Observable<Produto> {
    return this.http.get<Produto>(`${this.url}/${id}`);
  }

  create(request: CriarProdutoRequest): Observable<Produto> {
    return this.http.post<Produto>(this.url, request);
  }

  atualizarSaldo(id: number, request: AtualizarSaldoRequest): Observable<void> {
    return this.http.patch<void>(`${this.url}/${id}/saldo`, request);
  }
}