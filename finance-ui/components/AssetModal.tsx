'use client';

import { useState, useEffect } from 'react';
import { Asset, AssetType, AssetFormData, getAssetTypeLabel } from '@/types/asset';

interface AssetModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (assetData: AssetFormData) => Promise<void>;
  editingAsset: Asset | null;
}

export default function AssetModal({ isOpen, onClose, onSubmit, editingAsset }: AssetModalProps) {
  const [formData, setFormData] = useState<AssetFormData>({
    name: '',
    type: AssetType.BankAccount,
    currentValue: 0,
    purchaseValue: null,
    purchaseDate: null,
    currency: 'CAD',
    description: null,
    isLiquid: true
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Remplir le formulaire si on édite un actif existant
  useEffect(() => {
    if (editingAsset) {
      setFormData({
        name: editingAsset.name,
        type: editingAsset.type,
        currentValue: editingAsset.currentValue,
        purchaseValue: editingAsset.purchaseValue,
        purchaseDate: editingAsset.purchaseDate,
        currency: editingAsset.currency,
        description: editingAsset.description,
        isLiquid: editingAsset.isLiquid
      });
    } else {
      // Réinitialiser le formulaire
      setFormData({
        name: '',
        type: AssetType.BankAccount,
        currentValue: 0,
        purchaseValue: null,
        purchaseDate: null,
        currency: 'CAD',
        description: null,
        isLiquid: true
      });
    }
    setError(null);
  }, [editingAsset, isOpen]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await onSubmit(formData);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors de l\'enregistrement');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fadeIn">
      <div className="backdrop-blur-xl bg-[#1a1a2e] border border-white/10 rounded-2xl p-8 w-full max-w-2xl max-h-[90vh] overflow-y-auto animate-scaleIn">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold text-white">
            {editingAsset ? 'Modifier l\'actif' : 'Nouvel actif'}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-white transition-colors"
            disabled={isSubmitting}
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Formulaire */}
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Nom */}
          <div>
            <label className="block text-gray-300 text-sm mb-2">Nom de l'actif *</label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              placeholder="Ex: Compte épargne CIBC"
              required
              className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-white/20"
            />
          </div>

          {/* Type */}
          <div>
            <label className="block text-gray-300 text-sm mb-2">Type d'actif *</label>
            <select
              value={formData.type}
              onChange={(e) => setFormData({ ...formData, type: Number(e.target.value) as AssetType })}
              required
              className="w-full px-4 py-3 bg-[#1a1a2e] border border-white/10 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-white/20"
            >
              {Object.values(AssetType)
                .filter((v) => typeof v === 'number')
                .map((type) => (
                  <option key={type} value={type} className="bg-[#1a1a2e] text-white">
                    {getAssetTypeLabel(type as AssetType)}
                  </option>
                ))}
            </select>
          </div>

          {/* Valeur actuelle */}
          <div>
            <label className="block text-gray-300 text-sm mb-2">Valeur actuelle (CAD) *</label>
            <input
              type="number"
              step="0.01"
              value={formData.currentValue}
              onChange={(e) => setFormData({ ...formData, currentValue: parseFloat(e.target.value) || 0 })}
              placeholder="0.00"
              required
              className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-white/20"
            />
          </div>

          {/* Valeur d'achat (optionnel) */}
          <div>
            <label className="block text-gray-300 text-sm mb-2">Valeur d'achat (optionnel)</label>
            <input
              type="number"
              step="0.01"
              value={formData.purchaseValue ?? ''}
              onChange={(e) => setFormData({ 
                ...formData, 
                purchaseValue: e.target.value ? parseFloat(e.target.value) : null 
              })}
              placeholder="Laisser vide si inconnu"
              className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-white/20"
            />
          </div>

          {/* Date d'achat (optionnel) */}
          <div>
            <label className="block text-gray-300 text-sm mb-2">Date d'achat (optionnel)</label>
            <input
              type="date"
              value={formData.purchaseDate ?? ''}
              onChange={(e) => setFormData({ 
                ...formData, 
                purchaseDate: e.target.value || null 
              })}
              className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-white/20"
            />
          </div>

          {/* Description (optionnel) */}
          <div>
            <label className="block text-gray-300 text-sm mb-2">Description (optionnel)</label>
            <textarea
              value={formData.description ?? ''}
              onChange={(e) => setFormData({ 
                ...formData, 
                description: e.target.value || null 
              })}
              placeholder="Notes, détails supplémentaires..."
              rows={3}
              className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-white/20 resize-none"
            />
          </div>

          {/* Liquidité */}
          <div className="flex items-center gap-3">
            <input
              type="checkbox"
              id="isLiquid"
              checked={formData.isLiquid}
              onChange={(e) => setFormData({ ...formData, isLiquid: e.target.checked })}
              className="w-5 h-5 rounded bg-white/5 border-white/10 text-blue-500 focus:ring-2 focus:ring-white/20"
            />
            <label htmlFor="isLiquid" className="text-gray-300 text-sm cursor-pointer">
              Actif liquide (facilement convertible en cash)
            </label>
          </div>

          {/* Message d'erreur */}
          {error && (
            <div className="p-4 bg-red-500/10 border border-red-500/20 rounded-lg text-red-300 text-sm">
              {error}
            </div>
          )}

          {/* Boutons d'action */}
          <div className="flex gap-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="flex-1 px-6 py-3 bg-white/5 hover:bg-white/10 text-gray-300 rounded-lg transition-all duration-300 disabled:opacity-50"
            >
              Annuler
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex-1 px-6 py-3 bg-white/10 hover:bg-white/20 text-white rounded-lg transition-all duration-300 disabled:opacity-50 flex items-center justify-center gap-2"
            >
              {isSubmitting ? (
                <>
                  <div className="animate-spin rounded-full h-5 w-5 border-t-2 border-b-2 border-white"></div>
                  Enregistrement...
                </>
              ) : (
                editingAsset ? 'Mettre à jour' : 'Ajouter l\'actif'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
