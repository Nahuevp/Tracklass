export interface DashboardData {
  clasesHoy: ClaseHoy[];
  proximaClase?: ProximaClase;
  clasesRealizadasMes: number;
  ingresosMes: number;
  ingresosEstimadosMes: number;
  alumnosActivos: number;
  alumnosTotales: number;
  proximasClases: ProximaClaseDetalle[];
  totalClasesHoy: number;
  resumenAgenda?: ResumenAgenda;
}

export interface ClaseHoy {
  id: string;
  alumnoId: string;
  alumnoNombre: string;
  materia: string;
  fecha?: string;
  horaInicio: string;
  duracionMinutos: number;
  estado: string;
}

export interface ProximaClase {
  id: string;
  alumnoId: string;
  alumnoNombre: string;
  materia: string;
  fecha: string;
  horaInicio: string;
  duracionMinutos: number;
}

export interface ProximaClaseDetalle {
  id: string;
  alumnoId: string;
  alumnoNombre: string;
  materia: string;
  fecha: string;
  horaInicio: string;
  duracionMinutos: number;
}

export interface ResumenAgenda {
  hoy: number;
  estaSemana: number;
  esteMes: number;
  ingresoEstimadoMes: number;
}
