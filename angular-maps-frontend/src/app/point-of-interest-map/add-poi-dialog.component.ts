import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

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
        <h5>Add Point of Interest</h5>
        <div class="form-group">
          <label>Category</label>
          <select class="form-control" [(ngModel)]="model.category">
            <option *ngFor="let c of categories" [value]="c">{{c}}</option>
          </select>
        </div>
        <div class="form-group mt-2">
          <label>Details</label>
          <textarea class="form-control" rows="4" [(ngModel)]="model.details"></textarea>
        </div>
        <div class="d-flex justify-content-end gap-2 mt-3">
          <button class="btn btn-secondary" type="button" (click)="onCancel()">Cancel</button>
          <button class="btn btn-primary" type="button" (click)="onSave()">Save</button>
        </div>
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
export class AddPoiDialogComponent implements OnInit {
  @Input() latitude = 0;
  @Input() longitude = 0;
  @Input() categories: string[] = [];

  @Output() save = new EventEmitter<{ category: string; details: string }>();
  @Output() cancel = new EventEmitter<void>();

  model = { category: '', details: '' };

  ngOnInit(): void {
    this.model.category = this.categories && this.categories.length > 0 ? this.categories[0] : '';
  }

  onSave(): void {
    if (!this.model.details || this.model.details.trim().length === 0) {
      alert('Please enter details');
      return;
    }
    this.save.emit({ category: this.model.category, details: this.model.details.trim() });
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
