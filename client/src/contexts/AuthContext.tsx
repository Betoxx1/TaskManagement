import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { AuthContextType, User } from '../types/auth';
import authService from '../services/authService';

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Inicializar estado de autenticación al cargar la aplicación
  useEffect(() => {
    const initializeAuth = () => {
      try {
        const savedToken = authService.getToken();
        const savedUser = authService.getUser();

        if (savedToken && savedUser) {
          setToken(savedToken);
          setUser(savedUser);
        }
      } catch (error) {
        console.error('Error inicializando autenticación:', error);
        authService.clearAuthData();
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, []);

  const login = () => {
    const authUrl = authService.generateAuthUrl();
    console.log('login', {authUrl});
    window.location.href = authUrl;
  };

  const logout = () => {
    authService.clearAuthData();
    setUser(null);
    setToken(null);
    window.location.href = '/';
  };

  const handleAuthCallback = async (code: string, state?: string) => {
    try {
      setIsLoading(true);
      const response = await authService.processAuthCallback(code, state);

      if (response.success) {
        authService.saveAuthData(response.token, response.user);
        setToken(response.token);
        setUser(response.user);
      } else {
        throw new Error(response.message || 'Error en la autenticación');
      }
    } catch (error) {
      console.error('Error en callback de autenticación:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const value: AuthContextType = {
    user,
    token,
    isAuthenticated: !!(user && token),
    isLoading,
    login,
    logout,
    handleAuthCallback,
    setUser,
    setToken
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth debe ser usado dentro de un AuthProvider');
  }
  return context;
}; 