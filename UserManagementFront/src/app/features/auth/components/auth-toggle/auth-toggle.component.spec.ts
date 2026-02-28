import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuthToggleComponent } from './auth-toggle.component';

describe('AuthToggleComponent', () => {
  let component: AuthToggleComponent;
  let fixture: ComponentFixture<AuthToggleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuthToggleComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AuthToggleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
