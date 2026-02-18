import { Routes } from '@angular/router';
import { Layout } from './pages/layout/layout';
import { Dashboard } from './pages/dashboard/dashboard';
import { Alumnos } from './pages/alumnos/alumnos';
import { AlumnoPerfil } from './pages/alumno-perfil/alumno-perfil';
import { Agenda } from './pages/agenda/agenda';
import { AlumnoForm } from './pages/alumno-form/alumno-form';

export const routes: Routes = [
  {
    path: '',
    component: Layout,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: Dashboard },
      { path: 'alumnos', component: Alumnos },
      { path: 'alumnos/nuevo', component: AlumnoForm },
      { path: 'alumnos/editar/:id', component: AlumnoForm },
      { path: 'alumnos/:id', component: AlumnoPerfil },
      { path: 'agenda', component: Agenda },
      { path: '**', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];
