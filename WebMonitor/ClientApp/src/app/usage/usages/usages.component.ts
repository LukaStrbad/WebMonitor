import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { UsageGraphComponent } from "../usage-graph/usage-graph.component";
import { interval } from "rxjs";
import { SysInfoService } from "../../../services/sys-info.service";

@Component({
  selector: 'app-usages',
  templateUrl: './usages.component.html',
  styleUrls: ['./usages.component.css']
})
export class UsagesComponent implements AfterViewInit {
  @ViewChild("usageGraph")
  usageGraph!: UsageGraphComponent;

  constructor(public sysInfo: SysInfoService) {
  }

  ngAfterViewInit(): void {
    interval(1000).subscribe(() => {
      this.usageGraph.addValue(Math.random() * 100);
    });
  }

}
