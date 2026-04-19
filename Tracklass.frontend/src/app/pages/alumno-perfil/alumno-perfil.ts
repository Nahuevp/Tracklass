import { Component, inject, OnInit, signal } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

import { AlumnosService } from '../../services/alumnos.service';
import { ClasesService } from '../../services/clases.service';
import { Alumno } from '../../interfaces/alumno.interface';
import { Clase } from '../../interfaces/clase.interface';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { FormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { Spinner } from '../../components/spinner/spinner';
import { generarWhatsAppLink } from '../../utils/whatsapp.util';
import { generarGoogleCalendarUrl } from '../../utils/calendar.util';
import { ExportService } from '../../services/export.service';

@Component({
  selector: 'app-alumno-perfil',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatTabsModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    ReactiveFormsModule,
    Spinner,
    FormsModule,
     MatDividerModule
  ],
  templateUrl: './alumno-perfil.html',
  styleUrl: './alumno-perfil.css'
})
export class AlumnoPerfil implements OnInit {

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private alumnosService = inject(AlumnosService);
  private clasesService = inject(ClasesService);
  private toast = inject(ToastrService);
  private fb = inject(FormBuilder);
  private exportService = inject(ExportService);

  minDate = new Date(); // Para bloquear fechas pasadas

  horarios: string[] = [];

  tabIndex = 0; // 0 = Información, 1 = Clases, 2 = Notas

  editandoClaseId: string | null = null;
  
  exportarHistorial() {
    const a = this.alumno();
    if (!a) return;
    this.exportService.exportarClasesAlumnoPDF(a.nombre, a.materia, this.clases());
  }

  horariosOcupados: string[] = [];

  notaGeneral: string = '';


  constructor() {
    this.generarHorarios();
  }

  minFecha = new Date();

  generarHorarios() {
    this.horarios = [];

    const inicio = 8;
    const fin = 22;
    const intervalo = 30; // configurable

    for (let h = inicio; h < fin; h++) {
      for (let m = 0; m < 60; m += intervalo) {
        const hora = `${h.toString().padStart(2, '0')}:${m
          .toString()
          .padStart(2, '0')}`;

        this.horarios.push(hora);
      }
    }
  }


  mostrarFormClase = false;

  formClase = this.fb.group({
    fecha: this.fb.control<Date | null>(null, { validators: [Validators.required] }),
    horaInicio: this.fb.control<string>('', { validators: [Validators.required] }),
    duracionMinutos: this.fb.control<number>(60, {
      validators: [Validators.required, Validators.min(60)]
    }),
    estado: this.fb.control<'Programada' | 'Realizada' | 'Cancelada'>('Programada', {
      validators: [Validators.required]
    }),
    precio: this.fb.control<number>(300, {
      validators: [Validators.required, Validators.min(0)]
    }),
    notas: this.fb.control<string>(''),
  }, {
    validators: this.validarFechaHoraPasada.bind(this)
  });


  alumno = signal<Alumno | null>(null);
  clases = signal<Clase[]>([]);
  loading = signal(true);

  ngOnInit() {

    this.route.fragment.subscribe(fragment => {
      if (fragment === 'clases') {
        this.tabIndex = 1;
      }
    });

    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.router.navigate(['/alumnos']);
      return;
    }

    this.alumnosService.getById(id).subscribe({
      next: (a) => {
        this.alumno.set(a ?? null);
        if (a) {
          this.cargarClases(id);

          const fecha = this.formClase.value.fecha;
          if (fecha) {
            this.onFechaChange(fecha);
          }
        }
        this.loading.set(false);
        this.notaGeneral = a?.notas ?? '';
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/alumnos']);
      }
    });

    // recalcular cuando cambia duración
    this.formClase.get('duracionMinutos')?.valueChanges.subscribe(() => {
      this.formClase.updateValueAndValidity();
    });

    // recalcular cuando cambia fecha
    this.formClase.get('fecha')?.valueChanges.subscribe(fecha => {
      if (fecha) {
        this.onFechaChange(fecha);
        this.formClase.updateValueAndValidity();
      }
    });

  }


  cargarClases(id: string) {
    this.clasesService.getByAlumno(id).subscribe({
      next: (c) => this.clases.set(c),
      error: () => this.clases.set([])
    });
  }

  trackById(index: number, item: Clase) {
    return item.id;
  }

  formatFecha(fecha: string): string {
    return new Date(fecha).toLocaleDateString('es-UY', {
      weekday: 'short',
      day: 'numeric',
      month: 'short'
    });
  }

  getWhatsAppLink(clase: Clase | null = null): string | null {
    const a = this.alumno();
    if (!a?.telefono) return null;
    
    let mensaje = `Hola ${a.nombre}, te escribo por Tracklass.`;
    if (clase) {
      const fechaFormat = this.formatFecha(clase.fecha);
      mensaje = `Hola ${a.nombre}, te recuerdo tu clase de ${a.materia} programada para el ${fechaFormat} a las ${clase.horaInicio}.`;
    }
    
    return generarWhatsAppLink(a.telefono, mensaje);
  }

  getCalendarUrl(clase: Clase): string | null {
    const a = this.alumno();
    if (!a) return null;
    return generarGoogleCalendarUrl(clase, a.nombre, a.materia);
  }

  editarAlumno(id: string) {
    this.router.navigate(['/alumnos/editar', id]);
  }

  getEstadoClass(estado: string) {
    switch (estado?.toLowerCase()) {
      case 'realizada': return 'estado-realizada';
      case 'programada': return 'estado-programada';
      case 'cancelada': return 'estado-cancelada';
      default: return '';
    }
  }

  marcarPago(clase: Clase, pagada: boolean) {
    this.clasesService.actualizarPago(clase.id, pagada).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.toast.success(res.message);
          this.cargarClases(clase.alumnoId);
        }
      },
      error: () => this.toast.error('Error al actualizar estado de pago')
    });
  }

  validarFechaHoraPasada(): { [key: string]: boolean } | null {

    const fecha = this.formClase?.controls.fecha.value;
    const hora = this.formClase?.controls.horaInicio.value;

    if (!fecha || !hora) return null;

    const fechaClase = new Date(fecha);
    const [h, m] = hora.split(':').map(Number);
    fechaClase.setHours(h, m, 0, 0);

    const ahora = new Date();
    ahora.setSeconds(0, 0);

    return fechaClase < ahora ? { fechaPasada: true } : null;
  }

  guardarClase() {
    if (this.formClase.invalid) {
      this.formClase.markAllAsTouched();
      return;
    }

    const alumno = this.alumno();
    if (!alumno) return;

    const fechaValue = this.formClase.value.fecha!;
    const horaValue = this.formClase.value.horaInicio!;

    const ahora = new Date();
    const fechaClase = new Date(fechaValue);

    const [hora, minutos] = horaValue.split(':').map(Number);
    fechaClase.setHours(hora, minutos, 0, 0);

    // BLOQUEAR FECHA/HORA PASADA
    if (fechaClase < ahora) {
      this.toast.error('No se puede agendar una clase en el pasado');
      return;
    }

    // EVITAR DUPLICADO (solo si es nueva o cambió fecha/hora)
    const yaExiste = this.clases().some(c =>
      c.id !== this.editandoClaseId && // importante para edición
      new Date(c.fecha).toDateString() === fechaClase.toDateString() &&
      c.horaInicio === horaValue
    );

    if (yaExiste) {
      this.toast.warning('Ya existe una clase en ese día y horario');
      return;
    }

    const fechaFormateada = `${fechaClase.getFullYear()}-${(fechaClase.getMonth() + 1).toString().padStart(2, '0')
      }-${fechaClase.getDate().toString().padStart(2, '0')
      }`;

    const data = {
      alumnoId: alumno.id,
      fecha: fechaFormateada,
      horaInicio: horaValue + ':00',
      duracionMinutos: Number(this.formClase.value.duracionMinutos),
      estado: this.mapEstado(this.formClase.value.estado!),
      precio: Number(this.formClase.value.precio),
      notas: this.formClase.value.notas || ''
    };


    // EDITAR (PUT)
    if (this.editandoClaseId) {

      this.clasesService.actualizar(this.editandoClaseId, data).subscribe({
        next: () => {
          this.toast.success('Clase actualizada correctamente');
          this.resetFormClase();
          this.cargarClases(alumno.id);
        },
        error: () => {
          this.toast.error('Error al actualizar clase');
        }
      });

    }
    // NUEVA (POST)
    else {

      this.clasesService.crear(data).subscribe({
        next: () => {
          this.toast.success('Clase creada correctamente');
          this.resetFormClase();
          this.cargarClases(alumno.id);

          const fecha = this.formClase.value.fecha;
          if (fecha) {
            this.onFechaChange(fecha);
          }

          this.onFechaChange(fechaClase);
        },
        error: (err) => {
          console.log('ERROR BACKEND:', err);
          this.toast.error(err?.error?.message || 'No se pudo crear la clase');
        }
      });

    }

  }

  resetFormClase() {
    this.mostrarFormClase = false;
    this.editandoClaseId = null;

    this.formClase.reset({
      duracionMinutos: 60,
      estado: 'Programada',
      precio: 300
    });

    this.cargarClases(this.alumno()!.id);
  }


  private mapEstado(estado: 'Programada' | 'Realizada' | 'Cancelada'): number {
    switch (estado) {
      case 'Programada': return 0;
      case 'Realizada': return 1;
      case 'Cancelada': return 2;
      default: return 0;
    }
  }

  marcarRealizada(clase: Clase) {

    if (clase.estado !== 'Programada') return;

    let horaParse = clase.horaInicio;
    if (horaParse.split(':').length === 2) {
      horaParse += ':00'; // asegura hh:mm:ss
    }

    const d = new Date(clase.fecha);
    const fechaFormateada = `${d.getFullYear()}-${(d.getMonth() + 1).toString().padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')}`;

    const data = {
      alumnoId: clase.alumnoId,
      fecha: fechaFormateada,
      horaInicio: horaParse,
      duracionMinutos: clase.duracionMinutos,
      estado: 1, // Realizada
      precio: clase.precio ?? 0,
      notas: clase.notas ?? ''
    };

    this.clasesService.actualizar(clase.id, data).subscribe({
      next: () => {
        this.toast.success('Clase finalizada y registrada como ingreso');
        this.cargarClases(this.alumno()!.id);
      },
      error: () => this.toast.error('Error al actualizar estado')
    });
  }

  editarClase(clase: Clase) {

    if (clase.estado !== 'Programada') {
      this.toast.warning('No se pueden editar clases ya finalizadas');
      return;
    }

    this.editandoClaseId = clase.id;
    this.mostrarFormClase = true;

    this.formClase.patchValue({
      fecha: new Date(clase.fecha),
      horaInicio: clase.horaInicio,
      duracionMinutos: clase.duracionMinutos,
      estado: clase.estado,
      precio: clase.precio,
      notas: clase.notas
    });
  }

  private mapEstadoToString(estado: number): 'Programada' | 'Realizada' | 'Cancelada' {
    switch (estado) {
      case 0: return 'Programada';
      case 1: return 'Realizada';
      case 2: return 'Cancelada';
      default: return 'Programada';
    }
  }

  eliminarClase(id: string) {

    if (!confirm('¿Seguro que querés eliminar esta clase?')) return;

    this.clasesService.eliminar(id).subscribe({
      next: () => {
        this.toast.success('Clase eliminada');
        this.cargarClases(this.alumno()!.id);
      },
      error: () => this.toast.error('Error al eliminar')
    });
  }

  horaOcupada(hora: string): boolean {

    if (this.editandoClaseId && hora === this.formClase.value.horaInicio) {
      return false;
    }

    return this.horariosOcupados.includes(hora);
  }


  onFechaChange(fecha: Date) {
    if (!fecha) return;

    this.clasesService.getHorariosOcupados(fecha)
      .subscribe((res: any) => {

        this.horariosOcupados =
          (res.result ?? []).map((h: string) => h.substring(0, 5));

        if (this.horariosOcupados.length >= this.horarios.length) {
          this.toast.info('Este día ya está completamente ocupado');
        }

      });
  }

  get diaLleno(): boolean {
    return this.horarios
      .filter(h => !this.horaOcupada(h))
      .length === 0;
  }

  guardarNota() {
    const alumno = this.alumno();
    if (!alumno) return;

    const data = {
      id: alumno.id,
      nombre: alumno.nombre,
      email: alumno.email ?? null,
      materia: alumno.materia,
      activo: alumno.activo,
      telefono: alumno.telefono ?? null,
      notas: this.notaGeneral
    };

    this.alumnosService.actualizar(alumno.id, data)
      .subscribe({
        next: () => {
          this.toast.success('Nota guardada correctamente');
          this.alumno.set({ ...alumno, notas: this.notaGeneral });
        },
        error: () => this.toast.error('Error al guardar nota')
      });
  }

}
