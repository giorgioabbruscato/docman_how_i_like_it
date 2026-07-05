import { create } from 'zustand';
import type { Me, TenantPlanFeatures } from '@/types/me';

const EMPTY_PLAN_FEATURES: TenantPlanFeatures = {
  maxEmployees: 0,
  customRoles: false,
  auditLog: false,
  advancedReports: false,
};

interface AuthState {
  accessToken: string | null;
  user: { email: string; name: string; roles: string[] } | null;
  me: Me | null;
  permissions: string[];
  planFeatures: TenantPlanFeatures;
  isPlatformAdmin: boolean;
  setAuth: (token: string, user: AuthState['user']) => void;
  setMe: (me: Me) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()((set) => ({
  accessToken: null,
  user: null,
  me: null,
  permissions: [],
  planFeatures: EMPTY_PLAN_FEATURES,
  isPlatformAdmin: false,
  setAuth: (accessToken, user) => set({ accessToken, user }),
  setMe: (me) =>
    set({
      me,
      permissions: me.permissions,
      planFeatures: me.planFeatures,
      isPlatformAdmin: me.isPlatformAdmin,
    }),
  logout: () =>
    set({
      accessToken: null,
      user: null,
      me: null,
      permissions: [],
      planFeatures: EMPTY_PLAN_FEATURES,
      isPlatformAdmin: false,
    }),
}));
