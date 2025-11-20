import { Component, EventEmitter, HostListener, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { PointOfInterest } from '../model/point_of_interest';

/**
 * Lightweight standalone dialog component used to collect POI details from the user.
 * This component is created dynamically from the map component and appended to the document body.
 * 
 * @author AI generated
 */
@Component({
  selector: 'app-add-poi-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="poidialog-backdrop">
      <div class="poidialog">
        <h5><i [ngClass]="cssClass" class="text-primary"></i>&nbsp;{{action}} Point of Interest</h5>
        <div class="form-group">
          <label><i class="bi bi-tag"></i>&nbsp;Category</label>
          <select class="form-control" [(ngModel)]="pointOfInterest!.category">
            <option *ngFor="let c of categories" [value]="c">{{c | titlecase}}</option>
          </select>
        </div>
        <div class="form-group mt-2">
          <label>Name</label>
          <textarea class="form-control" rows="1" [(ngModel)]="pointOfInterest!.name"></textarea>
        </div>
        <div class="form-group mt-2">
          <label><i class="bi bi-card-text"></i>&nbsp;Details</label>
          <textarea class="form-control" rows="4" [(ngModel)]="pointOfInterest!.details"></textarea>
        </div>
        <div class="form-group mt-2">
          <label><i class="bi bi-geo-alt"></i>&nbsp;Location</label>
          <textarea class="form-control" rows="2" disabled>Lat: {{ pointOfInterest!.location.coordinates[1] }}\nLng: {{ pointOfInterest!.location.coordinates[0] }}</textarea>
        </div>
        <div class="d-flex justify-content-end gap-2 mt-3">
          <button class="btn btn-outline-secondary btn-sm" type="button" (click)="onCancel()"><i class="bi bi-x-circle"></i>&nbsp;Cancel</button>
          <button class="btn btn-outline-primary btn-sm" type="button" (click)="onSave()"><i class="bi bi-floppy2-fill"></i>&nbsp;Save</button>
        </div>
        <div class="shortcut-hint" aria-hidden="true">Esc to cancel · Enter to save</div>
      </div>
    </div>
  `,
  styles: [
    `
    .poidialog-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.4);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
    }
    .poidialog {
      background: white;
      padding: 16px;
      border-radius: 6px;
      width: 320px;
      box-shadow: 0 8px 24px rgba(0,0,0,0.2);
    }
    
    `
  ]
})
export class PoiDialogComponent {
  @Input() categories: string[] = [];

  @Input() pointOfInterest: PointOfInterest | null = null;

  @Output() save = new EventEmitter<{ pointOfInterest: PointOfInterest | null }>();
  @Output() cancel = new EventEmitter<void>();

  action = 'Add';
  cssClass = 'bi bi-file-earmark-plus';

  nameOld: string = '';
  categoryOld: string = '';
  detailsOld: string = '';

  ngOnInit(): void {
    if (this.pointOfInterest) {
      this.nameOld = this.pointOfInterest.name;
      this.categoryOld = this.pointOfInterest.category;
      this.detailsOld = this.pointOfInterest.details;
    }
  }

  onSave(): void {
    if (!this.pointOfInterest?.details || this.pointOfInterest.details.trim().length === 0) {
      alert('Please enter details');
      return;
    }

    this.pointOfInterest.details.trim();

    this.save.emit({ pointOfInterest: this.pointOfInterest });
  }

  onCancel(): void {
    if (this.pointOfInterest) {
      // restore old values
      this.pointOfInterest.name = this.nameOld;
      this.pointOfInterest.category = this.categoryOld;
      this.pointOfInterest.details = this.detailsOld;
    }
    this.cancel.emit();
  }

  // keyboard shortcuts: Esc -> cancel, Enter -> save (but ignore Enter when typing in textarea)
  @HostListener('document:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      event.preventDefault();
      this.onCancel();
      return;
    }

    if (event.key === 'Enter') {
      const target = event.target as HTMLElement | null;
      // if focus is inside a textarea, let Enter insert a newline
      if (target && target.tagName && target.tagName.toLowerCase() === 'textarea') {
        return;
      }
      event.preventDefault();
      this.onSave();
    }
  }
}
