import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import type { DashboardData } from '../interfaces/dashboard.interface';
import { environment } from '../../environments/environments';


export interface ApiResponse<T> {
  isSuccess: boolean;
  result: T;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private baseUrl = `${environment.apiUrl}/Dashboard`;
  private http = inject(HttpClient);

  get(fecha?: Date): Observable<DashboardData> {
    let url = this.baseUrl;
    if (fecha) {
      url += '?fecha=' + fecha.toISOString().split('T')[0];
    }
    return this.http.get<ApiResponse<DashboardData>>(url).pipe(
      map(r => r.result as unknown as DashboardData)
    );
  }
}
