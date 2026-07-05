import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';
import type { AxiosError } from 'axios';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

interface ProblemDetails {
  detail?: string;
  title?: string;
  errorCode?: string;
  extensions?: { errorCode?: string };
}

export function getApiErrorCode(error: unknown): string | undefined {
  const axiosError = error as AxiosError<ProblemDetails>;
  const data = axiosError.response?.data;
  return data?.errorCode ?? data?.extensions?.errorCode;
}

export function getApiErrorMessage(error: unknown, fallback: string): string {
  const axiosError = error as AxiosError<ProblemDetails>;
  return axiosError.response?.data?.detail ?? axiosError.response?.data?.title ?? fallback;
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function formatDate(value: string): string {
  return new Date(value).toLocaleDateString();
}

export function formatDateTime(value: string): string {
  return new Date(value).toLocaleString();
}

export function todayDateString(): string {
  return new Date().toISOString().slice(0, 10);
}

export function toDateString(date: Date): string {
  return date.toISOString().slice(0, 10);
}

export function getTodayDateRange(): { fromDate: string; toDate: string } {
  const today = todayDateString();
  return { fromDate: today, toDate: today };
}

export function getThisWeekDateRange(): { fromDate: string; toDate: string } {
  const now = new Date();
  const day = now.getDay();
  const diffToMonday = day === 0 ? -6 : 1 - day;
  const monday = new Date(now);
  monday.setDate(now.getDate() + diffToMonday);
  const sunday = new Date(monday);
  sunday.setDate(monday.getDate() + 6);
  return { fromDate: toDateString(monday), toDate: toDateString(sunday) };
}

export function getThisMonthDateRange(): { fromDate: string; toDate: string } {
  const now = new Date();
  const first = new Date(now.getFullYear(), now.getMonth(), 1);
  const last = new Date(now.getFullYear(), now.getMonth() + 1, 0);
  return { fromDate: toDateString(first), toDate: toDateString(last) };
}

export function formatPercent(value: number, decimals = 0): string {
  return `${(value * 100).toFixed(decimals)}%`;
}

export function currentTimeString(): string {
  const now = new Date();
  return `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}:${String(now.getSeconds()).padStart(2, '0')}`;
}

export function triggerBlobDownload(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}

export function confirmAction(message: string): boolean {
  return window.confirm(message);
}

export function formatDuration(minutes: number): string {
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  if (hours === 0) return `${mins}m`;
  if (mins === 0) return `${hours}h`;
  return `${hours}h ${mins}m`;
}

export function formatElapsed(seconds: number): string {
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  const s = seconds % 60;
  return [h, m, s].map((v) => String(v).padStart(2, '0')).join(':');
}
