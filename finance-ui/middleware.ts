import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// Routes protégées qui nécessitent une authentification
const protectedRoutes = [
  '/',
  '/transactions',
  '/statistiques',
  '/patrimoine',
  '/profil',
];

// Routes publiques (pas de redirection)
const publicRoutes = ['/connexion', '/inscription'];

export function middleware(request: NextRequest) {
  const pathname = request.nextUrl.pathname;

  // Vérifier si la route est protégée
  const isProtectedRoute = protectedRoutes.includes(pathname);
  const isPublicRoute = publicRoutes.some(route => pathname.startsWith(route));

  // Si c'est une route protégée
  if (isProtectedRoute) {
    // Vérifier s'il existe un token JWT (défini au login)
    const userCookie = request.cookies.get('auth_token');
    
    // Si pas de cookie user, rediriger vers connexion
    if (!userCookie) {
      const loginUrl = new URL('/connexion', request.url);
      return NextResponse.redirect(loginUrl);
    }
    
    return NextResponse.next();
  }

  // Si c'est une route publique (login/register)
  if (isPublicRoute) {
    // Optionnel: si user est déjà connecté, rediriger vers accueil
    const userCookie = request.cookies.get('auth_token');
    if (userCookie && pathname === '/connexion') {
      const homeUrl = new URL('/', request.url);
      return NextResponse.redirect(homeUrl);
    }
    return NextResponse.next();
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!_next/static|_next/image|favicon.ico).*)'],
};
