'use client';

import { useState, useEffect } from 'react';
import { API_CONFIG, getApiUrl } from '@/lib/api-config';
import { Asset, AssetFormData } from '@/types/asset';
import AssetList from '@/components/AssetList';
import AssetModal from '@/components/AssetModal';
import AIPortfolioInsights from '@/components/AIPortfolioInsights';

export default function PatrimoinePage() {
  // STATE MANAGEMENT - PATRIMOINE (ASSETS)
  const [assets, setAssets] = useState<Asset[]>([]);
  const [totalAssetValue, setTotalAssetValue] = useState<number>(0);
  const [assetsLoading, setAssetsLoading] = useState<boolean>(true);
  const [assetsError, setAssetsError] = useState<string | null>(null);
  const [showAssetModal, setShowAssetModal] = useState<boolean>(false);
  const [editingAsset, setEditingAsset] = useState<Asset | null>(null);
  const [mounted, setMounted] = useState<boolean>(false);

  // RÉCUPÉRATION DES ACTIFS
  useEffect(() => {
    setMounted(true);

    const fetchAssets = async () => {
      try {
        setAssetsLoading(true);
        setAssetsError(null);
        
        const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.ASSETS));
        
        if (!response.ok) {
          throw new Error(`Erreur HTTP ${response.status}`);
        }
        
        const data: Asset[] = await response.json();
        setAssets(data);
        
        const totalResponse = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.ASSETS_TOTAL_VALUE));
        if (totalResponse.ok) {
          const totalData = await totalResponse.json();
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

  // HANDLERS
  const handleAssetSubmit = async (assetData: AssetFormData) => {
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

      // Recharger les actifs depuis l'API
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
      throw err;
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

      const assetsResponse = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.ASSETS));
      const updatedAssets: Asset[] = await assetsResponse.json();
      setAssets(updatedAssets);

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

  return (
    <div className="min-h-screen bg-[#0f0f1e] text-white font-[family-name:var(--font-inter)]">
      {/* Cercles flous */}
      <div className="fixed top-0 left-0 w-full h-full overflow-hidden pointer-events-none">
        <div className="absolute top-[-20%] left-[-10%] w-[600px] h-[600px] bg-purple-600/30 rounded-full blur-[120px]"></div>
        <div className="absolute bottom-[-20%] right-[-10%] w-[700px] h-[700px] bg-pink-600/30 rounded-full blur-[120px]"></div>
      </div>

      <div className="relative z-10 max-w-7xl mx-auto px-6 py-12">
        {/* En-tête */}
        <header className={`mb-12 transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
          <h1 className="text-5xl font-bold mb-2 font-[family-name:var(--font-playfair)]">
            💎 Mon Patrimoine
          </h1>
          <p className="text-gray-400 text-lg">Gérez et suivez l'évolution de vos actifs</p>
        </header>

        {/* Liste des actifs */}
        <section className="mb-12">
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

        {/* Insights IA */}
        {assets.length > 0 && (
          <section className="mt-12">
            <AIPortfolioInsights assetCount={assets.length} />
          </section>
        )}
      </div>

      {/* Modal Asset */}
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
