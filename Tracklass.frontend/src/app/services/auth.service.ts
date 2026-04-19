import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, map } from 'rxjs';
import { environment } from '../../environments/environments';

interface ApiResponse<T> {
  isSuccess: boolean;
  result: T;
  message: string;
}

export interface AuthUser {
  token: string;
  email: string;
  nombre: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = `${environment.apiUrl}/Auth`;
  private http = inject(HttpClient);
  private router = inject(Router);

  private _user = signal<AuthUser | null>(this.loadUserFromStorage());

  user = this._user.asReadonly();
  isLoggedIn = computed(() => !!this._user());

  login(email: string, password: string): Observable<AuthUser> {
    return this.http.post<ApiResponse<AuthUser>>(`${this.baseUrl}/login`, { email, password }).pipe(
      map(res => {
        if (!res.isSuccess) throw new Error(res.message);
        return res.result;
      }),
      tap(user => this.setSession(user))
    );
  }

  register(nombre: string, email: string, password: string): Observable<AuthUser> {
    return this.http.post<ApiResponse<AuthUser>>(`${this.baseUrl}/register`, { nombre, email, password }).pipe(
      map(res => {
        if (!res.isSuccess) throw new Error(res.message);
        return res.result;
      }),
      tap(user => this.setSession(user))
    );
  }

  logout(): void {
    localStorage.removeItem('tracklass_user');
    this._user.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this._user()?.token ?? null;
  }

  getNombre(): string {
    return this._user()?.nombre ?? '';
  }

  getEmail(): string {
    return this._user()?.email ?? '';
  }

  getIniciales(): string {
    const nombre = this.getNombre();
    return nombre
      .split(' ')
      .map(n => n[0])
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }

  private setSession(user: AuthUser): void {
    localStorage.setItem('tracklass_user', JSON.stringify(user));
    this._user.set(user);
  }

  private loadUserFromStorage(): AuthUser | null {
    try {
      const data = localStorage.getItem('tracklass_user');
      return data ? JSON.parse(data) : null;
    } catch {
      return null;
    }
  }
}
