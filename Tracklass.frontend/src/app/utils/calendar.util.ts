import { Clase } from '../interfaces/clase.interface';

export function generarGoogleCalendarUrl(clase: any, alumnoNombre: string, materia: string): string {
  // Generates a Google Calendar event link
  // Format required by Google: YYYYMMDDTHHMMSSZ (UTC) or YYYYMMDDTHHMMSS (Local Time)
  
  const baseUrl = 'https://calendar.google.com/calendar/render?action=TEMPLATE';
  const titulo = encodeURIComponent(`Clase de ${materia} con ${alumnoNombre}`);
  
  // Format dates manually to avoid timezone issues. We use Local Time since the app works in local time.
  const fecha = new Date(clase.fecha);
  const [hora, min] = clase.horaInicio.split(':').map(Number);
  
  const start = new Date(fecha);
  start.setHours(hora, min, 0, 0);
  
  const end = new Date(start);
  end.setMinutes(start.getMinutes() + clase.duracionMinutos);

  const startStr = formatearFechaCalendar(start);
  const endStr = formatearFechaCalendar(end);
  
  const details = encodeURIComponent(clase.notas ? `Notas: ${clase.notas}` : 'Clase agendada desde Tracklass');

  return `${baseUrl}&text=${titulo}&dates=${startStr}/${endStr}&details=${details}`;
}

function formatearFechaCalendar(d: Date): string {
  const yyyy = d.getFullYear();
  const mm = (d.getMonth() + 1).toString().padStart(2, '0');
  const dd = d.getDate().toString().padStart(2, '0');
  const hh = d.getHours().toString().padStart(2, '0');
  const min = d.getMinutes().toString().padStart(2, '0');
  const ss = '00';
  
  return `${yyyy}${mm}${dd}T${hh}${min}${ss}`;
}
