# Configuration Frontend - FinanceApp

Ce document explique comment configurer votre frontend (Next.js, React, Vue, etc.) pour se connecter à l'API FinanceApp.

---

## ?? Configuration de l'URL de l'API

### Next.js / React

Créez un fichier `config/api.ts` (ou `.js`) :

```typescript
// config/api.ts

/**
 * Configuration centralisée de l'API
 */

// URL de base de l'API (changez selon votre environnement)
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5152";

/**
 * Configuration des endpoints
 */
export const API_CONFIG = {
  BASE_URL: API_BASE_URL,
  
  ENDPOINTS: {
    // Transactions
    TRANSACTIONS: "/api/transactions",
    TRANSACTION_BY_ID: (id: number) => `/api/transactions/${id}`,
    TRANSACTIONS_BALANCE: "/api/transactions/balance",
    
    // Finance / IA
    FINANCIAL_ADVICE: "/api/finance/advice",
    SUGGEST_CATEGORY: "/api/finance/suggest-category",
    FINANCIAL_SUMMARY: "/api/finance/summary",
    DETECT_ANOMALIES: "/api/finance/anomalies",
    PREDICT_BUDGET: "/api/finance/predict",
  },
  
  // Timeout en millisecondes
  TIMEOUT: 30000,
};

/**
 * Construire l'URL complète d'un endpoint
 */
export function getApiUrl(endpoint: string): string {
  return `${API_CONFIG.BASE_URL}${endpoint}`;
}

/**
 * Fonction utilitaire pour les appels fetch avec gestion d'erreurs
 */
export async function apiFetch<T>(
  endpoint: string,
  options?: RequestInit
): Promise<T> {
  const url = getApiUrl(endpoint);
  
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), API_CONFIG.TIMEOUT);
  
  try {
    const response = await fetch(url, {
      ...options,
      signal: controller.signal,
      headers: {
        "Content-Type": "application/json",
        ...options?.headers,
      },
    });
    
    clearTimeout(timeoutId);
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(
        errorData.message || `HTTP Error: ${response.status} ${response.statusText}`
      );
    }
    
    return await response.json();
  } catch (error) {
    clearTimeout(timeoutId);
    
    if (error instanceof Error) {
      if (error.name === "AbortError") {
        throw new Error("La requête a expiré. Vérifiez que l'API est démarrée.");
      }
      throw error;
    }
    
    throw new Error("Erreur inconnue lors de l'appel API");
  }
}

export default API_CONFIG;
```

---

## ?? Variables d'environnement

### Next.js

Créez un fichier `.env.local` à la racine de votre projet Next.js :

```env
# .env.local

# URL de l'API FinanceApp
NEXT_PUBLIC_API_URL=http://localhost:5152

# Environnement
NODE_ENV=development
```

**?? Important** : Ajoutez `.env.local` à votre `.gitignore` !

### En production

```env
# .env.production

NEXT_PUBLIC_API_URL=https://votre-api.azurewebsites.net
```

---

## ?? Exemples d'utilisation

### 1. Récupérer toutes les transactions

```typescript
import { apiFetch, API_CONFIG } from "@/config/api";

// Dans un composant ou une fonction
async function fetchTransactions() {
  try {
    const transactions = await apiFetch<Transaction[]>(
      API_CONFIG.ENDPOINTS.TRANSACTIONS
    );
    
    console.log("Transactions récupérées :", transactions);
    return transactions;
  } catch (error) {
    console.error("Erreur lors de la récupération des transactions :", error);
    throw error;
  }
}
```

### 2. Créer une transaction

```typescript
import { apiFetch, API_CONFIG } from "@/config/api";

interface CreateTransactionDto {
  description: string;
  amount: number;
  type: "Income" | "Expense";
  category: string;
  date: string; // ISO 8601 format
}

async function createTransaction(data: CreateTransactionDto) {
  try {
    const newTransaction = await apiFetch<Transaction>(
      API_CONFIG.ENDPOINTS.TRANSACTIONS,
      {
        method: "POST",
        body: JSON.stringify(data),
      }
    );
    
    console.log("Transaction créée :", newTransaction);
    return newTransaction;
  } catch (error) {
    console.error("Erreur lors de la création de la transaction :", error);
    throw error;
  }
}

// Exemple d'utilisation
const transaction = await createTransaction({
  description: "Salaire mensuel",
  amount: 3000,
  type: "Income",
  category: "Salaire",
  date: new Date().toISOString(),
});
```

### 3. Obtenir un conseil financier IA

```typescript
import { apiFetch, API_CONFIG } from "@/config/api";

interface AdviceResponse {
  advice: string;
}

async function getFinancialAdvice() {
  try {
    const response = await apiFetch<AdviceResponse>(
      API_CONFIG.ENDPOINTS.FINANCIAL_ADVICE
    );
    
    console.log("Conseil IA :", response.advice);
    return response.advice;
  } catch (error) {
    console.error("Erreur lors de la récupération du conseil :", error);
    return "Impossible de générer un conseil pour le moment.";
  }
}
```

### 4. React Hook personnalisé

```typescript
import { useState, useEffect } from "react";
import { apiFetch, API_CONFIG } from "@/config/api";

/**
 * Hook pour récupérer les transactions
 */
export function useTransactions() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  useEffect(() => {
    async function loadTransactions() {
      try {
        setLoading(true);
        setError(null);
        
        const data = await apiFetch<Transaction[]>(
          API_CONFIG.ENDPOINTS.TRANSACTIONS
        );
        
        setTransactions(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Erreur inconnue");
      } finally {
        setLoading(false);
      }
    }
    
    loadTransactions();
  }, []);
  
  return { transactions, loading, error };
}

// Utilisation dans un composant
function TransactionsList() {
  const { transactions, loading, error } = useTransactions();
  
  if (loading) return <div>Chargement...</div>;
  if (error) return <div>Erreur : {error}</div>;
  
  return (
    <ul>
      {transactions.map((t) => (
        <li key={t.id}>{t.description} - {t.amount}€</li>
      ))}
    </ul>
  );
}
```

---

## ?? Dépannage Frontend

### Erreur : "Failed to fetch"

**Causes possibles** :
1. L'API n'est pas démarrée ? Lancez `.\start-app.ps1`
2. Mauvaise URL ? Vérifiez `.env.local`
3. Problème CORS ? Vérifiez la console du navigateur (F12)

**Solution** :
```typescript
// Testez d'abord avec l'URL directe
const response = await fetch("http://localhost:5152/api/transactions");
console.log(response.status); // Devrait être 200
```

### Erreur CORS

**Symptôme dans la console** :
```
Access to fetch at 'http://localhost:5152/api/transactions' from origin 
'http://localhost:3000' has been blocked by CORS policy
```

**Solution** :
1. Vérifiez que votre port frontend (ex: 3000) est dans `Program.cs`
2. Redémarrez l'API après modification
3. Voir [TROUBLESHOOTING.md](../TROUBLESHOOTING.md) pour plus de détails

### Données non à jour

**Solution** : Désactiver le cache du navigateur

```typescript
const response = await fetch(url, {
  cache: "no-store", // Désactive le cache
});
```

---

## ?? Exemple de composant Next.js complet

```typescript
"use client";

import { useState, useEffect } from "react";
import { apiFetch, API_CONFIG } from "@/config/api";

interface Transaction {
  id: number;
  description: string;
  amount: number;
  type: "Income" | "Expense";
  category: string;
  date: string;
}

export default function FinanceDashboard() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [advice, setAdvice] = useState<string>("");
  
  // Récupérer les transactions
  useEffect(() => {
    async function fetchData() {
      try {
        setLoading(true);
        
        // Récupérer transactions
        const txs = await apiFetch<Transaction[]>(
          API_CONFIG.ENDPOINTS.TRANSACTIONS
        );
        setTransactions(txs);
        
        // Récupérer conseil IA
        const adviceResponse = await apiFetch<{ advice: string }>(
          API_CONFIG.ENDPOINTS.FINANCIAL_ADVICE
        );
        setAdvice(adviceResponse.advice);
        
      } catch (err) {
        setError(err instanceof Error ? err.message : "Erreur inconnue");
      } finally {
        setLoading(false);
      }
    }
    
    fetchData();
  }, []);
  
  if (loading) {
    return <div className="p-4">Chargement...</div>;
  }
  
  if (error) {
    return (
      <div className="p-4 bg-red-100 border border-red-400 rounded">
        <h2 className="text-red-700 font-bold">Erreur</h2>
        <p>{error}</p>
        <p className="text-sm mt-2">
          Vérifiez que l'API est démarrée : <code>.\start-app.ps1</code>
        </p>
      </div>
    );
  }
  
  return (
    <div className="p-8">
      <h1 className="text-3xl font-bold mb-6">Finance Dashboard</h1>
      
      {/* Conseil IA */}
      {advice && (
        <div className="mb-6 p-4 bg-blue-100 border border-blue-400 rounded">
          <h2 className="font-bold text-blue-700">?? Conseil financier IA</h2>
          <p className="mt-2">{advice}</p>
        </div>
      )}
      
      {/* Liste des transactions */}
      <div>
        <h2 className="text-2xl font-bold mb-4">
          Transactions ({transactions.length})
        </h2>
        
        {transactions.length === 0 ? (
          <p className="text-gray-500">Aucune transaction pour le moment.</p>
        ) : (
          <ul className="space-y-2">
            {transactions.map((tx) => (
              <li
                key={tx.id}
                className="p-4 border rounded flex justify-between"
              >
                <div>
                  <p className="font-semibold">{tx.description}</p>
                  <p className="text-sm text-gray-600">{tx.category}</p>
                </div>
                <div className="text-right">
                  <p
                    className={`font-bold ${
                      tx.type === "Income" ? "text-green-600" : "text-red-600"
                    }`}
                  >
                    {tx.type === "Income" ? "+" : "-"}{tx.amount}€
                  </p>
                  <p className="text-sm text-gray-600">
                    {new Date(tx.date).toLocaleDateString()}
                  </p>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
```

---

## ?? Ressources

- [Next.js Environment Variables](https://nextjs.org/docs/basic-features/environment-variables)
- [Fetch API Documentation](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API)
- [React Hooks](https://react.dev/reference/react)
- [TypeScript](https://www.typescriptlang.org/docs/)

---

**?? Astuce** : Gardez la console du navigateur (F12) ouverte pendant le développement pour voir les erreurs réseau !
