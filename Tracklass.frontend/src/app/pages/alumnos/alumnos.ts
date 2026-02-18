import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AlumnoCard } from '../../components/alumno-card/alumno-card';
import { Spinner } from '../../components/spinner/spinner';
import { AlumnosService } from '../../services/alumnos.service';
import { Alumno } from '../../interfaces/alumno.interface';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

type FiltroEstado = 'todos' | 'activos' | 'inactivos';

@Component({
  selector: 'app-alumnos',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule, AlumnoCard, Spinner],
  templateUrl: './alumnos.html',
  styleUrl: './alumnos.css'
})
export class Alumnos implements OnInit {
  private router = inject(Router);
  private alumnosService = inject(AlumnosService);
  private toast = inject(ToastrService);
  alumnos = signal<Alumno[]>([]);
  loading = signal(true);
  searchTerm = signal('');
  filtroEstado = signal<FiltroEstado>('todos');

  alumnosFiltrados = computed(() => {
    let list = this.alumnos();
    const search = this.searchTerm().toLowerCase().trim();
    const estado = this.filtroEstado();
    if (search) {
      list = list.filter(a =>
        a.nombre.toLowerCase().includes(search) ||
        a.email?.toLowerCase().includes(search) ||
        a.materia.toLowerCase().includes(search)
      );
    }
    if (estado === 'activos') list = list.filter(a => a.activo);
    else if (estado === 'inactivos') list = list.filter(a => !a.activo);
    return list;
  });

  counts = computed(() => {
    const list = this.alumnos();
    return {
      todos: list.length,
      activos: list.filter(a => a.activo).length,
      inactivos: list.filter(a => !a.activo).length
    };
  });

  ngOnInit() {
    this.cargar();
  }

  cargar() {
    this.loading.set(true);
    this.alumnosService.getAll().subscribe({
      next: (data) => {
        this.alumnos.set(Array.isArray(data) ? data : []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onSearchChange(value: string) {
    this.searchTerm.set(value);
  }

  setFiltro(f: FiltroEstado) {
    this.filtroEstado.set(f);
  }

  nuevoAlumno() {
  this.router.navigate(['/alumnos/nuevo']);
}
}
