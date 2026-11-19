import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, switchMap, of, catchError, tap, finalize } from 'rxjs';
import { VehicleService, Make, Model, VehicleType } from './vehicle.service';
@Component({ selector: 'app-root', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './app.component.html' })
export class AppComponent implements OnInit {
  private api = inject(VehicleService);
  private destroyRef = inject(DestroyRef);
  private query$ = new Subject<{ make: number; year: number; type?: string }>();
  makes: Make[] = []; types: VehicleType[] = []; models: Model[] = [];
  makeId: number | null = null; year = new Date().getFullYear(); type = '';
  years = Array.from({ length: 55 }, (_, i) => new Date().getFullYear() + 1 - i);
  makeSearch = ''; modelSearch = ''; loadingMakes = true; loadingTypes = false; loadingModels = false;
  error = ''; makeError = ''; explored = false;
  get filteredMakes() { return this.makes.filter(m => m.name.toLowerCase().includes(this.makeSearch.toLowerCase())).slice(0, 120); }
  get filteredModels() { return this.models.filter(m => m.name.toLowerCase().includes(this.modelSearch.toLowerCase())); }
  get selectedMake() { return this.makes.find(m => m.id === Number(this.makeId))?.name || 'your selection'; }
  ngOnInit() {
    this.api.makes().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: makes => { this.makes = makes.sort((a,b) => a.name.localeCompare(b.name)); this.loadingMakes = false; },
      error: () => { this.makeError = 'Could not load manufacturers. Please refresh to try again.'; this.loadingMakes = false; }
    });
    this.query$.pipe(
      tap(() => { this.loadingModels = true; this.error = ''; this.models = []; this.explored = true; }),
      switchMap(q => this.api.models(q.make, q.year, q.type).pipe(
        catchError((_: HttpErrorResponse) => { this.error = 'We could not reach the vehicle catalog. Please try again.'; return of([] as Model[]); }),
        finalize(() => { this.loadingModels = false; })
      )), takeUntilDestroyed(this.destroyRef)
    ).subscribe(models => { this.models = models; this.loadingModels = false; });
  }
  onMakeChange() {
    this.types = []; this.type = ''; this.models = []; this.explored = false; this.error = '';
    if (!this.makeId) return;
    this.loadingTypes = true;
    this.api.types(Number(this.makeId)).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: types => { this.types = types; this.loadingTypes = false; },
      error: () => { this.loadingTypes = false; this.error = 'Vehicle types are temporarily unavailable; you can still search all models.'; }
    });
  }
  onFilterChange() { this.explored = false; this.models = []; this.error = ''; }
  explore() { if (this.makeId) this.query$.next({make: Number(this.makeId), year: Number(this.year), type: this.type || undefined}); }
  trackModel(_: number, model: Model) { return model.id; }
}
