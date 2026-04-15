import { Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ProdutoService } from '../../../core/services/produto.service';
import { Produto } from '../../../core/models/produto.model';

@Component({
  selector: 'app-listagem',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './listagem.html',
  styleUrl: './listagem.scss'
})
export class ListagemComponent implements OnInit, OnDestroy {
  produtos: Produto[] = [];
  erro = '';
  carregando = false;

  private destroy$ = new Subject<void>();

  constructor(
    private produtoService: ProdutoService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  carregar(): void {
    this.carregando = true;
    this.erro = '';

    this.produtoService.getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (dados) => {
          this.produtos = dados;
          this.carregando = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.log('erro:', err);
          this.erro = 'Erro ao carregar produtos.';
          this.carregando = false;
          this.cdr.detectChanges();
        }
      });
  }
}