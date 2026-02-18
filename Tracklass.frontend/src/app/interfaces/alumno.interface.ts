export interface Alumno {
  id: string;
  nombre: string;
  email?: string;
  materia: string;
  activo: boolean;
  telefono?: string;
  ultimaClase?: string;
  clasesTotales?: number;
  notas?: string;
}
