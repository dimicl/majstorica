import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { combineLatest } from 'rxjs';
import { take } from 'rxjs/operators';
import { MarketplaceActions } from '../../shared/store/marketplace/marketplace.actions';
import {
  selectMarketplaceError,
  selectMarketplaceHasMore,
  selectMarketplaceJobs,
  selectMarketplaceLoading,
  selectMarketplacePage,
  selectMarketplacePageSize,
} from '../../shared/store/marketplace/marketplace.selectors';
import { BUTTON_TYPES } from '../../shared/types';
import { ButtonComponent } from '../../components/button/button.component';

@Component({
  selector: 'app-marketplace',
  templateUrl: './marketplace.component.html',
  styleUrl: './marketplace.component.scss',
  imports: [CommonModule, ButtonComponent],
})
export class MarketplaceComponent implements OnInit {
  private store = inject(Store);
  private shouldScrollAfterLoadMore = false;

  readonly jobs$ = this.store.select(selectMarketplaceJobs);
  readonly loading$ = this.store.select(selectMarketplaceLoading);
  readonly error$ = this.store.select(selectMarketplaceError);
  readonly page$ = this.store.select(selectMarketplacePage);
  readonly pageSize$ = this.store.select(selectMarketplacePageSize);
  readonly hasMore$ = this.store.select(selectMarketplaceHasMore);

  public eButtonType = BUTTON_TYPES;

  ngOnInit(): void {
    this.store.dispatch(MarketplaceActions.loadJobs({ page: 1, pageSize: 10 }));

    combineLatest([this.jobs$, this.loading$])
      .pipe(takeUntilDestroyed())
      .subscribe(([jobs, loading]) => {
        if (!loading && this.shouldScrollAfterLoadMore) {
          this.shouldScrollAfterLoadMore = false;
          if (jobs.length > 0) {
            setTimeout(() => {
              window.scrollTo({
                top: document.body.scrollHeight,
                behavior: 'smooth',
              });
            }, 120);
          }
        }
      });
  }

  public onShowMore(): void {
    combineLatest([this.page$, this.pageSize$, this.hasMore$, this.loading$])
      .pipe(take(1))
      .subscribe(([page, pageSize, hasMore, loading]) => {
        if (loading || !hasMore) return;
        this.shouldScrollAfterLoadMore = true;
        this.store.dispatch(
          MarketplaceActions.loadJobs({ page: page + 1, pageSize })
        );
      });
  }

  public onCompeteJob(): void {
    console.log('Compete job');
  }
}
