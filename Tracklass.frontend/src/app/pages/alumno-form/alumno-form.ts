import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { ToastrService } from 'ngx-toastr';
import { AlumnosService } from '../../services/alumnos.service';

@Component({
  selector: 'app-alumno-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatIconModule,
    MatSelectModule
  ],
  templateUrl: './alumno-form.html',
  styleUrl: './alumno-form.css'
})
export class AlumnoForm implements OnInit {

  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private alumnosService = inject(AlumnosService);
  private toast = inject(ToastrService);

  modoEdicion = false;
  idAlumno: string | null = null;

  form = this.fb.group({
    nombre: ['', Validators.required],
    email: ['', [Validators.email]],
    materia: ['', Validators.required],
    telefono: [''],
    activo: [true]
  });

  ngOnInit() {
    this.idAlumno = this.route.snapshot.paramMap.get('id');

    if (this.idAlumno) {
      this.modoEdicion = true;

      this.alumnosService.getById(this.idAlumno).subscribe(a => {
        if (a) {
          this.form.patchValue({
            nombre: a.nombre,
            email: a.email ?? '',
            materia: a.materia,
            telefono: a.telefono ?? '',
            activo: a.activo
          });
        }
      });
    }
  }

  guardar() {

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const emailValue = this.form.controls.email.value;

    const data = {
      nombre: this.form.controls.nombre.value!,
      email: emailValue && emailValue.trim() !== '' ? emailValue : null,
      materia: this.form.controls.materia.value!,
      telefono: this.form.controls.telefono.value || null,
      activo: this.form.controls.activo.value ?? true
    };

    if (this.modoEdicion && this.idAlumno) {

      this.alumnosService.actualizar(this.idAlumno, data).subscribe({
        next: () => {
          this.toast.success('Alumno actualizado correctamente');
          this.router.navigate(['/alumnos']);
        },
        error: () => {
          this.toast.error('Error al actualizar alumno');
        }
      });

    } else {

      this.alumnosService.crear(data).subscribe({
        next: () => {
          this.toast.success('Alumno creado correctamente');
          this.router.navigate(['/alumnos']);
        },
        error: () => {
          this.toast.error('Error al crear alumno');
        }
      });

    }
  }

  cancelar() {
    this.router.navigate(['/alumnos']);
  }
}
