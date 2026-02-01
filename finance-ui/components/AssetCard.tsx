'use client';

import { Asset, AssetType, getAssetTypeLabel, getAssetTypeIcon } from '@/types/asset';

interface AssetCardProps {
  asset: Asset;
  onEdit: (asset: Asset) => void;
  onDelete: (id: number) => void;
}

export default function AssetCard({ asset, onEdit, onDelete }: AssetCardProps) {
  // Format monétaire CAD (réutilise le même pattern que page.tsx)
  const formatMontant = (montant: number): string => {
    return new Intl.NumberFormat('fr-CA', {
      style: 'currency',
      currency: asset.currency || 'CAD'
    }).format(montant);
  };

  // Calcul du gain/perte si purchaseValue existe
  const gainLoss = asset.purchaseValue 
    ? asset.currentValue - asset.purchaseValue 
    : null;
  
  const gainLossPercent = asset.purchaseValue && asset.purchaseValue > 0
    ? ((gainLoss! / asset.purchaseValue) * 100).toFixed(1)
    : null;

  return (
    <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 hover:bg-white/10 transition-all duration-300 group">
      {/* Header */}
      <div className="flex justify-between items-start mb-4">
        <div className="flex items-center gap-3">
          <span className="text-3xl">{getAssetTypeIcon(asset.type)}</span>
          <div>
            <h3 className="text-lg font-semibold text-white">{asset.name}</h3>
            <p className="text-sm text-gray-400">{getAssetTypeLabel(asset.type)}</p>
          </div>
        </div>
        
        {/* Actions (visible au hover) */}
        <div className="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
          <button
            onClick={() => onEdit(asset)}
            className="p-2 hover:bg-white/10 rounded-lg transition-colors"
            title="Modifier"
          >
            <svg className="w-5 h-5 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
            </svg>
          </button>
          <button
            onClick={() => onDelete(asset.id)}
            className="p-2 hover:bg-red-500/20 rounded-lg transition-colors"
            title="Supprimer"
          >
            <svg className="w-5 h-5 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
          </button>
        </div>
      </div>

      {/* Valeur actuelle */}
      <div className="mb-3">
        <p className="text-sm text-gray-400 mb-1">Valeur actuelle</p>
        <p className="text-2xl font-bold text-white font-[family-name:var(--font-playfair)]">
          {formatMontant(asset.currentValue)}
        </p>
      </div>

      {/* Gain/Perte */}
      {gainLoss !== null && (
        <div className="flex items-center gap-2 mb-3">
          <span className={`text-sm font-medium ${gainLoss >= 0 ? 'text-emerald-400' : 'text-red-400'}`}>
            {gainLoss >= 0 ? '+' : ''}{formatMontant(gainLoss)}
          </span>
          {gainLossPercent && (
            <span className={`text-xs px-2 py-1 rounded-full ${
              gainLoss >= 0 ? 'bg-emerald-500/20 text-emerald-300' : 'bg-red-500/20 text-red-300'
            }`}>
              {gainLoss >= 0 ? '+' : ''}{gainLossPercent}%
            </span>
          )}
        </div>
      )}

      {/* Badges */}
      <div className="flex gap-2 flex-wrap">
        {asset.isLiquid && (
          <span className="inline-block bg-blue-500/20 text-blue-300 px-2 py-1 rounded-full text-xs">
            💧 Liquide
          </span>
        )}
        {asset.description && (
          <span className="inline-block bg-gray-500/20 text-gray-300 px-2 py-1 rounded-full text-xs">
            📝 Notes
          </span>
        )}
      </div>

      {/* Description (si présente) */}
      {asset.description && (
        <p className="text-sm text-gray-400 mt-3 line-clamp-2">
          {asset.description}
        </p>
      )}
    </div>
  );
}
