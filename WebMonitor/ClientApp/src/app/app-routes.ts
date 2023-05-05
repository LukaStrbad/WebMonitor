import { Routes } from "@angular/router";
import { HomeComponent } from "./home/home.component";
import { UsagesComponent } from "./usage/usages/usages.component";
import { ProcessListComponent } from "./process/process-list/process-list.component";
import { FileBrowserComponent } from "./file-browser/file-browser.component";
import { SettingsComponent } from "./settings/settings.component";

export const menuRoutes: Routes = [
    {
        path: '',
        component: HomeComponent,
        pathMatch: 'full',
        title: 'Home'
    },
    {
        path: 'usages',
        component: UsagesComponent,
        title: 'Component Usages'
    },
    {
        path: 'processes',
        component: ProcessListComponent,
        title: 'Processes'
    },
    {
        path: 'file-browser',
        component: FileBrowserComponent,
        title: 'File Browser'
    }
];

export const appRoutes: Routes = [
    ...menuRoutes,
    {
        path: "settings",
        component: SettingsComponent,
        title: 'Settings'
    }
];
