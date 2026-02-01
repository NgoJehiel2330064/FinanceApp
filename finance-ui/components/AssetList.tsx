'use client';

import { Asset } from '@/types/asset';
import AssetCard from './AssetCard';

interface AssetListProps {
  assets: Asset[];
  totalValue: number;
  isLoading: boolean;
  error: string | null;
  onEdit: (asset: Asset) => void;
  onDelete: (id: number) => void;
  onAddNew: () => void;
}

export default function AssetList({ 
  assets, 
  totalValue, 
  isLoading, 
  error, 
  onEdit, 
  onDelete,
  onAddNew 
}: AssetListProps) {
  const formatMontant = (montant: number): string => {
    return new Intl.NumberFormat('fr-CA', {
      style: 'currency',
      currency: 'CAD'
    }).format(montant);
  };

  // État de chargement
  if (isLoading) {
    return (
      <div className="flex justify-center items-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-white/50"></div>
      </div>
    );
  }

  // État d'erreur
  if (error) {
    return (
      <div className="backdrop-blur-xl bg-red-500/10 border border-red-500/20 rounded-2xl p-8 text-center">
        <svg className="w-12 h-12 text-red-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <p className="text-red-300 mb-4">{error}</p>
        <button
          onClick={() => window.location.reload()}
          className="px-4 py-2 bg-red-500/20 hover:bg-red-500/30 text-red-300 rounded-lg transition-colors"
        >
          Réessayer
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Carte récapitulatif patrimoine total */}
      <div className="backdrop-blur-xl bg-gradient-to-br from-purple-500/20 to-pink-500/20 border border-white/10 rounded-2xl p-6">
        <div className="flex justify-between items-start">
          <div>
            <p className="text-gray-300 text-sm mb-2">Patrimoine Total</p>
            <p className="text-4xl font-bold text-white font-[family-name:var(--font-playfair)]">
              {formatMontant(totalValue)}
            </p>
            <p className="text-gray-400 text-sm mt-2">
              {assets.length} actif{assets.length !== 1 ? 's' : ''}
            </p>
          </div>
          <button
            onClick={onAddNew}
            className="px-4 py-2 bg-white/10 hover:bg-white/20 text-white rounded-lg transition-all duration-300 flex items-center gap-2"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            Ajouter un actif
          </button>
        </div>
      </div>

      {/* Liste des actifs */}
      {assets.length === 0 ? (
        <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-12 text-center">
          <div className="text-6xl mb-4">💎</div>
          <p className="text-gray-300 text-lg mb-2">Aucun actif enregistré</p>
          <p className="text-gray-400 text-sm mb-6">
            Commencez à suivre votre patrimoine en ajoutant vos premiers actifs
          </p>
          <button
            onClick={onAddNew}
            className="px-6 py-3 bg-white/10 hover:bg-white/20 text-white rounded-lg transition-all duration-300"
          >
            Ajouter mon premier actif
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {assets.map((asset, index) => (
            <div
              key={asset.id}
              className="animate-fadeIn"
              style={{ animationDelay: `${index * 100}ms` }}
            >
              <AssetCard
                asset={asset}
                onEdit={onEdit}
                onDelete={onDelete}
              />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
