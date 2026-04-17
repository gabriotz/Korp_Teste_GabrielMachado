import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { NotaFiscalService } from '../../../core/services/nota-fiscal.service';
import { NotaFiscal } from '../../../core/models/nota-fiscal.model';
import { DatePipe } from '@angular/common';


@Component({
  selector: 'app-notas-listagem',
  standalone: true,
  imports: [RouterModule, DatePipe],
  templateUrl: './listagem.html',
  styleUrl: './listagem.scss'
})
export class NotasListagemComponent implements OnInit, OnDestroy {
  notas: NotaFiscal[] = [];
  erro = '';
  erroImpressao = '';
  sucesso = '';
  carregando = false;
  imprimindo: number | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private notaService: NotaFiscalService,
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

    this.notaService.getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (dados) => {
          this.notas = dados;
          this.carregando = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.erro = 'Erro ao carregar notas fiscais.';
          this.carregando = false;
          this.cdr.detectChanges();
        }
      });
  }

  imprimir(id: number): void {
    this.imprimindo = id;
    this.erroImpressao = '';
    this.sucesso = '';

    this.notaService.imprimir(id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.imprimindo = null;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (nota) => {
          const index = this.notas.findIndex(n => n.id === nota.id);
          if (index !== -1) this.notas[index] = nota;
          this.sucesso = `Nota ${nota.numero} impressa com sucesso!`;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.erroImpressao = err.error?.message ?? 'Erro ao imprimir nota.';
          this.cdr.detectChanges();
        }
      });
  }
}