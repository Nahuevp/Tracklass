import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import type { Clase } from '../interfaces/clase.interface';
import { environment } from '../../environments/environments';

export interface ApiResponse<T> {
  isSuccess: boolean;
  result: T;
  message: string;
}

export interface ClaseCrearActualizar {
  alumnoId: string;
  fecha: string;
  horaInicio: string;
  duracionMinutos: number;
  estado: number;
  precio: number;
  notas?: string;
  pagada?: boolean;
  fechaPago?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ClasesService {
  private baseUrl = `${environment.apiUrl}/Clases`;
  private http = inject(HttpClient);

  getAll(estado?: string, desde?: Date, hasta?: Date): Observable<Clase[]> {
    const params: Record<string, string> = {};
    if (estado) params['estado'] = estado;
    if (desde) params['desde'] = desde.toISOString().split('T')[0];
    if (hasta) params['hasta'] = hasta.toISOString().split('T')[0];
    const query = new URLSearchParams(params).toString();
    const url = query ? `${this.baseUrl}?${query}` : this.baseUrl;
    return this.http.get<ApiResponse<Clase[]>>(url).pipe(
      map(r => (r.result as unknown as Clase[]) || [])
    );
  }

  getByAlumno(alumnoId: string): Observable<Clase[]> {
    return this.http.get<ApiResponse<Clase[]>>(`${this.baseUrl}/alumno/${alumnoId}`).pipe(
      map(r => (r.result as unknown as Clase[]) || [])
    );
  }

  crear(data: ClaseCrearActualizar): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(this.baseUrl, data);
  }

  actualizar(id: string, data: ClaseCrearActualizar): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.baseUrl}/${id}`, data);
  }

  actualizarPago(id: string, pagada: boolean): Observable<ApiResponse<{pagada: boolean, fechaPago: string | null}>> {
    return this.http.patch<ApiResponse<{pagada: boolean, fechaPago: string | null}>>(`${this.baseUrl}/${id}/pago`, pagada);
  }

  eliminar(id: string): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.baseUrl}/${id}`);
  }

  getHorariosOcupados(fecha: Date) {
    const fechaStr = fecha.toISOString().split('T')[0];
    return this.http.get<any>(
      `${this.baseUrl}/horarios-ocupados?fecha=${fechaStr}`
    );
  }

}
