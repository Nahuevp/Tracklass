import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ClasesService } from '../../services/clases.service';
import { DashboardService } from '../../services/dashboard.service';
import { Clase } from '../../interfaces/clase.interface';
import { Spinner } from '../../components/spinner/spinner';
import { ToastrService } from 'ngx-toastr';

type FiltroPago = 'pendientes' | 'pagadas';

@Component({
  selector: 'app-pagos',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, Spinner],
  templateUrl: './pagos.html',
  styleUrl: './pagos.css'
})
export class Pagos implements OnInit {
  private clasesService = inject(ClasesService);
  private dashboardService = inject(DashboardService);
  private toast = inject(ToastrService);

  clases = signal<Clase[]>([]);
  loading = signal(true);
  filtro = signal<FiltroPago>('pendientes');
  deudaTotal = signal<number>(0);

  clasesFiltradas = computed(() => {
    let list = this.clases();
    if (this.filtro() === 'pendientes') {
      return list.filter(c => !c.pagada);
    } else {
      return list.filter(c => c.pagada);
    }
  });

  counts = computed(() => {
    const list = this.clases();
    return {
      pendientes: list.filter(c => !c.pagada).length,
      pagadas: list.filter(c => c.pagada).length
    };
  });

  ngOnInit() {
    this.cargarDatos();
  }

  cargarDatos() {
    this.loading.set(true);
    // Solo traemos clases Realizadas
    this.clasesService.getAll('realizadas').subscribe({
      next: (data) => {
        this.clases.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });

    this.dashboardService.get().subscribe({
      next: (d) => {
        this.deudaTotal.set(d.deudaTotal);
      }
    });
  }

  setFiltro(f: FiltroPago) {
    this.filtro.set(f);
  }

  formatFecha(fecha: string): string {
    return new Date(fecha).toLocaleDateString('es-UY', {
      weekday: 'short',
      day: 'numeric',
      month: 'short'
    });
  }

  formatCurrency(value: number): string {
    return `$ ${value.toLocaleString('es-UY')}`;
  }

  marcarPago(clase: Clase, pagada: boolean) {
    this.clasesService.actualizarPago(clase.id, pagada).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.toast.success(res.message);
          this.cargarDatos(); // recargar
        }
      },
      error: () => this.toast.error('Error al actualizar estado de pago')
    });
  }
}
