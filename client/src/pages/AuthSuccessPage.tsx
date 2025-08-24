import React, { useEffect, useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import { User } from '../types/auth';
import authService from '../services/authService';
import './AuthCallbackPage.css'; // Reutilizamos los estilos

const AuthSuccessPage: React.FC = () => {
  const { setUser, setToken } = useAuth();
  const navigate = useNavigate();
  const [status, setStatus] = useState<'processing' | 'success' | 'error'>('processing');
  const [errorMessage, setErrorMessage] = useState<string>('');
  const [userData, setUserData] = useState<User | null>(null);
  const [hasProcessed, setHasProcessed] = useState(false);

  useEffect(() => {
    if (hasProcessed) return;

    const processAuthSuccess = async () => {
      setHasProcessed(true);
      
      try {
        // Extraer datos del fragment (después del #)
        const hash = window.location.hash.substring(1);
        if (!hash) {
          throw new Error('No se encontraron datos de autenticación');
        }

        const params = new URLSearchParams(hash);
        const success = params.get('success');
        const error = params.get('error');

        // Verificar si hay error
        if (error || success === 'false') {
          throw new Error(error || 'Error de autenticación');
        }

        // Extraer datos del usuario y token
        const token = params.get('token');
        const userId = params.get('user_id');
        const userName = params.get('user_name');
        const userEmail = params.get('user_email');
        const userRole = params.get('user_role');
        const expiresIn = params.get('expires_in');

        // Validar datos requeridos
        if (!token || !userId || !userEmail || !userName) {
          throw new Error('Datos de autenticación incompletos');
        }

        // Crear objeto de usuario
        const user: User = {
          id: userId,
          name: userName,
          email: userEmail,
          role: userRole || 'User'
        };

        // Guardar datos de autenticación
        authService.saveAuthData(token, user);
        
        // Actualizar contexto (si tienes setters directos)
        if (setUser && setToken) {
          setUser(user);
          setToken(token);
        }

        setUserData(user);
        setStatus('success');

        // Limpiar la URL eliminando el fragment
        window.history.replaceState({}, document.title, window.location.pathname);

        // Redirigir al dashboard después de mostrar el éxito
        setTimeout(() => {
          navigate('/dashboard', { replace: true });
        }, 3000);

      } catch (error) {
        console.error('Error procesando autenticación:', error);
        setStatus('error');
        setErrorMessage(error instanceof Error ? error.message : 'Error desconocido');
        
        // Redirigir al login después de mostrar el error
        setTimeout(() => {
          navigate('/login', { replace: true });
        }, 5000);
      }
    };

    processAuthSuccess();
  }, [navigate, setUser, setToken, hasProcessed]);

  const renderContent = () => {
    switch (status) {
      case 'processing':
        return (
          <>
            <div className="loading-spinner-large"></div>
            <h2>Procesando autenticación...</h2>
            <p>Validando credenciales...</p>
          </>
        );
      
      case 'success':
        return (
          <>
            <div className="success-icon">✓</div>
            <h2>¡Bienvenido/a!</h2>
            {userData && (
              <div className="user-info">
                <p><strong>Nombre:</strong> {userData.name}</p>
                <p><strong>Email:</strong> {userData.email}</p>
                <p><strong>Rol:</strong> {userData.role}</p>
              </div>
            )}
            <p className="success-message">Autenticación completada exitosamente</p>
            <p>Redirigiendo al panel de control...</p>
          </>
        );
      
      case 'error':
        return (
          <>
            <div className="error-icon">✗</div>
            <h2>Error de autenticación</h2>
            <p className="error-message">{errorMessage}</p>
            <p>Serás redirigido al login en unos segundos...</p>
          </>
        );
      
      default:
        return null;
    }
  };

  return (
    <div className="callback-container">
      <div className="callback-card">
        {renderContent()}
      </div>
    </div>
  );
};

export default AuthSuccessPage;