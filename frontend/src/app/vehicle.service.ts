import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
export interface Make { id: number; name: string; }
export interface VehicleType { id: number; name: string; filterKey: string | null; }
export interface Model { id: number; name: string; make: string; vehicleType: string | null; }
@Injectable({ providedIn: 'root' })
export class VehicleService {
  private http = inject(HttpClient);
  makes(): Observable<Make[]> { return this.http.get<Make[]>('/api/vehicles/makes'); }
  types(makeId: number): Observable<VehicleType[]> { return this.http.get<VehicleType[]>(`/api/vehicles/makes/${makeId}/types`); }
  models(makeId: number, year: number, type?: string): Observable<Model[]> {
    let params = new HttpParams().set('makeId', makeId).set('year', year);
    if (type) params = params.set('type', type);
    return this.http.get<Model[]>('/api/vehicles/models', { params });
  }
}
