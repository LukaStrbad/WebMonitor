import { SupportedFeatures } from "./supported-features";

export interface User {
  username: string;
  displayName: string;
  isAdmin: boolean;
  allowedFeatures: SupportedFeatures;
}
