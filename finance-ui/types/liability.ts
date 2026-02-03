export interface Liability {
  id: number;
  userId: number;
  name: string;
  type: LiabilityType;
  currentBalance: number;
  creditLimit?: number;
  interestRate?: number;
  monthlyPayment?: number;
  maturityDate?: string;
  currency: string;
  description?: string;
  lastUpdated: string;
  createdAt: string;
}

export interface LiabilityFormData {
  name: string;
  type: LiabilityType;
  currentBalance: number;
  creditLimit?: number;
  interestRate?: number;
  monthlyPayment?: number;
  maturityDate?: string;
  currency: string;
  description?: string;
}

export enum LiabilityType {
  CreditCard = 0,
  Mortgage = 1,
  CarLoan = 2,
  PersonalLoan = 3,
  StudentLoan = 4,
  Other = 5
}

export const LiabilityTypeLabels: Record<LiabilityType, string> = {
  [LiabilityType.CreditCard]: 'Carte de crédit',
  [LiabilityType.Mortgage]: 'Prêt hypothécaire',
  [LiabilityType.CarLoan]: 'Prêt auto',
  [LiabilityType.PersonalLoan]: 'Prêt personnel',
  [LiabilityType.StudentLoan]: 'Prêt étudiant',
  [LiabilityType.Other]: 'Autre'
};

export interface NetWorthSummary {
  userId: number;
  totalAssets: number;
  totalLiabilities: number;
  netWorth: number;
  liquidAssets: number;
  transactionBalance: number;  // Solde net des transactions (peut être négatif)
  creditUtilization: number;
  assetBreakdown: Record<string, number>;
  liabilityBreakdown: Record<string, number>;
  lastUpdated: string;
}
