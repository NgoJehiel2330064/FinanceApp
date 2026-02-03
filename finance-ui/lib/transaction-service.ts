import { getApiUrl, API_CONFIG } from './api-config';
import { getAuthHeaders } from './cookie-utils';

export interface Transaction {
  id: number;
  userId: number;  // Ajout du userId - requis par le backend
  date: string;
  amount: number;
  description: string;
  category: string;
  type: number;
  createdAt: string;
}

export interface CreateTransactionDto {
  date: string;
  amount: number;
  description: string;
  category: string;
  type: number;
}

/**
 * Service pour gérer les transactions
 */
export const transactionService = {
  /**
   * Récupère toutes les transactions pour l'utilisateur courant
   */
  async getAll(userId: number): Promise<Transaction[]> {
    const response = await fetch(
      `${getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS)}?userId=${userId}`,
      { headers: getAuthHeaders() }
    );

    if (!response.ok) {
      throw new Error('Erreur lors de la récupération des transactions');
    }

    return await response.json();
  },

  /**
   * Récupère une transaction par son ID
   */
  async getById(id: number, userId: number): Promise<Transaction> {
    const response = await fetch(
      `${getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS)}/${id}?userId=${userId}`,
      { headers: getAuthHeaders() }
    );

    if (!response.ok) {
      throw new Error('Erreur lors de la récupération de la transaction');
    }

    return await response.json();
  },

  /**
   * Crée une nouvelle transaction
   */
  async create(data: CreateTransactionDto, userId: number): Promise<Transaction> {
    const response = await fetch(`${getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS)}?userId=${userId}`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || 'Erreur lors de la création de la transaction');
    }

    return await response.json();
  },

  /**
   * Met à jour une transaction
   */
  async update(id: number, data: Partial<CreateTransactionDto>, userId: number): Promise<Transaction> {
    // Récupérer d'abord la transaction existante pour avoir createdAt et userId
    const existingTransaction = await this.getById(id, userId);
    
    console.log('Transaction existante récupérée:', existingTransaction);
    
    // Fusionner les données existantes avec les nouvelles - INCLURE userId !
    const updatedTransaction: Transaction = {
      id,
      userId: existingTransaction.userId,  // IMPORTANT: requis par le backend
      date: data.date ?? existingTransaction.date,
      amount: data.amount ?? existingTransaction.amount,
      description: data.description ?? existingTransaction.description,
      category: data.category ?? existingTransaction.category,
      type: data.type ?? existingTransaction.type,
      createdAt: existingTransaction.createdAt, // Conserver la date de création originale
    };
    
    console.log('Transaction à envoyer:', updatedTransaction);
    
    const response = await fetch(
      `${getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS)}/${id}?userId=${userId}`,
      {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(updatedTransaction),
      }
    );

    if (!response.ok) {
      const errorText = await response.text().catch(() => '');
      let errorMessage = 'Erreur lors de la mise à jour de la transaction';
      
      if (errorText) {
        try {
          const errorData = JSON.parse(errorText);
          errorMessage = errorData.message || errorText;
        } catch {
          errorMessage = errorText;
        }
      }
      
      console.error('Erreur mise à jour transaction:', { status: response.status, body: errorMessage });
      throw new Error(errorMessage);
    }

    return updatedTransaction;
  },

  /**
   * Supprime une transaction
   */
  async delete(id: number, userId: number): Promise<void> {
    const response = await fetch(
      `${getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS)}/${id}?userId=${userId}`,
      {
        method: 'DELETE',
        headers: getAuthHeaders(),
      }
    );

    if (!response.ok) {
      throw new Error('Erreur lors de la suppression de la transaction');
    }
  },
};
