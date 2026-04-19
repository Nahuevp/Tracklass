import { Component, inject, OnInit, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Spinner } from '../../components/spinner/spinner';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardData } from '../../interfaces/dashboard.interface';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { generarGoogleCalendarUrl } from '../../utils/calendar.util';

import { ExportService } from '../../services/export.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, Spinner],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {

  private dashboardService = inject(DashboardService);
  private router = inject(Router);
  private exportService = inject(ExportService);

  data = signal<DashboardData | null>(null);
  loading = signal(true);

  mesActual = new Date();
  currentMonth = '';

  exportarReporte() {
    const d = this.data();
    if (!d) return;
    this.exportService.exportarDashboardPDF(d, this.currentMonth);
  }

  ngOnInit() {
    this.actualizarMesTexto();
    this.cargarDashboard();
  }

  cambiarMes(delta: number) {

    const hoy = new Date();
    const mesActualNormalizado = new Date(
      this.mesActual.getFullYear(),
      this.mesActual.getMonth(),
      1
    );

    const mesSiguiente = new Date(
      mesActualNormalizado.getFullYear(),
      mesActualNormalizado.getMonth() + delta,
      1
    );

    const mesHoy = new Date(hoy.getFullYear(), hoy.getMonth(), 1);

    if (mesSiguiente > mesHoy) {
      return;
    }

    this.mesActual = mesSiguiente;

    this.actualizarMesTexto();
    this.cargarDashboard();
  }


  actualizarMesTexto() {
    this.currentMonth = this.mesActual.toLocaleDateString('es-UY', {
      month: 'long',
      year: 'numeric'
    });
  }

  cargarDashboard() {
    this.loading.set(true);

    this.dashboardService.get(this.mesActual)
      .subscribe(res => {
        setTimeout(() => {
          this.data.set(res);
          this.loading.set(false);
        }, 150);
      });
  }


  trackById(index: number, item: any) {
    return item.id;
  }

  formatCurrency(value: number): string {
    return `$ ${value.toLocaleString('es-UY')}`;
  }

  formatFecha(fecha: string): string {
    return new Date(fecha).toLocaleDateString('es-UY', {
      weekday: 'long',
      day: 'numeric',
      month: 'long'
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

  getMinutosProxima(clase: { horaInicio: string; fecha?: string }): string | null {
    if (!clase.fecha) return null;

    const ahora = new Date();
    const [h, m] = clase.horaInicio.split(':').map(Number);
    const fechaClase = new Date(clase.fecha);
    fechaClase.setHours(h, m, 0, 0);

    const diff = (fechaClase.getTime() - ahora.getTime()) / (1000 * 60);

    if (diff > 0 && diff <= 120) {
      return `Siguiente en ${Math.round(diff)} min`;
    }

    return null;
  }

  getCalendarUrl(clase: any): string {
    return generarGoogleCalendarUrl(clase, clase.alumnoNombre, clase.materia);
  }

  irAAlumno(alumnoId: string) {
    this.router.navigate(['/alumnos', alumnoId], { fragment: 'clases' });
  }
}
