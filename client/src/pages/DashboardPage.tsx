import React from 'react';
import { useAuth } from '../contexts/AuthContext';
import './DashboardPage.css';

const DashboardPage: React.FC = () => {
  const { user, logout } = useAuth();

  const handleLogout = () => {
    if (window.confirm('¿Estás seguro de que quieres cerrar sesión?')) {
      logout();
    }
  };

  return (
    <div className="dashboard-container">
      <header className="dashboard-header">
        <div className="header-content">
          <h1>Task Management</h1>
          <div className="user-menu">
            <div className="user-info">
              <span className="user-name">{user?.name}</span>
              <span className="user-email">{user?.email}</span>
              <span className="user-role">{user?.role}</span>
            </div>
            <button className="logout-button" onClick={handleLogout}>
              Cerrar Sesión
            </button>
          </div>
        </div>
      </header>

      <main className="dashboard-main">
        <div className="welcome-section">
          <div className="welcome-card">
            <div className="welcome-icon">👋</div>
            <h2>¡Bienvenido, {user?.name}!</h2>
            <p>Has iniciado sesión exitosamente en el sistema de gestión de tareas.</p>
            
            <div className="user-details">
              <div className="detail-item">
                <label>Nombre:</label>
                <span>{user?.name}</span>
              </div>
              <div className="detail-item">
                <label>Email:</label>
                <span>{user?.email}</span>
              </div>
              <div className="detail-item">
                <label>Rol:</label>
                <span className="role-badge">{user?.role}</span>
              </div>
              <div className="detail-item">
                <label>ID de Usuario:</label>
                <span className="user-id">{user?.id}</span>
              </div>
            </div>

            <div className="status-section">
              <div className="status-indicator">
                <div className="status-dot"></div>
                <span>Usuario Autenticado</span>
              </div>
              <p className="status-description">
                Tu sesión está activa y segura. Puedes comenzar a utilizar el sistema.
              </p>
            </div>
          </div>
        </div>

        <div className="actions-section">
          <h3>Próximos pasos</h3>
          <div className="action-cards">
            <div className="action-card">
              <div className="action-icon">📋</div>
              <h4>Gestionar Tareas</h4>
              <p>Crea, edita y organiza tus tareas diarias</p>
              <button className="action-button" disabled>
                Próximamente
              </button>
            </div>
            
            <div className="action-card">
              <div className="action-icon">👥</div>
              <h4>Colaborar</h4>
              <p>Trabaja en equipo y comparte proyectos</p>
              <button className="action-button" disabled>
                Próximamente
              </button>
            </div>
            
            <div className="action-card">
              <div className="action-icon">📊</div>
              <h4>Reportes</h4>
              <p>Visualiza el progreso y estadísticas</p>
              <button className="action-button" disabled>
                Próximamente
              </button>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default DashboardPage; 