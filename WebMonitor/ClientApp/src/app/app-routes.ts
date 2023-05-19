import { Route, Routes } from "@angular/router";
import { HomeComponent } from "./home/home.component";
import { UsagesComponent } from "./usage/usages/usages.component";
import { ProcessListComponent } from "./process/process-list/process-list.component";
import { FileBrowserComponent } from "./file-browser/file-browser.component";
import { SettingsComponent } from "./settings/settings.component";

export interface RouteWithIcon extends Route {
    icon?: string;
}

export const menuRoutes: RouteWithIcon[] = [
    {
        path: '',
        component: HomeComponent,
        pathMatch: 'full',
        title: 'Home',
        icon: 'home'
    },
    {
        path: 'usages',
        component: UsagesComponent,
        title: 'Component Usages',
        icon: 'show_chart'
    },
    {
        path: 'processes',
        component: ProcessListComponent,
        title: 'Processes',
        icon: 'list'
    },
    {
        path: 'file-browser',
        component: FileBrowserComponent,
        title: 'File Browser',
        icon: 'folder'
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
