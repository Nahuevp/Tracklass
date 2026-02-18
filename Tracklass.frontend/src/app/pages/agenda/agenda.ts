import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { DashboardService } from '../../services/dashboard.service';
import { ClasesService } from '../../services/clases.service';
import { Clase } from '../../interfaces/clase.interface';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Spinner } from '../../components/spinner/spinner';


type FiltroEstado = 'todas' | 'programadas' | 'realizadas' | 'canceladas';

@Component({
  selector: 'app-agenda',
  standalone: true,
  imports: [CommonModule, MatIconModule, Spinner],
  templateUrl: './agenda.html',
  styleUrl: './agenda.css'
})
export class Agenda implements OnInit {
  private dashboardService = inject(DashboardService);
  private clasesService = inject(ClasesService);
  private router = inject(Router);

  clases = signal<Clase[]>([]);
  resumen = signal<{ hoy: number; estaSemana: number; esteMes: number; ingresoEstimadoMes: number } | null>(null);
  loading = signal(true);
  filtroEstado = signal<FiltroEstado>('todas');

  clasesFiltradas = computed(() => {
    let list = this.clases();
    const f = this.filtroEstado();
    if (f === 'programadas') list = list.filter(c => c.estado === 'Programada');
    else if (f === 'realizadas') list = list.filter(c => c.estado === 'Realizada');
    else if (f === 'canceladas') list = list.filter(c => c.estado === 'Cancelada');
    return list;
  });

  clasesPorSeccion = computed(() => {
    const list = this.clasesFiltradas();
    const hoy = new Date();
    hoy.setHours(0, 0, 0, 0);

    const hoyList: Clase[] = [];
    const proximas: Clase[] = [];
    const pasadas: Clase[] = [];

    for (const c of list) {
      const fecha = new Date(c.fecha);
      fecha.setHours(0, 0, 0, 0);
      const diff = fecha.getTime() - hoy.getTime();
      if (diff === 0) hoyList.push(c);
      else if (diff > 0) proximas.push(c);
      else pasadas.push(c);
    }

    proximas.sort((a, b) => new Date(a.fecha).getTime() - new Date(b.fecha).getTime());
    pasadas.sort((a, b) => new Date(b.fecha).getTime() - new Date(a.fecha).getTime());

    return { hoy: hoyList, proximas, pasadas };
  });

  counts = computed(() => {
    const list = this.clases();
    return {
      todas: list.length,
      programadas: list.filter(c => c.estado === 'Programada').length,
      realizadas: list.filter(c => c.estado === 'Realizada').length,
      canceladas: list.filter(c => c.estado === 'Cancelada').length
    };
  });

  ngOnInit() {
    this.loading.set(true);
    this.clasesService.getAll().subscribe({
      next: (data) => {
        this.clases.set(Array.isArray(data) ? data : []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });

    this.dashboardService.get().subscribe({
      next: (d) => {
        if (d.resumenAgenda) {
          this.resumen.set({
            hoy: d.resumenAgenda.hoy,
            estaSemana: d.resumenAgenda.estaSemana,
            esteMes: d.resumenAgenda.esteMes,
            ingresoEstimadoMes: d.resumenAgenda.ingresoEstimadoMes
          });
        }
      }
    });
  }

  setFiltro(f: FiltroEstado) {
    this.filtroEstado.set(f);
  }

  formatFecha(fecha: string): string {
    return new Date(fecha).toLocaleDateString('es-UY', {
      weekday: 'long',
      day: 'numeric',
      month: 'long'
    });
  }

  formatFechaCorta(fecha: string): string {
    return new Date(fecha).toLocaleDateString('es-UY', {
      weekday: 'short',
      day: 'numeric',
      month: 'short'
    });
  }

  getEstadoClass(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'realizada': return 'estado-realizada';
      case 'programada': return 'estado-programada';
      case 'cancelada': return 'estado-cancelada';
      default: return '';
    }
  }

  formatCurrency(value: number): string {
    return `$ ${value.toLocaleString('es-UY')}`;
  }

  groupByFecha(clases: Clase[]): Map<string, Clase[]> {
    const map = new Map<string, Clase[]>();
    for (const c of clases) {
      const key = new Date(c.fecha).toLocaleDateString('es-UY', {
        weekday: 'long',
        day: 'numeric',
        month: 'long'
      });
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(c);
    }
    return map;
  }

  irAAlumno(alumnoId: string) {
  this.router.navigate(['/alumnos', alumnoId], { fragment: 'clases' });
}

}
