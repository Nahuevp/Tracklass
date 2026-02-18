import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Alumno } from '../interfaces/alumno.interface';
import { environment } from '../../environments/environments';

export interface ApiResponse<T> {
  isSuccess: boolean;
  result: T;
  message: string;
}

export interface AlumnoCrearActualizar {
  nombre: string;
  email: string | null;
  materia: string;
  activo: boolean;
  telefono?: string | null;
  notas?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AlumnosService {
  private baseUrl = `${environment.apiUrl}/Alumnos`;
  private http = inject(HttpClient);

  getAll(search?: string, estado?: string): Observable<Alumno[]> {
    let params: Record<string, string> = {};
    if (search) params['search'] = search;
    if (estado) params['estado'] = estado;
    const query = new URLSearchParams(params).toString();
    const url = query ? `${this.baseUrl}?${query}` : this.baseUrl;
    return this.http.get<ApiResponse<Alumno[]>>(url).pipe(
      map(r => (r.result as unknown as Alumno[]) || [])
    );
  }

  getById(id: string): Observable<Alumno | null> {
    return this.http.get<ApiResponse<Alumno>>(`${this.baseUrl}/${id}`).pipe(
      map(r => r.isSuccess ? (r.result as unknown as Alumno) : null)
    );
  }

  crear(data: AlumnoCrearActualizar): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(this.baseUrl, data);
  }

  actualizar(id: string, data: AlumnoCrearActualizar): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.baseUrl}/${id}`, data);
  }

  eliminar(id: string): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.baseUrl}/${id}`);
  }
}
