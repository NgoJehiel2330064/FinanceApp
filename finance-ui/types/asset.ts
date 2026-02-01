// ===========================
// INTERFACE TYPESCRIPT - ASSET
// ===========================
// Cette interface DOIT correspondre EXACTEMENT au modèle C# Asset.cs
// SOURCE DE VÉRITÉ : FinanceApp/Models/Asset.cs

export interface Asset {
  id: number;
  name: string;
  type: AssetType;
  currentValue: number;
  purchaseValue: number | null;
  purchaseDate: string | null;
  currency: string;
  description: string | null;
  isLiquid: boolean;
  lastUpdated: string;
  createdAt: string;
}

// Enum AssetType - DOIT correspondre EXACTEMENT à l'enum C#
export enum AssetType {
  BankAccount = 0,
  Investment = 1,
  RealEstate = 2,
  Cryptocurrency = 3,
  Vehicle = 4,
  Other = 5
}

// Helper pour obtenir le label français de chaque type
export const getAssetTypeLabel = (type: AssetType): string => {
  const labels: Record<AssetType, string> = {
    [AssetType.BankAccount]: 'Compte Bancaire',
    [AssetType.Investment]: 'Investissement',
    [AssetType.RealEstate]: 'Immobilier',
    [AssetType.Cryptocurrency]: 'Crypto-monnaie',
    [AssetType.Vehicle]: 'Véhicule',
    [AssetType.Other]: 'Autre'
  };
  return labels[type];
};

// Helper pour l'icône emoji de chaque type
export const getAssetTypeIcon = (type: AssetType): string => {
  const icons: Record<AssetType, string> = {
    [AssetType.BankAccount]: '🏦',
    [AssetType.Investment]: '📈',
    [AssetType.RealEstate]: '🏠',
    [AssetType.Cryptocurrency]: '₿',
    [AssetType.Vehicle]: '🚗',
    [AssetType.Other]: '💼'
  };
  return icons[type];
};

// Interface pour le formulaire de création/édition
export interface AssetFormData {
  name: string;
  type: AssetType;
  currentValue: number;
  purchaseValue: number | null;
  purchaseDate: string | null;
  currency: string;
  description: string | null;
  isLiquid: boolean;
}
