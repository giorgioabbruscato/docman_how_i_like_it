import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { keycloak, mapKeycloakUser } from '@/lib/keycloak';
import { useAuthStore } from '@/stores/auth-store';

interface AuthContextValue {
  isAuthenticated: boolean;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

const TOKEN_REFRESH_INTERVAL_MS = 60_000;
const TOKEN_MIN_VALIDITY_SECONDS = 30;

function AuthLoadingScreen() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="text-center space-y-3">
        <div
          className="mx-auto h-8 w-8 animate-spin rounded-full border-2 border-primary border-t-transparent"
          role="status"
          aria-label="Loading"
        />
        <p className="text-sm text-muted-foreground">Signing in…</p>
      </div>
    </div>
  );
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const setAuth = useAuthStore((state) => state.setAuth);
  const logout = useAuthStore((state) => state.logout);
  const [isLoading, setIsLoading] = useState(true);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  const syncAuthState = useCallback(() => {
    if (keycloak.authenticated && keycloak.token) {
      const user = mapKeycloakUser(keycloak.tokenParsed);
      setAuth(keycloak.token, user);
      setIsAuthenticated(true);
      return;
    }

    logout();
    setIsAuthenticated(false);
  }, [logout, setAuth]);

  const refreshToken = useCallback(async () => {
    if (!keycloak.authenticated) {
      return;
    }

    try {
      const refreshed = await keycloak.updateToken(TOKEN_MIN_VALIDITY_SECONDS);
      if (refreshed || keycloak.token) {
        syncAuthState();
      }
    } catch {
      logout();
      setIsAuthenticated(false);
    }
  }, [logout, syncAuthState]);

  useEffect(() => {
    let refreshIntervalId: ReturnType<typeof setInterval> | undefined;

    const initKeycloak = async () => {
      try {
        const authenticated = await keycloak.init({
          onLoad: 'check-sso',
          pkceMethod: 'S256',
          checkLoginIframe: false,
        });

        if (authenticated) {
          syncAuthState();
        } else {
          logout();
          setIsAuthenticated(false);
        }

        keycloak.onTokenExpired = () => {
          void refreshToken();
        };

        refreshIntervalId = setInterval(() => {
          void refreshToken();
        }, TOKEN_REFRESH_INTERVAL_MS);
      } catch {
        logout();
        setIsAuthenticated(false);
      } finally {
        setIsLoading(false);
      }
    };

    void initKeycloak();

    return () => {
      if (refreshIntervalId) {
        clearInterval(refreshIntervalId);
      }
      keycloak.onTokenExpired = undefined;
    };
  }, [logout, refreshToken, syncAuthState]);

  const value = useMemo(
    () => ({
      isAuthenticated,
      isLoading,
    }),
    [isAuthenticated, isLoading],
  );

  if (isLoading) {
    return <AuthLoadingScreen />;
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
