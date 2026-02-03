'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { API_CONFIG, getApiUrl } from '@/lib/api-config';
import { transactionService } from '@/lib/transaction-service';
import { getAuthHeaders } from '@/lib/cookie-utils';

// Interface Transaction
interface Transaction {
  id: number;
  date: string;
  amount: number;
  description: string;
  category: string;
  type: number;
  createdAt: string;
}

interface NewTransaction {
  amount: number;
  description: string;
  category: string;
  type: number;
  date: string;
}

export default function TransactionsPage() {
  const router = useRouter();
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isOffline, setIsOffline] = useState<boolean>(false);
  
  const [showModal, setShowModal] = useState<boolean>(false);
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [formData, setFormData] = useState<NewTransaction>({
    amount: 0,
    description: '',
    category: '',
    type: 1,
    date: new Date().toISOString().split('T')[0]
  });
  
  // États pour la suppression
  const [showDeleteModal, setShowDeleteModal] = useState<boolean>(false);
  const [transactionToDelete, setTransactionToDelete] = useState<number | null>(null);
  const [deleting, setDeleting] = useState<boolean>(false);
  
  // États pour l'édition
  const [editingId, setEditingId] = useState<number | null>(null);
  const [isEditing, setIsEditing] = useState<boolean>(false);
  
  // États pour les filtres et tri
  const [filterCategory, setFilterCategory] = useState<string>('');
  const [filterType, setFilterType] = useState<string>('');
  const [filterStartDate, setFilterStartDate] = useState<string>('');
  const [filterEndDate, setFilterEndDate] = useState<string>('');
  const [sortBy, setSortBy] = useState<'date' | 'montant' | 'categorie'>('date');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');
  
  const [mounted, setMounted] = useState<boolean>(false);

  // Récupération des transactions
  useEffect(() => {
    setMounted(true);
    
    // Vérifier l'authentification
    const userStr = sessionStorage.getItem('user');
    if (!userStr) {
      router.push('/connexion');
      return;
    }
    
    const fetchTransactions = async () => {
      try {
        setLoading(true);
        setIsOffline(false);
        
        const user = JSON.parse(userStr);
        const userId = user.id;
        
        const response = await fetch(
          `${getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS)}?userId=${userId}`,
          { headers: getAuthHeaders() }
        );
        
        if (!response.ok) {
          throw new Error(`Erreur HTTP ${response.status}`);
        }
        
        const data: Transaction[] = await response.json();
        setTransactions(data);
        setError(null);
      } catch (err) {
        console.error('Erreur lors de la récupération des transactions:', err);
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
  }, []);



  // Les calculs seront faits après la définition de getFilteredAndSortedTransactions()

  const formatMontant = (montant: number): string => {
    return new Intl.NumberFormat('fr-CA', {
      style: 'currency',
      currency: 'CAD'
    }).format(montant);
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('fr-FR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);

    try {
      const userStr = sessionStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      const userId = user.id;
      
      const transactionData = {
        amount: formData.type === 1 ? formData.amount : -Math.abs(formData.amount),
        description: formData.description,
        category: formData.category,
        type: formData.type,
        date: new Date(formData.date).toISOString()
      };

      if (isEditing && editingId) {
        // Mode édition : PUT
        try {
          await transactionService.update(editingId, transactionData, userId);
          setTransactions(prev => prev.map(t => 
            t.id === editingId 
              ? { ...t, ...transactionData }
              : t
          ));
        } catch (err) {
          console.error('Erreur édition transaction:', err);
          alert(`Impossible de modifier la transaction: ${err instanceof Error ? err.message : 'Erreur inconnue'}`);
          setSubmitting(false);
          return;
        }
      } else {
        // Mode création : POST
        const response = await fetch(`${getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS)}?userId=${userId}`, {
          method: 'POST',
          headers: getAuthHeaders(),
          body: JSON.stringify(transactionData)
        });

        if (!response.ok) {
          throw new Error('Erreur lors de l\'ajout de la transaction');
        }

        const newTransaction: Transaction = await response.json();
        setTransactions(prev => [newTransaction, ...prev]);
      }

      setShowModal(false);
      setEditingId(null);
      setIsEditing(false);
      setFormData({
        amount: 0,
        description: '',
        category: '',
        type: 1,
        date: new Date().toISOString().split('T')[0]
      });
    } catch (err) {
      console.error('Erreur:', err);
      alert(isEditing ? 'Impossible de modifier la transaction. Vérifiez que l\'API est en ligne.' : 'Impossible d\'ajouter la transaction. Vérifiez que l\'API est en ligne.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!transactionToDelete) return;
    
    setDeleting(true);
    try {
      const userStr = sessionStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      const userId = user.id;
      
      await transactionService.delete(transactionToDelete, userId);
      setTransactions(prev => prev.filter(t => t.id !== transactionToDelete));
      setShowDeleteModal(false);
      setTransactionToDelete(null);
    } catch (err) {
      console.error('Erreur:', err);
      alert('Impossible de supprimer la transaction. Vérifiez que l\'API est en ligne.');
    } finally {
      setDeleting(false);
    }
  };

  const handleEdit = (transaction: Transaction) => {
    setEditingId(transaction.id);
    setIsEditing(true);
    setFormData({
      amount: Math.abs(transaction.amount),
      description: transaction.description,
      category: transaction.category,
      type: transaction.type === 1 ? 1 : 0,
      date: transaction.date.split('T')[0]
    });
    setShowModal(true);
  };

  // Fonction pour appliquer les filtres et tri
  const getFilteredAndSortedTransactions = () => {
    let filtered = transactions;

    // Filtre par catégorie
    if (filterCategory) {
      filtered = filtered.filter(t => t.category === filterCategory);
    }

    // Filtre par type
    if (filterType !== '') {
      filtered = filtered.filter(t => t.type === parseInt(filterType));
    }

    // Filtre par dates
    if (filterStartDate) {
      const startDate = new Date(filterStartDate).getTime();
      filtered = filtered.filter(t => new Date(t.date).getTime() >= startDate);
    }

    if (filterEndDate) {
      const endDate = new Date(filterEndDate).getTime();
      filtered = filtered.filter(t => new Date(t.date).getTime() <= endDate);
    }

    // Tri
    filtered.sort((a, b) => {
      let compareValue = 0;

      if (sortBy === 'date') {
        compareValue = new Date(a.date).getTime() - new Date(b.date).getTime();
      } else if (sortBy === 'montant') {
        compareValue = Math.abs(a.amount) - Math.abs(b.amount);
      } else if (sortBy === 'categorie') {
        compareValue = a.category.localeCompare(b.category);
      }

      return sortOrder === 'asc' ? compareValue : -compareValue;
    });

    return filtered;
  };

  // Calculs (sur les transactions filtrées)
  const filteredTransactions = getFilteredAndSortedTransactions();
  
  const revenus = filteredTransactions
    .filter(t => t.type === 1)
    .reduce((acc, t) => acc + t.amount, 0);

  const depenses = filteredTransactions
    .filter(t => t.type === 0)
    .reduce((acc, t) => acc + Math.abs(t.amount), 0);

  const soldeNet = revenus - depenses;

  return (
    <div className="min-h-screen bg-[#0f0f1e] text-white font-[family-name:var(--font-inter)]">
      {/* Cercles flous */}
      <div className="fixed top-0 left-0 w-full h-full overflow-hidden pointer-events-none">
        <div className="absolute top-[-20%] left-[-10%] w-[600px] h-[600px] bg-violet-600/30 rounded-full blur-[120px]"></div>
        <div className="absolute bottom-[-20%] right-[-10%] w-[700px] h-[700px] bg-blue-600/30 rounded-full blur-[120px]"></div>
      </div>

      <div className="relative z-10 max-w-7xl mx-auto px-6 py-12">
        {/* En-tête */}
        <header className={`mb-12 transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
          <h1 className="text-5xl font-bold mb-2 font-[family-name:var(--font-playfair)]">
            💳 Transactions
          </h1>
          <p className="text-gray-400 text-lg">Gérez vos revenus et dépenses</p>
        </header>

        {/* Mode déconnecté */}
        {isOffline && (
          <div className="backdrop-blur-xl bg-orange-500/10 border border-orange-500/30 rounded-2xl p-6 mb-8 animate-fadeIn">
            <div className="flex items-center gap-4">
              <div className="text-3xl">📡</div>
              <div className="flex-1">
                <h3 className="text-xl font-bold mb-1 text-orange-400">Mode Déconnecté</h3>
                <p className="text-gray-400">L&apos;API est actuellement hors ligne.</p>
              </div>
            </div>
          </div>
        )}

        {/* Loading */}
        {loading && (
          <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-violet-500"></div>
          </div>
        )}

        {/* Erreur */}
        {error && (
          <div className="backdrop-blur-xl bg-red-500/10 border border-red-500/30 rounded-2xl p-6 mb-8">
            <p className="text-red-400">❌ Erreur : {error}</p>
          </div>
        )}

        {!loading && !error && (
          <>
            {/* Cartes de résumé */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
              <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 animate-fadeIn" style={{ animationDelay: '0ms' }}>
                <p className="text-gray-400 text-sm uppercase tracking-wider mb-2">Solde Net</p>
                <p className={`text-4xl font-bold font-[family-name:var(--font-playfair)] ${soldeNet >= 0 ? 'text-emerald-400' : 'text-red-400'}`}>
                  {formatMontant(soldeNet)}
                </p>
              </div>

              <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 animate-fadeIn" style={{ animationDelay: '100ms' }}>
                <p className="text-gray-400 text-sm uppercase tracking-wider mb-2">Revenus</p>
                <p className="text-4xl font-bold text-emerald-400 font-[family-name:var(--font-playfair)]">
                  {formatMontant(revenus)}
                </p>
              </div>

              <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 animate-fadeIn" style={{ animationDelay: '200ms' }}>
                <p className="text-gray-400 text-sm uppercase tracking-wider mb-2">Dépenses</p>
                <p className="text-4xl font-bold text-red-400 font-[family-name:var(--font-playfair)]">
                  {formatMontant(depenses)}
                </p>
              </div>
            </div>

            {/* Bouton ajouter */}
            <div className="mb-8 flex justify-end">
              <button 
                onClick={() => setShowModal(true)}
                className="bg-gradient-to-r from-violet-600 to-purple-600 hover:from-violet-700 hover:to-purple-700 text-white font-semibold px-8 py-3 rounded-xl shadow-lg shadow-violet-500/50 transition-all duration-300 hover:scale-105"
              >
                + Ajouter une transaction
              </button>
            </div>

            {/* Barre de filtres et tri */}
            <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 mb-8">
              <h3 className="text-lg font-bold mb-4 text-white">🔍 Filtres et Tri</h3>
              
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-6 gap-4">
                {/* Filtre Catégorie */}
                <div>
                  <label className="block text-sm font-medium mb-2 text-gray-300">Catégorie</label>
                  <select 
                    value={filterCategory}
                    onChange={(e) => setFilterCategory(e.target.value)}
                    className="w-full bg-[#1a1a2e] border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                    style={{ colorScheme: 'dark' }}
                  >
                    <option value="">Tous</option>
                    <option value="Salaire">Salaire</option>
                    <option value="Freelance">Freelance</option>
                    <option value="Investissement">Investissement</option>
                    <option value="Alimentation">Alimentation</option>
                    <option value="Transport">Transport</option>
                    <option value="Logement">Logement</option>
                    <option value="Loisirs">Loisirs</option>
                    <option value="Santé">Santé</option>
                    <option value="Autre">Autre</option>
                  </select>
                </div>

                {/* Filtre Type */}
                <div>
                  <label className="block text-sm font-medium mb-2 text-gray-300">Type</label>
                  <select 
                    value={filterType}
                    onChange={(e) => setFilterType(e.target.value)}
                    className="w-full bg-[#1a1a2e] border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                    style={{ colorScheme: 'dark' }}
                  >
                    <option value="">Tous</option>
                    <option value="1">💰 Revenus</option>
                    <option value="0">💸 Dépenses</option>
                  </select>
                </div>

                {/* Filtre Date début */}
                <div>
                  <label className="block text-sm font-medium mb-2 text-gray-300">Du</label>
                  <input 
                    type="date"
                    value={filterStartDate}
                    onChange={(e) => setFilterStartDate(e.target.value)}
                    className="w-full bg-[#1a1a2e] border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                    style={{ colorScheme: 'dark' }}
                  />
                </div>

                {/* Filtre Date fin */}
                <div>
                  <label className="block text-sm font-medium mb-2 text-gray-300">Au</label>
                  <input 
                    type="date"
                    value={filterEndDate}
                    onChange={(e) => setFilterEndDate(e.target.value)}
                    className="w-full bg-[#1a1a2e] border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                    style={{ colorScheme: 'dark' }}
                  />
                </div>

                {/* Tri par */}
                <div>
                  <label className="block text-sm font-medium mb-2 text-gray-300">Trier par</label>
                  <select 
                    value={sortBy}
                    onChange={(e) => setSortBy(e.target.value as 'date' | 'montant' | 'categorie')}
                    className="w-full bg-[#1a1a2e] border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                    style={{ colorScheme: 'dark' }}
                  >
                    <option value="date">📅 Date</option>
                    <option value="montant">💵 Montant</option>
                    <option value="categorie">🏷️ Catégorie</option>
                  </select>
                </div>

                {/* Ordre tri */}
                <div>
                  <label className="block text-sm font-medium mb-2 text-gray-300">Ordre</label>
                  <select 
                    value={sortOrder}
                    onChange={(e) => setSortOrder(e.target.value as 'asc' | 'desc')}
                    className="w-full bg-[#1a1a2e] border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                    style={{ colorScheme: 'dark' }}
                  >
                    <option value="desc">⬇️ Décroissant</option>
                    <option value="asc">⬆️ Croissant</option>
                  </select>
                </div>
              </div>

              {/* Bouton réinitialiser */}
              {(filterCategory || filterType || filterStartDate || filterEndDate || sortBy !== 'date' || sortOrder !== 'desc') && (
                <div className="mt-4">
                  <button
                    onClick={() => {
                      setFilterCategory('');
                      setFilterType('');
                      setFilterStartDate('');
                      setFilterEndDate('');
                      setSortBy('date');
                      setSortOrder('desc');
                    }}
                    className="text-sm text-gray-400 hover:text-white transition-colors underline"
                  >
                    🔄 Réinitialiser les filtres
                  </button>
                </div>
              )}
            </div>

            {/* Tableau des transactions */}
            <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 overflow-hidden">
              <h2 className="text-2xl font-bold mb-6 font-[family-name:var(--font-playfair)]">
                Transactions Récentes
              </h2>

              {transactions.length === 0 ? (
                <p className="text-gray-400 text-center py-12">
                  Aucune transaction pour le moment.
                </p>
              ) : getFilteredAndSortedTransactions().length === 0 ? (
                <p className="text-gray-400 text-center py-12">
                  Aucune transaction ne correspond à vos filtres.
                </p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-white/10">
                        <th className="text-left py-4 px-4 text-gray-400 font-medium uppercase text-xs tracking-wider">Date</th>
                        <th className="text-left py-4 px-4 text-gray-400 font-medium uppercase text-xs tracking-wider">Description</th>
                        <th className="text-left py-4 px-4 text-gray-400 font-medium uppercase text-xs tracking-wider">Catégorie</th>
                        <th className="text-right py-4 px-4 text-gray-400 font-medium uppercase text-xs tracking-wider">Montant</th>
                        <th className="text-center py-4 px-4 text-gray-400 font-medium uppercase text-xs tracking-wider">Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {getFilteredAndSortedTransactions().map((transaction) => (
                        <tr key={transaction.id} className="border-b border-white/5 hover:bg-white/5 transition-colors">
                          <td className="py-4 px-4 text-gray-300">{formatDate(transaction.date)}</td>
                          <td className="py-4 px-4 font-medium">{transaction.description}</td>
                          <td className="py-4 px-4">
                            <span className="inline-block bg-violet-500/30 text-violet-200 px-3 py-1 rounded-full text-sm font-medium">
                              {transaction.category}
                            </span>
                          </td>
                          <td className={`py-4 px-4 text-right font-bold font-[family-name:var(--font-playfair)] ${
                            transaction.type === 1 ? 'text-emerald-400' : 'text-red-400'
                          }`}>
                            {transaction.type === 1 ? '+' : '-'}{formatMontant(Math.abs(transaction.amount))}
                          </td>
                          <td className="py-4 px-4 text-center">
                            <div className="flex gap-2 justify-center">
                              <button
                                onClick={() => handleEdit(transaction)}
                                className="inline-flex items-center justify-center w-8 h-8 rounded-lg bg-blue-500/20 hover:bg-blue-500/40 text-blue-400 hover:text-blue-300 transition-colors"
                                title="Modifier"
                              >
                                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                </svg>
                              </button>
                              <button
                                onClick={() => {
                                  setTransactionToDelete(transaction.id);
                                  setShowDeleteModal(true);
                                }}
                                className="inline-flex items-center justify-center w-8 h-8 rounded-lg bg-red-500/20 hover:bg-red-500/40 text-red-400 hover:text-red-300 transition-colors"
                                title="Supprimer"
                              >
                                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                  <path fillRule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                              </button>
                            </div>
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

      {/* Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 animate-fadeIn">
          <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={() => setShowModal(false)}></div>
          
          <div className="relative backdrop-blur-xl bg-white/10 border border-white/20 rounded-3xl p-8 max-w-md w-full shadow-2xl animate-scaleIn">
            <button onClick={() => {
              setShowModal(false);
              setEditingId(null);
              setIsEditing(false);
              setFormData({ amount: 0, description: '', category: '', type: 1, date: new Date().toISOString().split('T')[0] });
            }} className="absolute top-4 right-4 text-gray-400 hover:text-white transition-colors">
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>

            <h2 className="text-3xl font-bold mb-6 font-[family-name:var(--font-playfair)]">
              {isEditing ? '✏️ Modifier la transaction' : '📝 Nouvelle Transaction'}
            </h2>

            <form onSubmit={handleSubmit} className="space-y-5">
              {/* Type */}
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-300">Type</label>
                <div className="flex gap-4">
                  <button type="button" onClick={() => setFormData({...formData, type: 1})}
                    className={`flex-1 py-3 rounded-xl font-semibold transition-all ${formData.type === 1 ? 'bg-emerald-500 text-white shadow-lg shadow-emerald-500/50' : 'bg-white/5 text-gray-400 hover:bg-white/10'}`}>
                    💰 Revenu
                  </button>
                  <button type="button" onClick={() => setFormData({...formData, type: 0})}
                    className={`flex-1 py-3 rounded-xl font-semibold transition-all ${formData.type === 0 ? 'bg-red-500 text-white shadow-lg shadow-red-500/50' : 'bg-white/5 text-gray-400 hover:bg-white/10'}`}>
                    💸 Dépense
                  </button>
                </div>
              </div>

              {/* Montant */}
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-300">Montant ($)</label>
                <input type="number" step="0.01" required value={formData.amount || ''} onChange={(e) => setFormData({...formData, amount: parseFloat(e.target.value)})}
                  className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white placeholder-gray-500 focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                  placeholder="0.00" />
              </div>

              {/* Description */}
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-300">Description</label>
                <input type="text" required value={formData.description} onChange={(e) => setFormData({...formData, description: e.target.value})}
                  className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white placeholder-gray-500 focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                  placeholder="Ex: Salaire mensuel" />
              </div>

              {/* Date */}
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-300">Date</label>
                <input type="date" required value={formData.date} onChange={(e) => setFormData({...formData, date: e.target.value})}
                  className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                  style={{ colorScheme: 'dark' }} />
              </div>

              {/* Catégorie */}
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-300">Catégorie</label>
                <select required value={formData.category} onChange={(e) => setFormData({...formData, category: e.target.value})}
                  className="w-full bg-[#1a1a2e] border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                  style={{ colorScheme: 'dark' }}>
                  <option value="">Sélectionner...</option>
                  <option value="Salaire">Salaire</option>
                  <option value="Freelance">Freelance</option>
                  <option value="Investissement">Investissement</option>
                  <option value="Alimentation">Alimentation</option>
                  <option value="Transport">Transport</option>
                  <option value="Logement">Logement</option>
                  <option value="Loisirs">Loisirs</option>
                  <option value="Santé">Santé</option>
                  <option value="Autre">Autre</option>
                </select>
              </div>

              {/* Boutons */}
              <div className="flex gap-4 pt-4">
                <button type="button" onClick={() => {
                  setShowModal(false);
                  setEditingId(null);
                  setIsEditing(false);
                  setFormData({ amount: 0, description: '', category: '', type: 1, date: new Date().toISOString().split('T')[0] });
                }} className="flex-1 bg-white/5 hover:bg-white/10 text-white font-semibold py-3 rounded-xl transition-all">
                  Annuler
                </button>
                <button type="submit" disabled={submitting}
                  className="flex-1 bg-gradient-to-r from-violet-600 to-purple-600 hover:from-violet-700 hover:to-purple-700 text-white font-semibold py-3 rounded-xl shadow-lg shadow-violet-500/50 transition-all hover:scale-105 disabled:opacity-50">
                  {submitting ? (isEditing ? 'Modification...' : 'Ajout...') : (isEditing ? 'Modifier' : 'Ajouter')}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal de suppression */}
      {showDeleteModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 animate-fadeIn">
          <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={() => !deleting && setShowDeleteModal(false)}></div>
          
          <div className="relative backdrop-blur-xl bg-white/10 border border-white/20 rounded-3xl p-8 max-w-md w-full shadow-2xl animate-scaleIn">
            <h2 className="text-2xl font-bold mb-4 font-[family-name:var(--font-playfair)] text-red-400">
              Confirmer la suppression
            </h2>
            
            <p className="text-gray-300 mb-8">
              Êtes-vous sûr de vouloir supprimer cette transaction ? Cette action ne peut pas être annulée.
            </p>

            <div className="flex gap-4">
              <button 
                onClick={() => !deleting && setShowDeleteModal(false)}
                disabled={deleting}
                className="flex-1 bg-white/5 hover:bg-white/10 text-white font-semibold py-3 rounded-xl transition-all disabled:opacity-50">
                Annuler
              </button>
              <button 
                onClick={handleDelete}
                disabled={deleting}
                className="flex-1 bg-red-600 hover:bg-red-700 text-white font-semibold py-3 rounded-xl shadow-lg shadow-red-500/50 transition-all hover:scale-105 disabled:opacity-50">
                {deleting ? 'Suppression...' : 'Supprimer'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

