export interface User {
  id: string;
  name: string;
  email: string;
  role: string;
}

export interface AuthCallbackResponse {
  success: boolean;
  token: string;
  user: User;
  expiresIn: number;
  message: string;
}

export interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: () => void;
  logout: () => void;
  handleAuthCallback: (code: string, state?: string) => Promise<void>;
  setUser: (user: User | null) => void;
  setToken: (token: string | null) => void;
} 