// Configuration centralisée de l'API
// Utilise les variables d'environnement pour plus de flexibilité

export const API_CONFIG = {
  // URL de base de l'API C# (modifiable via .env.local)
  BASE_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5153',
  
  // Endpoints disponibles
  ENDPOINTS: {
    TRANSACTIONS: '/api/transactions',
    FINANCE_ADVICE: '/api/finance/advice',
    FINANCE: '/api/finance',
    CATEGORIES: '/api/transactions/categories',
      FINANCE_CHAT: '/api/finance/chat',
    SUMMARY: '/api/finance/summary',
    // Nouveaux endpoints Assets
    ASSETS: '/api/assets',
    ASSETS_TOTAL_VALUE: '/api/assets/total-value',
    PORTFOLIO_INSIGHTS: '/api/finance/portfolio-insights',
    // Endpoints Auth
    REGISTER: '/api/auth/register',
    LOGIN: '/api/auth/login',
    CHECK_EMAIL: '/api/auth/check-email',
    GET_PROFILE: '/api/auth/profile',
    CHANGE_PASSWORD: '/api/auth/change-password',
    DELETE_ACCOUNT: '/api/auth/delete-account'
  },
  
  // Configuration des requêtes
  TIMEOUT: 10000, // 10 secondes
  
  // Headers par défaut
  HEADERS: {
    'Content-Type': 'application/json',
  }
};

// Helper pour construire les URLs complètes
export const getApiUrl = (endpoint: string): string => {
  return `${API_CONFIG.BASE_URL}${endpoint}`;
};
