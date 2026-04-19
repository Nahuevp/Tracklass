import { Routes } from '@angular/router';
import { Layout } from './pages/layout/layout';
import { Dashboard } from './pages/dashboard/dashboard';
import { Alumnos } from './pages/alumnos/alumnos';
import { AlumnoPerfil } from './pages/alumno-perfil/alumno-perfil';
import { Agenda } from './pages/agenda/agenda';
import { AlumnoForm } from './pages/alumno-form/alumno-form';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { authGuard, guestGuard } from './guards/auth.guard';

export const routes: Routes = [
  // Public routes
  { path: 'login', component: Login, canActivate: [guestGuard] },
  { path: 'register', component: Register, canActivate: [guestGuard] },

  // Protected routes
  {
    path: '',
    component: Layout,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: Dashboard },
      { path: 'alumnos', component: Alumnos },
      { path: 'alumnos/nuevo', component: AlumnoForm },
      { path: 'alumnos/editar/:id', component: AlumnoForm },
      { path: 'alumnos/:id', component: AlumnoPerfil },
      { path: 'agenda', component: Agenda },
      { path: 'pagos', loadComponent: () => import('./pages/pagos/pagos').then(m => m.Pagos) },
      { path: '**', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];
