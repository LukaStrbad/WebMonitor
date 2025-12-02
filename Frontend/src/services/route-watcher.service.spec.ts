import { TestBed } from '@angular/core/testing';

import { RouteWatcherService } from './route-watcher.service';

describe('RouteWatcherService', () => {
  let service: RouteWatcherService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RouteWatcherService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
