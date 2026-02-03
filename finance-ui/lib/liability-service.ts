import { getApiUrl, API_CONFIG } from './api-config';
import { getAuthHeaders } from './cookie-utils';
import { Liability, LiabilityFormData, NetWorthSummary } from '@/types/liability';

export const liabilityService = {
  async getAll(userId: number): Promise<Liability[]> {
    const response = await fetch(
      `${getApiUrl('/api/liabilities')}?userId=${userId}`,
      { headers: getAuthHeaders() }
    );

    if (!response.ok) {
      throw new Error('Erreur lors de la récupération des passifs');
    }

    return await response.json();
  },

  async getById(id: number, userId: number): Promise<Liability> {
    const response = await fetch(
      `${getApiUrl(`/api/liabilities/${id}`)}?userId=${userId}`,
      { headers: getAuthHeaders() }
    );

    if (!response.ok) {
      throw new Error('Erreur lors de la récupération du passif');
    }

    return await response.json();
  },

  async create(data: LiabilityFormData, userId: number): Promise<Liability> {
    const response = await fetch(
      `${getApiUrl('/api/liabilities')}?userId=${userId}`,
      {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
      }
    );

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || 'Erreur lors de la création du passif');
    }

    return await response.json();
  },

  async update(id: number, data: LiabilityFormData, userId: number): Promise<void> {
    const response = await fetch(
      `${getApiUrl(`/api/liabilities/${id}`)}?userId=${userId}`,
      {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify({ id, ...data }),
      }
    );

    if (!response.ok) {
      throw new Error('Erreur lors de la mise à jour du passif');
    }
  },

  async delete(id: number, userId: number): Promise<void> {
    const response = await fetch(
      `${getApiUrl(`/api/liabilities/${id}`)}?userId=${userId}`,
      {
        method: 'DELETE',
        headers: getAuthHeaders(),
      }
    );

    if (!response.ok) {
      throw new Error('Erreur lors de la suppression du passif');
    }
  },

  async getTotalDebt(userId: number): Promise<number> {
    const response = await fetch(
      `${getApiUrl('/api/liabilities/total-debt')}?userId=${userId}`,
      { headers: getAuthHeaders() }
    );

    if (!response.ok) {
      throw new Error('Erreur lors du calcul du total des dettes');
    }

    return await response.json();
  }
};

export const netWorthService = {
  async getNetWorth(userId: number): Promise<NetWorthSummary> {
    const response = await fetch(
      `${getApiUrl('/api/networth')}?userId=${userId}`,
      { headers: getAuthHeaders() }
    );

    if (!response.ok) {
      throw new Error('Erreur lors de la récupération du patrimoine net');
    }

    return await response.json();
  }
};
