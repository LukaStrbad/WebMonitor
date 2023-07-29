import { AllowedFeatures } from "./supported-features";

export interface User {
  username: string;
  displayName: string;
  isAdmin: boolean;
  allowedFeatures: AllowedFeatures;
}
