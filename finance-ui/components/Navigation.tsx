'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';

interface NavItem {
  href: string;
  label: string;
  icon: string;
}

const navItems: NavItem[] = [
  { href: '/', label: 'Accueil', icon: '🏠' },
  { href: '/transactions', label: 'Transactions', icon: '💳' },
  { href: '/patrimoine', label: 'Patrimoine', icon: '💎' },
];

export default function Navigation() {
  const pathname = usePathname();

  // Ne pas afficher la navigation sur les pages d'authentification
  if (pathname === '/connexion' || pathname === '/inscription') {
    return null;
  }

  return (
    <nav className="fixed top-0 left-0 right-0 z-50 backdrop-blur-xl bg-[#0f0f1e]/80 border-b border-white/10">
      <div className="max-w-7xl mx-auto px-6">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <Link href="/" className="flex items-center gap-3 group">
            <span className="text-2xl">💰</span>
            <span className="text-xl font-bold text-white font-[family-name:var(--font-playfair)] group-hover:text-violet-400 transition-colors">
              FinanceApp
            </span>
          </Link>

          {/* Navigation Links */}
          <div className="flex items-center gap-2">
            {navItems.map((item) => {
              const isActive = pathname === item.href;
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-2 px-4 py-2 rounded-xl transition-all duration-300 ${
                    isActive
                      ? 'bg-white/10 text-white'
                      : 'text-gray-400 hover:text-white hover:bg-white/5'
                  }`}
                >
                  <span>{item.icon}</span>
                  <span className="hidden sm:inline">{item.label}</span>
                </Link>
              );
            })}
          </div>

          {/* User Menu */}
          <div className="flex items-center gap-4">
            <Link
              href="/connexion"
              className="px-4 py-2 text-gray-400 hover:text-white transition-colors"
            >
              Déconnexion
            </Link>
          </div>
        </div>
      </div>
    </nav>
  );
}
