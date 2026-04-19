import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NotaFiscal, CriarNotaRequest, ProdutoDisponivel } from '../models/nota-fiscal.model';

@Injectable({ providedIn: 'root' })
export class NotaFiscalService {
  private url = 'http://localhost:8082/notas-fiscais';

  constructor(private http: HttpClient) {}

  getAll(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.url);
  }

  getById(id: number): Observable<NotaFiscal> {
    return this.http.get<NotaFiscal>(`${this.url}/${id}`);
  }

  getProdutosDisponiveis(): Observable<ProdutoDisponivel[]> {
    return this.http.get<ProdutoDisponivel[]>(`${this.url}/produtos-disponiveis`);
  }

  create(request: CriarNotaRequest): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.url, request);
  }

  imprimir(id: number): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(`${this.url}/${id}/imprimir`, {});
  }

  gerarResumo(id: number): Observable<{ resumo: string }> {
    return this.http.post<{ resumo: string }>(`${this.url}/${id}/resumo`, {});
  }
}