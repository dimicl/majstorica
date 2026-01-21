import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MajstoriComponent } from './majstori.component';

describe('MajstoriComponent', () => {
  let component: MajstoriComponent;
  let fixture: ComponentFixture<MajstoriComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MajstoriComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MajstoriComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
