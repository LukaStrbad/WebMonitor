import { AfterViewInit, Component, Input } from '@angular/core';
import { FeatureDataWithFeatureName, getFeaturesList } from "../features-card/features-card.component";
import { User } from "../../../model/user";
import { MatCheckbox } from "@angular/material/checkbox";
import { AllowedFeatures, SupportedFeatures } from "../../../model/supported-features";
import { UserService } from "../../../services/user.service";
import { showErrorSnackbar, showOkSnackbar } from "../../../helpers/snackbar-helpers";
import { MatSnackBar } from "@angular/material/snack-bar";
import { SysInfoService } from "../../../services/sys-info.service";

@Component({
  selector: 'app-features-changer',
  templateUrl: './features-changer.component.html',
  styleUrls: ['./features-changer.component.css']
})
export class FeaturesChangerComponent implements AfterViewInit {
  @Input({ required: true }) user!: User;

  featuresList: FeatureDataWithFeatureName[] = [];
  systemFeatures?: SupportedFeatures;

  constructor(
    private userService: UserService,
    private snackBar: MatSnackBar,
    private sysInfo: SysInfoService
  ) {
    sysInfo.getSupportedFeatures().then(f => this.systemFeatures = f);
  }

  ngAfterViewInit(): void {
    this.refreshFeaturesList();
  }

  refreshFeaturesList() {
    const featuresList = getFeaturesList(this.user.allowedFeatures)
      .map(f => {
          if (f.supported) {
            f.note = "This feature is enabled for this user";
          } else {
            f.note = "This feature is disabled for this user";
          }
          return f;
        }
      );

    if (this.featuresList.length === 0) {
      this.featuresList = featuresList;
      return;
    }

    for (let i = 0; i < this.featuresList.length; i++) {
      this.featuresList[i].supported = featuresList[i].supported;
    }
  }

  shouldFeatureCheckboxBeDisabled(feature: FeatureDataWithFeatureName) {
    if (!this.systemFeatures) {
      return true;
    }

    const featureName = feature.featureName as keyof SupportedFeatures;
    return !this.systemFeatures[featureName];
  }

  async changeFeatures(e: Event, feature: FeatureDataWithFeatureName) {
    const target = e.target as MatCheckbox | null;

    e.preventDefault();
    if (!target) {
      return;
    }

    const features = this.user.allowedFeatures;
    const featureName = feature.featureName as keyof AllowedFeatures;
    // If the feature name is wrong, return
    if (features[featureName] === undefined) {
      return;
    }

    try {
      // Change the feature
      features[featureName] = !features[featureName];
      await this.userService.changeAllowedFeatures({ username: this.user.username, allowedFeatures: features });
      this.refreshFeaturesList();
      target.checked = features[featureName];
      showOkSnackbar(this.snackBar, `Successfully ${features[featureName] ? 'enabled' : 'disabled'} the '${feature.name}' feature`);
    } catch (e) {
      // Revert the change in case of an error
      features[featureName] = !features[featureName];
      showErrorSnackbar(this.snackBar, `Failed to change the features: ${e}`);
    }
  }
}
