import axios from 'axios';
import { keycloak, mapKeycloakUser } from '@/lib/keycloak';
import { useAuthStore } from '@/stores/auth-store';

function requireEnv(value: string | undefined, name: string, devFallback: string): string {
  if (value) {
    return value;
  }

  if (import.meta.env.DEV) {
    return devFallback;
  }

  throw new Error(`Missing required environment variable: ${name}`);
}

const apiBaseUrl = requireEnv(import.meta.env.VITE_API_BASE_URL, 'VITE_API_BASE_URL', '/api');
const tenantId = requireEnv(import.meta.env.VITE_TENANT_ID, 'VITE_TENANT_ID', 'demo');

export const apiClient = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    'Content-Type': 'application/json',
    'X-Tenant-Id': tenantId,
  },
});

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as (typeof error.config & { _retry?: boolean }) | undefined;

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry && keycloak.authenticated) {
      originalRequest._retry = true;

      try {
        await keycloak.updateToken(30);
        const token = keycloak.token;

        if (token) {
          useAuthStore.getState().setAuth(token, mapKeycloakUser(keycloak.tokenParsed));
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return apiClient.request(originalRequest);
        }
      } catch {
        useAuthStore.getState().logout();
      }
    } else if (error.response?.status === 401) {
      useAuthStore.getState().logout();
    }

    return Promise.reject(error);
  },
);
