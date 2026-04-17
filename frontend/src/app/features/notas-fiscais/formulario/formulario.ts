import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators, AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NotaFiscalService } from '../../../core/services/nota-fiscal.service';
import { ProdutoDisponivel } from '../../../core/models/nota-fiscal.model';

@Component({
  selector: 'app-notas-formulario',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './formulario.html',
  styleUrl: './formulario.scss'
})
export class NotasFormularioComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  produtos: ProdutoDisponivel[] = [];
  erro = '';
  erroProdutos = '';
  salvando = false;

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private notaService: NotaFiscalService,
    public router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({ itens: this.fb.array([]) });
    this.carregarProdutos();
    this.adicionarItem();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get itens(): FormArray {
    return this.form.get('itens') as FormArray;
  }

  carregarProdutos(): void {
    this.notaService.getProdutosDisponiveis()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (dados) => { this.produtos = dados; this.cdr.detectChanges(); },
        error: () => { this.erroProdutos = 'Serviço de estoque indisponível.'; this.cdr.detectChanges(); }
      });
  }

  adicionarItem(): void {
    this.itens.push(this.fb.group({
      produtoId: ['', Validators.required],
      quantidade: [1, [Validators.required, Validators.min(1)]]
    }));
  }

  removerItem(index: number): void {
    this.itens.removeAt(index);
  }

  // Retorna o produto selecionado num item pelo index
  getProduto(index: number): ProdutoDisponivel | undefined {
    const id = Number(this.itens.at(index).get('produtoId')?.value);
    return this.produtos.find(p => p.id === id);
  }

  // Verifica se o produto já foi selecionado em outro item
  produtoDuplicado(index: number): boolean {
    const id = this.itens.at(index).get('produtoId')?.value;
    if (!id) return false;
    return this.itens.controls.some((ctrl, i) => i !== index && ctrl.get('produtoId')?.value === id);
  }

  // Verifica se a quantidade excede o saldo disponível
  quantidadeExcedeSaldo(index: number): boolean {
    const produto = this.getProduto(index);
    const quantidade = Number(this.itens.at(index).get('quantidade')?.value);
    if (!produto) return false;
    return quantidade > produto.saldo;
  }

  // Validação geral antes de salvar
  formValido(): boolean {
    if (this.form.invalid || this.itens.length === 0) return false;
    for (let i = 0; i < this.itens.length; i++) {
      if (this.produtoDuplicado(i) || this.quantidadeExcedeSaldo(i)) return false;
    }
    return true;
  }

  salvar(): void {
    if (!this.formValido()) return;

    this.salvando = true;
    this.erro = '';

    const payload = {
      itens: this.itens.value.map((item: any) => ({
        produtoId: Number(item.produtoId),
        quantidade: Number(item.quantidade)
      }))
    };

    this.notaService.create(payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => { this.router.navigate(['/notas-fiscais']); },
        error: (err) => {
          this.erro = err.error?.message ?? 'Erro ao criar nota fiscal.';
          this.salvando = false;
          this.cdr.detectChanges();
        }
      });
  }
}