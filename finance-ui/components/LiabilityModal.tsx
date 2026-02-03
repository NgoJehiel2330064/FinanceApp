import { useState, useEffect } from 'react';
import { Liability, LiabilityFormData, LiabilityType, LiabilityTypeLabels } from '@/types/liability';

interface LiabilityModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: LiabilityFormData) => Promise<void>;
  liability?: Liability | null;
}

export default function LiabilityModal({ isOpen, onClose, onSubmit, liability }: LiabilityModalProps) {
  const [formData, setFormData] = useState<LiabilityFormData>({
    name: '',
    type: LiabilityType.CreditCard,
    currentBalance: 0,
    creditLimit: undefined,
    interestRate: undefined,
    monthlyPayment: undefined,
    maturityDate: undefined,
    currency: 'CAD',
    description: ''
  });

  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (liability) {
      setFormData({
        name: liability.name,
        type: liability.type,
        currentBalance: liability.currentBalance,
        creditLimit: liability.creditLimit,
        interestRate: liability.interestRate,
        monthlyPayment: liability.monthlyPayment,
        maturityDate: liability.maturityDate ? liability.maturityDate.split('T')[0] : undefined,
        currency: liability.currency,
        description: liability.description || ''
      });
    } else {
      setFormData({
        name: '',
        type: LiabilityType.CreditCard,
        currentBalance: 0,
        creditLimit: undefined,
        interestRate: undefined,
        monthlyPayment: undefined,
        maturityDate: undefined,
        currency: 'CAD',
        description: ''
      });
    }
  }, [liability, isOpen]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      await onSubmit(formData);
      onClose();
    } catch (error) {
      console.error('Erreur lors de la soumission:', error);
      alert('Erreur lors de la soumission du formulaire');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen) return null;

  const isCreditCard = formData.type === LiabilityType.CreditCard;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fadeIn">
      <div className="backdrop-blur-xl bg-[#1a1a2e] border border-white/10 rounded-2xl p-8 w-full max-w-2xl max-h-[90vh] overflow-y-auto animate-scaleIn">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold text-white">
            {liability ? 'Modifier le passif' : 'Nouveau passif'}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-white text-3xl transition-colors"
          >
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-400 mb-1">
                Nom *
              </label>
              <input
                type="text"
                required
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Ex: Visa Premier, Prêt Auto..."
                className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white placeholder-gray-500 focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-400 mb-1">
                Type *
              </label>
              <select
                required
                value={formData.type}
                onChange={(e) => setFormData({ ...formData, type: parseInt(e.target.value) as LiabilityType })}
                className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all [&>option]:bg-[#1a1a2e] [&>option]:text-white"
              >
                  {Object.entries(LiabilityTypeLabels).map(([key, label]) => (
                    <option key={key} value={key} className="bg-[#1a1a2e] text-white py-2">
                      {label}
                    </option>
                  ))}
                </select>
              </div>

            <div>
              <label className="block text-sm font-medium text-gray-400 mb-1">
                Dette actuelle * (CAD)
              </label>
              <input
                type="number"
                required
                step="0.01"
                min="0"
                value={formData.currentBalance || ''}
                onChange={(e) => setFormData({ ...formData, currentBalance: e.target.value ? parseFloat(e.target.value) : 0 })}
                className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white placeholder-gray-500 focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all"
              />
            </div>

            {isCreditCard && (
              <div>
                <label className="block text-sm font-medium text-gray-400 mb-1">
                  Limite de crédit (CAD)
                </label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  value={formData.creditLimit || ''}
                  onChange={(e) => setFormData({ ...formData, creditLimit: e.target.value ? parseFloat(e.target.value) : undefined })}
                  className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white placeholder-gray-500 focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all"
                />
                </div>
              )}

            <div>
              <label className="block text-sm font-medium text-gray-400 mb-1">
                Taux d'intérêt (%)
              </label>
              <input
                type="number"
                step="0.01"
                min="0"
                max="100"
                value={formData.interestRate || ''}
                onChange={(e) => setFormData({ ...formData, interestRate: e.target.value ? parseFloat(e.target.value) : undefined })}
                className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white placeholder-gray-500 focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-400 mb-1">
                Mensualité (CAD)
              </label>
              <input
                type="number"
                step="0.01"
                min="0"
                value={formData.monthlyPayment || ''}
                onChange={(e) => setFormData({ ...formData, monthlyPayment: e.target.value ? parseFloat(e.target.value) : undefined })}
                className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white placeholder-gray-500 focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all"
              />
            </div>

            {!isCreditCard && (
              <div>
                <label className="block text-sm font-medium text-gray-400 mb-1">
                  Date d'échéance
                </label>
                <input
                  type="date"
                  value={formData.maturityDate || ''}
                  onChange={(e) => setFormData({ ...formData, maturityDate: e.target.value || undefined })}
                  className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all"
                />
              </div>
            )}

            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-400 mb-1">
                Description
              </label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                rows={3}
                placeholder="Notes additionnelles..."
                className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white placeholder-gray-500 focus:ring-2 focus:ring-red-500 focus:border-transparent transition-all resize-none"
              />
            </div>
          </div>

          <div className="flex gap-3 mt-6">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-6 py-3 bg-white/5 border border-white/10 text-white rounded-xl hover:bg-white/10 transition-all font-semibold"
              disabled={isSubmitting}
            >
              Annuler
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex-1 px-6 py-3 bg-gradient-to-r from-red-600 to-red-700 text-white rounded-xl hover:from-red-700 hover:to-red-800 disabled:opacity-50 disabled:cursor-not-allowed transition-all font-semibold"
            >
              {isSubmitting ? 'Enregistrement...' : liability ? 'Modifier' : 'Ajouter'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
