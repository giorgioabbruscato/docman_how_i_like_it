import Keycloak, { type KeycloakTokenParsed } from 'keycloak-js';

function requireEnv(value: string | undefined, name: string, devFallback: string): string {
  if (value) {
    return value;
  }

  if (import.meta.env.DEV) {
    return devFallback;
  }

  throw new Error(`Missing required environment variable: ${name}`);
}

const keycloakUrl = requireEnv(import.meta.env.VITE_KEYCLOAK_URL, 'VITE_KEYCLOAK_URL', 'http://localhost:8080');
const keycloakRealm = requireEnv(import.meta.env.VITE_KEYCLOAK_REALM, 'VITE_KEYCLOAK_REALM', 'hrportal');
const keycloakClientId = requireEnv(
  import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
  'VITE_KEYCLOAK_CLIENT_ID',
  'hrportal-web',
);

export const keycloak = new Keycloak({
  url: keycloakUrl,
  realm: keycloakRealm,
  clientId: keycloakClientId,
});

export interface AuthUser {
  email: string;
  name: string;
  roles: string[];
}

const systemRoles = new Set(['offline_access', 'uma_authorization']);

export function mapKeycloakUser(tokenParsed: KeycloakTokenParsed | undefined): AuthUser | null {
  if (!tokenParsed) {
    return null;
  }

  const email =
    (typeof tokenParsed.email === 'string' && tokenParsed.email) ||
    (typeof tokenParsed.preferred_username === 'string' && tokenParsed.preferred_username) ||
    '';

  const name =
    (typeof tokenParsed.name === 'string' && tokenParsed.name) ||
    [tokenParsed.given_name, tokenParsed.family_name]
      .filter((part): part is string => typeof part === 'string' && part.length > 0)
      .join(' ') ||
    email;

  const roles = (tokenParsed.realm_access?.roles ?? []).filter(
    (role) => !systemRoles.has(role) && !role.startsWith('default-roles-'),
  );

  return { email, name, roles };
}
