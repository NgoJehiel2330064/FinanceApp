'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { API_CONFIG, getApiUrl } from '@/lib/api-config';
import { getAuthHeaders } from '@/lib/cookie-utils';
import {
  PieChart,
  Pie,
  Cell,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

interface Transaction {
  id: number;
  date: string;
  amount: number;
  description: string;
  category: string;
  type: number;
  createdAt: string;
}

const COLORS = [
  '#10b981', '#f59e0b', '#ef4444', '#3b82f6', '#8b5cf6',
  '#ec4899', '#14b8a6', '#f97316', '#6366f1', '#84cc16'
];

const MONTHS = ['Jan', 'Fév', 'Mar', 'Avr', 'Mai', 'Juin', 'Juil', 'Aoû', 'Sep', 'Oct', 'Nov', 'Déc'];

export default function StatisticsPage() {
  const router = useRouter();
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isOffline, setIsOffline] = useState<boolean>(false);
  const [mounted, setMounted] = useState<boolean>(false);
  const [selectedYear, setSelectedYear] = useState<number>(new Date().getFullYear());

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
          throw new Error('Erreur lors de la récupération des transactions');
        }

        const data: Transaction[] = await response.json();
        setTransactions(data);
        setError(null);
      } catch (err) {
        console.error('Erreur:', err);
        setIsOffline(true);
        setError('Impossible de charger les transactions.');
      } finally {
        setLoading(false);
      }
    };

    fetchTransactions();
  }, []);

  // Calculs pour pie chart - Dépenses par catégorie
  const getCategoryExpensesData = () => {
    const categoryMap: { [key: string]: number } = {};

    transactions
      .filter(t => t.type === 0) // Dépenses seulement
      .forEach(t => {
        const category = t.category || 'Autre';
        categoryMap[category] = (categoryMap[category] || 0) + Math.abs(t.amount);
      });

    return Object.entries(categoryMap)
      .map(([name, value]) => ({
        name,
        value: Math.round(value * 100) / 100,
      }))
      .sort((a, b) => b.value - a.value);
  };

  // Calculs pour line chart - Évolution mensuelle
  const getMonthlyEvolutionData = () => {
    const monthlyData: { [key: number]: { revenus: number; depenses: number } } = {};

    // Initialiser tous les mois de l'année sélectionnée
    for (let month = 0; month < 12; month++) {
      monthlyData[month] = { revenus: 0, depenses: 0 };
    }

    // Remplir avec les données
    transactions.forEach(t => {
      const date = new Date(t.date);
      if (date.getFullYear() === selectedYear) {
        const month = date.getMonth();
        if (t.type === 1) {
          monthlyData[month].revenus += t.amount;
        } else {
          monthlyData[month].depenses += Math.abs(t.amount);
        }
      }
    });

    return MONTHS.map((month, index) => ({
      month,
      revenus: Math.round(monthlyData[index].revenus * 100) / 100,
      depenses: Math.round(monthlyData[index].depenses * 100) / 100,
    }));
  };

  // Statistiques annuelles
  const getAnnualStats = () => {
    const yearData = transactions.filter(t => new Date(t.date).getFullYear() === selectedYear);

    const totalRevenus = yearData
      .filter(t => t.type === 1)
      .reduce((acc, t) => acc + t.amount, 0);

    const totalDepenses = yearData
      .filter(t => t.type === 0)
      .reduce((acc, t) => acc + Math.abs(t.amount), 0);

    const soldeNet = totalRevenus - totalDepenses;
    const avgMonthlyExpense = totalDepenses / 12;

    return { totalRevenus, totalDepenses, soldeNet, avgMonthlyExpense };
  };

  const formatMontant = (montant: number): string => {
    return new Intl.NumberFormat('fr-CA', {
      style: 'currency',
      currency: 'CAD',
    }).format(montant);
  };

  const categoryExpensesData = getCategoryExpensesData();
  const monthlyEvolutionData = getMonthlyEvolutionData();
  const annualStats = getAnnualStats();
  const availableYears = Array.from(
    new Set(transactions.map(t => new Date(t.date).getFullYear()))
  ).sort((a, b) => b - a);

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
            📊 Statistiques
          </h1>
          <p className="text-gray-400 text-lg">Analysez vos finances en détail</p>
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

        {/* Contenu principal */}
        {!loading && !error && (
          <>
            {/* Sélecteur d'année */}
            {availableYears.length > 0 && (
              <div className="mb-8 flex gap-4 items-center">
                <label className="text-gray-300 font-medium">Année :</label>
                <select
                  value={selectedYear}
                  onChange={(e) => setSelectedYear(parseInt(e.target.value))}
                  className="bg-[#1a1a2e] border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                  style={{ colorScheme: 'dark' }}
                >
                  {availableYears.map(year => (
                    <option key={year} value={year}>{year}</option>
                  ))}
                </select>
              </div>
            )}

            {transactions.length === 0 ? (
              <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-12 text-center">
                <p className="text-gray-400 text-lg">Aucune transaction pour afficher les statistiques.</p>
              </div>
            ) : (
              <>
                {/* Statistiques annuelles */}
                <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-12">
                  <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 animate-fadeIn" style={{ animationDelay: '0ms' }}>
                    <p className="text-gray-400 text-sm uppercase tracking-wider mb-2">Revenus</p>
                    <p className="text-3xl font-bold text-emerald-400 font-[family-name:var(--font-playfair)]">
                      {formatMontant(annualStats.totalRevenus)}
                    </p>
                  </div>

                  <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 animate-fadeIn" style={{ animationDelay: '100ms' }}>
                    <p className="text-gray-400 text-sm uppercase tracking-wider mb-2">Dépenses</p>
                    <p className="text-3xl font-bold text-red-400 font-[family-name:var(--font-playfair)]">
                      {formatMontant(annualStats.totalDepenses)}
                    </p>
                  </div>

                  <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 animate-fadeIn" style={{ animationDelay: '200ms' }}>
                    <p className="text-gray-400 text-sm uppercase tracking-wider mb-2">Solde Net</p>
                    <p className={`text-3xl font-bold font-[family-name:var(--font-playfair)] ${annualStats.soldeNet >= 0 ? 'text-emerald-400' : 'text-red-400'}`}>
                      {formatMontant(annualStats.soldeNet)}
                    </p>
                  </div>

                  <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 animate-fadeIn" style={{ animationDelay: '300ms' }}>
                    <p className="text-gray-400 text-sm uppercase tracking-wider mb-2">Dépense moy./mois</p>
                    <p className="text-3xl font-bold text-orange-400 font-[family-name:var(--font-playfair)]">
                      {formatMontant(annualStats.avgMonthlyExpense)}
                    </p>
                  </div>
                </div>

                {/* Graphiques */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-12">
                  {/* Pie Chart - Dépenses par catégorie */}
                  {categoryExpensesData.length > 0 && (
                    <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-8">
                      <h2 className="text-2xl font-bold mb-6 font-[family-name:var(--font-playfair)]">
                        💰 Dépenses par Catégorie
                      </h2>
                      <ResponsiveContainer width="100%" height={300}>
                        <PieChart>
                          <Pie
                            data={categoryExpensesData}
                            cx="50%"
                            cy="50%"
                            labelLine={false}
                            label={({ name, value }: any) => `${name} ${formatMontant(value)}`}
                            outerRadius={80}
                            fill="#8884d8"
                            dataKey="value"
                          >
                            {categoryExpensesData.map((entry, index) => (
                              <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                            ))}
                          </Pie>
                          <Tooltip formatter={(value: any) => formatMontant(value)} />
                        </PieChart>
                      </ResponsiveContainer>
                    </div>
                  )}

                  {/* Statistiques par catégorie */}
                  {categoryExpensesData.length > 0 && (
                    <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-8">
                      <h2 className="text-2xl font-bold mb-6 font-[family-name:var(--font-playfair)]">
                        📋 Détails par Catégorie
                      </h2>
                      <div className="space-y-3 max-h-[300px] overflow-y-auto">
                        {categoryExpensesData.map((category, index) => (
                          <div key={category.name} className="flex items-center justify-between p-3 rounded-lg bg-white/5 border border-white/10">
                            <div className="flex items-center gap-3 flex-1">
                              <div
                                className="w-4 h-4 rounded-full"
                                style={{ backgroundColor: COLORS[index % COLORS.length] }}
                              ></div>
                              <span className="font-medium">{category.name}</span>
                            </div>
                            <span className="text-right font-bold">{formatMontant(category.value)}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>

                {/* Line Chart - Évolution mensuelle */}
                <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-8">
                  <h2 className="text-2xl font-bold mb-6 font-[family-name:var(--font-playfair)]">
                    📈 Évolution Mensuelle {selectedYear}
                  </h2>
                  <ResponsiveContainer width="100%" height={350}>
                    <LineChart data={monthlyEvolutionData}>
                      <CartesianGrid strokeDasharray="3 3" stroke="rgba(255, 255, 255, 0.1)" />
                      <XAxis dataKey="month" stroke="rgba(255, 255, 255, 0.5)" />
                      <YAxis stroke="rgba(255, 255, 255, 0.5)" />
                      <Tooltip
                        formatter={(value: any) => formatMontant(value)}
                        contentStyle={{ backgroundColor: '#1a1a2e', border: '1px solid rgba(255, 255, 255, 0.2)' }}
                      />
                      <Legend />
                      <Line
                        type="monotone"
                        dataKey="revenus"
                        stroke="#10b981"
                        strokeWidth={2}
                        dot={{ fill: '#10b981', r: 4 }}
                        activeDot={{ r: 6 }}
                        name="Revenus"
                      />
                      <Line
                        type="monotone"
                        dataKey="depenses"
                        stroke="#ef4444"
                        strokeWidth={2}
                        dot={{ fill: '#ef4444', r: 4 }}
                        activeDot={{ r: 6 }}
                        name="Dépenses"
                      />
                    </LineChart>
                  </ResponsiveContainer>
                </div>
              </>
            )}
          </>
        )}
      </div>
    </div>
  );
}
