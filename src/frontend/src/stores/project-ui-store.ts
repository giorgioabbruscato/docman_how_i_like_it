import { create } from 'zustand';
import type { ProjectStatus } from '@/types/project';

interface ProjectUiState {
  filtersOpen: boolean;
  activeTab: 'info' | 'members';
  search: string;
  customerName: string;
  status: ProjectStatus | '';
  isArchived: boolean;
  page: number;
  setFiltersOpen: (open: boolean) => void;
  setActiveTab: (tab: 'info' | 'members') => void;
  setSearch: (search: string) => void;
  setCustomerName: (customerName: string) => void;
  setStatus: (status: ProjectStatus | '') => void;
  setIsArchived: (isArchived: boolean) => void;
  setPage: (page: number) => void;
  resetFilters: () => void;
}

export const useProjectUiStore = create<ProjectUiState>()((set) => ({
  filtersOpen: false,
  activeTab: 'info',
  search: '',
  customerName: '',
  status: '',
  isArchived: false,
  page: 1,
  setFiltersOpen: (filtersOpen) => set({ filtersOpen }),
  setActiveTab: (activeTab) => set({ activeTab }),
  setSearch: (search) => set({ search, page: 1 }),
  setCustomerName: (customerName) => set({ customerName, page: 1 }),
  setStatus: (status) => set({ status, page: 1 }),
  setIsArchived: (isArchived) => set({ isArchived, page: 1 }),
  setPage: (page) => set({ page }),
  resetFilters: () =>
    set({ search: '', customerName: '', status: '', isArchived: false, page: 1 }),
}));
