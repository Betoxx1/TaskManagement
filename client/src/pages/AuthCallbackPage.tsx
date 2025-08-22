import React, { useEffect, useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate, useSearchParams } from 'react-router-dom';
import './AuthCallbackPage.css';

const AuthCallbackPage: React.FC = () => {
  const { handleAuthCallback } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [status, setStatus] = useState<'processing' | 'success' | 'error'>('processing');
  const [errorMessage, setErrorMessage] = useState<string>('');
  const [hasProcessed, setHasProcessed] = useState(false); // ← NUEVO

  useEffect(() => {
    if (hasProcessed) return; // ← PROTECCIÓN

    const processCallback = async () => {
      setHasProcessed(true); // ← MARCAR COMO PROCESADO
      try {
        const code = searchParams.get('code');
        const state = searchParams.get('state');
        const error = searchParams.get('error');
        const errorDescription = searchParams.get('error_description');
        console.log({code, state, error, errorDescription});
        // Manejar errores de OAuth
        if (error) {
          throw new Error(errorDescription || error);
        }

        // Verificar que tenemos el código de autorización
        if (!code) {
          throw new Error('Código de autorización no encontrado');
        }

        // Procesar el callback
        await handleAuthCallback(code, state || undefined);
        
        setStatus('success');
        
        // Redirigir al dashboard después de un breve delay
        setTimeout(() => {
          navigate('/dashboard', { replace: true });
        }, 2000);

      } catch (error) {
        console.error('Error en callback:', error);
        setStatus('error');
        setErrorMessage(error instanceof Error ? error.message : 'Error desconocido');
        
        // Redirigir al login después de mostrar el error
        setTimeout(() => {
          navigate('/login', { replace: true });
        }, 5000);
      }
    };

    processCallback();
  }, [searchParams, handleAuthCallback, navigate]);

  const renderContent = () => {
    switch (status) {
      case 'processing':
        return (
          <>
            <div className="loading-spinner-large"></div>
            <h2>Procesando autenticación...</h2>
            <p>Por favor espera mientras validamos tu identidad</p>
          </>
        );
      
      case 'success':
        return (
          <>
            <div className="success-icon">✓</div>
            <h2>¡Autenticación exitosa!</h2>
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

export default AuthCallbackPage; 