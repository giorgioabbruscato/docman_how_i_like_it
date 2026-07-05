export const isSingleTenancyMode =
  (import.meta.env.VITE_TENANCY_MODE ?? 'multi').toLowerCase() === 'single';
