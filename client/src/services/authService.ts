import axios from 'axios';
import { AuthCallbackResponse } from '../types/auth';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:7071/api';

// Configuración de Azure AD - estas deberían venir de variables de entorno
const AZURE_AD_CONFIG = {
  clientId: import.meta.env.VITE_AZURE_CLIENT_ID || 'tu-client-id',
  tenantId: import.meta.env.VITE_AZURE_TENANT_ID || 'tu-tenant-id',
  redirectUri: import.meta.env.VITE_REDIRECT_URI || `${window.location.origin}/auth/callback`,
  scopes: ['openid', 'profile', 'email']
};

console.log({AZURE_AD_CONFIG});

class AuthService {
  // Generar URL de autorización de Azure AD
  generateAuthUrl(): string {
    const state = this.generateState();
    sessionStorage.setItem('auth_state', state);

    console.log('generateAuthUrl', {state});

    const params = new URLSearchParams({
      client_id: AZURE_AD_CONFIG.clientId,
      response_type: 'code',
      redirect_uri: AZURE_AD_CONFIG.redirectUri,
      scope: AZURE_AD_CONFIG.scopes.join(' '),
      state: state,
      response_mode: 'query'
    });

    return `https://login.microsoftonline.com/${AZURE_AD_CONFIG.tenantId}/oauth2/v2.0/authorize?${params.toString()}`;
  }

  // Generar estado aleatorio para CSRF protection
  private generateState(): string {
    return Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
  }

  // Validar estado de CSRF
  validateState(state: string): boolean {
    const savedState = sessionStorage?.getItem('auth_state') || '';
    console.log('validateState', {savedState, state});
    sessionStorage?.removeItem('auth_state');
    return savedState === state;
  }

  // Procesar callback de autenticación
  async processAuthCallback(code: string, state?: string): Promise<AuthCallbackResponse> {
    if (state && !this.validateState(state)) {
      throw new Error('Estado de autenticación inválido');
    }

    try {
      const response = await axios.get(`${API_BASE_URL}/auth/callback`, {
        params: { code, state }
      });

      return response.data;
    } catch (error) {
      console.error('Error procesando callback de autenticación:', error);
      throw new Error('Error en el proceso de autenticación');
    }
  }

  // Guardar token en localStorage
  saveAuthData(token: string, user: any): void {
    localStorage.setItem('auth_token', token);
    localStorage.setItem('user_data', JSON.stringify(user));
  }

  // Obtener token guardado
  getToken(): string | null {
    return localStorage.getItem('auth_token');
  }

  // Obtener datos de usuario guardados
  getUser(): any | null {
    const userData = localStorage.getItem('user_data');
    return userData ? JSON.parse(userData) : null;
  }

  // Limpiar datos de autenticación
  clearAuthData(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('user_data');
  }

  // Verificar si el usuario está autenticado
  isAuthenticated(): boolean {
    const token = this.getToken();
    const user = this.getUser();
    return !!(token && user);
  }
}

export default new AuthService(); 