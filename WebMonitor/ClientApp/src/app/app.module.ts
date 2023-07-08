import { BrowserModule } from '@angular/platform-browser';
import { LOCALE_ID, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import localeHr from '@angular/common/locales/hr';

import { AppComponent } from './app.component';
import { NavBarComponent } from "./nav/nav-bar/nav-bar.component";
import { HomeComponent } from './home/home.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatToolbarModule } from "@angular/material/toolbar";
import { MatIconModule } from "@angular/material/icon";
import { MatSidenavModule } from "@angular/material/sidenav";
import { NavMenuComponent } from './nav/nav-menu/nav-menu.component';
import { UsageGraphComponent } from './components/usage-graph/usage-graph.component';
import { UsagesComponent } from './usage/usages/usages.component';
import { MatExpansionModule } from "@angular/material/expansion";
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ProcessListComponent } from './process/process-list/process-list.component';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { appRoutes } from './app-routes';
import { FileBrowserComponent } from './file-browser/file-browser.component';
import { BreadcrumbsComponent } from './components/breadcrumbs/breadcrumbs.component';
import { SettingsComponent } from './settings/settings.component';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ColorInputComponent } from './components/color-input/color-input.component';
import { MatInputModule } from '@angular/material/input';
import { FileDialogComponent } from './components/file-dialog/file-dialog.component';
import { MatDialogModule } from '@angular/material/dialog';
import { registerLocaleData } from '@angular/common';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatRippleModule } from '@angular/material/core';
import { EllipsisPipe } from '../pipes/ellipsis.pipe';
import { MatSelectModule } from '@angular/material/select';
import { MatListModule } from "@angular/material/list";

@NgModule({
  declarations: [
    AppComponent,
    NavBarComponent,
    HomeComponent,
    NavMenuComponent,
    UsageGraphComponent,
    UsagesComponent,
    ProcessListComponent,
    FileBrowserComponent,
    BreadcrumbsComponent,
    SettingsComponent,
    ColorInputComponent,
    FileDialogComponent,
    EllipsisPipe
  ],
    imports: [
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        HttpClientModule,
        FormsModule,
        RouterModule.forRoot(appRoutes),
        BrowserAnimationsModule,
        MatButtonModule,
        MatCardModule,
        MatToolbarModule,
        MatIconModule,
        MatSidenavModule,
        MatExpansionModule,
        MatProgressBarModule,
        MatTableModule,
        MatSortModule,
        MatSlideToggleModule,
        MatCheckboxModule,
        MatTooltipModule,
        MatInputModule,
        MatDialogModule,
        MatSnackBarModule,
        MatRippleModule,
        MatSelectModule,
        MatListModule
    ],
  providers: [{ provide: LOCALE_ID, useValue: "hr-HR" }],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor() {
    registerLocaleData(localeHr);
  }
}
