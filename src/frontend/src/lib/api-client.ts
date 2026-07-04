import axios from 'axios';
import { useAuthStore } from '@/stores/auth-store';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '/api';
const tenantId = import.meta.env.VITE_TENANT_ID ?? 'demo';

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
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
    }
    return Promise.reject(error);
  },
);
