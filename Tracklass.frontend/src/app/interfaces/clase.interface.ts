export type EstadoClase = 'Programada' | 'Realizada' | 'Cancelada';

export interface Clase {
  id: string;
  alumnoId: string;
  alumnoNombre?: string;
  materia?: string;
  fecha: string;
  horaInicio: string;
  duracionMinutos: number;
  estado: EstadoClase;
  precio?: number;
  notas?: string;
  pagada?: boolean;
  fechaPago?: string;
}
