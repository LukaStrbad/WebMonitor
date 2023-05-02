import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-breadcrumbs',
  templateUrl: './breadcrumbs.component.html',
  styleUrls: ['./breadcrumbs.component.css']
})
export class BreadcrumbsComponent {
  @Input() items: BreadcrumbItem[] = [];
  @Input() separator = "/";
}

export interface BreadcrumbItem {
  title: string;
  onClick?: () => void;
}
