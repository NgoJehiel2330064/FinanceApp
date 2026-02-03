'use client';

import { useCallback, useEffect, useMemo, useState } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import { API_CONFIG, getApiUrl } from '@/lib/api-config';
import { getAuthHeaders } from '@/lib/cookie-utils';

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  timestamp: string;
}

export default function AIAssistantChat() {
  const pathname = usePathname();
  const router = useRouter();
  const [isOpen, setIsOpen] = useState(false);
  const [mounted, setMounted] = useState(false);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [context, setContext] = useState('');
  const [contextUpdatedAt, setContextUpdatedAt] = useState('');

  const userId = useMemo(() => {
    if (typeof window === 'undefined') return null;
    const userStr = sessionStorage.getItem('user');
    if (!userStr) return null;
    try {
      const user = JSON.parse(userStr);
      return user?.id ?? null;
    } catch {
      return null;
    }
  }, []);

  useEffect(() => {
    setMounted(true);
  }, []);

  const buildTransactionsContext = async () => {
    if (!userId) return '';
    const res = await fetch(
      `${getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS)}?userId=${userId}`,
      { headers: getAuthHeaders() }
    );

    if (!res.ok) return '';
    const data: Array<{ amount: number; type: number; category: string; description: string; date: string }> = await res.json();
    if (data.length === 0) return 'Aucune transaction enregistrée.';

    const totalIncome = data.filter(t => t.type === 1).reduce((sum, t) => sum + Math.abs(t.amount), 0);
    const totalExpense = data.filter(t => t.type !== 1).reduce((sum, t) => sum + Math.abs(t.amount), 0);
    const topCategory = data
      .filter(t => t.type !== 1)
      .reduce<Record<string, number>>((acc, t) => {
        const key = t.category || 'Autre';
        acc[key] = (acc[key] || 0) + Math.abs(t.amount);
        return acc;
      }, {});

    const topCategoryEntry = Object.entries(topCategory).sort((a, b) => b[1] - a[1])[0];
    const recent = data.slice(0, 5)
      .map(t => `- ${t.description} | ${t.category} | ${Math.abs(t.amount)} | ${new Date(t.date).toLocaleDateString('fr-CA')}`)
      .join('\n');

    return [
      `Page: Transactions`,
      `Total transactions: ${data.length}`,
      `Total revenus: ${totalIncome.toFixed(2)}`,
      `Total dépenses: ${totalExpense.toFixed(2)}`,
      `Catégorie principale: ${topCategoryEntry ? `${topCategoryEntry[0]} (${topCategoryEntry[1].toFixed(2)})` : 'N/A'}`,
      `Transactions récentes:\n${recent}`
    ].join('\n');
  };

  const buildAnalyticsContext = async () => {
    if (!userId) return '';

    try {
      const [patternsRes, anomaliesRes, recsRes] = await Promise.all([
        fetch(`${getApiUrl(API_CONFIG.ENDPOINTS.FINANCE)}/spending-patterns?userId=${userId}`, { headers: getAuthHeaders() }),
        fetch(`${getApiUrl(API_CONFIG.ENDPOINTS.FINANCE)}/smart-anomalies?userId=${userId}`, { headers: getAuthHeaders() }),
        fetch(`${getApiUrl(API_CONFIG.ENDPOINTS.FINANCE)}/recommendations?userId=${userId}`, { headers: getAuthHeaders() })
      ]);

      if (!patternsRes.ok || !anomaliesRes.ok || !recsRes.ok) return '';

      const patterns = await patternsRes.json();
      const anomalies = await anomaliesRes.json();
      const recs = await recsRes.json();

      return [
        `Page: Statistiques`,
        `Total dépenses 3 mois: ${patterns.totalSpent ?? 0}`,
        `Moyenne mensuelle: ${patterns.averageMonthlySpending ?? 0}`,
        `Volatilité: ${patterns.spendingVariance ?? 0}%`,
        `Anomalies: ${anomalies.totalAnomalies ?? 0} (High: ${anomalies.highSeverityCount ?? 0})`,
        `Recommandations: ${(recs.recommendations || []).length}`
      ].join('\n');
    } catch {
      return '';
    }
  };

  const refreshContext = useCallback(async () => {
    if (!userId) return;

    try {
      let nextContext = '';
      if (pathname.startsWith('/transactions')) {
        nextContext = await buildTransactionsContext();
      } else if (pathname.startsWith('/statistiques')) {
        nextContext = await buildAnalyticsContext();
      }

      if (!nextContext) {
        nextContext = `Page: ${pathname}`;
      }

      setContext(nextContext);
      setContextUpdatedAt(new Date().toLocaleString('fr-CA'));
    } catch {
      setContext(`Page: ${pathname}`);
    }
  }, [pathname, userId]);

  useEffect(() => {
    if (!isOpen) return;

    refreshContext();
    const intervalId = window.setInterval(refreshContext, 15000);
    return () => window.clearInterval(intervalId);
  }, [isOpen, refreshContext]);

  const sendMessage = async (message: string) => {
    if (!userId || !message.trim()) return;

    const newMessage: ChatMessage = {
      role: 'user',
      content: message,
      timestamp: new Date().toLocaleTimeString('fr-CA')
    };

    setMessages(prev => [...prev, newMessage]);
    setInput('');
    setLoading(true);

    try {
      await refreshContext();

      const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.FINANCE_CHAT), {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
          userId,
          message,
          context,
          page: pathname
        })
      });

      if (!response.ok) {
        throw new Error(`Erreur API ${response.status}`);
      }

      const data = await response.json();
      const replyContent = data.reply || "Je n'ai pas pu répondre pour le moment.";
      
      // Détection de redirection dans la réponse (format: [PageName](/path))
      const redirectMatch = replyContent.match(/\[([^\]]+)\]\((\/[^)]+)\)/);
      
      const reply: ChatMessage = {
        role: 'assistant',
        content: replyContent,
        timestamp: new Date().toLocaleTimeString('fr-CA')
      };

      setMessages(prev => [...prev, reply]);
      
      // Si une redirection est détectée, proposer de naviguer
      if (redirectMatch) {
        const [, pageName, path] = redirectMatch;
        // Délai pour laisser l'utilisateur lire le message avant redirection
        setTimeout(() => {
          if (confirm(`Veux-tu aller sur la page ${pageName} ?`)) {
            router.push(path);
            setIsOpen(false);
          }
        }, 1000);
      }
    } catch (error) {
      const reply: ChatMessage = {
        role: 'assistant',
        content: 'Je rencontre un problème pour répondre. Réessaie dans quelques secondes.',
        timestamp: new Date().toLocaleTimeString('fr-CA')
      };

      setMessages(prev => [...prev, reply]);
      console.error('Erreur chat IA:', error);
    } finally {
      setLoading(false);
    }
  };

  if (!mounted || !userId) {
    return null;
  }

  return (
    <div className="fixed bottom-6 right-6 z-50">
      {isOpen && (
        <div className="mb-4 w-80 sm:w-96 rounded-2xl border border-white/10 bg-[#141426]/95 backdrop-blur-xl shadow-2xl">
          <div className="flex items-center justify-between px-4 py-3 border-b border-white/10">
            <div>
              <div className="text-sm font-semibold text-white">Assistant IA</div>
              <div className="text-xs text-gray-400">Contexte mis à jour : {contextUpdatedAt || '...'}</div>
            </div>
            <button
              onClick={() => setIsOpen(false)}
              className="text-gray-400 hover:text-white transition"
              aria-label="Fermer"
            >
              ✕
            </button>
          </div>

          <div className="px-4 py-3 max-h-80 overflow-y-auto space-y-3">
            {messages.length === 0 && (
              <div className="text-sm text-gray-400">
                Pose une question sur tes finances ou demande une recommandation personnalisée.
              </div>
            )}

            {messages.map((msg, index) => (
              <div key={`${msg.timestamp}-${index}`} className={`text-sm ${msg.role === 'user' ? 'text-right' : 'text-left'}`}>
                <div className={`inline-block px-3 py-2 rounded-xl ${msg.role === 'user' ? 'bg-violet-500/70 text-white' : 'bg-white/10 text-gray-200'}`}>
                  {msg.content}
                </div>
                <div className="mt-1 text-[10px] text-gray-500">{msg.timestamp}</div>
              </div>
            ))}

            {loading && (
              <div className="text-sm text-gray-400">Réponse en cours...</div>
            )}
          </div>

          <div className="px-4 py-3 border-t border-white/10">
            <div className="flex gap-2 mb-2">
              <button
                onClick={() => sendMessage('Donne-moi 3 recommandations personnalisées.')}
                className="text-xs px-2 py-1 rounded-lg bg-white/10 text-gray-200 hover:bg-white/20"
              >
                Recommandations
              </button>
              <button
                onClick={() => sendMessage('Analyse mes transactions récentes et indique un point d’attention.')}
                className="text-xs px-2 py-1 rounded-lg bg-white/10 text-gray-200 hover:bg-white/20"
              >
                Analyse rapide
              </button>
            </div>
            <form
              onSubmit={(e) => {
                e.preventDefault();
                sendMessage(input);
              }}
              className="flex gap-2"
            >
              <input
                value={input}
                onChange={(e) => setInput(e.target.value)}
                placeholder="Écris ta question..."
                className="flex-1 rounded-lg bg-white/5 border border-white/10 px-3 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-violet-400"
              />
              <button
                type="submit"
                disabled={loading || !input.trim()}
                className="px-3 py-2 rounded-lg bg-violet-500 text-white text-sm disabled:opacity-50"
              >
                Envoyer
              </button>
            </form>
          </div>
        </div>
      )}

      <button
        onClick={() => setIsOpen(!isOpen)}
        className="w-14 h-14 rounded-full bg-violet-500 text-white shadow-lg hover:scale-105 transition flex items-center justify-center"
        aria-label="Ouvrir le chat IA"
      >
        💬
      </button>
    </div>
  );
}
