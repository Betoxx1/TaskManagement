import React from 'react';
import { useAuth } from '../contexts/AuthContext';
import './LoginPage.css';

const LoginPage: React.FC = () => {
  const { login, isLoading } = useAuth();

  const handleLogin = () => {
    if (!isLoading) {
      login();
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-header">
          <h1>Task Management</h1>
          <p>Sistema de Gestión de Tareas</p>
        </div>
        
        <div className="login-content">
          <h2>Iniciar Sesión</h2>
          <p>Accede con tu cuenta de Microsoft Azure AD</p>
          
          <button 
            className="login-button"
            onClick={handleLogin}
            disabled={isLoading}
          >
            {isLoading ? (
              <span className="loading-spinner"></span>
            ) : (
              <>
                <svg className="microsoft-icon" viewBox="0 0 23 23">
                  <path fill="#f35325" d="M0 0h11v11H0V0z"/>
                  <path fill="#81bc06" d="M12 0h11v11H12V0z"/>
                  <path fill="#05a6f0" d="M0 12h11v11H0V12z"/>
                  <path fill="#ffba08" d="M12 12h11v11H12V12z"/>
                </svg>
                Iniciar sesión con Microsoft
              </>
            )}
          </button>
        </div>
        
        <div className="login-footer">
          <p>Utiliza tu cuenta corporativa para acceder al sistema</p>
        </div>
      </div>
    </div>
  );
};

export default LoginPage; 