import { Liability, LiabilityType, LiabilityTypeLabels } from '@/types/liability';

interface LiabilityCardProps {
  liability: Liability;
  onEdit: (liability: Liability) => void;
  onDelete: (id: number) => void;
}

export default function LiabilityCard({ liability, onEdit, onDelete }: LiabilityCardProps) {
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('fr-CA', {
      style: 'currency',
      currency: liability.currency || 'CAD'
    }).format(amount);
  };

  const getCreditUtilization = () => {
    if (liability.type === LiabilityType.CreditCard && liability.creditLimit) {
      return ((liability.currentBalance / liability.creditLimit) * 100).toFixed(1);
    }
    return null;
  };

  const getTypeIcon = () => {
    switch (liability.type) {
      case LiabilityType.CreditCard:
        return '💳';
      case LiabilityType.Mortgage:
        return '🏠';
      case LiabilityType.CarLoan:
        return '🚗';
      case LiabilityType.PersonalLoan:
        return '💵';
      case LiabilityType.StudentLoan:
        return '🎓';
      default:
        return '📋';
    }
  };

  const utilization = getCreditUtilization();

  return (
    <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow">
      <div className="flex justify-between items-start mb-4">
        <div className="flex items-center gap-3">
          <span className="text-3xl">{getTypeIcon()}</span>
          <div>
            <h3 className="text-lg font-semibold text-gray-800">{liability.name}</h3>
            <p className="text-sm text-gray-500">{LiabilityTypeLabels[liability.type]}</p>
          </div>
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => onEdit(liability)}
            className="text-blue-600 hover:text-blue-800 p-2"
            title="Modifier"
          >
            ✏️
          </button>
          <button
            onClick={() => onDelete(liability.id)}
            className="text-red-600 hover:text-red-800 p-2"
            title="Supprimer"
          >
            🗑️
          </button>
        </div>
      </div>

      <div className="space-y-2">
        <div className="flex justify-between items-center">
          <span className="text-gray-600">Dette actuelle:</span>
          <span className="text-xl font-bold text-red-600">
            {formatCurrency(liability.currentBalance)}
          </span>
        </div>

        {liability.type === LiabilityType.CreditCard && liability.creditLimit && (
          <>
            <div className="flex justify-between items-center text-sm">
              <span className="text-gray-600">Limite:</span>
              <span className="text-gray-800">{formatCurrency(liability.creditLimit)}</span>
            </div>
            {utilization && (
              <div className="mt-2">
                <div className="flex justify-between text-sm mb-1">
                  <span className="text-gray-600">Utilisation:</span>
                  <span className={`font-semibold ${parseFloat(utilization) > 70 ? 'text-red-600' : 'text-gray-800'}`}>
                    {utilization}%
                  </span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className={`h-2 rounded-full ${
                      parseFloat(utilization) > 70 ? 'bg-red-500' : 'bg-blue-500'
                    }`}
                    style={{ width: `${Math.min(parseFloat(utilization), 100)}%` }}
                  />
                </div>
              </div>
            )}
          </>
        )}

        {liability.interestRate && (
          <div className="flex justify-between items-center text-sm">
            <span className="text-gray-600">Taux d'intérêt:</span>
            <span className="text-gray-800">{liability.interestRate}%</span>
          </div>
        )}

        {liability.monthlyPayment && (
          <div className="flex justify-between items-center text-sm">
            <span className="text-gray-600">Mensualité:</span>
            <span className="text-gray-800">{formatCurrency(liability.monthlyPayment)}</span>
          </div>
        )}

        {liability.maturityDate && (
          <div className="flex justify-between items-center text-sm">
            <span className="text-gray-600">Échéance:</span>
            <span className="text-gray-800">
              {new Date(liability.maturityDate).toLocaleDateString('fr-CA')}
            </span>
          </div>
        )}

        {liability.description && (
          <div className="mt-3 pt-3 border-t border-gray-200">
            <p className="text-sm text-gray-600">{liability.description}</p>
          </div>
        )}
      </div>
    </div>
  );
}
