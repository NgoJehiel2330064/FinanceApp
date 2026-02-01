'use client';

import { useState, useEffect } from 'react';
import { API_CONFIG, getApiUrl } from '@/lib/api-config';
import { Asset, AssetFormData } from '@/types/asset';
import AssetList from '@/components/AssetList';
import AssetModal from '@/components/AssetModal';

// ===========================
// INTERFACE TYPESCRIPT
// ===========================
// Cette interface définit le "contrat" TypeScript pour les données Transaction.
// TypeScript va vérifier que chaque objet reçu de l'API C# respecte cette structure.
// Si l'API renvoie un champ manquant ou d'un mauvais type, TypeScript signalera une erreur.
// IMPORTANT : Les noms de propriétés doivent correspondre EXACTEMENT à ceux de l'API C#
interface Transaction {
  id: number;           // TypeScript exige que 'id' soit un nombre
  date: string;         // La date de la transaction (format ISO string du C#)
  amount: number;       // Le montant de la transaction (peut être positif ou négatif)
  description: string;  // La description de la transaction
  category: string;     // La catégorie de la transaction (ex: "Revenus", "Dépenses")
  type: number;         // Le type de transaction (0 = dépense, 1 = revenu, par exemple)
  createdAt: string;    // La date de création de l'enregistrement (format ISO)
}

// Interface pour le formulaire de nouvelle transaction
interface NewTransaction {
  amount: number;
  description: string;
  category: string;
  type: number;
}

// ===========================
// COMPOSANT PRINCIPAL
// ===========================
export default function FinanceDashboard() {
  // STATE MANAGEMENT avec TypeScript
  // Le type Transaction[] indique que 'transactions' sera toujours un tableau de Transaction
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isOffline, setIsOffline] = useState<boolean>(false);
  const [aiAdvice, setAiAdvice] = useState<string>('');
  const [loadingAdvice, setLoadingAdvice] = useState<boolean>(false);
  
  // État pour le modal d'ajout de transaction
  const [showModal, setShowModal] = useState<boolean>(false);
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [formData, setFormData] = useState<NewTransaction>({
    amount: 0,
    description: '',
    category: '',
    type: 1
  });
  
  // État pour les animations
  const [mounted, setMounted] = useState<boolean>(false);

  // ===========================
  // STATE MANAGEMENT - PATRIMOINE (ASSETS)
  // ===========================
  const [assets, setAssets] = useState<Asset[]>([]);
  const [totalAssetValue, setTotalAssetValue] = useState<number>(0);
  const [assetsLoading, setAssetsLoading] = useState<boolean>(true);
  const [assetsError, setAssetsError] = useState<string | null>(null);
  const [showAssetModal, setShowAssetModal] = useState<boolean>(false);
  const [editingAsset, setEditingAsset] = useState<Asset | null>(null);
  const [assetSubmitting, setAssetSubmitting] = useState<boolean>(false);

  // ===========================
  // MESSAGE DE MOTIVATION DYNAMIQUE
  // ===========================
  const getMotivationMessage = (): string => {
    const heure = new Date().getHours();
    
    if (heure >= 5 && heure < 12) {
      return "☀️ Bonjour ! Commencez la journée du bon pied financier.";
    } else if (heure >= 12 && heure < 18) {
      return "🌤️ Bon après-midi ! Votre argent travaille pour vous.";
    } else if (heure >= 18 && heure < 22) {
      return "🌆 Bonsoir ! Prenez le contrôle de vos finances.";
    } else {
      return "🌙 Bonne nuit ! Vos investissements dorment... pas vous ?";
    }
  };

  // ===========================
  // RÉCUPÉRATION DES DONNÉES API
  // ===========================
  useEffect(() => {
    // Animation de montage
    setMounted(true);
    
    // Fonction asynchrone pour récupérer les transactions depuis l'API C#
    const fetchTransactions = async () => {
      try {
        setLoading(true);
        setIsOffline(false);
        
        // Utilise la configuration centralisée
        const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS));
        
        // Vérification si la requête HTTP a réussi (status 200-299)
        if (!response.ok) {
          throw new Error(`Erreur HTTP ${response.status}`);
        }
        
        // TypeScript : Nous "promettons" que data sera du type Transaction[]
        // Si l'API renvoie des données incompatibles, on aura une erreur à l'exécution
        const data: Transaction[] = await response.json();
        
        // VALIDATION TYPESCRIPT EXPLICITE (optionnelle mais recommandée)
        // Cette validation runtime vérifie que chaque propriété reçue de l'API C#
        // correspond bien au type attendu par TypeScript. C'est une double sécurité.
        const isValid = data.every(t => 
          typeof t.id === 'number' &&
          typeof t.date === 'string' &&
          typeof t.amount === 'number' &&
          typeof t.description === 'string' &&
          typeof t.category === 'string' &&
          typeof t.type === 'number' &&
          typeof t.createdAt === 'string'
        );
        
        if (!isValid) {
          throw new Error('Les données reçues ne correspondent pas au format Transaction');
        }
        
        setTransactions(data);
        setError(null);
      } catch (err) {
        console.error('Erreur lors de la récupération des transactions:', err);
        // Mode déconnecté : au lieu d'afficher une erreur brutale
        if (err instanceof TypeError && err.message.includes('fetch')) {
          setIsOffline(true);
          setError(null);
        } else {
          setError(err instanceof Error ? err.message : 'Erreur inconnue');
        }
      } finally {
        setLoading(false);
      }
    };

    fetchTransactions();
  }, []); // [] signifie : exécuter une seule fois au montage du composant

  // ===========================
  // RÉCUPÉRATION DES ACTIFS (PATRIMOINE)
  // ===========================
  useEffect(() => {
    const fetchAssets = async () => {
      try {
        setAssetsLoading(true);
        setAssetsError(null);
        
        // Récupérer la liste des actifs
        const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.ASSETS));
        
        if (!response.ok) {
          throw new Error(`Erreur HTTP ${response.status}`);
        }
        
        const data: Asset[] = await response.json();
        setAssets(data);
        
        // Récupérer la valeur totale depuis l'API (backend calcule)
        const totalResponse = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.ASSETS_TOTAL_VALUE));
        if (totalResponse.ok) {
          const totalData = await totalResponse.json();
          // Le backend peut renvoyer { totalValue: 123456 } ou juste un nombre
          setTotalAssetValue(typeof totalData === 'number' ? totalData : totalData.totalValue || 0);
        }
      } catch (err) {
        console.error('Erreur lors de la récupération des actifs:', err);
        setAssetsError(err instanceof Error ? err.message : 'Erreur lors du chargement des actifs');
      } finally {
        setAssetsLoading(false);
      }
    };

    fetchAssets();
  }, []);

  // ===========================
  // RÉCUPÉRATION DES CONSEILS IA
  // ===========================
  useEffect(() => {
    const fetchAiAdvice = async () => {
      try {
        setLoadingAdvice(true);
        const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.FINANCE_ADVICE));
        
        if (response.ok) {
          // L'API peut renvoyer du JSON ou du texte brut
          const contentType = response.headers.get('content-type');
          
          if (contentType && contentType.includes('application/json')) {
            // Format JSON : { "advice": "message" }
            const data = await response.json();
            setAiAdvice(data.advice || data.message || 'Conseil non disponible');
          } else {
            // Format texte brut
            const advice = await response.text();
            setAiAdvice(advice);
          }
        }
      } catch (err) {
        console.log('Conseil IA non disponible');
      } finally {
        setLoadingAdvice(false);
      }
    };

    // Récupérer les conseils uniquement si on a des transactions
    if (transactions.length > 0) {
      fetchAiAdvice();
    }
  }, [transactions]);

  // ===========================
  // CALCULS DES STATISTIQUES
  // ===========================
  // TypeScript garantit que 'amount' est un nombre, donc .reduce() est sûr
  // On utilise le champ 'type' pour distinguer revenus (type=1) et dépenses (type=0)
  const revenus = transactions
    .filter(t => t.type === 1) // type 1 = revenu
    .reduce((acc, t) => acc + t.amount, 0);

  const depenses = transactions
    .filter(t => t.type === 0) // type 0 = dépense
    .reduce((acc, t) => acc + Math.abs(t.amount), 0);

  const soldeNet = revenus - depenses;

  // ===========================
  // FORMATAGE DES MONTANTS
  // ===========================
  const formatMontant = (montant: number): string => {
    return new Intl.NumberFormat('fr-CA', {
      style: 'currency',
      currency: 'CAD'
    }).format(montant);
  };

  // ===========================
  // FORMATAGE DES DATES
  // ===========================
  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('fr-FR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    });
  };

  // ===========================
  // GESTION DU FORMULAIRE
  // ===========================
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);

    try {
      const API_URL = 'https://localhost:7219';
      // Préparer les données au format attendu par l'API C#
      const transactionData = {
        amount: formData.type === 1 ? formData.amount : -Math.abs(formData.amount),
        description: formData.description,
        category: formData.category,
        type: formData.type,
        date: new Date().toISOString()
      };

      // POST vers l'API C# avec configuration centralisée
      const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS), {
        method: 'POST',
        headers: API_CONFIG.HEADERS,
        body: JSON.stringify(transactionData)
      });

      if (!response.ok) {
        throw new Error('Erreur lors de l\'ajout de la transaction');
      }

      // Récupérer la nouvelle transaction créée
      const newTransaction: Transaction = await response.json();

      // Mettre à jour la liste instantanément (optimistic update)
      setTransactions(prev => [newTransaction, ...prev]);

      // Fermer le modal et réinitialiser le formulaire
      setShowModal(false);
      setFormData({
        amount: 0,
        description: '',
        category: '',
        type: 1
      });

    } catch (err) {
      console.error('Erreur:', err);
      alert('Impossible d\'ajouter la transaction. Vérifiez que l\'API est en ligne.');
    } finally {
      setSubmitting(false);
    }
  };

  // ===========================
  // HANDLERS - GESTION DES ACTIFS
  // ===========================
  const handleAssetSubmit = async (assetData: AssetFormData) => {
    setAssetSubmitting(true);
    
    try {
      const url = editingAsset 
        ? `${getApiUrl(API_CONFIG.ENDPOINTS.ASSETS)}/${editingAsset.id}`
        : getApiUrl(API_CONFIG.ENDPOINTS.ASSETS);
      
      const method = editingAsset ? 'PUT' : 'POST';
      
      const response = await fetch(url, {
        method,
        headers: API_CONFIG.HEADERS,
        body: JSON.stringify(assetData)
      });

      if (!response.ok) {
        throw new Error(`Erreur ${response.status} lors de l'enregistrement de l'actif`);
      }

      // Recharger les actifs depuis l'API (backend est source de vérité)
      const assetsResponse = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.ASSETS));
      const updatedAssets: Asset[] = await assetsResponse.json();
      setAssets(updatedAssets);

      // Recharger la valeur totale
      const totalResponse = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.ASSETS_TOTAL_VALUE));
      if (totalResponse.ok) {
        const totalData = await totalResponse.json();
        setTotalAssetValue(typeof totalData === 'number' ? totalData : totalData.totalValue || 0);
      }

      setShowAssetModal(false);
      setEditingAsset(null);
    } catch (err) {
      console.error('Erreur lors de l\'enregistrement de l\'actif:', err);
      throw err; // Le modal affichera l'erreur
    } finally {
      setAssetSubmitting(false);
    }
  };

  const handleAssetEdit = (asset: Asset) => {
    setEditingAsset(asset);
    setShowAssetModal(true);
  };

  const handleAssetDelete = async (id: number) => {
    if (!confirm('Êtes-vous sûr de vouloir supprimer cet actif ?')) {
      return;
    }

    try {
      const response = await fetch(`${getApiUrl(API_CONFIG.ENDPOINTS.ASSETS)}/${id}`, {
        method: 'DELETE'
      });

      if (!response.ok) {
        throw new Error('Erreur lors de la suppression');
      }

      // Recharger depuis l'API
      const assetsResponse = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.ASSETS));
      const updatedAssets: Asset[] = await assetsResponse.json();
      setAssets(updatedAssets);

      // Recharger la valeur totale
      const totalResponse = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.ASSETS_TOTAL_VALUE));
      if (totalResponse.ok) {
        const totalData = await totalResponse.json();
        setTotalAssetValue(typeof totalData === 'number' ? totalData : totalData.totalValue || 0);
      }
    } catch (err) {
      console.error('Erreur lors de la suppression:', err);
      alert('Impossible de supprimer l\'actif. Vérifiez que l\'API est en ligne.');
    }
  };

  const handleAddAsset = () => {
    setEditingAsset(null);
    setShowAssetModal(true);
  };

  // ===========================
  // RENDER
  // ===========================
  return (
    <div className="min-h-screen bg-[#0f0f1e] text-white font-[family-name:var(--font-inter)] overflow-hidden">
      {/* CERCLES FLOUS EN ARRIÈRE-PLAN */}
      <div className="fixed top-0 left-0 w-full h-full overflow-hidden pointer-events-none">
        <div className="absolute top-[-20%] left-[-10%] w-[600px] h-[600px] bg-violet-600/30 rounded-full blur-[120px]"></div>
        <div className="absolute bottom-[-20%] right-[-10%] w-[700px] h-[700px] bg-blue-600/30 rounded-full blur-[120px]"></div>
      </div>

      {/* CONTENU PRINCIPAL */}
      <div className="relative z-10 max-w-7xl mx-auto px-6 py-12">
        
        {/* EN-TÊTE */}
        <header className="mb-12">
          <h1 className="text-5xl font-bold mb-2 font-[family-name:var(--font-playfair)]">
            Finance Dashboard
          </h1>
          <p className="text-gray-400 text-lg">{getMotivationMessage()}</p>
        </header>

        {/* CONSEIL DE L'IA avec effet scintillement doré */}
        {aiAdvice && (
          <div className={`backdrop-blur-xl bg-gradient-to-r from-yellow-500/10 to-amber-500/10 border border-yellow-500/30 rounded-2xl p-6 mb-8 transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 -translate-y-4'}`}>
            <div className="flex items-start gap-4">
              <div className="text-3xl animate-pulse">✨</div>
              <div className="flex-1">
                <h3 className="text-xl font-bold mb-2 text-transparent bg-clip-text bg-gradient-to-r from-yellow-400 to-amber-400 animate-shimmer">
                  Conseil de l&apos;IA
                </h3>
                {loadingAdvice ? (
                  <div className="flex gap-2">
                    <div className="w-2 h-2 bg-yellow-400 rounded-full animate-bounce" style={{ animationDelay: '0ms' }}></div>
                    <div className="w-2 h-2 bg-yellow-400 rounded-full animate-bounce" style={{ animationDelay: '150ms' }}></div>
                    <div className="w-2 h-2 bg-yellow-400 rounded-full animate-bounce" style={{ animationDelay: '300ms' }}></div>
                  </div>
                ) : (
                  <p className="text-gray-300 leading-relaxed">{aiAdvice}</p>
                )}
              </div>
            </div>
          </div>
        )}

        {/* MODE DÉCONNECTÉ - Message élégant */}
        {isOffline && (
          <div className={`backdrop-blur-xl bg-orange-500/10 border border-orange-500/30 rounded-2xl p-6 mb-8 transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 -translate-y-4'}`}>
            <div className="flex items-center gap-4">
              <div className="text-3xl">📡</div>
              <div className="flex-1">
                <h3 className="text-xl font-bold mb-1 text-orange-400">Mode Déconnecté</h3>
                <p className="text-gray-400">
                  L&apos;API est actuellement hors ligne. Vos transactions seront synchronisées dès la reconnexion.
                </p>
              </div>
            </div>
          </div>
        )}

        {/* GESTION DES ÉTATS LOADING/ERROR */}
        {loading && (
          <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-violet-500"></div>
          </div>
        )}

        {error && (
          <div className="backdrop-blur-xl bg-red-500/10 border border-red-500/30 rounded-2xl p-6 mb-8">
            <p className="text-red-400">❌ Erreur : {error}</p>
            <p className="text-sm text-gray-400 mt-2">
              Assurez-vous que votre API C# tourne sur {API_CONFIG.BASE_URL}
            </p>
          </div>
        )}

        {!loading && !error && (
          <>
            {/* CARTES DE RÉSUMÉ avec animation */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
              
              {/* CARTE SOLDE NET */}
              <div className={`backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 hover:bg-white/10 transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`} style={{ transitionDelay: '100ms' }}>
                <p className="text-gray-400 text-sm uppercase tracking-wider mb-2">
                  Solde Net
                </p>
                <p className={`text-4xl font-bold font-[family-name:var(--font-playfair)] ${
                  soldeNet >= 0 ? 'text-emerald-400' : 'text-red-400'
                }`}>
                  {formatMontant(soldeNet)}
                </p>
              </div>

              {/* CARTE REVENUS */}
              <div className={`backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 hover:bg-white/10 transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`} style={{ transitionDelay: '200ms' }}>
                <p className="text-gray-400 text-sm uppercase tracking-wider mb-2">
                  Revenus
                </p>
                <p className="text-4xl font-bold text-emerald-400 font-[family-name:var(--font-playfair)]">
                  {formatMontant(revenus)}
                </p>
              </div>

              {/* CARTE DÉPENSES */}
              <div className={`backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 hover:bg-white/10 transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`} style={{ transitionDelay: '300ms' }}>
                <p className="text-gray-400 text-sm uppercase tracking-wider mb-2">
                  Dépenses
                </p>
                <p className="text-4xl font-bold text-red-400 font-[family-name:var(--font-playfair)]">
                  {formatMontant(depenses)}
                </p>
              </div>
            </div>

            {/* BOUTON AJOUTER TRANSACTION */}
            <div className="mb-8 flex justify-end">
              <button 
                onClick={() => setShowModal(true)}
                className="bg-gradient-to-r from-violet-600 to-purple-600 hover:from-violet-700 hover:to-purple-700 text-white font-semibold px-8 py-3 rounded-xl shadow-lg shadow-violet-500/50 transition-all duration-300 hover:scale-105"
              >
                + Ajouter une transaction
              </button>
            </div>

            {/* TABLEAU DES TRANSACTIONS */}
            <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 overflow-hidden">
              <h2 className="text-2xl font-bold mb-6 font-[family-name:var(--font-playfair)]">
                Transactions Récentes
              </h2>

              {transactions.length === 0 ? (
                <p className="text-gray-400 text-center py-12">
                  Aucune transaction pour le moment.
                </p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-white/10">
                        <th className="text-left py-4 px-4 text-gray-400 font-medium uppercase text-xs tracking-wider">
                          Date
                        </th>
                        <th className="text-left py-4 px-4 text-gray-400 font-medium uppercase text-xs tracking-wider">
                          Description
                        </th>
                        <th className="text-left py-4 px-4 text-gray-400 font-medium uppercase text-xs tracking-wider">
                          Catégorie
                        </th>
                        <th className="text-right py-4 px-4 text-gray-400 font-medium uppercase text-xs tracking-wider">
                          Montant
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {transactions.map((transaction) => (
                        <tr 
                          key={transaction.id} 
                          className="border-b border-white/5 hover:bg-white/5 transition-colors"
                        >
                          <td className="py-4 px-4 text-gray-300">
                            {formatDate(transaction.date)}
                          </td>
                          <td className="py-4 px-4 font-medium">
                            {transaction.description}
                          </td>
                          <td className="py-4 px-4">
                            <span className="inline-block bg-violet-500/30 text-violet-200 px-3 py-1 rounded-full text-sm font-medium">
                              {transaction.category}
                            </span>
                          </td>
                          <td className={`py-4 px-4 text-right font-bold font-[family-name:var(--font-playfair)] ${
                            transaction.type === 1 ? 'text-emerald-400' : 'text-red-400'
                          }`}>
                            {transaction.type === 1 ? '+' : '-'}
                            {formatMontant(Math.abs(transaction.amount))}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </>
        )}
      </div>

      {/* MODAL GLASSMORPHISM pour ajouter une transaction */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 animate-fadeIn">
          {/* Backdrop avec blur */}
          <div 
            className="absolute inset-0 bg-black/60 backdrop-blur-sm"
            onClick={() => setShowModal(false)}
          ></div>
          
          {/* Modal */}
          <div className="relative backdrop-blur-xl bg-white/10 border border-white/20 rounded-3xl p-8 max-w-md w-full shadow-2xl animate-scaleIn">
            <button
              onClick={() => setShowModal(false)}
              className="absolute top-4 right-4 text-gray-400 hover:text-white transition-colors"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>

            <h2 className="text-3xl font-bold mb-6 font-[family-name:var(--font-playfair)]">
              Nouvelle Transaction
            </h2>

            <form onSubmit={handleSubmit} className="space-y-5">
              {/* Type de transaction */}
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-300">Type</label>
                <div className="flex gap-4">
                  <button
                    type="button"
                    onClick={() => setFormData({...formData, type: 1})}
                    className={`flex-1 py-3 rounded-xl font-semibold transition-all ${
                      formData.type === 1 
                        ? 'bg-emerald-500 text-white shadow-lg shadow-emerald-500/50' 
                        : 'bg-white/5 text-gray-400 hover:bg-white/10'
                    }`}
                  >
                    💰 Revenu
                  </button>
                  <button
                    type="button"
                    onClick={() => setFormData({...formData, type: 0})}
                    className={`flex-1 py-3 rounded-xl font-semibold transition-all ${
                      formData.type === 0 
                        ? 'bg-red-500 text-white shadow-lg shadow-red-500/50' 
                        : 'bg-white/5 text-gray-400 hover:bg-white/10'
                    }`}
                  >
                    💸 Dépense
                  </button>
                </div>
              </div>

              {/* Montant */}
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-300">Montant ($)</label>
                <input
                  type="number"
                  step="0.01"
                  required
                  value={formData.amount || ''}
                  onChange={(e) => setFormData({...formData, amount: parseFloat(e.target.value)})}
                  className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white placeholder-gray-500 focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                  placeholder="0.00"
                />
              </div>

              {/* Description */}
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-300">Description</label>
                <input
                  type="text"
                  required
                  value={formData.description}
                  onChange={(e) => setFormData({...formData, description: e.target.value})}
                  className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white placeholder-gray-500 focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                  placeholder="Ex: Salaire mensuel"
                />
              </div>

              {/* Catégorie */}
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-300">Catégorie</label>
                <select
                  required
                  value={formData.category}
                  onChange={(e) => setFormData({...formData, category: e.target.value})}
                  className="w-full bg-[#1a1a2e] border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all [&>option]:bg-[#1a1a2e] [&>option]:text-white [&>option]:py-2"
                  style={{ colorScheme: 'dark' }}
                >
                  <option value="" className="bg-[#1a1a2e] text-gray-400">Sélectionner...</option>
                  <option value="Salaire" className="bg-[#1a1a2e] text-white">Salaire</option>
                  <option value="Freelance" className="bg-[#1a1a2e] text-white">Freelance</option>
                  <option value="Investissement" className="bg-[#1a1a2e] text-white">Investissement</option>
                  <option value="Alimentation" className="bg-[#1a1a2e] text-white">Alimentation</option>
                  <option value="Transport" className="bg-[#1a1a2e] text-white">Transport</option>
                  <option value="Logement" className="bg-[#1a1a2e] text-white">Logement</option>
                  <option value="Loisirs" className="bg-[#1a1a2e] text-white">Loisirs</option>
                  <option value="Santé" className="bg-[#1a1a2e] text-white">Santé</option>
                  <option value="Autre" className="bg-[#1a1a2e] text-white">Autre</option>
                </select>
              </div>

              {/* Boutons */}
              <div className="flex gap-4 pt-4">
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  className="flex-1 bg-white/5 hover:bg-white/10 text-white font-semibold py-3 rounded-xl transition-all"
                >
                  Annuler
                </button>
                <button
                  type="submit"
                  disabled={submitting}
                  className="flex-1 bg-gradient-to-r from-violet-600 to-purple-600 hover:from-violet-700 hover:to-purple-700 text-white font-semibold py-3 rounded-xl shadow-lg shadow-violet-500/50 transition-all hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {submitting ? 'Ajout...' : 'Ajouter'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ===========================
           SECTION PATRIMOINE (ASSETS)
           =========================== */}
      <section className="mt-16">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h2 className="text-3xl font-bold font-[family-name:var(--font-playfair)]">
              💎 Mon Patrimoine
            </h2>
            <p className="text-gray-400 mt-1">Suivez l'évolution de vos actifs</p>
          </div>
        </div>

        <AssetList
          assets={assets}
          totalValue={totalAssetValue}
          isLoading={assetsLoading}
          error={assetsError}
          onEdit={handleAssetEdit}
          onDelete={handleAssetDelete}
          onAddNew={handleAddAsset}
        />
      </section>

      {/* MODAL ASSET */}
      <AssetModal
        isOpen={showAssetModal}
        onClose={() => {
          setShowAssetModal(false);
          setEditingAsset(null);
        }}
        onSubmit={handleAssetSubmit}
        editingAsset={editingAsset}
      />
    </div>
  );
}
