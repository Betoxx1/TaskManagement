import React, { useEffect, useState } from 'react';
import { AlertCircle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import './AuthCallbackPage.css';

const AuthErrorPage: React.FC = () => {
  const navigate = useNavigate();
  const [errorMessage, setErrorMessage] = useState<string>('Error de autenticación');
  const [countdown, setCountdown] = useState<number>(5);

  useEffect(() => {
    // Extraer mensaje de error del fragment
    const hash = window.location.hash.substring(1);
    if (hash) {
      const params = new URLSearchParams(hash);
      const error = params.get('error');
      if (error) {
        setErrorMessage(decodeURIComponent(error));
      }
    }

    // Limpiar la URL
    window.history.replaceState({}, document.title, window.location.pathname);

    // Iniciar countdown
    const interval = setInterval(() => {
      setCountdown(prev => {
        if (prev <= 1) {
          clearInterval(interval);
          navigate('/login', { replace: true });
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, [navigate]);

  const handleReturnToLogin = () => {
    navigate('/login', { replace: true });
  };

  return (
    <div className="callback-container">
      <div className="callback-card">
        <div className="error-icon">
          <AlertCircle size={64} color="#ef4444" />
        </div>
        <h2>Error de autenticación</h2>
        <p className="error-message">{errorMessage}</p>
        <p>Redirigiendo al login en {countdown} segundos...</p>
        
        <button 
          onClick={handleReturnToLogin}
          className="retry-button"
          style={{
            marginTop: '20px',
            padding: '10px 20px',
            backgroundColor: '#007bff',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          Volver al Login
        </button>
      </div>
    </div>
  );
};

export default AuthErrorPage;