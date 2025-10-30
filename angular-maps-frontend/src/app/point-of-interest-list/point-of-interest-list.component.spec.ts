import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { PointOfInterestListComponent } from './point-of-interest-list.component';

describe('PointOfInterestListComponent', () => {
  let component: PointOfInterestListComponent;
  let fixture: ComponentFixture<PointOfInterestListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PointOfInterestListComponent, HttpClientTestingModule]
    })
      .compileComponents();

    fixture = TestBed.createComponent(PointOfInterestListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('confirmDeleteSelectedPoi should call deletePoi and hide currentBsModal if set', () => {
    // Arrange
    const poiStub: any = { name: 'ToDelete' };
    component['selectedPoi'] = poiStub;

    // spy on deletePoi
    const deleteSpy = spyOn<any>(component, 'deletePoi').and.callFake(() => { });

    // create a mock modal with hide method
    const fakeModal = { hide: jasmine.createSpy('hide') };
    component['currentBsModal'] = fakeModal;

    // Act
    (component as any).confirmDeleteSelectedPoi();

    // Assert
    expect(deleteSpy).toHaveBeenCalledWith(poiStub);
    expect(fakeModal.hide).toHaveBeenCalled();
    expect(component['selectedPoi']).toBeUndefined();
    expect(component['currentBsModal']).toBeUndefined();
  });
});
