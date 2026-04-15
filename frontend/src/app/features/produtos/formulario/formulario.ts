import { Component, OnInit, OnDestroy } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ProdutoService } from '../../../core/services/produto.service';

@Component({
  selector: 'app-formulario',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './formulario.html',
  styleUrl: './formulario.scss'
})
export class FormularioComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  erro = '';
  sucesso = '';
  salvando = false;

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private produtoService: ProdutoService,
    public router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      codigo: ['', [Validators.required, Validators.maxLength(20)]],
      descricao: ['', Validators.required],
      saldo: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  salvar(): void {
    if (this.form.invalid) return;

    this.salvando = true;
    this.erro = '';

    this.produtoService.create(this.form.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.router.navigate(['/produtos']);
        },
        error: (err) => {
          this.erro = err.error?.message ?? 'Erro ao salvar produto.';
          this.salvando = false;
        }
      });
  }
}