import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-paginator',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './paginator.component.html'
})
export class PaginatorComponent implements OnChanges {
  @Input() currentPage: number = 1;
  @Input() pageSize: number = 10;
  @Input() totalItems: number = 0;
  @Input() totalPages: number = 1;
  @Input() hasPreviousPage: boolean = false;
  @Input() hasNextPage: boolean = false;

  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();

  pages: number[] = [];
  pageSizeOptions: number[] = [5, 10, 25, 50, 100];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['totalPages'] || changes['currentPage']) {
      this.updatePages();
    }
  }

  private updatePages(): void {
    const maxVisiblePages = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
    let end = Math.min(this.totalPages, start + maxVisiblePages - 1);

    if (end - start + 1 < maxVisiblePages) {
      start = Math.max(1, end - maxVisiblePages + 1);
    }

    this.pages = [];
    for (let i = start; i <= end; i++) {
      this.pages.push(i);
    }
  }

  goToPage(page: number): void {
    if (page !== this.currentPage && page >= 1 && page <= this.totalPages) {
      this.pageChange.emit(page);
    }
  }

  onPageSizeChange(event: Event): void {
    const newSize = parseInt((event.target as HTMLSelectElement).value);
    this.pageSizeChange.emit(newSize);
  }

  get showingFrom(): number {
    if (this.totalItems === 0) return 0;
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get showingTo(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }
}
