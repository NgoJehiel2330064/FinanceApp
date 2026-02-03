import { getApiUrl, API_CONFIG } from './api-config';

export interface RegisterData {
  nom: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginData {
  email: string;
  password: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: {
    id: number;
    nom: string;
    email: string;
    createdAt: string;
  };
}

/**
 * Service d'authentification
 */
export const authService = {
  /**
   * Inscription d'un nouvel utilisateur
   */
  async register(data: RegisterData): Promise<AuthResponse> {
    const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.REGISTER), {
      method: 'POST',
      headers: API_CONFIG.HEADERS,
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || 'Erreur lors de l\'inscription');
    }

    return await response.json();
  },

  /**
   * Connexion d'un utilisateur
   */
  async login(data: LoginData): Promise<AuthResponse> {
    const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.LOGIN), {
      method: 'POST',
      headers: API_CONFIG.HEADERS,
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || 'Erreur lors de la connexion');
    }

    return await response.json();
  },

  /**
   * Vérifier si un email existe déjà
   */
  async checkEmail(email: string): Promise<boolean> {
    const response = await fetch(
      `${getApiUrl(API_CONFIG.ENDPOINTS.CHECK_EMAIL)}?email=${encodeURIComponent(email)}`,
      {
        method: 'GET',
        headers: API_CONFIG.HEADERS,
      }
    );

    if (!response.ok) {
      return false;
    }

    const data = await response.json();
    return data.exists;
  },
};
