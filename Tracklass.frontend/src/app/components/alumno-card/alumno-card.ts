import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import type { Alumno } from '../../interfaces/alumno.interface';
import { CommonModule } from '@angular/common';
import { generarWhatsAppLink } from '../../utils/whatsapp.util';

@Component({
  selector: 'app-alumno-card',
  standalone: true,
  imports: [RouterLink, MatIconModule, CommonModule],
  templateUrl: './alumno-card.html',
  styleUrl: './alumno-card.css'
})
export class AlumnoCard {
  alumno = input.required<Alumno>();

  getIniciales(nombre: string): string {
    return nombre
      .split(' ')
      .map(n => n[0])
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }

  formatFecha(fecha?: string): string {
    if (!fecha) return '—';
    const d = new Date(fecha);
    return d.toLocaleDateString('es-UY', { day: 'numeric', month: 'short' });
  }

  getTelefonoLink(): string | null {
    const al = this.alumno();
    if (!al?.telefono) return null;
    
    const defaultMessage = `Hola ${al.nombre}, te escribo por tus clases.`;
    return generarWhatsAppLink(al.telefono, defaultMessage);
  }
}
